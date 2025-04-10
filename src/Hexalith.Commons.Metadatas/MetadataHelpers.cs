// <copyright file="MetadataHelpers.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

/// <summary>
/// Provides helper methods for working with metadata.
/// </summary>
public static class MetadataHelpers
{
    /// <summary>
    /// The name of the domain identifier property.
    /// </summary>
    public const string DomainIdentifierPropertyName = "DomainId";

    /// <summary>
    /// The name of the domain name property.
    /// </summary>
    public const string DomainNamePropertyName = "DomainName";

    /// <summary>
    /// Creates a <see cref="DomainMetadata"/> instance from the specified object.
    /// </summary>
    /// <param name="instance">The object instance to extract metadata from.</param>
    /// <returns>A <see cref="DomainMetadata"/> instance containing the extracted metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="instance"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required properties <see cref="DomainIdentifierPropertyName"/> or
    /// <see cref="DomainNamePropertyName"/> are missing or undefined.
    /// </exception>
    public static DomainMetadata CreateDomainMetadata([NotNull] this object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        // Get the type of the instance object
        Type type = instance.GetType();

        // Function to get property value (instance or static, including base classes)
        string? GetPropertyValue(string propertyName)
        {
            // Check for instance property
            PropertyInfo? instanceProperty = type.GetProperty(propertyName);
            if (instanceProperty != null)
            {
                return instanceProperty.GetValue(instance)?.ToString();
            }

            // Check for static property (including base classes)
            Type? currentType = type;
            while (currentType != null)
            {
                PropertyInfo? staticProperty = currentType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (staticProperty != null)
                {
                    return staticProperty.GetValue(null)?.ToString();
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        // Get the values of the properties
        string? id = GetPropertyValue(DomainIdentifierPropertyName);
        string? name = GetPropertyValue(DomainNamePropertyName);

        return id == null || name == null
            ? throw new InvalidOperationException($"Invalid domain instance: the {DomainIdentifierPropertyName} or {DomainNamePropertyName} properties are missing or undefined. Add these properties to your class or set their values. {JsonSerializer.Serialize(instance)}")
            : new DomainMetadata(id, name);
    }
}