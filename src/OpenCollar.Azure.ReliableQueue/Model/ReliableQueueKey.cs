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

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Model
{
    /// <summary>Represents a key that can be used to identify a message key.</summary>
    /// <seealso cref="System.IEquatable{T}"/>
    /// <seealso cref="System.IComparable{T}"/>
    /// <seealso cref="System.IComparable"/>
    public sealed class ReliableQueueKey : IEquatable<ReliableQueueKey>, IComparable<ReliableQueueKey>, IComparable
    {
        /// <summary>The safe identifier created from the key for use in Azure.</summary>
        [NotNull]
        private readonly string _identifier;

        /// <summary>
        ///     The key that uniquely identifies the reliable queue in the configuration.  The key is case insensitive.  The key must not be
        ///     <see langword="null"/>, zero-length or contain only white-space characters.
        /// </summary>
        [NotNull]
        private readonly string _key;

        /// <summary>Initializes a new instance of the <see cref="ReliableQueueKey"/> class.</summary>
        /// <param name="key">
        ///     The key that uniquely identifies the reliable queue in the configuration.  The key is case insensitive.  The key must not be
        ///     <see langword="null"/>, zero-length or contain only white-space characters.
        /// </param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="key"/> was zero-length or contains only white-space characters.</exception>
        public ReliableQueueKey([NotNull] string key)
        {
            key.Validate(nameof(key), StringIs.NotNullEmptyOrWhiteSpace);
            _key = key;
            _identifier = Identifiers.MakeSafe(_key);
            TableIdentifier = Identifiers.MakeTableSafe(_key);
        }

        /// <summary>Gets the safe identifier created from the key for use in Azure.</summary>
        /// <value>The safe identifier created from the key for use in Azure.</value>
        [NotNull]
        public string Identifier => _identifier;

        /// <summary>Gets the safe identifier suitable for use in table names created from the key for use in Azure.</summary>
        /// <value>The safe identifier suitable for use in table names created from the key for use in Azure.</value>
        [NotNull]
        public string TableIdentifier { get; }

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

            return obj is ReliableQueueKey other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ReliableQueueKey)}");
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
        public int CompareTo(ReliableQueueKey other)
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
        public bool Equals(ReliableQueueKey other)
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
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is ReliableQueueKey other && Equals(other);

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_identifier);

        /// <summary>Returns a value that indicates whether the values of two <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ReliableQueueKey left, ReliableQueueKey right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value is greater than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(ReliableQueueKey left, ReliableQueueKey right) => Comparer<ReliableQueueKey>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value is greater than or equal to
        ///     another <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(ReliableQueueKey left, ReliableQueueKey right) => Comparer<ReliableQueueKey>.Default.Compare(left, right) >= 0;

        /// <summary>Performs an implicit conversion from <see cref="ReliableQueueKey"/> to <see cref="string"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull")]
        public static implicit operator string(ReliableQueueKey value) => value?._key;

        /// <summary>Performs an implicit conversion from <see cref="string"/> to <see cref="ReliableQueueKey"/>.</summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [ContractAnnotation("null=>null;notnull=>notnull")]
        public static implicit operator ReliableQueueKey(string value) => value is null ? null : new ReliableQueueKey(value);

        /// <summary>Returns a value that indicates whether two <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ReliableQueueKey left, ReliableQueueKey right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value is less than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(ReliableQueueKey left, ReliableQueueKey right) => Comparer<ReliableQueueKey>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value is less than or equal to another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.ReliableQueueKey"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(ReliableQueueKey left, ReliableQueueKey right) => Comparer<ReliableQueueKey>.Default.Compare(left, right) <= 0;

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => _key;
    }
}