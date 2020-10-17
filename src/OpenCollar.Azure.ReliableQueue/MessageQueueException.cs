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
using OpenCollar.Extensions.Validation;

#pragma warning disable CA1032 // Implement standard exception constructors

namespace OpenCollar.Azure.ReliableQueue
{
#pragma warning disable CA1032 // Add standard constructors.

    /// <summary>A class used to represent an exception involving a reliable queue.</summary>
    /// <seealso cref="ReliableQueueException"/>
    [Serializable]
    public class ReliableQueueException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ReliableQueueException"></see> class with a specified error message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The message that describes the error.</param>
        public ReliableQueueException([CanBeNull] ReliableQueueKey reliableQueueKey, [CanBeNull] string message) : base(message)
        {
            ReliableQueueKey = reliableQueueKey;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReliableQueueException"></see> class with a specified error message and a reference to the inner
        ///     exception that is the cause of this exception.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public ReliableQueueException([NotNull] ReliableQueueKey reliableQueueKey, [CanBeNull] string message, [CanBeNull] Exception innerException) : base(
            message, innerException)
        {
            ReliableQueueKey = reliableQueueKey;
        }

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueException"></see> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue involved.</param>
        public ReliableQueueException([CanBeNull] ReliableQueueKey reliableQueueKey) : base(
            $@"An error occurred involving the reliable queue with the key: {GetReliableQueueKey(reliableQueueKey)}.")
        {
            ReliableQueueKey = reliableQueueKey;
        }

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueException"></see> class with serialized data.</summary>
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
        protected ReliableQueueException([CanBeNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var reliableQueueKey = info.GetString(nameof(ReliableQueueKey));
            if(!(reliableQueueKey is null))
            {
                ReliableQueueKey = new ReliableQueueKey(reliableQueueKey);
            }
        }

        /// <summary>Gets the key identifying the reliable queue involved.</summary>
        /// <value>The key identifying the reliable queue involved.</value>
        [CanBeNull]
        public ReliableQueueKey ReliableQueueKey { get; }

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
            info.Validate(nameof(info), ObjectIs.NotNull);

            info.AddValue(nameof(ReliableQueueKey), ReliableQueueKey?.ToString());
            base.GetObjectData(info, context);
        }

        /// <summary>Gets the reliable queue key, quoted if appropriate, or placeholders for special values.</summary>
        /// <param name="reliableQueueKey">The reliable queue key, can be <see langword="null"/>.</param>
        /// <returns>The reliable queue key, quoted if appropriate, or placeholders for special values.</returns>
        [NotNull]
        protected internal static string GetReliableQueueKey([CanBeNull] ReliableQueueKey reliableQueueKey)
        {
            if(reliableQueueKey is null)
            {
                return @"[NULL]";
            }

            if(string.IsNullOrWhiteSpace(reliableQueueKey))
            {
                return @"[EMPTY]";
            }

            return string.Concat("\"", reliableQueueKey, "\"");
        }
    }
}