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

namespace OpenCollar.Azure.ReliableQueue.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    ///     Defines the <see cref="IMessageTopicService" />.
    /// </summary>
    internal interface IMessageTopicService
    {
        /// <summary>
        ///     The GetLiveTopics.
        /// </summary>
        /// <param name="queueKey">
        ///     The key identifying the queue from which to read the topics.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     A sequence containing the topics found to be active.
        /// </returns>
        [NotNull]
        public IEnumerable<Topic> GetLiveTopics([NotNull] QueueKey queueKey, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>
        ///     The OnReceivedAsync.
        /// </summary>
        /// <param name="message">
        ///     The message received.
        /// </param>
        /// <param name="ReliableQueueService">
        ///     The reliable queue service that received the message and will be responsible for notifying consumers.
        /// </param>
        /// <param name="timeout">
        ///     The timeout <see cref="TimeSpan" />.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellationToken <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        ///     The <see cref="Task{T}" />.
        /// </returns>
        [NotNull]
        public Task<bool> OnReceivedAsync([NotNull] Message message, [NotNull] IReliableQueueService ReliableQueueService, TimeSpan? timeout = null,
            [CanBeNull] CancellationToken? cancellationToken = null);
    }
}