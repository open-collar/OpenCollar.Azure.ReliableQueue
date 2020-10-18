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

    using OpenCollar.Azure.ReliableQueue.Model;

    /// <summary>
    /// Defines the <see cref="Identifiers" />.
    /// </summary>
    internal static class Identifiers
    {
        /// <summary>
        /// Defines the SafeDelimiter.
        /// </summary>
        public const char SafeDelimiter = '-';

        /// <summary>
        /// Defines the SequenceTableName.
        /// </summary>
        [NotNull]
        public const string SequenceTableName = @"ReliableQueueSequence";

        /// <summary>
        /// Defines the TableSafeDelimiter.
        /// </summary>
        public const char TableSafeDelimiter = 'x';

        /// <summary>
        /// The GetMessageContainerName.
        /// </summary>
        /// <param name="queueKey">The reliable queue key for which to get the message container name.</param>
        /// <returns>The name of the message container.</returns>
        [NotNull]
        public static string GetMessageContainerName([NotNull] QueueKey queueKey) => @"reliable-queue-body-" + queueKey.Identifier;

        /// <summary>
        /// The GetReliableQueueName.
        /// </summary>
        /// <param name="queueKey">The reliable queue key.</param>
        /// <returns>.</returns>
        public static string GetReliableQueueName([NotNull] QueueKey queueKey) => @"reliable-queue-" + queueKey.Identifier;

        /// <summary>
        /// The GetStateTableName.
        /// </summary>
        /// <param name="queueKey">The reliable queue key for which to get the state table name.</param>
        /// <returns>The name of the state table.</returns>
        [NotNull]
        public static string GetStateTableName([NotNull] QueueKey queueKey) => @"ReliableQueueState" + queueKey.TableIdentifier;

        /// <summary>
        /// The GetTopicTableName.
        /// </summary>
        /// <param name="queueKey">The reliable queue key for which to get the topic table name.</param>
        /// <returns>The name of the topic table.</returns>
        [NotNull]
        public static string GetTopicTableName([NotNull] QueueKey queueKey) => @"ReliableQueueTopic" + queueKey.TableIdentifier;

        /// <summary>
        /// The MakeSafe.
        /// </summary>
        /// <param name="name">The identifier in which the characters are to be replaced.</param>
        /// <returns>Returns a string in which all non-alphanumeric characters are replaced with hyphens and all alphabetic characters are made lower-case.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull;")]
        public static string? MakeSafe([CanBeNull] string name)
        {
            if (ReferenceEquals(name, null))
            {
                return null;
            }

            if (name.Length <= 0)
            {
                return string.Empty;
            }

            var newString = new char[name.Length];
            var n = 0;
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    newString[n++] = char.ToLowerInvariant(c);
                }
                else
                {
                    newString[n++] = SafeDelimiter;
                }
            }

            return new string(newString);
        }

        /// <summary>
        /// The MakeTableSafe.
        /// </summary>
        /// <param name="name">The identifier in which the characters are to be replaced.</param>
        /// <returns>Returns a string in which all non-alphanumeric characters are replaced with xs and all alphabetic characters are made lower-case.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull;")]
        public static string? MakeTableSafe([CanBeNull] string name)
        {
            if (ReferenceEquals(name, null))
            {
                return null;
            }

            if (name.Length <= 0)
            {
                return string.Empty;
            }

            var toUpper = true;
            var newString = new char[name.Length];
            var n = 0;
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (toUpper)
                    {
                        toUpper = false;
                        newString[n++] = char.ToUpperInvariant(c);
                    }
                    else
                    {
                        newString[n++] = char.ToLowerInvariant(c);
                    }
                }
                else
                {
                    toUpper = true;
                    newString[n++] = TableSafeDelimiter;
                }
            }

            return new string(newString);
        }
    }
}
