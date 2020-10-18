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
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="IMessageStateService" />.
    /// </summary>
    internal interface IMessageStateService
    {
        /// <summary>
        /// The AddNewMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to record.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan?"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken?"/>.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        [NotNull]
        public Task<Message> AddNewMessageAsync([NotNull] QueueKey queueKey, [NotNull] Message message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// The GetQueuedMessagesInTopic.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue from which to return messages.</param>
        /// <param name="topic">The topic from which to take messages.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan?"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken?"/>.</param>
        /// <returns>The <see cref="IEnumerable{Message}"/>.</returns>
        [NotNull]
        [ItemNotNull]
        public IEnumerable<Message> GetQueuedMessagesInTopic([NotNull] QueueKey queueKey, [NotNull] Topic topic, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// The ProcessMessage.
        /// </summary>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="queueKey">The key identifying the reliable queue to which the messages belong.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool ProcessMessage([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] QueueKey queueKey,
            [NotNull] Message message);

        /// <summary>
        /// Changes the state of the message specified to queued asynchronously and returns the new state of the message that was updated, with updated
        ///     properties.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to queue.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan?"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken?"/>.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        [NotNull]
        public Task<Message> QueueMessageAsync([NotNull] QueueKey queueKey, [NotNull] Message message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);
    }
}
