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
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="IReliableQueueReceiverService" />.
    /// </summary>
    internal interface IReliableQueueReceiverService
    {
        /// <summary>
        /// Checks to see if there are any unprocessed messages waiting on the queue specified, and if there are, starts processing them,
        ///     asynchronously.
        /// </summary>
        /// <param name="reliableQueueService">The reliable queue service that will be responsible for notifying consumers of messages to be processed.</param>
        /// <param name="queueKey">The key identifying the reliable queue to process.</param>
        public void CheckForWaitingMessages([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] QueueKey queueKey);

        /// <summary>
        /// The OnReceivedAsync.
        /// </summary>
        /// <param name="base64">The Base-64 encoded JSON representation of the serialized message.</param>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan?"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken?"/>.</param>
        /// <returns>A task that processes the message supplied.</returns>
        public Task OnReceivedAsync([CanBeNull] string base64, [NotNull] IReliableQueueServiceInternal reliableQueueService, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);
    }
}
