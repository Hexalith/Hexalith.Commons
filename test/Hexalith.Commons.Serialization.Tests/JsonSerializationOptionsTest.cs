// <copyright file="JsonSerializationOptionsTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization.Tests;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Shouldly;

/// <summary>
/// Verifies explicit JSON resolver composition.
/// </summary>
public sealed class JsonSerializationOptionsTest
{
    /// <summary>
    /// Resolver order is significant and the first resolver that can describe a type wins.
    /// </summary>
    [Fact]
    public void CreateWebShouldQueryResolversInDeclaredOrder()
    {
        JsonSerializerOptions options = JsonSerializationOptions.CreateWeb(
            [
                RenameSamplePropertyResolver("firstValue"),
                RenameSamplePropertyResolver("secondValue"),
            ]);

        JsonSerializer.Serialize(new Sample("value"), options).ShouldBe("""{"firstValue":"value"}""");
    }

    /// <summary>
    /// Reflection fallback is appended only when requested.
    /// </summary>
    [Fact]
    public void CreateWebShouldAppendReflectionFallbackOnlyWhenRequested()
    {
        JsonSerializerOptions withoutFallback = JsonSerializationOptions.CreateWeb([new NullJsonTypeInfoResolver()]);
        _ = Should.Throw<NotSupportedException>(() => JsonSerializer.Serialize(new Sample("value"), withoutFallback));

        JsonSerializerOptions withFallback = JsonSerializationOptions.CreateWeb(
            [new NullJsonTypeInfoResolver()],
            includeReflectionFallback: true);

        JsonSerializer.Serialize(new Sample("value"), withFallback).ShouldBe("""{"value":"value"}""");
    }

    /// <summary>
    /// Missing resolver inputs are rejected at construction time.
    /// </summary>
    [Fact]
    public void CreateWebShouldRejectMissingResolvers()
    {
        _ = Should.Throw<ArgumentNullException>(() =>
            JsonSerializationOptions.CreateWeb(null!));
        _ = Should.Throw<ArgumentException>(() =>
            JsonSerializationOptions.CreateWeb([]));
        _ = Should.Throw<ArgumentException>(() =>
            JsonSerializationOptions.CreateWeb([null!]));
    }

    private static DefaultJsonTypeInfoResolver RenameSamplePropertyResolver(string propertyName)
        => new()
        {
            Modifiers =
            {
                typeInfo =>
                {
                    if (typeInfo.Type == typeof(Sample))
                    {
                        typeInfo.Properties.Single().Name = propertyName;
                    }
                },
            },
        };

    private sealed record Sample(string Value);

    private sealed class NullJsonTypeInfoResolver : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) => null;
    }
}
