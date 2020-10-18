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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="IReliableQueueServiceInternal" />.
    /// </summary>
    internal interface IReliableQueueServiceInternal : IReliableQueueService
    {
        /// <summary>
        /// The CanReceive.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue to check.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanReceive([NotNull] QueueKey queueKey);

        /// <summary>
        /// The CanSend.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue to check.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CanSend([NotNull] QueueKey queueKey);

        /// <summary>
        /// The OnProcessMessage.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue from which the message originates.</param>
        /// <param name="message">The message of which to notify the consumer.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool OnProcessMessage([NotNull] QueueKey queueKey, [NotNull] Message message);

        /// <summary>
        /// The SendMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">The topic<see cref="Topic"/>.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>A task that performs the action specified.</returns>
        public Task SendMessageAsync([NotNull] QueueKey queueKey, [CanBeNull] byte[]? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        /// The SendMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">The topic<see cref="Topic"/>.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>A task that performs the action specified.</returns>
        public Task SendMessageAsync([NotNull] QueueKey queueKey, [CanBeNull] Stream? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        /// The SendMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">The topic<see cref="Topic"/>.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>A task that performs the action specified.</returns>
        public Task SendMessageAsync([NotNull] QueueKey queueKey, [CanBeNull] string? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        /// The Subscribe.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue to which to subscribe.</param>
        /// <param name="callbackHandler">The event handler to call when a message arrives.</param>
        /// <returns>A token that can be used to unsubscribe, either by calling <see cref="IReliableQueueService.Unsubscribe"/> or by disposing.</returns>
        [NotNull]
        public SubscriptionToken Subscribe([NotNull] QueueKey queueKey, [NotNull] EventHandler<ReceivedMessageEventArgs> callbackHandler);

        /// <summary>
        /// The IsSubscribed.
        /// </summary>
        /// <param name="queueKey">The ket identifying the reliable queue for which to check for keys.</param>
        /// <returns><see langword="true"/> if the specified reliable queue key is subscribed; otherwise, <see langword="false"/>.</returns>
        bool IsSubscribed([NotNull] QueueKey queueKey);
    }
}
