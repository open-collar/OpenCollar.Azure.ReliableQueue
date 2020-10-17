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

using System;
using System.Diagnostics;

using JetBrains.Annotations;

using Microsoft.Azure.Cosmos.Table;

using Newtonsoft.Json;

using OpenCollar.Azure.ReliableQueue.Model.Newtonsoft;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Model
{
    /// <summary>A class representing a topic affinity recorded in a storage table.</summary>
    /// <seealso cref="Microsoft.Azure.Cosmos.Table.TableEntity"/>
    [DebuggerDisplay("Queue: {ReliableQueueKey,nq}; Topic: {Topic,nq}; Owner: {Owner,nq}.")]
    internal sealed class TopicAffinity : TableEntity
    {
        /// <summary>The default value used in <see cref="DateTime"/> fields to prevent Azure Tables from throwing out-of-range errors.</summary>
        public static readonly DateTime DefaultDateTime = new DateTime(1970, 1, 1);

        /// <summary>Initializes a new instance of the <see cref="TopicAffinity"/> class.</summary>
        public TopicAffinity()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TopicAffinity"/> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the queue to which the message belongs.</param>
        /// <param name="owner">
        ///     The ID of the endpoint that most recently processed messages for this topic. This will change throughout the lifetime of the queue, but will
        ///     never be <see langword="null"/>.
        /// </param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="owner"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="owner"/> was zero-length or contains only white-space characters.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="topic"/> was <see langword="null"/>.</exception>
        [JsonConstructor]
        public TopicAffinity([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] string owner, [NotNull] Topic topic)
        {
            owner.Validate(nameof(owner), StringIs.NotNullEmptyOrWhiteSpace);
            reliableQueueKey.Validate(nameof(ReliableQueueKey), ObjectIs.NotNull);
            topic.Validate(nameof(topic), ObjectIs.NotNull);

            ReliableQueueKey = reliableQueueKey;
            Owner = owner;
            Topic = topic;

            PartitionKey = reliableQueueKey.Identifier;
            RowKey = Topic.Identifier;
        }

        /// <summary>
        ///     Get or set the UTC date/time at which the topic ownership last changed state.  This will change throughout the lifetime of the topic
        ///     ownership.
        /// </summary>
        /// <value>The UTC date/time at which the topic ownership last changed state.  This will change throughout the lifetime of the topic ownership.</value>
        public DateTime LastUpdated { get; set; } = DefaultDateTime;

        /// <summary>Gets or sets the key identifying the reliable queue on which the topic is being processed.  This is fixed at creation.</summary>
        /// <value>The key identifying the reliable queue on which the topic is being processed.  This is fixed at creation.</value>
        [NotNull]
        [JsonConverter(typeof(ReliableQueueKeyConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.ReliableQueueKeyConverter))]
        public ReliableQueueKey ReliableQueueKey { get; set; }

        /// <summary>
        ///     Get or set the ID of the endpoint that most recently processed messages for this topic. This will change throughout the lifetime of the message,
        ///     but will never be <see langword="null"/>.
        /// </summary>
        /// <value>
        ///     The ID of the endpoint that most recently processed messages for this topic. This will change throughout the lifetime of the message, but will
        ///     never be <see langword="null"/>.
        /// </value>
        [NotNull]
        public string Owner { get; set; }

        /// <summary>
        ///     Get or set a key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This
        ///     is fixed at creation.
        /// </summary>
        /// <value>
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters.
        /// </value>
        [NotNull]
        [JsonConverter(typeof(TopicConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.TopicConverter))]
        public Topic Topic { get; set; }

        /// <summary>
        ///     Creates a new topic affinity, with the <see cref="Topic"/> and <see cref="ReliableQueueKey"/> set from the arguments given, and the
        ///     <see cref="Owner"/> property set to the current host identity, <see cref="Identity.Current"/>.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the topic affinity.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <returns>
        ///     A new topic affinity, with the <see cref="Topic"/> and <see cref="ReliableQueueKey"/> set from the arguments given, and the <see cref="Owner"/>
        ///     property set to the current host identity, <see cref="Identity.Current"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        public static TopicAffinity CreateNew([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] Topic topic)
        {
            reliableQueueKey.Validate(nameof(reliableQueueKey), ObjectIs.NotNull);
            topic.Validate(nameof(topic), ObjectIs.NotNull);

            var identity = Identity.Current;

            return new TopicAffinity(reliableQueueKey, identity, topic);
        }
    }
}