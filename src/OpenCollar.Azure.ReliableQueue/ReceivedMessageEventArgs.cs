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

namespace OpenCollar.Azure.ReliableQueue
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Azure.ReliableQueue.Services;

    /// <summary>
    /// Defines the <see cref="ReceivedMessageEventArgs" />.
    /// </summary>
    [DebuggerDisplay("ReceivedMessageEventArgs: {" + nameof(QueueKey) + ",nq}")]
    public sealed class ReceivedMessageEventArgs : HandledEventArgs
    {
        /// <summary>
        /// Defines the _message.
        /// </summary>
        private readonly Message _message;

        /// <summary>
        /// Defines the _storage.
        /// </summary>
        private readonly IMessageStorageService _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedMessageEventArgs"/> class.
        /// </summary>
        /// <param name="storage">The message storage service from which to load the contents of the message if requested.</param>
        /// <param name="queueKey">The key identifying the queue from which the message was delivered.</param>
        /// <param name="message">The message that has been received.</param>
        internal ReceivedMessageEventArgs([NotNull] IMessageStorageService storage, [NotNull] QueueKey queueKey, [NotNull] Message message)
        {
            _storage = storage;
            this.queueKey = queueKey;
            _message = message;
        }

        /// <summary>
        /// Gets the queueKey.
        /// </summary>
        public QueueKey queueKey { get; }

        /// <summary>
        /// Gets the Size.
        /// </summary>
        public long? Size => _message.Size;

        /// <summary>
        /// Gets the Topic.
        /// </summary>
        public Topic Topic => _message.Topic;

        /// <summary>
        /// The GetBodyAsBufferAsync.
        /// </summary>
        /// <param name="timeout">The maximum period of time to wait whilst attempting to read the message body to the BLOB storage before failing with an error.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to read the message body to the BLOB storage.</param>
        /// <returns>A task that returns the message body as an array of bytes (or <see langword="null"/> if the message had no body).</returns>
        [CanBeNull]
        public async Task<byte[]> GetBodyAsBufferAsync(TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            await using var stream = new MemoryStream();

            await _storage.ReadMessageAsync(queueKey, _message, stream, timeout, cancellationToken).ConfigureAwait(true);

            return stream.ToArray();
        }

        /// <summary>
        /// The GetBodyAsStreamAsync.
        /// </summary>
        /// <param name="timeout">The maximum period of time to wait whilst attempting to read the message body to the BLOB storage before failing with an error.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to read the message body to the BLOB storage.</param>
        /// <returns>A task that returns the message body as a stream (or <see langword="null"/> if the message had no body).</returns>
        [CanBeNull]
        public async Task<Stream> GetBodyAsStreamAsync(TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            var stream = new MemoryStream();

            await _storage.ReadMessageAsync(queueKey, _message, stream, timeout, cancellationToken).ConfigureAwait(true);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        /// <summary>
        /// The GetBodyAsStringAsync.
        /// </summary>
        /// <param name="timeout">The maximum period of time to wait whilst attempting to read the message body to the BLOB storage before failing with an error.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to abandon the attempt to read the message body to the BLOB storage.</param>
        /// <returns>A task that returns the message body as a string (or <see langword="null"/> if the message had no body).</returns>
        [CanBeNull]
        public async Task<string> GetBodyAsStringAsync(TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            await using var stream = new MemoryStream();

            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024 * 1024, true);

            await _storage.ReadMessageAsync(queueKey, _message, stream, timeout, cancellationToken).ConfigureAwait(true);

            stream.Seek(0, SeekOrigin.Begin);

            return await reader.ReadToEndAsync().ConfigureAwait(true);
        }
    }
}
