// <copyright file="PolymorphicTypeRegistry.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization;

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Maps bounded, content-safe discriminators to explicit runtime types.
/// </summary>
public sealed class PolymorphicTypeRegistry
{
    /// <summary>The maximum discriminator length accepted by the registry.</summary>
    public const int MaxDiscriminatorLength = 128;

    private readonly FrozenDictionary<string, Type> _types;

    private PolymorphicTypeRegistry(FrozenDictionary<string, Type> types)
        => _types = types;

    /// <summary>
    /// Gets the registered discriminator-to-type entries.
    /// </summary>
    public IReadOnlyDictionary<string, Type> Entries => _types;

    /// <summary>
    /// Creates a registry from explicit discriminator-to-type entries.
    /// </summary>
    /// <param name="registrations">The registrations to include.</param>
    /// <returns>The created registry.</returns>
    public static PolymorphicTypeRegistry Create(IEnumerable<PolymorphicTypeRegistration> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        Dictionary<string, Type> entries = new(StringComparer.Ordinal);
        foreach (PolymorphicTypeRegistration? registration in registrations)
        {
            if (registration is null)
            {
                throw new ArgumentException("Registration entries cannot be null.", nameof(registrations));
            }

            ValidateDiscriminator(registration.Discriminator);
            ValidateType(registration.Type);

            if (!entries.TryAdd(registration.Discriminator, registration.Type))
            {
                throw new InvalidOperationException($"Polymorphic discriminator '{registration.Discriminator}' is already registered.");
            }
        }

        if (entries.Count == 0)
        {
            throw new ArgumentException("At least one polymorphic type registration must be supplied.", nameof(registrations));
        }

        return new PolymorphicTypeRegistry(entries.ToFrozenDictionary(StringComparer.Ordinal));
    }

    /// <summary>
    /// Creates a registry from explicit runtime types keyed by their simple type names.
    /// </summary>
    /// <param name="types">The types to include.</param>
    /// <returns>The created registry.</returns>
    public static PolymorphicTypeRegistry FromTypeNames(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);
        return Create(types.Select(static type => new PolymorphicTypeRegistration(type.Name, type)));
    }

    /// <summary>
    /// Tries to resolve a discriminator by exact ordinal match.
    /// </summary>
    /// <param name="discriminator">The discriminator to resolve.</param>
    /// <param name="type">The resolved type.</param>
    /// <returns>True when the discriminator is registered.</returns>
    public bool TryResolve(string? discriminator, [NotNullWhen(true)] out Type? type)
    {
        type = null;
        return !string.IsNullOrWhiteSpace(discriminator)
            && _types.TryGetValue(discriminator, out type);
    }

    /// <summary>
    /// Tries to resolve by exact match first, then by ordinal suffix match.
    /// </summary>
    /// <param name="discriminator">The discriminator or qualified type name to resolve.</param>
    /// <param name="type">The resolved type.</param>
    /// <returns>True when the discriminator resolves to a registered type.</returns>
    public bool TryResolveExactThenSuffix(string? discriminator, [NotNullWhen(true)] out Type? type)
    {
        if (TryResolve(discriminator, out type))
        {
            return true;
        }

        type = null;
        if (string.IsNullOrWhiteSpace(discriminator))
        {
            return false;
        }

        foreach (KeyValuePair<string, Type> entry in _types)
        {
            if (discriminator.EndsWith(entry.Key, StringComparison.Ordinal))
            {
                type = entry.Value;
                return true;
            }
        }

        return false;
    }

    private static void ValidateDiscriminator(string discriminator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(discriminator);
        if (discriminator.Length > MaxDiscriminatorLength)
        {
            throw new ArgumentException(
                $"Polymorphic discriminator length must not exceed {MaxDiscriminatorLength} characters.",
                nameof(discriminator));
        }

        for (int i = 0; i < discriminator.Length; i++)
        {
            char c = discriminator[i];
            if (!char.IsAsciiLetterOrDigit(c) && c is not '.' and not '_' and not '-')
            {
                throw new ArgumentException(
                    "Polymorphic discriminators may contain only ASCII letters, digits, '.', '_', or '-'.",
                    nameof(discriminator));
            }
        }
    }

    private static void ValidateType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (type.ContainsGenericParameters)
        {
            throw new ArgumentException("Open generic types cannot be registered.", nameof(type));
        }
    }
}
