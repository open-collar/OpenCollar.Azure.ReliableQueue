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

namespace OpenCollar.Azure.ReliableQueue.Model
{
    using System;
    using System.Collections.Generic;

    using global::Newtonsoft.Json;

    using JetBrains.Annotations;

    using OpenCollar.Azure.ReliableQueue.Model.Newtonsoft;

    /// <summary>
    ///     Defines the <see cref="Topic" />.
    /// </summary>
    [JsonConverter(typeof(TopicConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.TopicConverter))]
    public sealed class Topic : IEquatable<Topic>, IComparable<Topic>, IComparable
    {
        /// <summary>
        ///     Defines the DefaultIdentifier.
        /// </summary>
        [NotNull]
        public const string DefaultIdentifier = "__default__";

        /// <summary>
        ///     Defines the Default.
        /// </summary>
        [NotNull]
        public static readonly Topic Default = new Topic(null);

        /// <summary>
        ///     Defines the _identifier.
        /// </summary>
        [NotNull]
        private readonly string _identifier;

        /// <summary>
        ///     The key that uniquely identifies the reliable queue in the configuration. The key is case insensitive.
        ///     The key must not be <see langword="null" />, zero-length or contain only white-space characters..
        /// </summary>
        [CanBeNull]
        private readonly string? _key;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Topic" /> class.
        /// </summary>
        /// <param name="key">
        ///     The key <see cref="string" />.
        /// </param>
        [JsonConstructor]
        public Topic([CanBeNull] string? key)
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
#pragma warning disable CS8601 // Possible null reference assignment.
                _identifier = Identifiers.MakeSafe(_key.ToLowerInvariant());
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CA1308 // Normalize strings to uppercase
            }
        }

        /// <summary>
        ///     Gets the Identifier.
        /// </summary>
        [NotNull]
        public string Identifier => _identifier;

        /// <summary>
        ///     Gets a value indicating whether IsEmpty.
        /// </summary>
        public bool IsEmpty => _key is null;

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="obj">
        ///     An object to compare with this instance.
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
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
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        ///     other object.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this instance.
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
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

        /// <summary>
        ///     The Equals.
        /// </summary>
        /// <param name="obj">
        ///     The object to compare with the current object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is Topic other && Equals(other);

        /// <summary>
        ///     The Equals.
        /// </summary>
        /// <param name="other">
        ///     An object to compare with this object.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter;
        ///     otherwise, <see langword="false" />.
        /// </returns>
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

        /// <summary>
        ///     The GetHashCode.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_identifier);

        /// <summary>
        ///     The ToString.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string? ToString() => _key;

        /// <summary>
        ///     Returns a value that indicates whether the values of two
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> objects are equal.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the <paramref name="left" /> and <paramref name="right" /> parameters have
        ///     the same value; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(Topic left, Topic right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value
        ///     is greater than another <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value
        ///     is greater than or equal to another <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >=(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) >= 0;

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Topic" /> to <see cref="string" />.
        /// </summary>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        [CanBeNull]
        public static implicit operator string?(Topic? value) => value?._key;

        /// <summary>
        ///     Performs an implicit conversion from <see cref="string" /> to <see cref="Topic" />.
        /// </summary>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        [NotNull]
        public static implicit operator Topic([CanBeNull] string? value) => value is null ? Default : new Topic(value);

        /// <summary>
        ///     Converts a <see cref="string" /> to a <see cref="Topic" />.
        /// </summary>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <returns>
        ///     The result of the conversion.
        /// </returns>
        public static Topic? ToTopic(string? value)
        {
            return value is null ? Default : new Topic(value);
        }

        /// <summary>
        ///     Returns a value that indicates whether two <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" />
        ///     objects have different values.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(Topic left, Topic right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value
        ///     is less than another <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value
        ///     is less than or equal to another <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic" /> value.
        /// </summary>
        /// <param name="left">
        ///     The first value to compare.
        /// </param>
        /// <param name="right">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <=(Topic left, Topic right) => Comparer<Topic>.Default.Compare(left, right) <= 0;
    }
}