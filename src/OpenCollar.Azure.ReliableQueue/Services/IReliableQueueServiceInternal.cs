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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The internal interface of the service used to coordinate the sending and receiving of messages using Azure Storage Queues.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.IReliableQueueService"/>
    internal interface IReliableQueueServiceInternal : IReliableQueueService
    {
        /// <summary>Determines whether the queue specified can be used to receive message</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the queue specified by <paramref name="reliableQueueKey"/> can be used to receive messages; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public bool CanReceive([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Determines whether the queue specified can be used to send message</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the queue specified by <paramref name="reliableQueueKey"/> can be used to send messages; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public bool CanSend([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Determines whether any subscriptions exist for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The ket identifying the reliable queue for which to check for keys.</param>
        /// <returns><see langword="true"/> if the specified reliable queue key is subscribed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        bool IsSubscribed([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Called when a message is ready to be processed by the consumer.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue from which the message originates.</param>
        /// <param name="message">The message of which to notify the consumer.</param>
        /// <returns>
        ///     <see langword="true"/> if the message was successfully processed and should be removed from the queue; otherwise, <see langword="false"/> to
        ///     indicate that the message should be re-queued and tried again.
        /// </returns>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        public bool OnProcessMessage([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] Message message);

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to send the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to send the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>A task that performs the action specified.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [CanBeNull] string? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to send the message before failing with an error.   Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to send the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>A task that performs the action specified.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [CanBeNull] byte[]? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to send the message before failing with an error.   Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to send the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>A task that performs the action specified.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [CanBeNull] Stream? body, [CanBeNull] Topic? topic = null,
            TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Subscribes the specified reliable queue key.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to which to subscribe.</param>
        /// <param name="callbackHandler">The event handler to call when a message arrives.</param>
        /// <returns>A token that can be used to unsubscribe, either by calling <see cref="IReliableQueueService.Unsubscribe"/> or by disposing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="callbackHandler"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        [NotNull]
        public SubscriptionToken Subscribe([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] EventHandler<ReceivedMessageEventArgs> callbackHandler);
    }
}