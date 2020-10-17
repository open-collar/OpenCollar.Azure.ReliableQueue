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

using OpenCollar.Azure.ReliableQueue.Model;

namespace OpenCollar.Azure.ReliableQueue
{
    /// <summary>Utilities supporting identifiers.</summary>
    internal static class Identifiers
    {
        /// <summary>The name of the table in which sequence data is held.</summary>
        [NotNull]
        public const string SequenceTableName = @"ReliableQueueSequence";

        /// <summary>Gets the name of the message container.</summary>
        /// <param name="reliableQueueKey">The reliable queue key for which to get the message container name.</param>
        /// <returns>The name of the message container.</returns>
        [NotNull]
        public static string GetMessageContainerName([NotNull] ReliableQueueKey reliableQueueKey) => @"reliable-queue-body-" + reliableQueueKey.Identifier;

        /// <summary>Gets the name of the reliable queue.</summary>
        /// <param name="reliableQueueKey">The reliable queue key.</param>
        /// <returns></returns>
        public static string GetReliableQueueName([NotNull] ReliableQueueKey reliableQueueKey) => @"reliable-queue-" + reliableQueueKey.Identifier;

        /// <summary>Gets the name of the state table.</summary>
        /// <param name="reliableQueueKey">The reliable queue key for which to get the state table name.</param>
        /// <returns>The name of the state table.</returns>
        [NotNull]
        public static string GetStateTableName([NotNull] ReliableQueueKey reliableQueueKey) => @"ReliableQueueState" + reliableQueueKey.TableIdentifier;

        /// <summary>Gets the name of the topic table.</summary>
        /// <param name="reliableQueueKey">The reliable queue key for which to get the topic table name.</param>
        /// <returns>The name of the topic table.</returns>
        [NotNull]
        public static string GetTopicTableName([NotNull] ReliableQueueKey reliableQueueKey) => @"ReliableQueueTopic" + reliableQueueKey.TableIdentifier;

        /// <summary>Replaces all unsafe characters with hyphens.</summary>
        /// <param name="name">The identifier in which the characters are to be replaced.</param>
        /// <returns>Returns a string in which all non-alphanumeric characters are replaced with hyphens and all alphabetic characters are made lower-case.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull;")]
        public static string? MakeSafe([CanBeNull] string name)
        {
            if(ReferenceEquals(name, null))
            {
                return null;
            }

            if(name.Length <= 0)
            {
                return string.Empty;
            }

            var newString = new char[name.Length];
            var n = 0;
            foreach(var c in name)
            {
                if(char.IsLetterOrDigit(c))
                {
                    newString[n++] = char.ToLowerInvariant(c);
                }
                else
                {
                    newString[n++] = '-';
                }
            }

            return new string(newString);
        }

        /// <summary>Replaces all unsafe characters with xs and capitalizes the first character in each name.</summary>
        /// <param name="name">The identifier in which the characters are to be replaced.</param>
        /// <returns>Returns a string in which all non-alphanumeric characters are replaced with xs and all alphabetic characters are made lower-case.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull;")]
        public static string? MakeTableSafe([CanBeNull] string name)
        {
            if(ReferenceEquals(name, null))
            {
                return null;
            }

            if(name.Length <= 0)
            {
                return string.Empty;
            }

            var toUpper = true;
            var newString = new char[name.Length];
            var n = 0;
            foreach(var c in name)
            {
                if(char.IsLetterOrDigit(c))
                {
                    if(toUpper)
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
                    newString[n++] = 'x';
                }
            }

            return new string(newString);
        }
    }
}