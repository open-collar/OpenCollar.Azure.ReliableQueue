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
    /// <summary>The public interface of the service used to manage topic affinity and the ordering of the messages belonging to the same topic.</summary>
    internal interface IMessageTopicService
    {
        /// <summary>Gets the live topics from the reliable queue.</summary>
        /// <param name="reliableQueueKey">The key identifying the queue from which to read the topics.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to retrieve topics before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to retrieve topics.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>A sequence containing the topics found to be active.</returns>
        [NotNull]
        public IEnumerable<Topic> GetLiveTopics([NotNull] ReliableQueueKey reliableQueueKey, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be processed.</summary>
        /// <param name="message">The message received.</param>
        /// <param name="ReliableQueueService">The reliable queue service that received the message and will be responsible for notifying consumers.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to process the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to process the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <returns>
        ///     A task that processes the message supplied and returns <see langword="true"/> if the message will be processed; otherwise,
        ///     <see langword="false"/> if it is to be ignored.
        /// </returns>
        [NotNull]
        public Task<bool> OnReceivedAsync([NotNull] Message message, [NotNull] IReliableQueueService ReliableQueueService, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);
    }
}