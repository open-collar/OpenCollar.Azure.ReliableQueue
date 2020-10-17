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

namespace OpenCollar.Azure.ReliableQueue
{
    /// <summary>The public interface of the object representing a reliable queue.</summary>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

    public interface IReliableQueue
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        /// <summary>Gets a value indicating whether the queue can be used to receive message.</summary>
        /// <value><see langword="true"/> if the queue can be used to receive messages; otherwise, <see langword="false"/>.</value>
        public bool CanReceive { get; }

        /// <summary>Gets a value indicating whether the queue can be used to send message.</summary>
        /// <value><see langword="true"/> if the queue can be used to send messages; otherwise, <see langword="false"/>.</value>
        public bool CanSend { get; }

        /// <summary>Gets the key identifying the reliable queue represented.</summary>
        /// <value>The identifying the reliable queue represented.</value>
        public ReliableQueueKey ReliableQueueKey { get; }

        /// <summary>Determines whether any subscriptions exist for the reliable queue.</summary>
        /// <returns><see langword="true"/> if the specified reliable queue key is subscribed; otherwise, <see langword="false"/>.</returns>
        public bool IsSubscribed();

        /// <summary>
        ///     Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be processed.  The notification can be intended
        ///     for any reliable queue.
        /// </summary>
        /// <param name="base64">The Base-64 encoded JSON representation of the serialized message.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to process the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to process the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="base64"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="base64"/> was zero-length or contains only white-space characters.</exception>
        /// <returns>A task that processes the message supplied.</returns>
        /// <example>
        /// An example of what a storage queue function might look like is below:
        /// <code lang="c#">
        ///    public sealed class StorageQueueMessageReceivedFunction
        ///    {
        ///        private readonly IReliableQueue _reliableQueue;
        ///
        ///        public StorageQueueMessageReceivedFunction(IReliableQueueService ReliableQueueService)
        ///        {
        ///            _reliableQueue = ReliableQueueService["TEST+1"];
        ///        }
        ///
        ///        [FunctionName("StorageQueueMessageReceivedFunction")]
        ///        public void Run([QueueTrigger("message-queue-test-1", Connection = "ReliableQueues:Queues:TEST+1:StorageConnectionString")] string myQueueItem, ILogger log)
        ///        {
        ///            _reliableQueue.OnReceivedAsync(myQueueItem);
        ///        }
        ///    }
        /// </code>
        /// Note that the connection string must be under the "Values" section in the appsettings.json file, as well as in the configuration node.
        /// Also, the ReliableQueueService must be initialized on startup to ensure the queue has been created; this can be done using a timer function.
        /// </example>
        public Task OnReceivedAsync([NotNull] string base64, TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Sends the message body provided on the reliable queue, optionally on the topic given.</summary>
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
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([CanBeNull] byte[]? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Sends the message body provided on the reliable queue, optionally on the topic given.</summary>
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
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([CanBeNull] Stream? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Sends the message body provided on the reliable queue, optionally on the topic given.</summary>
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
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public Task SendMessageAsync([CanBeNull] string? body, [CanBeNull] Topic? topic = null, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Subscribes the specified reliable queue key.</summary>
        /// <param name="callbackHandler">The event handler to call when a message arrives.</param>
        /// <returns>A token that can be used to unsubscribe, either by calling <see cref="Unsubscribe"/> or by disposing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="callbackHandler"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        public SubscriptionToken Subscribe([NotNull] EventHandler<ReceivedMessageEventArgs> callbackHandler);

        /// <summary>Unsubscribes from the reliable queue by the token given.</summary>
        /// <param name="token">The token returned by <see cref="Subscribe"/> when the subscription was created.</param>
        /// <returns>
        ///     <see langword="true"/> if the subscription was found and unsubscribed; otherwise, <see langword="false"/> if there was no subscription to
        ///     remove.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="token"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        public bool Unsubscribe([NotNull] SubscriptionToken token);
    }
}