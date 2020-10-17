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
using System.Collections.Generic;

using JetBrains.Annotations;

using Newtonsoft.Json;

using OpenCollar.Azure.ReliableQueue.Model.Newtonsoft;

namespace OpenCollar.Azure.ReliableQueue.Model
{
    /// <summary>Represents a key that can be used to identify a message topic.</summary>
    /// <seealso cref="System.IEquatable{T}"/>
    /// <seealso cref="System.IComparable{T}"/>
    /// <seealso cref="System.IComparable"/>
    [JsonConverter(typeof(TopicConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.TopicConverter))]
    public sealed class Topic : IEquatable<Topic>, IComparable<Topic>, IComparable
    {
        /// <summary>The default topic that will be used when the one specified is <see langword="null"/>, zero-length or contains only white-space characters.</summary>
        [NotNull]
        private const string DefaultIdentifier = "__default__";

        /// <summary>The default (empty) topic.</summary>
        [NotNull]
        public static readonly Topic Default = new Topic(DefaultIdentifier);

        /// <summary>The safe identifier created from the key for use in Azure.</summary>
        [NotNull]
        private readonly string _identifier;

        /// <summary>
        ///     The key that uniquely identifies the reliable queue in the configuration.  The key is case insensitive.  The key must not be
        ///     <see langword="null"/>, zero-length or contain only white-space characters.
        /// </summary>
        [CanBeNull]
        private readonly string? _key;

        /// <summary>Initializes a new instance of the <see cref="Topic"/> class.</summary>
        /// <param name="key">
        ///     The key that uniquely identifies the reliable queue in the configuration.  The key is case insensitive.  The key must not be
        ///     <see langword="null"/>, zero-length or contain only white-space characters.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="key"/> was zero-length or contains only white-space characters.</exception>
        [JsonConstructor]
        public Topic([CanBeNull] string key)
        {
            if(string.IsNullOrWhiteSpace(key))
            {
                _key = null;
                _identifier = DefaultIdentifier;
            }
            else
            {
                _key = key;
#pragma warning disable CA1308 // Normalize strings to uppercase
                _identifier = Identifiers.MakeSafe(_key.ToLowerInvariant());
#pragma warning restore CA1308 // Normalize strings to uppercase
            }
        }

        /// <summary>Gets the safe identifier created from the key for use in Azure.</summary>
        /// <value>The safe identifier created from the key for use in Azure.</value>
        [NotNull]
        public string Identifier => _identifier;

        /// <summary>Gets a value indicating whether this topic has been set.</summary>
        /// <value><see langword="true"/> if this topic has not been set; otherwise, <see langword="false"/> if the topic has not been set.</value>
        public bool IsEmpty => _key is null;

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes,
        ///     follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader><term>Value</term> <description>Meaning</description></listheader>
        ///         <item><term>Less than zero</term> <description>This instance precedes <paramref name="obj"/> in the sort order.</description></item>
        ///         <item><term>Zero</term> <description>This instance occurs in the same position in the sort order as <paramref name="obj"/>.</description></item>
        ///         <item><term>Greater than zero</term> <description>This instance follows <paramref name="obj"/> in the sort order.</description></item>
        ///     </list>
        /// </returns>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
        public int CompareTo(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return 1;
            }

            if(ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is Topic other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Topic)}");
        }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes,
        ///     follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader><term>Value</term> <description>Meaning</description></listheader>
        ///         <item><term>Less than zero</term> <description>This instance precedes <paramref name="other"/> in the sort order.</description></item>
        ///         <item><term>Zero</term> <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description></item>
        ///         <item><term>Greater than zero</term> <description>This instance follows <paramref name="other"/> in the sort order.</description></item>
        ///     </list>
        /// </returns>
        public int CompareTo(Topic other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }

            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(_identifier, other._identifier, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Topic other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(_identifier, other._identifier, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object  is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Topic other && Equals(other);

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_identifier);

        /// <summary>Returns a value that indicates whether the values of two <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Topic left, Topic right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value is greater than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value is greater than or equal to another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) >= 0;

        /// <summary>Performs an implicit conversion from <see cref="Topic"/> to <see cref="string"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CanBeNull]
        public static implicit operator string(Topic value) => value?._key;

        /// <summary>Performs an implicit conversion from <see cref="string"/> to <see cref="Topic"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [NotNull]
        public static implicit operator Topic([CanBeNull] string value) => value is null ? Default : new Topic(value);

        /// <summary>Returns a value that indicates whether two <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Topic left, Topic right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value is less than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value is less than or equal to another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) <= 0;

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => _key;
    }
}