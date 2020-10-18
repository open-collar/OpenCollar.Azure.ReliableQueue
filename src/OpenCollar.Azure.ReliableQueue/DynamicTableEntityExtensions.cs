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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using JetBrains.Annotations;

    using Microsoft.Azure.Cosmos.Table;

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Extension methods to simplify the conversion of <see cref="TableEntity"/> objects with complex properties to values that can be serialized, and
    ///     vice-versa.
    /// </summary>
    internal static class DynamicTableEntityExtensions
    {
        /// <summary>
        /// Defines the _skipProperties.
        /// </summary>
        [NotNull]
        private static readonly Dictionary<Type, Dictionary<string, string>> _skipProperties = new Dictionary<Type, Dictionary<string, string>>
        {
            {
                typeof(Message), new Dictionary<string, string>
                {
                    { nameof(Message.QueueKey), nameof(Message.QueueKey) },
                    { nameof(Message.Topic), nameof(Message.Topic) },
                    { nameof(Message.Sequence), nameof(Message.Sequence) }
                }
            },
            {
                typeof(TopicAffinity), new Dictionary<string, string>
                {
                    { nameof(TopicAffinity.QueueKey), nameof(TopicAffinity.QueueKey) },
                    { nameof(TopicAffinity.Topic), nameof(TopicAffinity.Topic) }
                }
            }
        };

        /// <summary>
        /// The Deserialize.
        /// </summary>
        /// <typeparam name="TTableEntity">The type of the table entity.</typeparam>
        /// <param name="dynamicTableEntity">The dynamic table entity to derserlaize.</param>
        /// <param name="context">The context in which the deserialization will occur.</param>
        /// <returns>The table entity represented by the dynamic table entity given.</returns>
        [NotNull]
        public static TTableEntity Deserialize<TTableEntity>([NotNull] this DynamicTableEntity dynamicTableEntity, [NotNull] OperationContext context)
            where TTableEntity : ITableEntity
        {
            if (!_skipProperties.TryGetValue(typeof(TTableEntity), out var skipProperties))
            {
                skipProperties = new Dictionary<string, string>();
            }

            // Remove the properties that cannot be converted, and pass the rest to the initializer.
            var properties = dynamicTableEntity.Properties.Where(p => !skipProperties.ContainsKey(p.Key)).ToDictionary(p => p.Key, p => p.Value);

            var entity = TableEntity.ConvertBack<TTableEntity>(properties, context);

            var messageRecord = entity as Message;
            if (!(messageRecord is null))
            {
                messageRecord.QueueKey = new QueueKey(dynamicTableEntity.Properties[nameof(Message.QueueKey)].StringValue);
                messageRecord.Topic = new Topic(dynamicTableEntity.Properties[nameof(Message.Topic)].StringValue);
                var sequence = dynamicTableEntity.Properties[nameof(Message.Sequence)].Int64Value;
                if (sequence.HasValue && sequence.Value != 0)
                {
                    messageRecord.Sequence = sequence.Value;
                }
                else
                {
                    messageRecord.Sequence = messageRecord.Timestamp.UtcTicks;
                }
            }
            else
            {
                var topicAffinityRecord = entity as TopicAffinity;
                if (!(topicAffinityRecord is null))
                {
                    topicAffinityRecord.QueueKey =
                        new QueueKey(dynamicTableEntity.Properties[nameof(TopicAffinity.QueueKey)].StringValue);
                    topicAffinityRecord.Topic = new Topic(dynamicTableEntity.Properties[nameof(TopicAffinity.Topic)].StringValue);
                }
            }

            entity.ETag = dynamicTableEntity.ETag;
            entity.RowKey = dynamicTableEntity.RowKey;
            entity.PartitionKey = dynamicTableEntity.PartitionKey;
            entity.Timestamp = dynamicTableEntity.Timestamp;
            return entity;
        }

        /// <summary>
        /// The Serialize.
        /// </summary>
        /// <param name="entity">The entity to convert.</param>
        /// <param name="context">The context in which to convert.</param>
        /// <returns>The resulting dynamic table entity.</returns>
        [NotNull]
        public static DynamicTableEntity Serialize([NotNull] this ITableEntity entity, [NotNull] OperationContext context)
        {
            var dynamicTableEntity = new DynamicTableEntity(entity.PartitionKey, entity.RowKey)
            {
                Properties = TableEntity.Flatten(entity, context)
            };

            var message = entity as Message;
            if (!(message is null))
            {
                dynamicTableEntity.Properties.Add(nameof(Message.QueueKey), new EntityProperty(message.QueueKey.ToString()));
                dynamicTableEntity.Properties.Add(nameof(Message.Topic), new EntityProperty(message.Topic.ToString()));
                if (dynamicTableEntity.Properties.ContainsKey(nameof(Message.Sequence)))
                {
                    dynamicTableEntity.Properties[nameof(Message.Sequence)].Int64Value = message.Sequence;
                }
                else
                {
                    dynamicTableEntity.Properties.Add(nameof(Message.Sequence),
                        new EntityProperty(message.Sequence.ToString("D", CultureInfo.InvariantCulture)));
                }
            }
            else
            {
                var topic = entity as TopicAffinity;
                if (!(topic is null))
                {
                    dynamicTableEntity.Properties.Add(nameof(TopicAffinity.QueueKey), new EntityProperty(topic.QueueKey.ToString()));
                    dynamicTableEntity.Properties.Add(nameof(TopicAffinity.Topic), new EntityProperty(topic.Topic.ToString()));
                }
            }

            dynamicTableEntity.ETag = entity.ETag;
            dynamicTableEntity.RowKey = entity.RowKey;
            dynamicTableEntity.PartitionKey = entity.PartitionKey;
            dynamicTableEntity.Timestamp = entity.Timestamp;

            return dynamicTableEntity;
        }
    }
}
