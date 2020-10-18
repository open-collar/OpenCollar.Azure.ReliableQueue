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

#pragma warning disable CA1032 // Add standard constructors.
namespace OpenCollar.Azure.ReliableQueue
{
    using System;
    using System.Runtime.Serialization;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="UnknownReliableQueueException" />.
    /// </summary>
    [Serializable]
    public class UnknownReliableQueueException : ReliableQueueException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        public UnknownReliableQueueException([NotNull] QueueKey queueKey) : base(queueKey,
            $@"There is no configuration for this reliable queue: {GetQueueKey(queueKey)}.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The message that describes the error.</param>
        public UnknownReliableQueueException([NotNull] QueueKey queueKey, string message) : base(queueKey, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public UnknownReliableQueueException([NotNull] QueueKey queueKey, string message, Exception innerException) : base(queueKey,
            message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownReliableQueueException"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        protected UnknownReliableQueueException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
