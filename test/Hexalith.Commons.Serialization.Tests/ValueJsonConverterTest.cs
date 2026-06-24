// <copyright file="ValueJsonConverterTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;

using Shouldly;

/// <summary>
/// Verifies the ruleless string and integer value converter bases.
/// </summary>
public sealed class ValueJsonConverterTest
{
    /// <summary>
    /// The string converter accepts only JSON string tokens and preserves malformed-payload exceptions as JSON errors.
    /// </summary>
    [Fact]
    public void StringValueConverterShouldGuardTokenTypeAndRoundTrip()
    {
        JsonSerializer.Deserialize<StringValue>("\"valid\"").ShouldBe(new StringValue("valid"));
        JsonSerializer.Serialize(new StringValue("valid")).ShouldBe("\"valid\"");

        _ = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<StringValue>("123"));
        _ = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<StringValue>("\"\""));
    }

    /// <summary>
    /// The integer converter accepts only Int32 JSON integer tokens and preserves range failures as JSON errors.
    /// </summary>
    [Fact]
    public void IntValueConverterShouldGuardTokenTypeAndRoundTrip()
    {
        JsonSerializer.Deserialize<IntValue>("7").ShouldBe(new IntValue(7));
        JsonSerializer.Serialize(new IntValue(7)).ShouldBe("7");

        _ = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<IntValue>("\"7\""));
        _ = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<IntValue>("1.5"));
        _ = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<IntValue>("-1"));
    }

    [JsonConverter(typeof(IntValueConverter))]
    private readonly record struct IntValue(int Value);

    [JsonConverter(typeof(StringValueConverter))]
    private sealed record StringValue(string Value);

    private sealed class StringValueConverter : StringValueJsonConverter<StringValue>
    {
        protected override StringValue Create(string value)
            => !string.IsNullOrWhiteSpace(value)
                ? new StringValue(value)
                : throw new ArgumentException("Value is required.", nameof(value));

        protected override string GetValue(StringValue value) => value.Value;
    }

    private sealed class IntValueConverter : IntValueJsonConverter<IntValue>
    {
        protected override IntValue Create(int value)
            => value >= 0
                ? new IntValue(value)
                : throw new ArgumentOutOfRangeException(nameof(value));

        protected override int GetValue(IntValue value) => value.Value;
    }
}
