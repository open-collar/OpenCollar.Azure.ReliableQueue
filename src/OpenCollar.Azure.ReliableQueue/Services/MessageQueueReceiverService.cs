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
    using System.Collections.Immutable;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.Extensions.Logging;

    using OpenCollar.Azure.ReliableQueue.Configuration;
    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Extensions;
    using OpenCollar.Extensions.Validation;

    /// <summary>
    /// Defines the <see cref="ReliableQueueReceiverService" />.
    /// </summary>
    internal sealed class ReliableQueueReceiverService : Disposable, IReliableQueueReceiverService
    {
        /// <summary>
        /// Defines the _activeTopicProcessors.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Topic, Topic> _activeTopicProcessors = new ConcurrentDictionary<Topic, Topic>();

        /// <summary>
        /// Defines the _logger.
        /// </summary>
        [NotNull]
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the _messageStateService.
        /// </summary>
        [NotNull]
        private readonly IMessageStateService _messageStateService;

        /// <summary>
        /// Defines the _messageTopicService.
        /// </summary>
        [NotNull]
        private readonly IMessageTopicService _messageTopicService;

        /// <summary>
        /// Defines the _reliableQueueConfigurationService.
        /// </summary>
        [NotNull]
        private readonly IReliableQueueConfigurationService _reliableQueueConfigurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueReceiverService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to record information about usage and activities.</param>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="messageTopicService">The service used to manage topic affinity and the ordering of the messages belonging to the same topic.</param>
        /// <param name="messageStateService">The service that is used to manage the state of messages in the queue.</param>
        /// <param name="storageResourceService">The service used to create and manage clients for the various Azure Storage resources used reliable queues.</param>
        public ReliableQueueReceiverService([NotNull] ILogger<IReliableQueueReceiverService> logger,
            [NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService, [NotNull] IMessageTopicService messageTopicService,
            [NotNull] IMessageStateService messageStateService, [NotNull] IStorageResourceService storageResourceService)
        {
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);
            messageTopicService.Validate(nameof(messageTopicService), ObjectIs.NotNull);
            messageStateService.Validate(nameof(messageStateService), ObjectIs.NotNull);
            logger.Validate(nameof(logger), ObjectIs.NotNull);
            storageResourceService.Validate(nameof(storageResourceService), ObjectIs.NotNull);

            _reliableQueueConfigurationService = ReliableQueueConfigurationService;
            _messageTopicService = messageTopicService;
            _messageStateService = messageStateService;
            _logger = logger;

            var tasks = new List<Task>();

            foreach (var queue in ReliableQueueConfigurationService.ReliableQueues)
            {
                var queueClient = storageResourceService.GetQueueClient(queue.Key);

                tasks.Add(queueClient.CreateIfNotExistsAsync());
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Checks to see if there are any unprocessed messages waiting on the queue specified, and if there are, starts processing them,
        ///     asynchronously.
        /// </summary>
        /// <param name="reliableQueueService">The reliable queue service that will be responsible for notifying consumers of messages to be processed.</param>
        /// <param name="queueKey">The key identifying the reliable queue to process.</param>
        public void CheckForWaitingMessages(IReliableQueueServiceInternal reliableQueueService, QueueKey queueKey)
        {
            var topics = _messageTopicService.GetLiveTopics(queueKey);

            foreach (var topic in topics)
            {
                ProcessTopic(reliableQueueService, queueKey, topic);
            }
        }

        /// <summary>
        /// The OnReceivedAsync.
        /// </summary>
        /// <param name="base64">The Base-64 encoded JSON representation of the serialized message.</param>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>A task that processes the message supplied.</returns>
        public async Task OnReceivedAsync(string base64, IReliableQueueServiceInternal reliableQueueService, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            if (IsDisposed)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(base64))
            {
                _logger.LogWarning("Message received with no content.");
                return;
            }

            // It is a bit unclear exactly when and how the Storage Queue converts strings to and from base-64.
            // We supply a base-64 string, but in the storage explorer it appears as a straight JSON strings; however,
            // is flagged as being base-64 encoded.  When the function app receives the string it is straight
            // JSON.  Who knows what format it is in when it comes from the event grid or from the listener.
            // Best to assume that it could be plain or encoded and convert as necessary.
            // In base-64 '{' = "e2" and '}' = "e3". The character '{' is not used in base-64 - so if the first
            // character of a string is '{' then it is not a base-64 string.  As we are always expected JSON it is
            // safe to assume that all un-encoded messages will start with an open-brace.
            var json = (base64[0] != '{') ? Encoding.UTF8.GetString(Convert.FromBase64String(base64)) : base64;

            var message = Message.FromJson(json);

            if (message is null)
            {
                _logger.LogWarning("Message received with no JSON content.");
                return;
            }

            try
            {
                // First, we'll pass it to the topic service to be filtered for affinity and queued
                if (await _messageTopicService.OnReceivedAsync(message, reliableQueueService, timeout, cancellationToken).ConfigureAwait(true))
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    // Next we must put this into the sliding window and allow another thread to process the raising of events.
                    QueueMessage(reliableQueueService, message);
                }
            }
            catch (MessageStateException)
            {
                // The message had already changed state.  This usually means that another consumer has already started processing the message.
                // We'll leave that for now, if something goes wrong we'll deal with that later,
            }
        }

        /// <summary>
        /// Process all the messages sent on a topic until there are no more added.  Allow messages to remain in the queue for a period of time, and reorder
        ///     according to sequence to allow out-of-sequence messages the opportunity to arrive and be processed in the correct order.
        /// </summary>
        /// <param name="state">The state object in which details of the context is supplied.</param>
        private void OnProcessTopic(object state)
        {
            // We'll use this to allow the thread to linger a whole "window" after the last message, allowing it to catch more messages.
            var overrun = TimeSpan.Zero;
            var iterationDuration = TimeSpan.FromMilliseconds(100);

            TopicQueueContext? context = null;
            try
            {
                context = (TopicQueueContext)state;
                var window = TimeSpan.FromSeconds(context.Configuration.SlidingWindowDurationSeconds);

                // Further messages could be added at any time (that really is the idea of using topic affinity and the sliding window).
                // We keep looking for messages until no more are added.
                ImmutableArray<Message> snapshot;
                do
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    // Wait for messages to arrive and fall within the window.
                    Thread.Sleep(iterationDuration);

                    if (IsDisposed)
                    {
                        return;
                    }

                    // Don't rely upon the messages received, instead fetch exactly what is currently in the database and use that/
                    snapshot = _messageStateService.GetQueuedMessagesInTopic(context.QueueKey, context.Topic).ToImmutableArray();

                    if (IsDisposed)
                    {
                        return;
                    }

                    // This is the oldest message that we'll consider raising an event for
                    var cutoff = DateTime.UtcNow.AddSeconds(-context.Configuration.SlidingWindowDurationSeconds);

                    foreach (var message in snapshot)
                    {
                        if (message.LastUpdated < cutoff)
                        {
                            _messageStateService.ProcessMessage(context.ReliableQueueService, context.QueueKey, message);
                        }

                        if (IsDisposed)
                        {
                            return;
                        }
                    }

                    if (snapshot.Length <= 0)
                    {
                        overrun += iterationDuration;
                    }
                    else
                    {
                        overrun = TimeSpan.Zero;
                    }
                }
                while (snapshot.Length > 0 || overrun <= window);

                if (!_activeTopicProcessors.TryRemove(context.Topic.Identifier, out var _))
                {
                    _logger.LogWarning($"Unable to remove active message processor: {context.Topic}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (context is null)
                {
                    _logger.LogError(ex, @"Error whilst processing messages in topic sliding window.");
                }
                else
                {
                    _logger.LogError(ex,
                        $@"Error whilst processing messages in topic sliding window.  Reliable Queue Key: {context.QueueKey}; Topic: ""{context.Topic}"".");
                }
            }
        }

        /// <summary>
        /// The ProcessTopic.
        /// </summary>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="queueKey">The key identifying the reliable queue to which the topic belongs.</param>
        /// <param name="topic">The key identifying the topic to process.</param>
        private void ProcessTopic([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] QueueKey queueKey, [NotNull] Topic topic)
        {
            // If we created a new topic queue then we'll need to start a thread to process that queue as well.
            if (_activeTopicProcessors.TryAdd(topic, topic))
            {
                try
                {
                    // Start a new thread that will be responsible for processing all the messages in that topic, in order.
                    var configuration = _reliableQueueConfigurationService[queueKey];

                    // Concurrency Risks:
                    // We need to be sure that only one thread is ever processing a topic, and that only one dictionary
                    // is ever in existence for a topic.
                    var topicThread = new Thread(OnProcessTopic)
                    {
                        Name = "Topic Processor: " + topic.Identifier,
                        IsBackground = false
                    };
                    topicThread.Start(new TopicQueueContext(reliableQueueService, queueKey, topic, configuration));
                }
                catch
                {
                    // Undo the flag we set to ensure we don't block another attempt.
                    _activeTopicProcessors.TryRemove(topic, out var _);

                    throw;
                }
            }
        }

        /// <summary>
        /// The QueueMessage.
        /// </summary>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="message">The message to queue.</param>
        private void QueueMessage([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] Message message)
        {
            if (IsDisposed)
            {
                return;
            }

            ProcessTopic(reliableQueueService, message.QueueKey, message.Topic);
        }

        /// <summary>
        /// Defines the <see cref="TopicQueueContext" />.
        /// </summary>
        private class TopicQueueContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TopicQueueContext"/> class.
            /// </summary>
            /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
            /// <param name="queueKey">The key identifying the reliable queue to which the messages belong.</param>
            /// <param name="topic">The topic to which the messages in the queue belong.</param>
            /// <param name="configuration">The configuration for the reliable queue.</param>
            public TopicQueueContext([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] QueueKey queueKey,
                [NotNull] Topic topic, [NotNull] IReliableQueueConfiguration configuration)
            {
                ReliableQueueService = reliableQueueService;
                QueueKey = queueKey;
                Topic = topic;
                Configuration = configuration;
            }

            /// <summary>
            /// Gets the Configuration.
            /// </summary>
            [NotNull]
            public IReliableQueueConfiguration Configuration { get; }

            /// <summary>
            /// Gets the QueueKey.
            /// </summary>
            [NotNull]
            public QueueKey QueueKey { get; }

            /// <summary>
            /// Gets the ReliableQueueService.
            /// </summary>
            [NotNull]
            public IReliableQueueServiceInternal ReliableQueueService { get; }

            /// <summary>
            /// Gets the Topic.
            /// </summary>
            [NotNull]
            public Topic Topic { get; }
        }
    }
}
