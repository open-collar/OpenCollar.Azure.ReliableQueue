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

namespace OpenCollar.Azure.ReliableQueue.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using JetBrains.Annotations;

    using Microsoft.WindowsAzure.Storage;

    using OpenCollar.Azure.ReliableQueue.Configuration;
    using OpenCollar.Azure.ReliableQueue.Model;
    using OpenCollar.Extensions;
    using OpenCollar.Extensions.Configuration;
    using OpenCollar.Extensions.Validation;

    /// <summary>A service used to access the configuration for the queues used to send and receive messages.</summary>
    /// <seealso cref="IReliableQueueConfigurationService"/>
    internal sealed class ReliableQueueConfigurationService : Disposable, IReliableQueueConfigurationService
    {
        /// <summary>The configuration from which the service was initialized.</summary>
        [NotNull]
        private readonly IReliableQueuesConfiguration _configuration;

        /// <summary>A read-only dictionary defining all of the configured reliable queues, keyed (case-insensitive) on their reliable queue key.</summary>
        [NotNull]
        private readonly IReadOnlyDictionary<QueueKey, IReliableQueueConfiguration> _reliableQueues;

        /// <summary>A cache of parsed storage accounts, keyed on the connection string.</summary>
        [NotNull]
        private readonly OpenCollar.Extensions.Collections.Concurrent.InMemoryCache<string, CloudStorageAccount> _storageAccountCache =
            new OpenCollar.Extensions.Collections.Concurrent.InMemoryCache<string, CloudStorageAccount>(TimeSpan.FromSeconds(120), CloudStorageAccount.Parse, false, true);

        /// <summary>A cache of parsed table accounts, keyed on the connection string.</summary>
        [NotNull]
        private readonly OpenCollar.Extensions.Collections.Concurrent.InMemoryCache<string, Microsoft.Azure.Cosmos.Table.CloudStorageAccount> _tableAccountCache =
            new OpenCollar.Extensions.Collections.Concurrent.InMemoryCache<string, Microsoft.Azure.Cosmos.Table.CloudStorageAccount>(TimeSpan.FromSeconds(120),
                Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse, false, true);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to
        /// release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _storageAccountCache.Dispose();
                _tableAccountCache.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueConfigurationService"/> class.</summary>
        /// <param name="configuration">The configuration object from which to initialize.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="configuration"/> was <see langword="null"/>.</exception>
        /// <exception cref="ConfigurationException"><paramref name="configuration"/> was <see langword="null"/>.</exception>
        public ReliableQueueConfigurationService([NotNull] IReliableQueuesConfiguration configuration)
        {
            configuration.Validate(nameof(configuration), ObjectIs.NotNull);

            _configuration = configuration;

            var d = configuration.Queues.Where(q => q.Value.IsEnabled).ToDictionary(p => new QueueKey(p.Key), p => p.Value);

            _reliableQueues = new ReadOnlyDictionary<QueueKey, IReliableQueueConfiguration>(d);

            foreach (var queue in _reliableQueues)
            {
                var mode = queue.Value.Mode;
                if (string.IsNullOrWhiteSpace(mode))
                {
                    throw new ConfigurationException(
                        $"No value has been provided for the '{nameof(IReliableQueueConfiguration.Mode)}' property of the {queue.Key} reliable queue configuration.");
                }

                if (!IReliableQueueConfiguration.ModeReceive.Equals(mode, StringComparison.OrdinalIgnoreCase) &&
                   !IReliableQueueConfiguration.ModeSend.Equals(mode, StringComparison.OrdinalIgnoreCase) &&
                   !IReliableQueueConfiguration.ModeBoth.Equals(mode, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ConfigurationException(
                        $"The value has been provided for the '{nameof(IReliableQueueConfiguration.Mode)}' property of the {queue.Key} reliable queue configuration is not valid: \"{mode}\".  Only \"{IReliableQueueConfiguration.ModeReceive}\" or \"{IReliableQueueConfiguration.ModeSend}\" are permitted.");
                }
            }
        }

        /// <summary>Gets the <see cref="IReliableQueueConfiguration"/> object with the specified reliable queue key.</summary>
        /// <value>The <see cref="IReliableQueueConfiguration"/>.</value>
        /// <param name="queueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The configuration for the queue identified by the <paramref name="queueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        public IReliableQueueConfiguration this[QueueKey queueKey]
        {
            get
            {
                queueKey.Validate(nameof(queueKey), ObjectIs.NotNull);

                if (_reliableQueues.TryGetValue(queueKey, out var ReliableQueueConfiguration))
                {
                    return ReliableQueueConfiguration;
                }

                throw new UnknownReliableQueueException(queueKey);
            }
        }

        /// <summary>Gets the storage account for the reliable queue specified.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The storage account for the queue identified by the <paramref name="queueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        public CloudStorageAccount GetStorageAccount(QueueKey queueKey)
        {
            var configuration = this[queueKey];

            return _storageAccountCache[configuration.StorageConnectionString];
        }

        /// <summary>Gets the table storage account for the reliable queue specified.</summary>
        /// <param name="queueKey">The key identifying the reliable queue for which the configuration is required.</param>
        /// <returns>The storage account for the queue identified by the <paramref name="queueKey"/> specified.</returns>
        /// <exception cref="UnknownReliableQueueException">There is no configuration for the reliable queue specified.</exception>
        public Microsoft.Azure.Cosmos.Table.CloudStorageAccount GetTableStorageAccount(QueueKey queueKey)
        {
            var configuration = this[queueKey];

            return _tableAccountCache[configuration.StorageConnectionString];
        }

        /// <summary>Gets a read-only dictionary defining all of the configured reliable queues, keyed (case-insensitive) on their reliable queue key.</summary>
        /// <value>A read-only dictionary defining all of the configured reliable queues, keyed (case-insensitive) on their reliable queue key.</value>
        public IReadOnlyDictionary<QueueKey, IReliableQueueConfiguration> ReliableQueues => _reliableQueues;
    }
}
