// <copyright file="JsonSerializationOptions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Creates <see cref="JsonSerializerOptions"/> instances with explicit resolver ordering.
/// </summary>
public static class JsonSerializationOptions
{
    /// <summary>
    /// Creates web-default JSON options whose resolver chain is ordered exactly as supplied.
    /// </summary>
    /// <param name="resolvers">Resolvers to query in precedence order.</param>
    /// <param name="includeReflectionFallback">True to append <see cref="DefaultJsonTypeInfoResolver"/> after all supplied resolvers.</param>
    /// <returns>The configured JSON serializer options.</returns>
    public static JsonSerializerOptions CreateWeb(
        IEnumerable<IJsonTypeInfoResolver> resolvers,
        bool includeReflectionFallback = false)
    {
        ArgumentNullException.ThrowIfNull(resolvers);

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        int count = 0;
        foreach (IJsonTypeInfoResolver? resolver in resolvers)
        {
            if (resolver is null)
            {
                throw new ArgumentException("Resolver entries cannot be null.", nameof(resolvers));
            }

            options.TypeInfoResolverChain.Add(resolver);
            count++;
        }

        if (count == 0)
        {
            throw new ArgumentException("At least one JSON type-info resolver must be supplied.", nameof(resolvers));
        }

        if (includeReflectionFallback)
        {
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
        }

        return options;
    }

    /// <summary>
    /// Creates web-default JSON options whose resolver chain is ordered exactly as supplied.
    /// </summary>
    /// <param name="includeReflectionFallback">True to append <see cref="DefaultJsonTypeInfoResolver"/> after all supplied resolvers.</param>
    /// <param name="resolvers">Resolvers to query in precedence order.</param>
    /// <returns>The configured JSON serializer options.</returns>
    public static JsonSerializerOptions CreateWeb(
        bool includeReflectionFallback,
        params IJsonTypeInfoResolver[] resolvers)
        => CreateWeb((IEnumerable<IJsonTypeInfoResolver>)resolvers, includeReflectionFallback);
}
