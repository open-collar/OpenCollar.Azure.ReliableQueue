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

using JetBrains.Annotations;

using Newtonsoft.Json;

using OpenCollar.Extensions.Validation;

#nullable enable

namespace OpenCollar.Azure.ReliableQueue.Model.Newtonsoft
{
    /// <summary>A converter that ensures that reliable queue topics are represented in the correct format.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/>
    public sealed class TopicConverter : JsonConverter<Topic>
    {
        /// <summary>Gets a common instance of the converter that can be reused as necessary.</summary>
        /// <value>A common instance of the converter that can be reused as necessary.</value>
        [NotNull]
        public static TopicConverter Instance { get; } = new TopicConverter();

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <see langword="null"/> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reader"/> is <see langword="null"/>.</exception>
        public override Topic? ReadJson(JsonReader reader, Type objectType, Topic existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            reader.Validate(nameof(reader), ObjectIs.NotNull);

            if(reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            return reader.TokenType == JsonToken.String
                ? new Topic(reader.Value as string)
                : throw new JsonSerializationException("Unexpected token type.  Expected token type 'String'.");
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="writer"/> is <see langword="null"/>.</exception>
        public override void WriteJson(JsonWriter writer, Topic value, JsonSerializer serializer)
        {
            writer.Validate(nameof(writer), ObjectIs.NotNull);

            writer.WriteValue(value.ToString());
        }
    }
}