/*
 * This file is part of OpenCollar.Azure.ReliableQueue.
 *
 * OpenCollar.Azure.ReliableQueue is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Azure.ReliableQueue is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Azure.ReliableQueue.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>A service used to store and retrieve the body of messages.</summary>
    /// <seealso cref="IMessageStorageService"/>
    /// <remarks>
    ///     This implementation uses blob storage and leases to manage concurrency.  An alternate version was tried using tables and ETag validation.  Under
    ///     high concurrent loads this proved to be very inefficient when compared to the blob/lease approach and so has not been used.
    /// </remarks>
    internal sealed class MessageStorageService : IMessageStorageService
    {
        /// <summary>The minimum sleep period (in milliseconds).</summary>
        private const int MinSleep = 250;

        /// <summary>The maximum sleep period (in milliseconds).</summary>
        private const int MaxSleep = 500;

        /// <summary>The maximum permitted duration of a Blob lease (apart from infinite).</summary>
        private static readonly TimeSpan MaxLeaseDuration = TimeSpan.FromSeconds(60);

        /// <summary>A random number generator used to produce values for the sleep used when backing-off and retrying.</summary>
        private static readonly Random _random = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>The service used to access the configuration for the queues used to send and receive messages.</summary>
        private readonly IReliableQueueConfigurationService _reliableQueueConfigurationService;

        /// <summary>The service used to create and manage clients for the various Azure Storage resources used reliable queues.</summary>
        [NotNull]
        private readonly IStorageResourceService _storageResourceService;

        /// <summary>Initializes a new instance of the <see cref="MessageStorageService"/> class.</summary>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="storageResourceService">The service used to create and manage clients for the various Azure Storage resources used reliable queues.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="ReliableQueueConfigurationService"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="storageResourceService"/> was <see langword="null"/>.</exception>
        public MessageStorageService([NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService,
            [NotNull] IStorageResourceService storageResourceService)
        {
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);
            storageResourceService.Validate(nameof(storageResourceService), ObjectIs.NotNull);

            _reliableQueueConfigurationService = ReliableQueueConfigurationService;
            _storageResourceService = storageResourceService;

            var tasks = new List<Task>();

            foreach(var queue in ReliableQueueConfigurationService.ReliableQueues)
            {
                var blobClient = _storageResourceService.GetBlobClient(queue.Key);
                var containerReference = blobClient.GetContainerReference(Identifiers.GetMessageContainerName(queue.Key));
                tasks.Add(containerReference.CreateIfNotExistsAsync());
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>Writes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to write the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be written.</param>
        /// <param name="blob">A stream containing the BLOB to write into the BLOB storage.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to write the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to write the message body to the BLOB storage.</param>
        /// <returns>A task that writes the message body to the BLOB storage.</returns>
        public async Task<Message> WriteMessageAsync(ReliableQueueKey reliableQueueKey, Message message, Stream? blob, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if(blob is null)
            {
                message.Size = null;
                message.BodyIsNull = true;
                return message;
            }

            // Start by getting the configuration.
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            var blobClient = _storageResourceService.GetBlobClient(reliableQueueKey);

            var context = new OperationContext
            {
                ClientRequestID = message.Id.ToString("D", CultureInfo.InvariantCulture)
            };
            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var options = new BlobRequestOptions
            {
                MaximumExecutionTime = timeoutPeriod
            };
            var token = cancellationToken ?? CancellationToken.None;

            // Get a reference to the container in which we keep the blob.
            var containerReference = blobClient.GetContainerReference(Identifiers.GetMessageContainerName(reliableQueueKey));

            // Now get a reference to the blob itself.
            var blobIdentity = message.Id.ToString("D", CultureInfo.InvariantCulture);
            var blobReference = containerReference.GetBlockBlobReference(blobIdentity);

            // Now we must lease the blob so we can write the new value.  We use the maximum permitted duration, because we
            // might encounter delays and the timeout supplied might exceed the maximum permitted by Azure.
            string? leaseId = null;
            var start = DateTime.UtcNow;
            var uploaded = false;
            while(!uploaded)
            {
                try
                {
                    leaseId = await blobReference.AcquireLeaseAsync(MaxLeaseDuration, context.ClientRequestID, AccessCondition.GenerateEmptyCondition(),
                        options, context, token).ConfigureAwait(true);

                    if(!string.IsNullOrWhiteSpace(leaseId))
                    {
                        break;
                    }
                }
                catch(StorageException ex)
                {
                    switch(ex.RequestInformation.HttpStatusCode)
                    {
                        case 404:
                            if(string.Equals(ex.RequestInformation.ErrorCode, @"ContainerNotFound", StringComparison.Ordinal))
                            {
                                await containerReference.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, options, context, token).ConfigureAwait(true);
                            }
                            else
                            {
                                if(string.Equals(ex.RequestInformation.ErrorCode, @"BlobNotFound", StringComparison.Ordinal))
                                {
                                    // If the blob doesn't already exist, create it and default it zero.
                                    await blobReference.UploadFromStreamAsync(blob, AccessCondition.GenerateEmptyCondition(), options, context, token).ConfigureAwait(true);
                                    leaseId = null;
                                    uploaded = true;
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            break;

                        case 409:
                            // We expect to receive this if someone else already holds the lease.
                            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(MinSleep, MaxSleep)), token).ConfigureAwait(true);
                            break;

                        default:
                            throw;
                    }
                }

                var now = DateTime.UtcNow;
                if(!uploaded && now - start > timeoutPeriod)
                {
                    throw new TimeoutException("Timeout waiting for lease.");
                }
            }

            try
            {
                if(!uploaded)
                {
                    await blobReference.UploadFromStreamAsync(blob, AccessCondition.GenerateLeaseCondition(leaseId), options, context, token).ConfigureAwait(true);
                }

                await blobReference.FetchAttributesAsync().ConfigureAwait(true);
                var length = blobReference.Properties.Length;
                if(length > 0)
                {
                    message.Size = length;
                    message.BodyIsNull = false;
                }
                else
                {
                    message.Size = null;
                    message.BodyIsNull = true;
                }

                return message;
            }
            finally
            {
                if(!(leaseId is null))
                {
                    await blobReference.ReleaseLeaseAsync(new AccessCondition
                    {
                        LeaseId = leaseId
                    }, options, context, token).ConfigureAwait(true);
                }
            }
        }

        /// <summary>Reads the body of a message from BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to read the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be read.</param>
        /// <param name="blob">The stream into which the BLOB will be read.</param>
        /// <param name="timeout">The maximum period of time to wait whilst attempting to read the message body to the BLOB storage before failing with an error.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to read the message body to the BLOB storage.</param>
        /// <returns>A task that returns a stream containing the message body from the BLOB storage, or <see langword="null"/> if there is no body.</returns>
        public async Task<Stream?> ReadMessageAsync(ReliableQueueKey reliableQueueKey, Message message, Stream blob, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if(message.BodyIsNull || message.Size is null)
            {
                return null;
            }

            // Start by getting the configuration.
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            // Create a client from those details.
            var blobClient = _storageResourceService.GetBlobClient(reliableQueueKey);

            var context = new OperationContext
            {
                ClientRequestID = message.Id.ToString("D", CultureInfo.InvariantCulture)
            };
            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var options = new BlobRequestOptions
            {
                MaximumExecutionTime = timeoutPeriod
            };
            var token = cancellationToken ?? CancellationToken.None;

            // Get a reference to the container in which we keep the blob.
            var containerReference = blobClient.GetContainerReference(Identifiers.GetMessageContainerName(reliableQueueKey));

            // Now get a reference to the blob itself.
            var blobIdentity = message.Id.ToString("D", CultureInfo.InvariantCulture);
            var blobReference = containerReference.GetBlockBlobReference(blobIdentity);

            // Now we must lease the blob so we can write the new value.  We use the maximum permitted duration, because we
            // might encounter delays and the timeout supplied might exceed the maximum permitted by Azure.
            string leaseId;
            var start = DateTime.UtcNow;
            while(true)
            {
                try
                {
                    leaseId = await blobReference.AcquireLeaseAsync(MaxLeaseDuration, context.ClientRequestID, AccessCondition.GenerateEmptyCondition(),
                        options, context, token).ConfigureAwait(true);

                    if(!string.IsNullOrWhiteSpace(leaseId))
                    {
                        break;
                    }
                }
                catch(StorageException ex)
                {
                    switch(ex.RequestInformation.HttpStatusCode)
                    {
                        case 404:
                            if(string.Equals(ex.RequestInformation.ErrorCode, @"ContainerNotFound", StringComparison.Ordinal))
                            {
                                await containerReference.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Container, options, context, token).ConfigureAwait(true);
                            }
                            else
                            {
                                if(string.Equals(ex.RequestInformation.ErrorCode, @"BlobNotFound", StringComparison.Ordinal))
                                {
                                    message.Size = null;
                                    message.BodyIsNull = true;
                                    return null;
                                }

                                throw;
                            }

                            break;

                        case 409:
                            // We expect to receive this if someone else already holds the lease.
                            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(MinSleep, MaxSleep)), token).ConfigureAwait(true);
                            break;

                        default:
                            throw;
                    }
                }

                var now = DateTime.UtcNow;
                if(now - start > timeoutPeriod)
                {
                    throw new TimeoutException("Timeout waiting for lease.");
                }
            }

            try
            {
                await blobReference.DownloadToStreamAsync(blob, AccessCondition.GenerateEmptyCondition(), options, context, token).ConfigureAwait(true);

                return blob;
            }
            finally
            {
                await blobReference.ReleaseLeaseAsync(new AccessCondition
                {
                    LeaseId = leaseId
                }, options, context, token).ConfigureAwait(true);
            }
        }

        /// <summary>Deletes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to delete the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be deleted.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to delete the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to delete the message body to the BLOB storage.</param>
        /// <returns>A task that deletes the message body to the BLOB storage.</returns>
        public async Task DeleteMessageAsync(ReliableQueueKey reliableQueueKey, Message message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if(message.BodyIsNull || message.Size is null)
            {
                return;
            }

            // Start by getting the configuration.
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            var blobClient = _storageResourceService.GetBlobClient(reliableQueueKey);

            var context = new OperationContext
            {
                ClientRequestID = message.Id.ToString("D", CultureInfo.InvariantCulture)
            };
            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var options = new BlobRequestOptions
            {
                MaximumExecutionTime = timeoutPeriod
            };
            var token = cancellationToken ?? CancellationToken.None;

            // Get a reference to the container in which we keep the blob.
            var containerReference = blobClient.GetContainerReference(Identifiers.GetMessageContainerName(reliableQueueKey));

            // Now get a reference to the blob itself.
            var blobIdentity = message.Id.ToString("D", CultureInfo.InvariantCulture);
            var blobReference = containerReference.GetBlockBlobReference(blobIdentity);

            // Now we must lease the blob so we can write the new value.  We use the maximum permitted duration, because we
            // might encounter delays and the timeout supplied might exceed the maximum permitted by Azure.
            string leaseId;
            var start = DateTime.UtcNow;
            while(true)
            {
                try
                {
                    leaseId = await blobReference.AcquireLeaseAsync(MaxLeaseDuration, context.ClientRequestID, AccessCondition.GenerateEmptyCondition(),
                        options, context, token).ConfigureAwait(true);

                    if(!string.IsNullOrWhiteSpace(leaseId))
                    {
                        break;
                    }
                }
                catch(StorageException ex)
                {
                    switch(ex.RequestInformation.HttpStatusCode)
                    {
                        case 404:
                            if(string.Equals(ex.RequestInformation.ErrorCode, @"ContainerNotFound", StringComparison.Ordinal))
                            {
                                // No container?  No blob to delete then.
                                return;
                            }
                            else
                            {
                                if(string.Equals(ex.RequestInformation.ErrorCode, @"BlobNotFound", StringComparison.Ordinal))
                                {
                                    // No blob to delete.
                                    return;
                                }
                            }

                            throw;

                        case 409:
                            // We expect to receive this if someone else already holds the lease.
                            await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(MinSleep, MaxSleep)), token).ConfigureAwait(true);
                            break;

                        default:
                            throw;
                    }
                }

                var now = DateTime.UtcNow;
                if(now - start > timeoutPeriod)
                {
                    throw new TimeoutException("Timeout waiting for lease.");
                }
            }

            var deleted = false;
            try
            {
                await blobReference.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, new AccessCondition
                {
                    LeaseId = leaseId
                }, options, context, token).ConfigureAwait(true);

                deleted = true;
            }
            finally
            {
                if(!deleted)
                {
                    await blobReference.ReleaseLeaseAsync(new AccessCondition
                    {
                        LeaseId = leaseId
                    }, options, context, token).ConfigureAwait(true);
                }
            }
        }
    }
}