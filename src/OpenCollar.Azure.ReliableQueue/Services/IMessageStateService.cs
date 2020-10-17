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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The public interface of a service that is used to manage the state of messages in the queue.</summary>
    internal interface IMessageStateService
    {
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
        [NotNull]
        public Task<MessageRecord> AddNewMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] MessageRecord message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);

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
        [NotNull]
        [ItemNotNull]
        public IEnumerable<MessageRecord> GetQueuedMessagesInTopic([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] Topic topic, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);

        /// <summary>Processes the message given, raising events on the queue service specified in <paramref name="reliableQueueService"/>,</summary>
        /// <param name="reliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="reliableQueueKey">The key identifying the reliable queue to which the messages belong.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>
        ///     <see langword="true"/> if the message was successfully processed; otherwise, <see langword="false"/> to return the queue and try again
        ///     later.
        /// </returns>
        public bool ProcessMessage([NotNull] IReliableQueueServiceInternal reliableQueueService, [NotNull] ReliableQueueKey reliableQueueKey,
            [NotNull] MessageRecord message);

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
        [NotNull]
        public Task<MessageRecord> QueueMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] MessageRecord message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);
    }
}