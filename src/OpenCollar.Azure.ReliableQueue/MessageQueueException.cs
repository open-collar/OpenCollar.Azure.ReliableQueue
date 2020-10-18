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

#pragma warning disable CA1032 // Implement standard exception constructors
namespace OpenCollar.Azure.ReliableQueue
{
    using System;
    using System.Runtime.Serialization;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Extensions.Validation;

    /// <summary>
    /// Defines the <see cref="ReliableQueueException" />.
    /// </summary>
    [Serializable]
    public class ReliableQueueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        public ReliableQueueException([CanBeNull] QueueKey queueKey) : base(
            $@"An error occurred involving the reliable queue with the key: {GetQueueKey(queueKey)}.")
        {
            QueueKey = queueKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The message that describes the error.</param>
        public ReliableQueueException([CanBeNull] QueueKey queueKey, [CanBeNull] string message) : base(message)
        {
            QueueKey = queueKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public ReliableQueueException([NotNull] QueueKey queueKey, [CanBeNull] string message, [CanBeNull] Exception innerException) : base(
            message, innerException)
        {
            QueueKey = queueKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableQueueException"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        protected ReliableQueueException([CanBeNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var queueKey = info.GetString(nameof(QueueKey));
            if (!(queueKey is null))
            {
                QueueKey = new QueueKey(queueKey);
            }
        }

        /// <summary>
        /// Gets the QueueKey.
        /// </summary>
        [CanBeNull]
        public QueueKey QueueKey { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"></see> with information about the
        ///     exception.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.Validate(nameof(info), ObjectIs.NotNull);

            info.AddValue(nameof(QueueKey), QueueKey?.ToString());
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// The GetQueueKey.
        /// </summary>
        /// <param name="queueKey">The reliable queue key, can be <see langword="null"/>.</param>
        /// <returns>The reliable queue key, quoted if appropriate, or placeholders for special values.</returns>
        [NotNull]
        protected internal static string GetQueueKey([CanBeNull] QueueKey queueKey)
        {
            if (queueKey is null)
            {
                return @"[NULL]";
            }

            if (string.IsNullOrWhiteSpace(queueKey))
            {
                return @"[EMPTY]";
            }

            return string.Concat("\"", queueKey, "\"");
        }
    }
}
