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
    using System.Globalization;
    using System.Runtime.Serialization;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="MessageException" />.
    /// </summary>
    [Serializable]
    public class MessageException : ReliableQueueException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        public MessageException([NotNull] QueueKey queueKey, Guid messageId) : base(queueKey,
            $"Error processing message. Message ID: {GetMessageId(messageId)}; Reliable Queue Key: {GetQueueKey(queueKey)}.")
        {
            MessageId = messageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="message">The message that describes the error.</param>
        public MessageException([NotNull] QueueKey queueKey, Guid messageId, string message) : base(queueKey, message)
        {
            MessageId = messageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public MessageException([NotNull] QueueKey queueKey, Guid messageId, string message, Exception innerException) : base(queueKey,
            message, innerException)
        {
            MessageId = messageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageException"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        protected MessageException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var value = info.GetString(nameof(MessageId));
            if (Guid.TryParseExact(value, "D", out var messageId))
            {
                MessageId = messageId;
            }
        }

        /// <summary>
        /// Gets the MessageId.
        /// </summary>
        public Guid MessageId { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"></see> with information about the
        ///     exception.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(MessageId), MessageId.ToString("D", CultureInfo.InvariantCulture));
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// The GetMessageId.
        /// </summary>
        /// <param name="messageId">The reliable queue key, can be empty.</param>
        /// <returns>The message ID, quoted if appropriate, or placeholders for special values.</returns>
        [NotNull]
        protected internal static string GetMessageId(Guid messageId)
        {
            if (messageId == Guid.Empty)
            {
                return @"{EMPTY}";
            }

            return messageId.ToString("D", CultureInfo.InvariantCulture);
        }
    }
}
