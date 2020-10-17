﻿/*
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
using Microsoft.Extensions.Logging;

using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>A service that is used to manage the state of messages in the queue.</summary>
    /// <seealso cref="IMessageStateService"/>
    internal sealed class MessageStateService : IMessageStateService
    {
        /// <summary>The logger used to record information about usage and activities.</summary>
        [NotNull]
        private readonly ILogger _logger;

        /// <summary>The service used to access the configuration for the queues used to send and receive messages.</summary>
        [NotNull]
        private readonly IReliableQueueConfigurationService _reliableQueueConfigurationService;

        /// <summary>The service used to send messages.</summary>
        [NotNull]
        private readonly IReliableQueueSenderService _send;

        /// <summary>The service used to store and retrieve the body of messages.</summary>
        [NotNull]
        private readonly IMessageStorageService _storage;

        /// <summary>The service used to create and manage clients for the various Azure Storage resources used reliable queues.</summary>
        [NotNull]
        private readonly IStorageResourceService _storageResourceService;

        /// <summary>Initializes a new instance of the <see cref="MessageStateService"/> class.</summary>
        /// <param name="logger">The logger used to record information about usage and activities.</param>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <param name="storage">The service used to store and retrieve the body of messages.</param>
        /// <param name="send">The service used to send messages.</param>
        /// <param name="storageResourceService">The service used to create and manage clients for the various Azure Storage resources used reliable queues.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="ReliableQueueConfigurationService"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="storage"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="send"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="storageResourceService"/> was <see langword="null"/>.</exception>
        public MessageStateService([NotNull] ILogger<IMessageStateService> logger, [NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService,
            [NotNull] IMessageStorageService storage, [NotNull] IReliableQueueSenderService send, [NotNull] IStorageResourceService storageResourceService)
        {
            logger.Validate(nameof(logger), ObjectIs.NotNull);
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);
            storage.Validate(nameof(storage), ObjectIs.NotNull);
            send.Validate(nameof(send), ObjectIs.NotNull);
            storageResourceService.Validate(nameof(storageResourceService), ObjectIs.NotNull);

            _logger = logger;
            _reliableQueueConfigurationService = ReliableQueueConfigurationService;
            _storage = storage;
            _send = send;
            _storageResourceService = storageResourceService;

            var tasks = new List<Task>();

            foreach(var queue in ReliableQueueConfigurationService.ReliableQueues)
            {
                var table = _storageResourceService.GetStateTable(queue.Key);

                tasks.Add(table.CreateIfNotExistsAsync());
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>Adds the new message asynchronously and returns the new state of the message that was created, with updated properties.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to record.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to send the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to send the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        public async Task<MessageRecord> AddNewMessageAsync(ReliableQueueKey reliableQueueKey, MessageRecord message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            // Create the table client on we'll use for maintaining

            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

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

            var table = _storageResourceService.GetStateTable(reliableQueueKey);

            message.Created = DateTime.UtcNow;
            message.LastUpdated = message.Created;
            message.MessageState = MessageState.New;
            var insertQuery = TableOperation.Insert(message.Serialize(context));
            TableResult insertResult;
            try
            {
                // Attempt to insert the new message record.
                insertResult = await table.ExecuteAsync(insertQuery, options, context, token);
                if(insertResult.HttpStatusCode < 200 || insertResult.HttpStatusCode >= 300)
                {
                    throw new ReliableQueueException(reliableQueueKey,
                        $"Unable to read message record for queue: {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}; Message ID {message.Id.ToString("D", CultureInfo.InvariantCulture)}.  Result: {insertResult.HttpStatusCode}.");
                }

                message = ((DynamicTableEntity)insertResult.Result).Deserialize<MessageRecord>(context);
            }
            catch(StorageException ex)
            {
                if(ex.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.TableNotFound)
                {
                    await table.CreateIfNotExistsAsync(options, context, token);

                    insertResult = await table.ExecuteAsync(insertQuery, options, context, token);
                    if(insertResult.HttpStatusCode < 200 || insertResult.HttpStatusCode >= 300)
                    {
                        throw new ReliableQueueException(reliableQueueKey,
                            $"Unable to read message record for queue: {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}; Message ID {message.Id.ToString("D", CultureInfo.InvariantCulture)}.  Result: {insertResult.HttpStatusCode}.");
                    }

                    message = ((DynamicTableEntity)insertResult.Result).Deserialize<MessageRecord>(context);
                }
                else
                {
                    throw;
                }
            }

            return message;
        }

        /// <summary>
        ///     Changes the state of the message specified to queued asynchronously and returns the new state of the message that was updated, with updated
        ///     properties.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to queue.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to send the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to send the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        public async Task<MessageRecord> QueueMessageAsync(ReliableQueueKey reliableQueueKey, MessageRecord message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
        {
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

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

            var table = _storageResourceService.GetStateTable(reliableQueueKey);

            message.LastUpdated = DateTime.UtcNow;
            message.MessageState = MessageState.Queued;

            var merge = new DynamicTableEntity(message.Topic.Identifier, message.Id.ToString("D", CultureInfo.InvariantCulture));
            merge.Properties.Add(nameof(MessageRecord.LastUpdated), new EntityProperty(message.LastUpdated));
            merge.Properties.Add(nameof(MessageRecord.MessageState), new EntityProperty(message.MessageState.ToString()));
            merge.ETag = message.ETag;
            var insertQuery = TableOperation.Merge(merge);
            TableResult mergeResult;
            try
            {
                // Attempt to insert the new message record.
                mergeResult = await table.ExecuteAsync(insertQuery, options, context, token);
                if(mergeResult.HttpStatusCode < 200 || mergeResult.HttpStatusCode >= 300)
                {
                    throw new ReliableQueueException(reliableQueueKey,
                        $"Unable to read message record for queue: {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}; Message ID {message.Id.ToString("D", CultureInfo.InvariantCulture)}.  Result: {mergeResult.HttpStatusCode}.");
                }

                var dynamicEntity = (DynamicTableEntity)mergeResult.Result;
                message.Timestamp = dynamicEntity.Timestamp;
                message.ETag = dynamicEntity.ETag;
            }
            catch(StorageException ex)
            {
                if(ex.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.TableNotFound)
                {
                    await table.CreateIfNotExistsAsync(options, context, token);

                    mergeResult = await table.ExecuteAsync(insertQuery, options, context, token);
                    if(mergeResult.HttpStatusCode < 200 || mergeResult.HttpStatusCode >= 300)
                    {
                        throw new ReliableQueueException(reliableQueueKey,
                            $"Unable to read message record for queue: {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}; Message ID {message.Id.ToString("D", CultureInfo.InvariantCulture)}.  Result: {mergeResult.HttpStatusCode}.");
                    }

                    var dynamicEntity = (DynamicTableEntity)mergeResult.Result;
                    message.Timestamp = dynamicEntity.Timestamp;
                    message.ETag = dynamicEntity.ETag;
                }
                else
                {
                    throw;
                }
            }

            return message;
        }

        /// <summary>Gets all messages that are in a <see cref="MessageState.Queued"/> state for the topic specified, in order of their sequence number.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue from which to return messages.</param>
        /// <param name="topic">The topic from which to take messages.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to read messages before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to read messages.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>
        ///     An sequence of all the messages that are in a <see cref="MessageState.Queued"/> state for the topic specified, in order of their sequence
        ///     number.
        /// </returns>
        public IEnumerable<MessageRecord> GetQueuedMessagesInTopic(ReliableQueueKey reliableQueueKey, Topic topic, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null)
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

            var topicMessageQuery = new TableQuery().Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(MessageRecord.Topic), QueryComparisons.Equal, topic), TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(MessageRecord.MessageState), QueryComparisons.Equal, MessageState.Queued.ToString())));

            IEnumerable<DynamicTableEntity> topicMessageResult;
            try
            {
                topicMessageResult = table.ExecuteQuery(topicMessageQuery, options, context);
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
                            $@"Unable to fetch messages for topic {topic} from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex2.Message}"".  See inner exception for details.",
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
                return Enumerable.Empty<MessageRecord>();
            }

            return topicMessageResult.Select(m => m.Deserialize<MessageRecord>(context)).OrderBy(m => m);
        }

        /// <summary>Processes the message given, raising events on the queue service specified in <paramref name="reliableQueueService"/>,</summary>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to which the messages belong.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>
        ///     <see langword="true"/> if the message was successfully processed; otherwise, <see langword="false"/> to return the queue and try again
        ///     later.
        /// </returns>
        public bool ProcessMessage(IReliableQueueServiceInternal reliableQueueService, ReliableQueueKey reliableQueueKey, MessageRecord message)
        {
            // If there is no-one listening don't increment the attempts count and just skip.
            if(!reliableQueueService.IsSubscribed(reliableQueueKey))
            {
                return false;
            }

            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            var timeoutPeriod = TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds);
            var token = CancellationToken.None;
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

            var table = _storageResourceService.GetStateTable(reliableQueueKey);

            var currentMessage = GetCurrentMessageState(reliableQueueKey, message, table, options, context, table.Name);

            switch(currentMessage.MessageState)
            {
                case MessageState.Queued:
                    break;

                case MessageState.Processing:
                    // Someone else has got there first - just leave it
                    // TODO: Add logic to timeout processing that has stalled.
                    return false;

                default:
                    throw new MessageStateException(reliableQueueKey, message.Id, MessageState.Queued, message.MessageState);
            }

            currentMessage.Owner = Identity.Current;
            currentMessage.LastUpdated = DateTime.UtcNow;
            currentMessage.Attempts = currentMessage.Attempts + 1;

            currentMessage.MessageState = currentMessage.Attempts > currentMessage.MaxAttempts ? MessageState.Failed : MessageState.Processing;

            var updateQuery = TableOperation.Replace(currentMessage.Serialize(context));
            try
            {
                table.Execute(updateQuery, options, context);
            }
            catch(Exception ex)
            {
                throw new MessageException(reliableQueueKey, message.Id,
                    $@"Unable to update message {MessageException.GetMessageId(message.Id)} from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex.Message}"".  See inner exception for details.",
                    ex);
            }

            bool completed;
            if(currentMessage.MessageState == MessageState.Processing)
            {
                try
                {
                    // Now process the message
                    completed = reliableQueueService.OnProcessMessage(reliableQueueKey, message);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch(Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Then remove it from the queue or push it back to Queued
                    completed = false;
                    _logger.LogError(ex, $"Consumer threw exception whilst processing message: {message.Id} on queue {reliableQueueKey}.");
                }
            }
            else
            {
                completed = false;
            }

            if(completed)
            {
                currentMessage = GetCurrentMessageState(reliableQueueKey, message, table, options, context, table.Name);

                if(currentMessage.MessageState != MessageState.Processing)
                {
                    throw new MessageStateException(reliableQueueKey, message.Id, MessageState.Processing, message.MessageState);
                }

                var deleteQuery = TableOperation.Delete(currentMessage);
                try
                {
                    table.Execute(deleteQuery, options, context);
                    _storage.DeleteMessageAsync(reliableQueueKey, message, timeoutPeriod).Wait((int)timeoutPeriod.TotalMilliseconds, token);
                }
                catch(Exception ex)
                {
                    throw new MessageException(reliableQueueKey, message.Id,
                        $@"Unable to delete message {MessageException.GetMessageId(message.Id)} from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex.Message}"".  See inner exception for details.",
                        ex);
                }
            }
            else
            {
                currentMessage = GetCurrentMessageState(reliableQueueKey, message, table, options, context, table.Name);

                if(currentMessage.MessageState != MessageState.Processing)
                {
                    throw new MessageStateException(reliableQueueKey, message.Id, MessageState.Processing, message.MessageState);
                }

                currentMessage.MessageState = MessageState.Queued;
                currentMessage.Owner = Identity.Current;
                currentMessage.LastUpdated = DateTime.UtcNow;

                updateQuery = TableOperation.Replace(currentMessage.Serialize(context));
                try
                {
                    // Push the message back into the queued state.
                    table.Execute(updateQuery, options, context);
                }
                catch(StorageException ex)
                {
                    if(ex.RequestInformation.HttpStatusCode != 404)
                    {
                        throw new MessageException(reliableQueueKey, message.Id,
                            $@"Unable to update message {MessageException.GetMessageId(message.Id)} from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex.Message}"".  See inner exception for details.",
                            ex);
                    }

                    // We can let missing tables/records go - same effect.
                }
                catch(Exception ex)
                {
                    throw new MessageException(reliableQueueKey, message.Id,
                        $@"Unable to update message {MessageException.GetMessageId(message.Id)} from table ""{table.Name}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex.Message}"".  See inner exception for details.",
                        ex);
                }

                // And finally, post the message back onto the queue.
                _send.SendMessageAsync(reliableQueueKey, message, timeoutPeriod, token).Wait((int)timeoutPeriod.TotalMilliseconds, token);
            }

            return completed;
        }

        /// <summary>Gets the current state of a message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to which the messages belong.</param>
        /// <param name="message">The message to refresh.</param>
        /// <param name="table">The table from which to read the information.</param>
        /// <param name="options">The table request options.</param>
        /// <param name="context">The operation context.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The current version of the message.</returns>
        private static MessageRecord GetCurrentMessageState(ReliableQueueKey reliableQueueKey, MessageRecord message, CloudTable table,
            TableRequestOptions options, OperationContext context, string tableName)
        {
            var currentMessageQuery = TableOperation.Retrieve(message.PartitionKey, message.RowKey);

            TableResult currentMessageResult;

            try
            {
                try
                {
                    currentMessageResult = table.Execute(currentMessageQuery, options, context);
                }
                catch(StorageException ex)
                {
                    if(ex.RequestInformation.ExtendedErrorInformation.ErrorCode == TableErrorCodeStrings.TableNotFound)
                    {
                        table.CreateIfNotExists(options, context);
                        currentMessageResult = table.Execute(currentMessageQuery, options, context);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new MessageException(reliableQueueKey, message.Id,
                    $@"Unable to fetch message {MessageException.GetMessageId(message.Id)} from table ""{tableName}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.  Reason: ""{ex.Message}"".  See inner exception for details.",
                    ex);
            }

            if(currentMessageResult is null || currentMessageResult.HttpStatusCode != 200)
            {
                throw new MessageException(reliableQueueKey, message.Id,
                    $@"Unable to find existing message {MessageException.GetMessageId(message.Id)} in table ""{tableName}"" on queue {ReliableQueueException.GetReliableQueueKey(reliableQueueKey)}.");
            }

            var currentMessage = ((DynamicTableEntity)currentMessageResult.Result).Deserialize<MessageRecord>(context);

            return currentMessage;
        }
    }
}