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

namespace OpenCollar.Azure.ReliableQueue
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>The public interface of the service used to coordinate the sending and receiving of messages using Azure Storage Queues.</summary>
    public interface IReliableQueueService
    {
        /// <summary>Gets the <see cref="IReliableQueue"/> object for the reliable queue with the specified reliable queue key.</summary>
        /// <value>The <see cref="IReliableQueue"/> object requested.</value>
        /// <param name="queueKey">The key for the reliable queue for which to return the object.</param>
        /// <returns>A thin wrapper around the functionality of the reliable queue specified.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queueKey"/> was <see langword="null"/>.</exception>
        [NotNull]
#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers
        public IReliableQueue this[[NotNull] QueueKey queueKey] { get; }

#pragma warning restore CA1043 // Use Integral Or String Argument For Indexers

        /// <summary>Called when a trigger (e.g. EventGrid or StorageQueue) receives a notification of a message to be processed.</summary>
        /// <param name="base64">The Base-64 encoded JSON representation of the serialized message.</param>
        /// <param name="timeout">
        ///     The maximum period of time to wait whilst attempting to process the message before failing with an error.  Defaults to the value in the
        ///     <see cref="Configuration.IReliableQueueConfiguration.DefaultTimeoutSeconds"/> property of the queue configuration.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to abandon the attempt to process the message.  Defaults to <see langword="null"/>, meaning there can be no
        ///     cancellation.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="base64"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="base64"/> was zero-length or contains only white-space characters.</exception>
        /// <returns>A task that processes the message supplied.</returns>
        public Task OnReceivedAsync([NotNull] string base64, TimeSpan? timeout = null, [CanBeNull] CancellationToken? cancellationToken = null);

        /// <summary>Unsubscribes from the reliable queue by the token given.</summary>
        /// <param name="token">The token returned by <see cref="IReliableQueue.Subscribe"/> when the subscription was created.</param>
        /// <returns>
        ///     <see langword="true"/> if the subscription was found and unsubscribed; otherwise, <see langword="false"/> if there was no subscription to
        ///     remove.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="token"/> was <see langword="null"/>.</exception>
        public bool Unsubscribe([NotNull] SubscriptionToken token);
    }
}
