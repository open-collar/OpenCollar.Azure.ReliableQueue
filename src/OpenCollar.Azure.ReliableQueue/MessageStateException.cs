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
using System.Runtime.Serialization;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue
{
    /// <summary>An exception thrown when a message is not in the expected state.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.MessageException"/>
    [Serializable]
    public class MessageStateException : MessageException
    {
        /// <summary>Initializes a new instance of the <see cref="MessageException"></see> class with a specified error message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        /// <param name="message">The message that describes the error.</param>
        public MessageStateException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId, MessageState expectedState, MessageState actualState,
            string message) : base(reliableQueueKey, messageId, message)
        {
            ExpectedState = expectedState;
            ActualState = actualState;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageException"></see> class with a specified error message and a reference to the inner exception
        ///     that is the cause of this exception.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public MessageStateException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId, MessageState expectedState, MessageState actualState,
            string message, Exception innerException) : base(reliableQueueKey, messageId, message, innerException)
        {
            ExpectedState = expectedState;
            ActualState = actualState;
        }

        /// <summary>Initializes a new instance of the <see cref="MessageException"></see> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        public MessageStateException([NotNull] ReliableQueueKey reliableQueueKey, Guid messageId, MessageState expectedState, MessageState actualState) : base(
            reliableQueueKey, messageId,
            $@"Message in wrong state: expected state: {expectedState}, actual found: {actualState}. Message ID: {GetMessageId(messageId)}; Reliable Queue Key: {GetReliableQueueKey(reliableQueueKey)}.")
        {
            ExpectedState = expectedState;
            ActualState = actualState;
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
        protected MessageStateException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExpectedState = (MessageState)info.GetValue(nameof(ExpectedState), typeof(MessageState));
            ActualState = (MessageState)info.GetValue(nameof(ActualState), typeof(MessageState));
        }

        /// <summary>Gets the actual state of the message.</summary>
        /// <value>The actual state of the message.</value>
        public MessageState ActualState { get; }

        /// <summary>Gets the state that was expected.</summary>
        /// <value>The state that was expected.</value>
        public MessageState ExpectedState { get; }

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
            info.AddValue(nameof(ExpectedState), ExpectedState);
            info.AddValue(nameof(ActualState), ActualState);

            base.GetObjectData(info, context);
        }
    }
}