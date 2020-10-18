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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.Extensions.Logging;

    using OpenCollar.Azure.ReliableQueue.Configuration;
    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Extensions;
    using OpenCollar.Extensions.Validation;

    /// <summary>A service used to coordinate the sending and receiving of messages using Azure Storage Queues.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.IReliableQueueService"/>
    internal sealed class ReliableQueueService : Disposable, IReliableQueueServiceInternal
    {
        /// <summary>A lookup of the proxies created, keyed on the reliable queue key.</summary>
        [NotNull]
        private static readonly ConcurrentDictionary<QueueKey, ReliableQueue> _proxies = new ConcurrentDictionary<QueueKey, ReliableQueue>();

        /// <summary>The service used to access the configuration for the queues used to send and receive messages.</summary>
        [NotNull]
        private readonly IReliableQueueConfigurationService _configuration;

        /// <summary>A dictionary of the listeners owned by this service.</summary>
        [NotNull]
        private readonly Dictionary<QueueKey, ReliableQueueListener> _listeners = new Dictionary<QueueKey, ReliableQueueListener>();

        /// <summary>The logger used to record information about usage and activities.</summary>
        [NotNull]
        private readonly ILogger _logger;

        /// <summary>The service used to receive messages.</summary>
        [NotNull]
        private readonly IReliableQueueReceiverService _receive;

        /// <summary>The service used to send messages.</summary>
        [NotNull]
        private readonly IReliableQueueSenderService _send;

        /// <summary>The service that is used to manage the state of messages in the queue.</summary>
        [NotNull]
        private readonly IMessageStateService _state;

        /// <summary>The service used to store and retrieve the body of messages.</summary>
        [NotNull]
        private readonly IMessageStorageService _storage;

        /// <summary>The subscriptions for which callbacks will be made when messages arrive.</summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, SubscriptionToken> _subscriptions = new ConcurrentDictionary<Guid, SubscriptionToken>();

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueService"/> class.</summary>
        /// <param name="logger">The logger used to record information about usage and activities.</param>
        /// <param name="configuration">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="storage">The service used to store and retrieve the body of messages.</param>
        /// <param name="state">The service that is used to manage the state of messages in the queue.</param>
        /// <param name="send">The service used to send messages.</param>
        /// <param name="receive">The service used to receive messages.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="configuration"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="storage"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="state"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="send"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="receive"/> was <see langword="null"/>.</exception>
        public ReliableQueueService([NotNull] ILogger<IReliableQueueService> logger, [NotNull] IReliableQueueConfigurationService configuration,
            [NotNull] IMessageStorageService storage, [NotNull] IMessageStateService state, [NotNull] IReliableQueueSenderService send,
            [NotNull] IReliableQueueReceiverService receive)
        {
            logger.Validate(nameof(logger), ObjectIs.NotNull);
            configuration.Validate(nameof(configuration), ObjectIs.NotNull);
            storage.Validate(nameof(storage), ObjectIs.NotNull);
            state.Validate(nameof(state), ObjectIs.NotNull);
            send.Validate(nameof(send), ObjectIs.NotNull);
            receive.Validate(nameof(receive), ObjectIs.NotNull);

            _logger = logger;
            _configuration = configuration;
            _storage = storage;
            _state = state;
            _send = send;
            _receive = receive;

            foreach (var ReliableQueue in configuration.ReliableQueues)
            {
                if (!ReliableQueue.Value.CreateListener)
                {
                    continue;
                }

                if (!IReliableQueueConfiguration.ModeReceive.Equals(ReliableQueue.Value.Mode, StringComparison.OrdinalIgnoreCase)
                && !IReliableQueueConfiguration.ModeBoth.Equals(ReliableQueue.Value.Mode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var listener = new ReliableQueueListener(ReliableQueue.Key, configuration, this);

                _listeners.Add(ReliableQueue.Key, listener);
            }
        }

        /// <summary>Called when a message is ready to be processed by the consumer.</summary>
        /// <param name="queueKey">The key identifying the reliable queue from which the message originates.</param>
        /// <param name="message">The message of which to notify the consumer.</param>
        /// <returns>
        ///     <see langword="true"/> if the message was successfully processed and should be removed from the queue; otherwise, <see langword="false"/> to
        ///     indicate that the message should be re-queued and tried again.
        /// </returns>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        public bool OnProcessMessage(QueueKey queueKey, Message message)
        {
            if (!CanReceive(queueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be send-only.");
            }

            return OnReceivedMessage(queueKey, message);
        }

        /// <summary>Gets the <see cref="IReliableQueue"/> with the specified reliable queue key.</summary>
        /// <value>The <see cref="IReliableQueue"/> object for the queue specified.</value>
        /// <param name="queueKey">The key identifying the reliable queue for which an <see cref="IReliableQueue"/> object is required.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        public IReliableQueue this[QueueKey queueKey]
        {
            get
            {
                queueKey.Validate(nameof(queueKey), ObjectIs.NotNull);

                return _proxies.GetOrAdd(queueKey, k => new ReliableQueue(this, queueKey));
            }
        }

        /// <summary>Determines whether the queue specified can be used to receive message</summary>
        /// <param name="queueKey">The key identifying the reliable queue to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the queue specified by <paramref name="queueKey"/> can be used to receive messages; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public bool CanReceive(QueueKey queueKey)
        {
            var configuration = _configuration[queueKey];

            var mode = configuration.Mode;

            if (string.IsNullOrWhiteSpace(mode))
            {
                return false;
            }

            return IReliableQueueConfiguration.ModeReceive.Equals(mode, StringComparison.OrdinalIgnoreCase) || IReliableQueueConfiguration.ModeBoth.Equals(mode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Determines whether the queue specified can be used to send message</summary>
        /// <param name="queueKey">The key identifying the reliable queue to check.</param>
        /// <returns>
        ///     <see langword="true"/> if the queue specified by <paramref name="queueKey"/> can be used to send messages; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public bool CanSend(QueueKey queueKey)
        {
            var configuration = _configuration[queueKey];

            var mode = configuration.Mode;

            if (string.IsNullOrWhiteSpace(mode))
            {
                return false;
            }

            return IReliableQueueConfiguration.ModeSend.Equals(mode, StringComparison.OrdinalIgnoreCase) || IReliableQueueConfiguration.ModeBoth.Equals(mode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Determines whether any subscriptions exist for the reliable queue specified.</summary>
        /// <param name="queueKey">The ket identifying the reliable queue for which to check for keys.</param>
        /// <returns><see langword="true"/> if the specified reliable queue key is subscribed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        public bool IsSubscribed(QueueKey queueKey)
        {
            queueKey.Validate(nameof(queueKey), ObjectIs.NotNull);

            if (!CanReceive(queueKey))
            {
                return false;
            }

            return _subscriptions.Values.Any(s => s.QueueKey == queueKey);
        }

        /// <summary>Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be processed.</summary>
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
        public async Task OnReceivedAsync(string base64, TimeSpan? timeout = null, CancellationToken? cancellationToken = null) => await _receive.OnReceivedAsync(base64, this).ConfigureAwait(true);

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
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
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public async Task SendMessageAsync(QueueKey queueKey, string? body, Topic? topic = null, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if (!CanSend(queueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be receive-only.");
            }

            await SendMessageAsync(queueKey, body, (m, b) =>
                {
                    if (b is null || b.Length <= 0)
                    {
                        m.Size = null;
                        m.BodyIsNull = true;
                    }
                    else
                    {
                        m.Size = sizeof(char) * b.Length;
                        m.BodyIsNull = false;
                    }
                },
                (QueueKey1, body1, timeout1, cancellationToken1, message) =>
                    RecordBodyAsync(QueueKey1, message, body1, timeout1, cancellationToken1),
                topic, timeout, cancellationToken).ConfigureAwait(true);
        }

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
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
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be receive-only.</exception>
        public async Task SendMessageAsync(QueueKey queueKey, byte[]? body, Topic? topic = null, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if (!CanSend(queueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be receive-only.");
            }

            await SendMessageAsync(queueKey, body, (m, b) =>
                {
                    if (b is null || b.Length <= 0)
                    {
                        m.Size = null;
                        m.BodyIsNull = true;
                    }
                    else
                    {
                        m.Size = b.Length;
                        m.BodyIsNull = false;
                    }
                },
                (QueueKey1, body1, timeout1, cancellationToken1, message) =>
                    RecordBodyAsync(QueueKey1, message, body1, timeout1, cancellationToken1),
                topic, timeout, cancellationToken).ConfigureAwait(true);
        }

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
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
        public async Task SendMessageAsync(QueueKey queueKey, Stream? body, Topic? topic = null, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if (!CanSend(queueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be receive-only.");
            }

            await SendMessageAsync(queueKey, body, (m, b) =>
                {
                    if (b is null || b.Length <= 0)
                    {
                        m.Size = null;
                        m.BodyIsNull = true;
                    }
                    else
                    {
                        m.Size = b.Length;
                        m.BodyIsNull = false;
                    }
                },
                (QueueKey1, body1, timeout1, cancellationToken1, message) =>
                    RecordBodyAsync(QueueKey1, message, body1, timeout1, cancellationToken1),
                topic, timeout, cancellationToken).ConfigureAwait(true);
        }

        /// <summary>Subscribes the specified reliable queue key.</summary>
        /// <param name="queueKey">The key identifying the reliable queue to which to subscribe.</param>
        /// <param name="callbackHandler">The event handler to call when a message arrives.</param>
        /// <returns>A token that can be used to unsubscribe, either by calling <see cref="IReliableQueueService.Unsubscribe"/> or by disposing.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="callbackHandler"/> was <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        public SubscriptionToken Subscribe(QueueKey queueKey, EventHandler<ReceivedMessageEventArgs> callbackHandler)
        {
            if (!CanReceive(queueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be send-only.");
            }

            var token = new SubscriptionToken(queueKey, callbackHandler, this);

            var isFirst = _subscriptions.Values.All(s => s.QueueKey != queueKey);

            _subscriptions.TryAdd(token.Id, token);

            if (isFirst)
            {
                _logger.LogInformation($"Checking for existing messages on reliable queue \"{queueKey}\".");
                _receive.CheckForWaitingMessages(this, queueKey);
            }

            return token;
        }

        /// <summary>Unsubscribes from the reliable queue specified by the token given.</summary>
        /// <param name="token">The token returned by <see cref="IReliableQueueService.Unsubscribe"/> when the subscription was created.</param>
        /// <returns>
        ///     <see langword="true"/> if the subscription was found and unsubscribed; otherwise, <see langword="false"/> if there was no subscription to
        ///     remove.
        /// </returns>
        /// <exception cref="InvalidOperationException">Message queue is configured to be send-only.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="token"/> was <see langword="null"/>.</exception>
        public bool Unsubscribe(SubscriptionToken token)
        {
            if (!CanReceive(token.QueueKey))
            {
                throw new InvalidOperationException("Message queue is configured to be send-only.");
            }

            return _subscriptions.TryRemove(token.Id, out var _);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///     <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var listener in _listeners.Values)
                {
                    listener.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>Called when [received message].</summary>
        /// <param name="queueKey">The key identifying the queue from which the message was delivered.</param>
        /// <param name="message">The message that has been received.</param>
        /// <returns><see langword="true"/> if a consumer has processed the message; otherwise, <see langword="false"/>.</returns>
        private bool OnReceivedMessage([NotNull] QueueKey queueKey, [NotNull] Message message)
        {
            var callbacks = _subscriptions.Values.Where(s => s.QueueKey == queueKey).Select(s => s.EventHandler).ToArray();

            if (callbacks.Length <= 0)
            {
                return false;
            }

            var args = new ReceivedMessageEventArgs(_storage, queueKey, message);

            foreach (var callback in callbacks)
            {
                callback.DynamicInvoke(this, args);

                if (args.Handled)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Writes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to write the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be written.</param>
        /// <param name="body">
        ///     A string containing the BLOB to write into the BLOB storage.  Can be <see langword="null"/> or zero-length if the message has not
        ///     body.
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to write the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to write the message body to the BLOB storage.</param>
        /// <returns>A task that writes the message body to the BLOB storage.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="message"/> was <see langword="null"/>.</exception>
        private async Task RecordBodyAsync([NotNull] QueueKey queueKey, [NotNull] Message message, [CanBeNull] byte[]? body,
            TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (!(body is null) && body.Length > 0)
            {
                await using var stream = new MemoryStream(body);

                await _storage.WriteMessageAsync(queueKey, message, stream, timeout, cancellationToken).ConfigureAwait(true);
            }
        }

        /// <summary>Writes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to write the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be written.</param>
        /// <param name="body">
        ///     A string containing the BLOB to write into the BLOB storage.  Can be <see langword="null"/> or zero-length if the message has not
        ///     body.
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to write the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to write the message body to the BLOB storage.</param>
        /// <returns>A task that writes the message body to the BLOB storage.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="message"/> was <see langword="null"/>.</exception>
        private async Task RecordBodyAsync([NotNull] QueueKey queueKey, [NotNull] Message message, [CanBeNull] string? body,
            TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                await using var stream = new MemoryStream();
                await using var writer = new StreamWriter(stream, Encoding.UTF8, 1024 * 1024, true);
                await writer.WriteAsync(body).ConfigureAwait(true);
                await writer.FlushAsync().ConfigureAwait(true);

                stream.Seek(0, SeekOrigin.Begin);

                await _storage.WriteMessageAsync(queueKey, message, stream, timeout, cancellationToken).ConfigureAwait(true);
            }
        }

        /// <summary>Writes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to write the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be written.</param>
        /// <param name="body">
        ///     A stream containing the BLOB to write into the BLOB storage.  Can be <see langword="null"/> or zero-length if the message has not
        ///     body.
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to write the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to write the message body to the BLOB storage.</param>
        /// <returns>A task that writes the message body to the BLOB storage.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="message"/> was <see langword="null"/>.</exception>
        private async Task RecordBodyAsync([NotNull] QueueKey queueKey, [NotNull] Message message, [CanBeNull] Stream? body,
            TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (!(body is null))
            {
                await _storage.WriteMessageAsync(queueKey, message, body, timeout, cancellationToken).ConfigureAwait(true);
            }
        }

        /// <summary>Sends the message body provided on the reliable queue specified, optionally on the topic given.</summary>
        /// <typeparam name="TBody">The type of the body.</typeparam>
        /// <param name="queueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="body">The body of the message to send.  This can be <see langword="null"/> or contain any content.</param>
        /// <param name="initializeMessageFieldsFromBody">Initialize the message fields from the supplied body.</param>
        /// <param name="recordBody">Returns a task that the records body.</param>
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
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        private async Task SendMessageAsync<TBody>(QueueKey queueKey, TBody body,
            [NotNull] Action<Message, TBody> initializeMessageFieldsFromBody,
            [NotNull] Func<QueueKey, TBody, TimeSpan?, CancellationToken, Message, Task> recordBody, Topic? topic = null, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            var configuration = _configuration[queueKey];

            // We create the message as early as possible to capture the timestamps and sequence ID.
            var message = Message.CreateNew(queueKey, configuration, topic);

            var timeoutPeriod = timeout ?? TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var token = cancellationToken ?? CancellationToken.None;

            initializeMessageFieldsFromBody(message, body);

            // We'll need the complete message object returned when the record has been created later.
            var addMessage = _state.AddNewMessageAsync(queueKey, message, timeoutPeriod, cancellationToken);

            // The "new" message, and the blob can be created in parallel, there is no risk of anything attempting to use either yet.
            var tasks = new List<Task>
            {
                addMessage,
                recordBody(queueKey, body, timeoutPeriod, token, message)
            };

            // Once the new message and blob have been stored ...
            Task.WaitAll(tasks.ToArray());

            // ... we can change the record state to "queued" ...
            message = await _state.QueueMessageAsync(queueKey, addMessage.Result, timeoutPeriod, cancellationToken).ConfigureAwait(true);

            // ... and send a message to listeners (do this async, we don't need to wait).
#pragma warning disable 4014
            _send.SendMessageAsync(queueKey, message, timeoutPeriod, cancellationToken);
#pragma warning restore 4014
        }
    }
}
