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

using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.WindowsAzure.Storage;

using OpenCollar.Azure.ReliableQueue.Configuration;
using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The public interface of the service used to access the configuration for the queues used to send and receive messages.</summary>
    internal interface IReliableQueueConfigurationService
    {
        /// <summary>Gets the <see cref="IReliableQueueConfiguration"/> object with the specified reliable queue key.</summary>
        /// <value>The <see cref="IReliableQueueConfiguration"/>.</value>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The configuration for the queue identified by the <paramref name="reliableQueueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        [NotNull]
        public IReliableQueueConfiguration this[[NotNull] ReliableQueueKey reliableQueueKey] { get; }

        /// <summary>Gets a read-only dictionary defining all of the configured reliable queues, keyed (case-insensitive) on their reliable queue key.</summary>
        /// <value>A read-only dictionary defining all of the configured reliable queues, keyed (case-insensitive) on their reliable queue key.</value>
        [NotNull]
        IReadOnlyDictionary<ReliableQueueKey, IReliableQueueConfiguration> ReliableQueues { get; }

        /// <summary>Gets the storage account for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The storage account for the queue identified by the <paramref name="reliableQueueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        [NotNull]
        public CloudStorageAccount GetStorageAccount([NotNull] ReliableQueueKey reliableQueueKey);

        /// <summary>Gets the table storage account for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The storage account for the queue identified by the <paramref name="reliableQueueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        [NotNull]
        public Microsoft.Azure.Cosmos.Table.CloudStorageAccount GetTableStorageAccount([NotNull] ReliableQueueKey reliableQueueKey);
    }
}