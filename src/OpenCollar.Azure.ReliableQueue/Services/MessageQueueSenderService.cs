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

namespace OpenCollar.Azure.ReliableQueue.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Azure;

    using JetBrains.Annotations;

    using Microsoft.WindowsAzure.Storage.Queue.Protocol;

    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Extensions.Validation;

    /// <summary>
    /// Defines the <see cref="ReliableQueueSenderService" />.
    /// </summary>
    internal sealed class ReliableQueueSenderService : IReliableQueueSenderService
    {
        /// <summary>
        /// Defines the _storageResourceService.
        /// </summary>
        [NotNull]
        private readonly IStorageResourceService _storageResourceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueSenderService"/> class.
        /// </summary>
        /// <param name="storageResourceService">The service used to create and manage clients for the various Azure Storage resources used reliable queues.</param>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        public ReliableQueueSenderService([NotNull] IStorageResourceService storageResourceService,
            [NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService)
        {
            storageResourceService.Validate(nameof(storageResourceService), ObjectIs.NotNull);
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);

            _storageResourceService = storageResourceService;

            var tasks = new List<Task>();

            foreach (var queue in ReliableQueueConfigurationService.ReliableQueues)
            {
                var queueClient = _storageResourceService.GetQueueClient(queue.Key);

                tasks.Add(queueClient.CreateIfNotExistsAsync());
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// The SendMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to record.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        public async Task<Message> SendMessageAsync(QueueKey queueKey, Message message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            var queueClient = _storageResourceService.GetQueueClient(queueKey);

            var json = message.ToJson();
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            try
            {
                await queueClient.SendMessageAsync(base64, token).ConfigureAwait(true);
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == QueueErrorCodeStrings.QueueNotFound)
                {
                    var metadata = new Dictionary<string, string>
                    {
                        { "QueueKey", queueKey.ToString() },
                        { "ReliableQueueIdentifier", queueKey.Identifier }
                    };
                    await queueClient.CreateIfNotExistsAsync(metadata, token).ConfigureAwait(true);
                    await queueClient.SendMessageAsync(base64, token).ConfigureAwait(true);
                }
                else
                {
                    throw;
                }
            }

            return message;
        }
    }
}
