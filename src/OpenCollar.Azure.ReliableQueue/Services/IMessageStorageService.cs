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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The public interface of the service used to store and retrieve the body of messages.</summary>
    internal interface IMessageStorageService
    {
        /// <summary>Deletes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to delete the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be deleted.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to delete the message body to the BLOB storage before failing with an error.  Defaults to
        ///     the value in the <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to delete the message body to the BLOB storage.</param>
        /// <returns>A task that deletes the message body to the BLOB storage.</returns>
        [NotNull]
        public Task DeleteMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] MessageRecord message, TimeSpan? timeout = null,
            CancellationToken? cancellationToken = null);

        /// <summary>Reads the body of a message from BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to read the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be read.</param>
        /// <param name="blob">The stream into which the BLOB will be read.</param>
        /// <param name="timeout">The maximum period of time to wait whilst attempting to read the message body to the BLOB storage before failing with an error.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to read the message body to the BLOB storage.</param>
        /// <returns>A task that returns a stream containing the message body from the BLOB storage, or <see langword="null"/> if there is no body.</returns>
        [NotNull]
        public Task<Stream?> ReadMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] MessageRecord message, [NotNull] Stream blob,
            TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

        /// <summary>Writes the body of a message to BLOB storage, asynchronously.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to write the body of a message.</param>
        /// <param name="message">The details of the message for which the BLOB is to be written.</param>
        /// <param name="blob">
        ///     A stream containing the BLOB to write into the BLOB storage.  Can be <see langword="null"/> or zero-length if the message has not
        ///     body.
        /// </param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to write the message body to the BLOB storage before failing with an
        ///     error.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to write the message body to the BLOB storage.</param>
        /// <returns>A task that writes the message body to the BLOB storage.</returns>
        [NotNull]
        public Task<MessageRecord> WriteMessageAsync([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] MessageRecord message, Stream? blob,
            TimeSpan? timeout = null, CancellationToken? cancellationToken = null);
    }
}