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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Extensions;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>An in-memory listener for message arriving on a Storage Queue.</summary>
    internal sealed class ReliableQueueListener : Disposable
    {
        /// <summary>The timer used to regularly fire the keep-alive method.</summary>
        [NotNull]
        private readonly Timer _consumingTimer;

        /// <summary>The reliable queue service to which to send received messages.</summary>
        [NotNull]
        private readonly IReliableQueueService _reliableQueueService;

        /// <summary>The client with which to connect to the queue.</summary>
        [NotNull]
        private readonly QueueClient _queueClient;

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueReceiverService"/> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="ReliableQueueService">The reliable queue service to which to send received messages.</param>
        public ReliableQueueListener(ReliableQueueKey reliableQueueKey, [NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService,
            IReliableQueueService ReliableQueueService)
        {
            reliableQueueKey.Validate(nameof(reliableQueueKey), ObjectIs.NotNull);
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);
            ReliableQueueService.Validate(nameof(ReliableQueueService), ObjectIs.NotNull);

            _reliableQueueService = ReliableQueueService;

            var configuration = ReliableQueueConfigurationService[reliableQueueKey];

            _queueClient = new QueueClient(configuration.StorageConnectionString, Identifiers.GetReliableQueueName(reliableQueueKey));
            InitializeQueueClientAsync().Wait();

            // Finally, start the consumption timer to poll for messages.
            _consumingTimer = new Timer(OnConsume, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///     <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _consumingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _consumingTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>Initializes the queue client, creating the queue if necessary.</summary>
        [NotNull]
        [DebuggerHidden]
        [DebuggerStepThrough]
        private async Task InitializeQueueClientAsync()
        {
            await _queueClient.CreateIfNotExistsAsync();
        }

        /// <summary>Called when the keep-alive timer is fired and the keep-alive record is to be updated.</summary>
        /// <param name="ignored">The state of the timer (not used).</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        private void OnConsume([CanBeNull] object ignored)
        {
            if(IsDisposed)
            {
                return;
            }

            bool messageReceived;
            do
            {
                if(IsDisposed)
                {
                    return;
                }

                var messages = SafeReceiveMessagesAsync().Result;

                if(IsDisposed)
                {
                    return;
                }

                messageReceived = messages.Length > 0;

                if(messageReceived)
                {
                    foreach(var message in messages)
                    {
                        if(IsDisposed)
                        {
                            return;
                        }

                        _reliableQueueService.OnReceivedAsync(message.MessageText);

                        if(IsDisposed)
                        {
                            return;
                        }

                        _queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
                    }
                }
            }
            while(messageReceived);
        }

        /// <summary>Receive messages from the queue and deals with the queue being deleted.</summary>
        /// <returns>Any received messages.</returns>
        /// <remarks>
        ///     Uses debugger attributes to attempt to hide exceptions thrown in this method.  This is particularly important because exceptions are quite likely
        ///     to be thrown if a debugger is attached and breakpoints are hit elsewhere.
        /// </remarks>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [NotNull]
        private async Task<QueueMessage[]> SafeReceiveMessagesAsync()
        {
            try
            {
                return await _queueClient.ReceiveMessagesAsync(1);
            }
            catch(RequestFailedException)
            {
                await InitializeQueueClientAsync();

                return await _queueClient.ReceiveMessagesAsync(1);
            }
        }
    }
}