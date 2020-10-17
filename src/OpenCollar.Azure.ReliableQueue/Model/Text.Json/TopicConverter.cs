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
using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using OpenCollar.Extensions.Validation;

namespace OpenCollar.Azure.ReliableQueue.Model.Text.Json
{
    /// <summary>A converter that ensures that reliable queue topics are represented in the correct format.</summary>
    /// <seealso cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/>
    public sealed class TopicConverter : JsonConverter<Topic?>
    {
        /// <summary>Gets a common instance of the converter that can be reused as necessary.</summary>
        /// <value>A common instance of the converter that can be reused as necessary.</value>
        [NotNull]
        public static TopicConverter Instance { get; } = new TopicConverter();

        /// <summary>Reads and converts the JSON to type <see cref="OpenCollar.Azure.ReliableQueue.Model.Topic"/>.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override Topic? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return Topic.Default;
            }

            if(reader.TokenType == JsonTokenType.String)
            {
                return new Topic(reader.GetString());
            }

            throw new JsonException("Unexpected token type.  Expected token type 'String'.", null, null, reader.TokenStartIndex);
        }

        /// <summary>Writes a specified value as JSON.</summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, Topic? value, JsonSerializerOptions options)
        {
            writer.Validate(nameof(writer), ObjectIs.NotNull);

            if(value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}