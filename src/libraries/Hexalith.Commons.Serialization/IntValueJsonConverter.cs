// <copyright file="IntValueJsonConverter.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Ruleless base converter for value types encoded as one JSON integer.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public abstract class IntValueJsonConverter<T> : JsonConverter<T>
    where T : notnull
{
    /// <inheritdoc/>
    public sealed override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int value))
        {
            throw new JsonException($"{typeToConvert.Name} must be encoded as a JSON integer (no fractional, exponent, or string values).");
        }

        try
        {
            return Create(value);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new JsonException($"{typeToConvert.Name} payload is out of range: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteNumberValue(GetValue(value));

    /// <summary>
    /// Creates the target value from its integer payload.
    /// </summary>
    /// <param name="value">The JSON integer payload.</param>
    /// <returns>The converted value.</returns>
    protected abstract T Create(int value);

    /// <summary>
    /// Gets the integer payload to write for the value.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The integer payload.</returns>
    protected abstract int GetValue(T value);
}
