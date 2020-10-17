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

using JetBrains.Annotations;

using OpenCollar.Extensions.Configuration;

namespace OpenCollar.Azure.ReliableQueue.Configuration
{
    /// <summary>The configuration for a single reliable queue.</summary>
    /// <seealso cref="OpenCollar.Extensions.Configuration.IConfigurationObject"/>
    /// <example>
    /// A typical section of the configuration file might look like this:
    /// <code lang="json">
    ///     "ReliableQueues": {
    ///         "Queues": {
    ///             "TEST+1": {
    ///                 "StorageConnectionString": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
    ///                 "Mode": "Send",
    ///                 "IsEnabled": "true"
    ///             }
    ///         }
    ///     }
    /// </code>
    /// This class represents the elements under the "Queues" node in that hierarchy (e.g. below "TEST+1").
    /// </example>
    public interface IReliableQueueConfiguration : IConfigurationObject
    {
        /// <summary>The value to assign to the <see cref="Mode"/> property if the service should be permitted to send messages.</summary>
        [NotNull]
        public const string ModeSend = @"Send";

        /// <summary>The value to assign to the <see cref="Mode"/> property if the service should be permitted to receive messages.</summary>
        [NotNull]
        public const string ModeReceive = @"Receive";

        /// <summary>The value to assign to the <see cref="Mode"/> property if the service should be permitted to both send and receive messages.</summary>
        [NotNull]
        public const string ModeBoth = @"Both";

        /// <summary>Gets or sets a flag that indicates this queue is enabled.</summary>
        /// <value>
        ///     A flag that is set to <see langword="true"/> the queue will be available for use in code; otherwise, <see langword="false"/> if will not
        ///     be initialized and may cause errors if any attempt is made to use it.
        /// </value>
        /// <remarks>By default this value is set to <see langword="true"/>.</remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = true)]
        [Path(PathIs.Relative, @"IsEnabled")]
        public bool IsEnabled { get; set; }

        /// <summary>Gets or sets a flag that indicates that a listener should be created, or whether the host will receive messages using functions.</summary>
        /// <value>
        ///     A flag that is set to <see langword="true"/> if a listener should be created; otherwise, <see langword="false"/> if the host will receive
        ///     messages using functions.
        /// </value>
        /// <remarks>By default this value is set to <see langword="false"/> and the consumer must provide events.</remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = false)]
        [Path(PathIs.Relative, @"CreateListener")]
        public bool CreateListener { get; set; }

        /// <summary>Gets or sets the default timeout when sending a message.  Measured in seconds.</summary>
        /// <value>The default timeout when sending a message.  Measured in seconds.</value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 30)]
        [Path(PathIs.Relative, @"DefaultTimeoutSeconds")]
        public int DefaultTimeoutSeconds { get; set; }

        /// <summary>Get or set the maximum number of delivery attempts permitted before the message is moved to the poison reliable queue.</summary>
        /// <value>The maximum number of delivery attempts permitted before the message is moved to the poison reliable queue.</value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 3)]
        [Path(PathIs.Relative, @"MaxAttempts")]
        public int MaxAttempts { get; set; }

        /// <summary>Gets or sets the maximum period of time a message can remain on a queue before it is deleted without being processed.  Measured in seconds.</summary>
        /// <value>The maximum period of time a message can remain on a queue before it is deleted without being processed.  Measured in seconds.</value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 60 * 60 * 24 * 2)]
        [Path(PathIs.Relative, @"MessageTimeToLiveSeconds")]
        public int MessageTimeToLiveSeconds { get; set; }

        /// <summary>Gets or sets a value used to determine whether messages can be sent or received from the connection.</summary>
        /// <value>
        ///     The value used to determine whether messages can be sent or received from the connection.
        ///     <para>
        ///         <list type="table">
        ///             <listheader><term>Value</term> <description>Description</description></listheader>
        ///             <item>
        ///                 <term><c>Receive</c></term>
        ///                 <description>Messages can be received by the service, but not sent.  This is the default value.</description>
        ///             </item>
        ///             <item><term><c>Send</c></term> <description>Messages can be sent by the service, but not received.</description></item>
        ///             <item><term><c>Both</c></term> <description>Messages can be both sent and received by the service.</description></item>
        ///         </list>
        ///     </para>
        ///     <para>Only these two values are valid.  Sending and receiving services are mutually exclusive.</para>
        ///     <para>The constants <see cref="ModeSend"/>, <see cref="ModeReceive"/> and <see cref="ModeBoth"/> can be used to refer to these values programmatically.</para>
        /// </value>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly)]
        [Path(PathIs.Relative, @"Mode")]
        public string Mode { get; set; }

        /// <summary>Gets or sets the sliding window duration when receiving messages.  Measured in seconds.</summary>
        /// <value>The sliding window duration when receiving messages.  Measured in seconds.</value>
        /// <remarks>
        ///     This determines for how long messages are buffered before the consumer is notified.  This allows out-of-order messages to be re-ordered
        ///     correctly.
        /// </remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 1)]
        [Path(PathIs.Relative, @"SlidingWindowDurationSeconds")]
        public int SlidingWindowDurationSeconds { get; set; }

        /// <summary>Gets or sets the connection string for the storage service on which the leases and blobs are hosted.</summary>
        /// <value>The connection string for the storage service on which the leases and blobs are hosted.</value>
        /// <remarks>
        ///     If this is left undefined or <see langword="null"/> it is assumed that it is a receive-only connection and the host will be using the
        ///     <see cref="IReliableQueueService.OnReceivedAsync"/> method to pass messages received using an Event Grid function or similar.
        /// </remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = null)]
        [Path(PathIs.Relative, @"StorageConnectionString")]
        public string StorageConnectionString { get; set; }

        /// <summary>Gets or sets the topic affinity time-to-live when receiving messages.  Measured in seconds.</summary>
        /// <value>The topic affinity time-to-live when receiving messages.  Measured in seconds.</value>
        /// <remarks>
        ///     This determines how long after the last message on a topic is received that the endpoint that previously processed those message should be
        ///     preferred over others.
        /// </remarks>
        [Configuration(Persistence = ConfigurationPersistenceActions.LoadOnly, DefaultValue = 30)]
        [Path(PathIs.Relative, @"TopicAffinityTtlSeconds")]
        public int TopicAffinityTtlSeconds { get; set; }
    }
}