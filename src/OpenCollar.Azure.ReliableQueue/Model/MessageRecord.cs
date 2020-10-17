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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

using JetBrains.Annotations;

using Microsoft.Azure.Cosmos.Table;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using OpenCollar.Azure.ReliableQueue.Configuration;
using OpenCollar.Azure.ReliableQueue.Model.Newtonsoft;
using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Model
{
    /// <summary>A class representing a message recorded in a storage table.</summary>
    /// <seealso cref="Microsoft.Azure.Cosmos.Table.TableEntity"/>
    [DebuggerDisplay("Queue: {ReliableQueueKey,nq}; Topic: {Topic,nq}; Sequence: {Sequence}.")]
    internal sealed class MessageRecord : TableEntity, IComparable<MessageRecord>
    {
        /// <summary>The current local sequence index.</summary>
        private static int _currentLocalSequence;

        /// <summary>The default value used in <see cref="DateTime"/> fields to prevent Azure Tables from throwing out-of-range errors.</summary>
        public static readonly DateTime DefaultDateTime = new DateTime(1970, 1, 1);

        /// <summary>
        ///     The sequence number of the message. This is used to determine the order of messages when being delivered and processed.  This is fixed the first
        ///     time the message is saved.  It is populated from the <see cref="TableEntity.Timestamp"/> property, using the
        ///     <see cref="DateTimeOffset.UtcTicks"/> property.
        /// </summary>
        private long _sequence;

        /// <summary>Initializes a new instance of the <see cref="MessageRecord"/> class.</summary>
        public MessageRecord()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MessageRecord"/> class.</summary>
        /// <param name="reliableQueueKey">The key identifying the queue to which the message belongs.</param>
        /// <param name="id">A GUID uniquely identifying the message. This is fixed at creation.</param>
        /// <param name="owner">
        ///     The ID of the endpoint currently processing the message (sender or receiver). This will change throughout the lifetime of the message, but will
        ///     never be <see langword="null"/>.
        /// </param>
        /// <param name="source">The ID of the endpoint of which the message was created. This is fixed at creation.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="owner"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="owner"/> was zero-length or contains only white-space characters.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="source"/> was <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="source"/> was zero-length or contains only white-space characters.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="reliableQueueKey"/> was <see langword="null"/>.</exception>
        [JsonConstructor]
        public MessageRecord([NotNull] ReliableQueueKey reliableQueueKey, Guid id, [NotNull] string owner, [NotNull] string source,
            [CanBeNull] Topic? topic = null)
        {
            if(id == Guid.Empty)
            {
                throw new ArgumentException("'id' is empty.", nameof(id));
            }

            owner.Validate(nameof(owner), StringIs.NotNullEmptyOrWhiteSpace);
            source.Validate(nameof(source), StringIs.NotNullEmptyOrWhiteSpace);
            reliableQueueKey.Validate(nameof(ReliableQueueKey), ObjectIs.NotNull);

            ReliableQueueKey = reliableQueueKey;
            Id = id;
            Owner = owner;
            Source = source;
            Topic = topic ?? Topic.Default;
            MessageState = MessageState.New;

            PartitionKey = Topic.Identifier;
            RowKey = Id.ToString("D", CultureInfo.InvariantCulture);
        }

        /// <summary>Get or set the number of times there has been an attempt to deliver this message. This will change throughout the lifetime of the message.</summary>
        /// <value>The number of times there has been an attempt to deliver this message. This will change throughout the lifetime of the message.</value>
        public int Attempts { get; set; }

        /// <summary>Gets or sets a value indicating whether the body is <see langword="null"/> (and so there is no BLOB stored for the body).</summary>
        /// <value>
        ///     <see langword="true"/> if body is <see langword="null"/> (and so there is no BLOB stored for the body); otherwise, <see langword="false"/> if
        ///     there is a BLOB stored representing the body of the message.
        /// </value>
        public bool BodyIsNull { get; set; }

        /// <summary>Get or set the UTC date/time at which the message was enqueued.  This is fixed at creation.</summary>
        /// <value>The UTC date/time at which the message was enqueued.  This is fixed at creation.</value>
        public DateTime Created { get; set; } = DefaultDateTime;

        /// <summary>Get or set a GUID uniquely identifying the message. This is fixed at creation.</summary>
        /// <value>A GUID uniquely identifying the message. This is fixed at creation.</value>
        public Guid Id { get; set; }

        /// <summary>Get or set the UTC date/time at which the message last changed state.  This will change throughout the lifetime of the message.</summary>
        /// <value>The UTC date/time at which the message last changed state.  This will change throughout the lifetime of the message.</value>
        public DateTime LastUpdated { get; set; } = DefaultDateTime;

        /// <summary>
        ///     Get or set the local sequence number of the message. This is used to determine the order of messages from the same source when being delivered
        ///     and processed.  This is fixed at creation.
        /// </summary>
        /// <value>
        ///     The local sequence number of the message. This is used to determine the order of messages from the same source when being delivered and
        ///     processed.  This is fixed at creation.
        /// </value>
        public int LocalSequence { get; set; }

        /// <summary>
        ///     Get or set the maximum number of delivery attempts permitted before the message is moved to the poison reliable queue. This is fixed at
        ///     creation.
        /// </summary>
        /// <value>The maximum number of delivery attempts permitted before the message is moved to the poison reliable queue. This is fixed at creation.</value>
        public int MaxAttempts { get; set; }

        /// <summary>Gets or sets the key identifying the reliable queue on which the message is to be sent.  This is fixed at creation.</summary>
        /// <value>The key identifying the reliable queue on which the message is to be sent.  This is fixed at creation.</value>
        [NotNull]
        [global::Newtonsoft.Json.JsonConverter(typeof(ReliableQueueKeyConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.ReliableQueueKeyConverter))]
        public ReliableQueueKey ReliableQueueKey { get; set; }

        /// <summary>Get or set the current state of the message. This will change throughout the lifetime of the message.</summary>
        /// <value>The current state of the message. This will change throughout the lifetime of the message.</value>
        [global::Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public MessageState MessageState { get; set; }

        /// <summary>
        ///     Get or set the ID of the endpoint currently processing the message (sender or receiver). This will change throughout the lifetime of the message,
        ///     but will never be <see langword="null"/>.
        /// </summary>
        /// <value>
        ///     The ID of the endpoint currently processing the message (sender or receiver). This will change throughout the lifetime of the message, but will
        ///     never be <see langword="null"/>.
        /// </value>
        [NotNull]
        public string Owner { get; set; }

        /// <summary>
        ///     Get or set the period of time permitted for an individual message to process the message before it is returned to the reliable queue. This is
        ///     fixed at creation.
        /// </summary>
        /// <value>
        ///     The period of time permitted for an individual message to process the message before it is returned to the reliable queue. This is fixed at
        ///     creation.
        /// </value>
        public TimeSpan Processing { get; set; }

        /// <summary>
        ///     Get or set the sequence number of the message. This is used to determine the order of messages when being delivered and processed.  This is fixed
        ///     at creation.
        /// </summary>
        /// <value>
        ///     The sequence number of the message. This is used to determine the order of messages when being delivered and processed.  This is fixed at
        ///     creation.
        /// </value>
        public long Sequence
        {
            get
            {
                if(_sequence == 0)
                {
                    _sequence = Timestamp.UtcTicks;
                }

                return _sequence;
            }
            set => _sequence = value;
        }

        /// <summary>Gets or sets the size of the message, measured in bytes.</summary>
        /// <value>The size of the message, in bytes, or <see langword="null"/> if the message is empty or not yet been supplied.</value>
        public long? Size { get; set; }

        /// <summary>Get or set the ID of the endpoint of which the message was created. This is fixed at creation.</summary>
        /// <value>The ID of the endpoint of which the message was created. This is fixed at creation.</value>
        [NotNull]
        public string Source { get; set; }

        /// <summary>
        ///     Get or set the period of time permitted before the message must be either processed or moved to the poison reliable queue. This is fixed at
        ///     creation.
        /// </summary>
        /// <value>The period of time permitted before the message must be either processed or moved to the poison reliable queue. This is fixed at creation.</value>
        public TimeSpan Timeout { get; set; }

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
        [global::Newtonsoft.Json.JsonConverter(typeof(TopicConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(Text.Json.TopicConverter))]
        public Topic Topic { get; set; }

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
        public int CompareTo(MessageRecord other)
        {
            if(ReferenceEquals(this, other))
            {
                return 0;
            }

            if(ReferenceEquals(null, other))
            {
                return 1;
            }

            if(string.Equals(Source, other.Source, StringComparison.Ordinal))
            {
                return LocalSequence.CompareTo(other.LocalSequence);
            }

            return Sequence.CompareTo(other.Sequence);
        }

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

            return obj is MessageRecord other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MessageRecord)}");
        }

        /// <summary>
        ///     Creates a new message, with a new <see cref="Id"/> and the <see cref="Owner"/> and <see cref="Source"/> properties set to the current host
        ///     identity, <see cref="Identity.Current"/>.
        /// </summary>
        /// <param name="reliableQueueKey">The key identifying the reliable queue for which to create the message.</param>
        /// <param name="configuration">The configuration for the queue on which the message will be sent.</param>
        /// <param name="topic">
        ///     A key used to identify messages that are related to one-another.  These are guaranteed to be delivered sequentially and in order. This is fixed
        ///     at creation.  The value in <see cref="Model.Topic.Default"/> will be used if the one specified is <see langword="null"/>, zero-length or contains
        ///     only white-space characters
        /// </param>
        /// <returns>
        ///     A new message, with a new <see cref="Id"/> and the <see cref="Owner"/> and <see cref="Source"/> properties set to the current host identity,
        ///     <see cref="Identity.Current"/>.
        /// </returns>
        public static MessageRecord CreateNew([NotNull] ReliableQueueKey reliableQueueKey, [NotNull] IReliableQueueConfiguration configuration,
            [CanBeNull] Topic? topic = null)
        {
            var localSequence = GetLocalSequence();

            var identity = Identity.Current;

            var message = new MessageRecord(reliableQueueKey, Guid.NewGuid(), identity, identity, topic)
            {
                MaxAttempts = configuration.MaxAttempts,
                LocalSequence = localSequence,
                Processing = TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds),
                Timeout = TimeSpan.FromSeconds(configuration.DefaultTimeoutSeconds)
            };

            return message;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MessageRecord other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object  is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is MessageRecord other && Equals(other);

        /// <summary>Converts from the JSON representation of a message record into a fully instantiated object.</summary>
        /// <param name="json">The JSON representation of the message record to instantiate.</param>
        /// <returns>
        ///     The fully instantiate message record or <see langword="null"/> if <paramref name="json"/> was <see langword="null"/>, zero-length or contained
        ///     only white space characters.
        /// </returns>
        [CanBeNull]
        public static MessageRecord FromJson([CanBeNull] string json)
        {
            if(string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var serializer = JsonSerializer.CreateDefault();

            using var stringReader = new StringReader(json);
            using var reader = new JsonTextReader(stringReader);

            return serializer.Deserialize<MessageRecord>(reader);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

        /// <summary>Gets the next local sequence value.</summary>
        /// <returns>The next local sequence value.</returns>
        public static int GetLocalSequence() => Interlocked.Increment(ref _currentLocalSequence);

        /// <summary>Returns a value that indicates whether the values of two <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise,
        ///     <see langword="false"/>.
        /// </returns>
        public static bool operator ==(MessageRecord left, MessageRecord right) => Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value is greater than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(MessageRecord left, MessageRecord right) => Comparer<MessageRecord>.Default.Compare(left, right) > 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value is greater than or equal to another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(MessageRecord left, MessageRecord right) => Comparer<MessageRecord>.Default.Compare(left, right) >= 0;

        /// <summary>Returns a value that indicates whether two <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(MessageRecord left, MessageRecord right) => !Equals(left, right);

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value is less than another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(MessageRecord left, MessageRecord right) => Comparer<MessageRecord>.Default.Compare(left, right) < 0;

        /// <summary>
        ///     Returns a value that indicates whether a <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value is less than or equal to another
        ///     <see cref="OpenCollar.Azure.ReliableQueue.Model.MessageRecord"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(MessageRecord left, MessageRecord right) => Comparer<MessageRecord>.Default.Compare(left, right) <= 0;

        /// <summary>Converts to the current message record into a JSON representation.</summary>
        /// <returns>The JSON representation of this instance.</returns>
        [NotNull]
        public string ToJson()
        {
            var serializer = JsonSerializer.CreateDefault();

            using var stringWriter = new StringWriter();
            using var writer = new JsonTextWriter(stringWriter);

            serializer.Serialize(writer, this);

            return stringWriter.ToString();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Id.ToString("D", CultureInfo.InvariantCulture);
    }
}