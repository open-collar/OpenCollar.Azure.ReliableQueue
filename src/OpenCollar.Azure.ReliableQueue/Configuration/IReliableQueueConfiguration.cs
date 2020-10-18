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

namespace OpenCollar.Azure.ReliableQueue.Configuration
{
    using JetBrains.Annotations;

    using OpenCollar.Extensions.Configuration;

    /// <summary>
    /// Defines the <see cref="IReliableQueueConfiguration" />.
    /// </summary>
    public interface IReliableQueueConfiguration : IConfigurationObject
    {
        /// <summary>
        /// Defines the ModeBoth.
        /// </summary>
        [NotNull]
        public const string ModeBoth = @"Both";

        /// <summary>
        /// Defines the ModeReceive.
        /// </summary>
        [NotNull]
        public const string ModeReceive = @"Receive";

        /// <summary>
        /// Defines the ModeSend.
        /// </summary>
        [NotNull]
        public const string ModeSend = @"Send";

        /// <summary>
        /// Gets or sets a value indicating whether CreateListener.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = false)]
        [Path(PathIs.Relative, @"CreateListener")]
        public bool CreateListener { get; set; }

        /// <summary>
        /// Gets or sets the DefaultTimeoutSeconds.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 30)]
        [Path(PathIs.Relative, @"DefaultTimeoutSeconds")]
        public int DefaultTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsEnabled.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = true)]
        [Path(PathIs.Relative, @"IsEnabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the MaxAttempts.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 3)]
        [Path(PathIs.Relative, @"MaxAttempts")]
        public int MaxAttempts { get; set; }

        /// <summary>
        /// Gets or sets the MessageTimeToLiveSeconds.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 60 * 60 * 24 * 2)]
        [Path(PathIs.Relative, @"MessageTimeToLiveSeconds")]
        public int MessageTimeToLiveSeconds { get; set; }

        /// <summary>
        /// Gets or sets the Mode.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly)]
        [Path(PathIs.Relative, @"Mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the SlidingWindowDurationSeconds.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 1)]
        [Path(PathIs.Relative, @"SlidingWindowDurationSeconds")]
        public int SlidingWindowDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the StorageConnectionString.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = null)]
        [Path(PathIs.Relative, @"StorageConnectionString")]
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the TopicAffinityTtlSeconds.
        /// </summary>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 30)]
        [Path(PathIs.Relative, @"TopicAffinityTtlSeconds")]
        public int TopicAffinityTtlSeconds { get; set; }
    }
}
