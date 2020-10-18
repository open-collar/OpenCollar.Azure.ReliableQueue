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
    using JetBrains.Annotations;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using OpenCollar.Azure.ReliableQueue.Configuration;
    using OpenCollar.Azure.ReliableQueue.Services;
    using OpenCollar.Extensions.Configuration;
    using OpenCollar.Extensions.Validation;

    /// <summary>
    /// Defines the <see cref="ServiceCollectionExtensions" />.
    /// </summary>
    [UsedImplicitly]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// The AddReliableQueues.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which to add the configuration reader. This must not be <see langword="null"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddReliableQueues(this IServiceCollection serviceCollection)
        {
            serviceCollection.Validate(nameof(serviceCollection), ObjectIs.NotNull);

            serviceCollection.AddConfigurationReader<IReliableQueuesConfiguration>(new ConfigurationObjectSettings
            {
                EnableNewtonSoftJsonSupport = false
            });
            serviceCollection.TryAddSingleton<IReliableQueueConfigurationService, ReliableQueueConfigurationService>();
            serviceCollection.TryAddSingleton<IStorageResourceService, StorageResourceService>();
            serviceCollection.TryAddSingleton<IMessageStorageService, MessageStorageService>();
            serviceCollection.TryAddSingleton<IReliableQueueReceiverService, ReliableQueueReceiverService>();
            serviceCollection.TryAddSingleton<IReliableQueueSenderService, ReliableQueueSenderService>();
            serviceCollection.TryAddSingleton<IMessageTopicService, MessageTopicService>();
            serviceCollection.TryAddSingleton<IMessageStateService, MessageStateService>();

            // This is the public one that consumers will notice.
            serviceCollection.TryAddSingleton<IReliableQueueService, ReliableQueueService>();

            return serviceCollection;
        }
    }
}
