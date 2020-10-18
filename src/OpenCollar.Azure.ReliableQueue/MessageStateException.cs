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

    /// <summary>
    /// Defines the <see cref="MessageStateException" />.
    /// </summary>
    [Serializable]
    public class MessageStateException : MessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageStateException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        public MessageStateException([NotNull] QueueKey queueKey, Guid messageId, MessageState expectedState, MessageState actualState) : base(
            queueKey, messageId,
            $@"Message in wrong state: expected state: {expectedState}, actual found: {actualState}. Message ID: {GetMessageId(messageId)}; Reliable Queue Key: {GetQueueKey(queueKey)}.")
        {
            ExpectedState = expectedState;
            ActualState = actualState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageStateException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        /// <param name="message">The message that describes the error.</param>
        public MessageStateException([NotNull] QueueKey queueKey, Guid messageId, MessageState expectedState, MessageState actualState,
            string message) : base(queueKey, messageId, message)
        {
            ExpectedState = expectedState;
            ActualState = actualState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageStateException"/> class.
        /// </summary>
        /// <param name="queueKey">The key identifying the reliable queue involved.</param>
        /// <param name="messageId">The ID of the message involved.</param>
        /// <param name="expectedState">The state that was expected.</param>
        /// <param name="actualState">The actual state of the message.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public MessageStateException([NotNull] QueueKey queueKey, Guid messageId, MessageState expectedState, MessageState actualState,
            string message, Exception innerException) : base(queueKey, messageId, message, innerException)
        {
            ExpectedState = expectedState;
            ActualState = actualState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageStateException"/> class.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        protected MessageStateException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ExpectedState = (MessageState)info.GetValue(nameof(ExpectedState), typeof(MessageState));
            ActualState = (MessageState)info.GetValue(nameof(ActualState), typeof(MessageState));
        }

        /// <summary>
        /// Gets the ActualState.
        /// </summary>
        public MessageState ActualState { get; }

        /// <summary>
        /// Gets the ExpectedState.
        /// </summary>
        public MessageState ExpectedState { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"></see> with information about the
        ///     exception.
        /// </summary>
        /// <param name="info">The info<see cref="SerializationInfo"/>.</param>
        /// <param name="context">The context<see cref="StreamingContext"/>.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ExpectedState), ExpectedState);
            info.AddValue(nameof(ActualState), ActualState);

            base.GetObjectData(info, context);
        }
    }
}
