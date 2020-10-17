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

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Azure.ReliableQueue.Configuration
{
    /// <summary>The root configuration object in which all configuration for reliable queues is defined.</summary>
    /// <seealso cref="OpenCollar.Extensions.Configuration.IConfigurationObject"/>
    public interface IReliableQueuesConfiguration : IConfigurationObject
    {
        /// <summary>Gets or sets a dictionary of the configuration for the individual reliable queues.</summary>
        /// <value>The dictionary of the configuration for the individual reliable queues.</value>
        [Path(PathIs.Absolute, @"ReliableQueues:Queues")]
        public IConfigurationDictionary<IReliableQueueConfiguration> Queues { get; set; }
    }
}