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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Protocol;

using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>A service used to manage topic affinity and the ordering of the messages belonging to the same topic.</summary>
    /// <seealso cref="IMessageTopicService"/>
    internal sealed class MessageTopicService : IMessageTopicService
    {
        /// <summary>The service used to access the configuration for the queues used to send and receive messages.</summary>
        [NotNull]
        private readonly IReliableQueueConfigurationService _reliableQueueConfigurationService;

        /// <summary>The service used to create and manage clients for the various Azure Storage resources used reliable queues.</summary>
        [NotNull]
        private readonly IStorageResourceService _storageResourceService;

        /// <summary>Initializes a new instance of the <see cref="MessageTopicService"/> class.</summary>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="storageResourceService">The service used to create and manage clients for the various Azure Storage resources used reliable queues.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="ReliableQueueConfigurationService"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="storageResourceService"/> was <see langword="null"/>.</exception>
        public MessageTopicService([NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService,
            [NotNull] IStorageResourceService storageResourceService)
        {
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);
            storageResourceService.Validate(nameof(storageResourceService), ObjectIs.NotNull);

            _reliableQueueConfigurationService = ReliableQueueConfigurationService;
            _storageResourceService = storageResourceService;

            var tasks = new List<Task>();

            foreach(var queue in ReliableQueueConfigurationService.ReliableQueues)
            {
                var table = _storageResourceService.GetTopicTable(queue.Key);

                tasks.Add(table.CreateIfNotExistsAsync());
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>Gets the live topics from the reliable queue.</summary>
        /// <param name="reliableQueueKey">The key identifying the queue from which to read the topics.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to retrieve topics before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to retrieve topics.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>A sequence containing the topics found to be active.</returns>
        /// <exception cref="ReliableQueueException">"Unable to fetch topics from table on queue.</exception>
        public IEnumerable<Topic> GetLiveTopics(ReliableQueueKey reliableQueueKey, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var context = new OperationContext
            {
                ClientRequestID = Guid.NewGuid().ToString(@"D", CultureInfo.InvariantCulture)
            };
            var options = new TableRequestOptions
            {
                MaximumExecutionTime = timeoutPeriod,
                ServerTimeout = timeoutPeriod
            };

            var table = _storageResourceService.GetStateTable(reliableQueueKey);

            var topicMessageQuery = new TableQuery().Select(new[] { nameof(Message.Topic) }).OrderBy(nameof(Message.Timestamp));

            IEnumerable<DynamicTableEntity> topicMessageResult;
            try
            {
                topicMessageResult = table.ExecuteQuery(topicMessageQuery, options, context).ToArray();
            }
            catch(StorageException ex1)
            {
                if(ex1.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.TableNotFound)
                {
                    table.CreateIfNotExists(options, context);
                    try
                    {
                        topicMessageResult = table.ExecuteQuery(topicMessageQuery, options, context);
                    }
                    catch(Exception ex2)
                    {
                        throw new ReliableQueueException(reliableQueueKey,
                            $@"Unable to fetch topics from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex2.Message}"".  See inner exception for details.",
                            ex2);
                    }
                }
                else
                {
                    throw;
                }
            }

            if(topicMessageResult is null)
            {
                return Enumerable.Empty<Topic>();
            }

            return topicMessageResult.Select(t => t[nameof(Message.Topic)].StringValue).Distinct().Select(t => new Topic(t));
        }

        /// <summary>Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be processed.</summary>
        /// <param name="message">The message received.</param>
        /// <param name="ReliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to process the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to process the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>
        ///     A task that processes the message supplied and returns <see langword="true"/> if the message will be processed; otherwise,
        ///     <see langword="false"/> if it is to be ignored.
        /// </returns>
        public async Task<bool> OnReceivedAsync(Message message, IReliableQueueService ReliableQueueService, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            // 1) Always accept default topic.
            // 2) Check for existing affinity record:
            //      2.1) If there isn't an existing affinity record, create a new one:
            //          2.1.1) Process the message
            //      2.2) If there is an existing affinity record check to see if it is still in scope:
            //          2.2.1) If it has timed-out, delete the record and go to 1.1.
            //          2.2.2) If it is in scope:
            //              2.2.2.3) If we have affinity process the message.
            //              2.2.2.4) If we do not have affinity leave the message for the endpoint with affinity.

            var now = DateTime.UtcNow;

            if(!ReliableQueueService[message.ReliableQueueKey].CanReceive)
            {
                throw new InvalidOperationException("Message queue is configured to be send-only.");
            }

            if(message.Topic.IsEmpty)
            {
                // The default topic can be handled anywhere with no affinity.
                return true;
            }

            var configuration = _reliableQueueConfigurationService[message.ReliableQueueKey];

            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var token = cancellationToken ?? CancellationToken.None;
            var context = new OperationContext
            {
                ClientRequestID = message.Id.ToString("D", CultureInfo.InvariantCulture)
            };
            var options = new TableRequestOptions
            {
                MaximumExecutionTime = timeoutPeriod,
                ServerTimeout = timeoutPeriod,
                TableQueryMaxItemCount = 1
            };

            var table = _storageResourceService.GetTopicTable(message.ReliableQueueKey);

            var selectQuery = TableOperation.Retrieve(message.ReliableQueueKey.Identifier, message.Topic.Identifier);

            TopicAffinity? currentAffinityRecord = null;

            bool retrySelect;
            do
            {
                // Attempt to select the current record.
                var selectResult = await table.ExecuteAsync(selectQuery, options, context, token).ConfigureAwait(true);

                switch(selectResult.HttpStatusCode)
                {
                    case 404:
                        // Attempt to insert a new record.
                        var insertQuery = TableOperation.Insert(new TopicAffinity(message.ReliableQueueKey, Identity.Current, message.Topic)
                        {
                            LastUpdated = now
                        }.Serialize(context));
                        try
                        {
                            await table.ExecuteAsync(insertQuery, options, context, token).ConfigureAwait(true);
                        }
                        catch(StorageException ex)
                        {
                            switch(ex.RequestInformation.HttpStatusCode)
                            {
                                case 404:
                                    // The table doesn't exists!
                                    await table.CreateIfNotExistsAsync(options, context, token).ConfigureAwait(true);
                                    break;

                                case 409:
                                    // There could be a conflict if the record had already been created by another instance.
                                    // In which case we should let that existing successful attempt win.
                                    break;

                                default:
                                    throw;
                            }
                        }

                        retrySelect = true;
                        break;

                    case 200:
                        currentAffinityRecord = ((DynamicTableEntity)selectResult.Result).Deserialize<TopicAffinity>(context);
                        retrySelect = false;
                        break;

                    default:
                        throw new ReliableQueueException(message.ReliableQueueKey,
                            $"Unable to read topic record for queue: {ReliableQueueException.GetReliableQueueKey(message.ReliableQueueKey)}; Topic: {message.Topic.Identifier}.");
                }
            }
            while(retrySelect);

            // So, we have got to this point.  That means:
            //  We have a non-empty topic.
            //  There is an existing topic record.

            //      2.2) If there is an existing affinity record check to see if it is still in scope:
            //          2.2.1) If it has timed-out, delete the record and go to 1.1.
            //          2.2.2) If it is in scope:
            //              2.2.2.3) If we have affinity process the message.
            //              2.2.2.4) If we do not have affinity leave the message for the endpoint with affinity.

            TableOperation updateQuery;
            if(currentAffinityRecord.LastUpdated.AddSeconds(configuration.TopicAffinityTtlSeconds) < now)
            {
                // Replace the existing record and go ahead (unless someone else go there first!)
                currentAffinityRecord.LastUpdated = now;
                currentAffinityRecord.Owner = Identity.Current;

                updateQuery = TableOperation.Replace(currentAffinityRecord.Serialize(context));
                try
                {
                    await table.ExecuteAsync(updateQuery, options, context, token).ConfigureAwait(true);
                    return true;
                }
                catch(StorageException ex)
                {
                    switch(ex.RequestInformation.HttpStatusCode)
                    {
                        case 412:
                            // Someone else go there first.
                            return false;

                        default:
                            throw;
                    }
                }
            }

            // Do we hold the affinity?
            if(currentAffinityRecord.Owner != Identity.Current)
            {
                // Defer to the existing holder of the affinity.
                return false;
            }

            //  Update the record with a new timestamp and proceed.
            currentAffinityRecord.LastUpdated = now;
            updateQuery = TableOperation.Replace(currentAffinityRecord.Serialize(context));
            try
            {
                await table.ExecuteAsync(updateQuery, options, context, token).ConfigureAwait(true);
                return true;
            }
            catch(StorageException ex)
            {
                switch(ex.RequestInformation.HttpStatusCode)
                {
                    case 412:
                        // Someone else go there first (maybe we were right on the cusp of expiring).
                        return false;

                    default:
                        throw;
                }
            }
        }
    }
}