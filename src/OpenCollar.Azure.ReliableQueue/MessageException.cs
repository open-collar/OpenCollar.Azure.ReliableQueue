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
using System.Globalization;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue
{
#pragma warning disable CA1032 // Add standard constructors.

    /// <summary>A class used to represent an exception that occurs involving a message in a queue.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.ReliableQueueException"/>
    [Serializable]
    public class MessageException : ReliableQueueException
    {
        /// <summary>Initializes a new instance of the <see cref="MessageException"></see> class with a specified error message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="message">The message that describes the error.</param>
        public MessageException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId, string message) : base(reliableQueueKey, message)
        {
            MessageId = messageId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageException"></see> class with a specified error message and a reference to the inner exception
        ///     that is the cause of this exception.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public MessageException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId, string message, Exception innerException) : base(reliableQueueKey,
            message, innerException)
        {
            MessageId = messageId;
        }

        /// <summary>Initializes a new instance of the <see cref="MessageException"></see> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        public MessageException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId) : base(reliableQueueKey,
            $"Error processing message. Message ID: {GetMessageId(messageId)}; Reliable Queue Key: {GetReliableQueueKey(reliableQueueKey)}.")
        {
            MessageId = messageId;
        }

        /// <summary>Initializes a new instance of the <see cref="MessageException"></see> class with serialized data.</summary>
        /// <param name="info">
        ///     The <see cref="System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception
        ///     being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or
        ///     destination.
        /// </param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="info">info</paramref> parameter is <see langword="null"/>.</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        ///     The class name is null or <see cref="System.Exception.HResult"></see> is zero
        ///     (0).
        /// </exception>
        protected MessageException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var value = info.GetString(nameof(MessageId));
            if(Guid.TryParseExact(value, "D", out var messageId))
            {
                MessageId = messageId;
            }
        }

        /// <summary>Gets the ID of the message involved.</summary>
        /// <value>The ID of the message involved.</value>
        public Guid MessageId { get; }

        /// <summary>
        ///     When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"></see> with information about the
        ///     exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception
        ///     being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or
        ///     destination.
        /// </param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="info">info</paramref> parameter is <see langword="null"/>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(MessageId), MessageId.ToString("D", CultureInfo.InvariantCulture));
            base.GetObjectData(info, context);
        }

        /// <summary>Gets the message ID, quoted if appropriate, or placeholders for special values.</summary>
        /// <param name="messageId">The reliable queue key, can be empty.</param>
        /// <returns>The message ID, quoted if appropriate, or placeholders for special values.</returns>
        [NotNull]
        protected internal static string GetMessageId(Guid messageId)
        {
            if(messageId == Guid.Empty)
            {
                return @"{EMPTY}";
            }

            return messageId.ToString("D", CultureInfo.InvariantCulture);
        }
    }
}