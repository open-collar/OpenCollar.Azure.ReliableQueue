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
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="IReliableQueueSenderService" />.
    /// </summary>
    internal interface IReliableQueueSenderService
    {
        /// <summary>
        /// The SendMessageAsync.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue for which to add the new message.</param>
        /// <param name="message">The current state of the message to record.</param>
        /// <param name="timeout">The timeout<see cref="TimeSpan?"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken?"/>.</param>
        /// <returns>The new state of the message that was created, with updated properties.</returns>
        [NotNull]
        public Task<Message> SendMessageAsync([NotNull] QueueKey queueKey, [NotNull] Message message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);
    }
}
