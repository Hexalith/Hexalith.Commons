// <copyright file="StringValueJsonConverter.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Ruleless base converter for value types encoded as one JSON string.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public abstract class StringValueJsonConverter<T> : JsonConverter<T>
    where T : notnull
{
    /// <inheritdoc/>
    public sealed override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"{typeToConvert.Name} must be encoded as a JSON string.");
        }

        string raw = reader.GetString() ?? throw new JsonException($"{typeToConvert.Name} cannot be null.");
        try
        {
            return Create(raw);
        }
        catch (ArgumentException)
        {
            throw new JsonException($"{typeToConvert.Name} payload is malformed.");
        }
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(GetValue(value));

    /// <summary>
    /// Creates the target value from its string payload.
    /// </summary>
    /// <param name="value">The JSON string payload.</param>
    /// <returns>The converted value.</returns>
    protected abstract T Create(string value);

    /// <summary>
    /// Gets the string payload to write for the value.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The string payload.</returns>
    protected abstract string GetValue(T value);
}
