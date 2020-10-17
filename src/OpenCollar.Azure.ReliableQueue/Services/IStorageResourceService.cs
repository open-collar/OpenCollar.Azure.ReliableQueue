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

using Azure.Storage.Queues;

using JetBrains.Annotations;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.WindowsAzure.Storage.Blob;

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The public interface of the service used to create and manage clients for the various Azure Storage resources used reliable queues.</summary>
    internal interface IStorageResourceService
    {
        /// <summary>Gets the BLOB storage client used to access Azure Storage BLOB Storage for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the BLOB storage client.</param>
        /// <returns>The BLOB storage client for the reliable queue specified by the key given.</returns>
        [NotNull]
        public CloudBlobClient GetBlobClient([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Gets the queue client for the reliable queue specified by the key given.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which get the client.</param>
        /// <returns>The queue client for the reliable queue specified by the key given.</returns>
        [NotNull]
        public QueueClient GetQueueClient([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Gets the table used for storing details about the current state of a message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the state table.</param>
        /// <returns>The state table for the reliable queue specified by the key given.</returns>
        [NotNull]
        public CloudTable GetStateTable([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Gets the table used for storing details about topic affinity.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the topic affinity table.</param>
        /// <returns>The topic able for the reliable queue specified by the key given.</returns>
        [NotNull]
        public CloudTable GetTopicTable([NotNull] ReliableQueueKey reliableQueueKey);
    }
}