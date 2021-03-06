﻿/*
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

namespace OpenCollar.Azure.ReliableQueue.Model.Newtonsoft
{
    using System;

    using JetBrains.Annotations;

    using global::Newtonsoft.Json;

    using OpenCollar.Extensions.Validation;

    /// <summary>
    /// Defines the <see cref="TopicConverter" />.
    /// </summary>
    public sealed class TopicConverter : JsonConverter<Topic?>
    {
        /// <summary>
        /// Gets the Instance.
        /// </summary>
        [NotNull]
        public static TopicConverter Instance { get; } = new TopicConverter();

        /// <summary>
        /// The ReadJson.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <see langword="null"/> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override Topic? ReadJson(JsonReader reader, Type objectType, Topic? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            reader.Validate(nameof(reader), ObjectIs.NotNull);

            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            return reader.TokenType == JsonToken.String
                ? new Topic(reader.Value as string)
                : throw new JsonSerializationException("Unexpected token type.  Expected token type 'String'.");
        }

        /// <summary>
        /// The WriteJson.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, Topic? value, JsonSerializer serializer)
        {
            writer.Validate(nameof(writer), ObjectIs.NotNull);

            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
