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
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

using OpenCollar.Azure.ReliableQueue.Model;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Services
{
    /// <summary>The service used to create and manage clients for the various Azure Storage resources used reliable queues.</summary>
    internal sealed class StorageResourceService : IStorageResourceService
    {
        /// <summary>The logger used to record information about usage and activities.</summary>
        [NotNull]
        private readonly ILogger _logger;

        /// <summary>The service used to access the configuration for the queues used to send and receive messages.</summary>
        [NotNull]
        private readonly IReliableQueueConfigurationService _reliableQueueConfigurationService;

        /// <summary>Initializes a new instance of the <see cref="StorageResourceService"/> class.</summary>
        /// <param name="logger">The logger used to record information about usage and activities.</param>
        /// <param name="ReliableQueueConfigurationService">The service used to access the configuration for the queues used to send and receive messages.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="ReliableQueueConfigurationService"/> was <see langword="null"/>.</exception>
        public StorageResourceService([NotNull] ILogger<StorageResourceService> logger,
            [NotNull] IReliableQueueConfigurationService ReliableQueueConfigurationService)
        {
            logger.Validate(nameof(logger), ObjectIs.NotNull);
            ReliableQueueConfigurationService.Validate(nameof(ReliableQueueConfigurationService), ObjectIs.NotNull);

            _logger = logger;
            _reliableQueueConfigurationService = ReliableQueueConfigurationService;
        }

        /// <summary>Gets the BLOB storage client used to access Azure Storage BLOB Storage for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the BLOB storage client.</param>
        /// <returns>The BLOB storage client for the reliable queue specified by the key given.</returns>
        public CloudBlobClient GetBlobClient(QueueKey reliableQueueKey)
        {
            // Extract the details of the storage account from the connection string.
            var storageAccount = _reliableQueueConfigurationService.GetStorageAccount(reliableQueueKey);

            // Create a client from those details.
            return storageAccount.CreateCloudBlobClient();
        }

        /// <summary>Gets the queue client for the reliable queue specified by the key given.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which get the client.</param>
        /// <returns>The queue client for the reliable queue specified by the key given.</returns>
        public QueueClient GetQueueClient(QueueKey reliableQueueKey)
        {
            var configuration = _reliableQueueConfigurationService[reliableQueueKey];

            var queueName = Identifiers.GetReliableQueueName(reliableQueueKey);

            // Create the queue on which we will receive messages.
            return new QueueClient(configuration.StorageConnectionString, queueName);
        }

        /// <summary>Gets the table used for storing details about the current state of a message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the state table.</param>
        /// <returns>The state table for the reliable queue specified by the key given.</returns>
        public CloudTable GetStateTable(QueueKey reliableQueueKey)
        {
            var tableClient = GetTableClient(reliableQueueKey);

            return GetStateTable(reliableQueueKey, tableClient);
        }

        /// <summary>Gets the table used for storing details about topic affinity.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the topic affinity table.</param>
        /// <returns>The topic able for the reliable queue specified by the key given.</returns>
        public CloudTable GetTopicTable(QueueKey reliableQueueKey)
        {
            var tableClient = GetTableClient(reliableQueueKey);

            return GetTopicTable(reliableQueueKey, tableClient);
        }

        /// <summary>Gets the table client used to access Azure Storage Tables for the reliable queue specified.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the table client.</param>
        /// <returns>The table client for the reliable queue specified by the key given.</returns>
        public CloudTableClient GetTableClient(QueueKey reliableQueueKey)
        {
            var cloudStorageAccount = _reliableQueueConfigurationService.GetTableStorageAccount(reliableQueueKey);
            return cloudStorageAccount.CreateCloudTableClient();
        }

        /// <summary>Gets the table used for storing details about the current state of a message.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the state table.</param>
        /// <param name="tableClient">The table client from which to take the cloud table.</param>
        /// <returns></returns>
        [NotNull]
        private static CloudTable GetStateTable([NotNull] QueueKey reliableQueueKey, [NotNull] CloudTableClient tableClient)
        {
            var tableName = Identifiers.GetStateTableName(reliableQueueKey);
            return tableClient.GetTableReference(tableName);
        }

        /// <summary>Gets the table used for storing details about topic affinity.</summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to get the topic table.</param>
        /// <param name="tableClient">The table client from which to take the cloud table.</param>
        /// <returns></returns>
        [NotNull]
        private static CloudTable GetTopicTable([NotNull] QueueKey reliableQueueKey, [NotNull] CloudTableClient tableClient)
        {
            var tableName = Identifiers.GetTopicTableName(reliableQueueKey);
            return tableClient.GetTableReference(tableName);
        }
    }
}