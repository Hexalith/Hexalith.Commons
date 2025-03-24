// <copyright file="ObjectDescriptionHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Objects;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Hexalith.Commons.Reflections;
using Humanizer;

/// <summary>
/// Class ObjectDescriptionHelper.
/// </summary>
public static class ObjectDescriptionHelper
{
    /// <summary>
    /// Describes the type using attributes.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>System.ValueTuple&lt;System.String, System.String, System.String&gt;.</returns>
    public static (string TypeName, string DisplayName, string Description) Describe(
        [NotNull] this Type type
    )
    {
        ArgumentNullException.ThrowIfNull(type);
        string? typeName = null;

        // Get the TypeMapName if the type implements IMappableType.
        if (
            typeof(IMappableType).IsAssignableFrom(type)
            && Activator.CreateInstance(type) is IMappableType mappable
        )
        {
            typeName = mappable.TypeMapName;
        }

        // Get the Type name if the type does not have an MapTypeName.
        typeName ??= type.Name;

        string? name;
        string? description;

        (name, description) = NameDescriptionFromDisplayAttribute(type);

        name ??= DisplayNameFromAttribute(type);

        name ??= typeName.Humanize(); // If name attributes are not found use the type name converted to a sentence.

        description ??= DescriptionFromAttribute(type);

        description ??= name; // If description attributes are not found, use the name.

        return (typeName, name, description);
    }

    /// <summary>
    /// Describes the specified property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>System.ValueTuple&lt;System.String, System.String, System.Nullable&lt;System.Object&gt;, System.Boolean&gt;.</returns>
    public static (
        string DisplayName,
        string Description,
        object? DefaultValue,
        bool IsRequired
    ) Describe([NotNull] this PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);
        object? defaultValue = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        bool required = property.GetCustomAttribute<RequiredAttribute>() != null;

        // Get the type description from the display attribute.
        (string? name, string? description) = property.NameDescriptionFromDisplayAttribute();
        name ??= property.DisplayNameFromAttribute();

        name ??= property.Name.Humanize(); // If name attributes are not found use the type name converted to a sentence.

        description ??= DescriptionFromAttribute(property);

        description ??= name; // If description attributes are not found, use the name.

        return (name, description, defaultValue, required);
    }

    /// <summary>
    /// Describes the instance write properties.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>IDictionary&lt;System.String, System.ValueTuple&lt;System.String, System.String, System.Nullable&lt;System.Object&gt;, System.Boolean&gt;&gt;.</returns>
    public static IDictionary<
        string,
        (string DisplayName, string Description, object? DefaultValue, bool IsRequired)
    > DescribeInstanceWriteProperties([NotNull] this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Dictionary<
            string,
            (string DisplayName, string Description, object? DefaultValue, bool IsRequired)
        > result = [];
        foreach (
            PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite)
        )
        {
            result.Add(property.Name, property.Describe());
        }

        return result;
    }

    /// <summary>
    /// Describes the type using attributes.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>System.ValueTuple&lt;System.String, System.String, System.String&gt;.</returns>
    public static IDictionary<
        string,
        (string DisplayName, string Description, object? DefaultValue, bool IsRequired)
    > DescribeProperties([NotNull] this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Dictionary<
            string,
            (string DisplayName, string Description, object? DefaultValue, bool IsRequired)
        > result = [];
        foreach (PropertyInfo property in type.GetProperties())
        {
            result.Add(property.Name, property.Describe());
        }

        return result;
    }

    public static string? DescriptionFromAttribute(this Type type) =>
        type.GetCustomAttribute<DescriptionAttribute>()?.Description;

    public static string? DescriptionFromAttribute(this PropertyInfo type) =>
        type.GetCustomAttribute<DescriptionAttribute>()?.Description;

    public static string? DisplayNameFromAttribute(this Type type) =>
        type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

    public static string? DisplayNameFromAttribute(this PropertyInfo type) =>
        type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

    public static (string? Name, string? Description) NameDescriptionFromDisplayAttribute(
        this PropertyInfo type
    )
    {
        DisplayAttribute? displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
        return (displayAttribute?.Name, displayAttribute?.Description);
    }

    public static (string? Name, string? Description) NameDescriptionFromDisplayAttribute(
        this Type type
    )
    {
        DisplayAttribute? displayAttribute = type.GetCustomAttribute<DisplayAttribute>();
        return (displayAttribute?.Name, displayAttribute?.Description);
    }
}
