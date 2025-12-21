// <copyright file="MetadataHelpers.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

using Hexalith.Commons.UniqueIds;

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

    /// <summary>
    /// Creates a new instance of the <see cref="MessageMetadata"/> class for a specific message.
    /// </summary>
    /// <param name="message">The message object.</param>
    /// <param name="dateTimeOffset">The creation date of the message.</param>
    /// <returns>A new instance of <see cref="MessageMetadata"/>.</returns>
    public static MessageMetadata CreateMessageMetadata([NotNull] this object message, DateTimeOffset dateTimeOffset)
    {
        ArgumentNullException.ThrowIfNull(message);
        (string name, string _, int version) = message.GetPolymorphicTypeDiscriminator();
        return new MessageMetadata(
            UniqueIdHelper.GenerateUniqueStringId(),
            name,
            version,
            message.CreateDomainMetadata(),
            dateTimeOffset);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Metadata"/> class with updated message information.
    /// </summary>
    /// <param name="message">The new message object to be included in the metadata.</param>
    /// <param name="metadata">The existing metadata to derive context from.</param>
    /// <param name="dateTime">The timestamp for the new message.</param>
    /// <returns>A new instance of the <see cref="Metadata"/> class with updated message information and existing context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> or <paramref name="metadata"/> is null.</exception>
    public static Metadata CreateMetadata(this object message, Metadata metadata, DateTimeOffset dateTime)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(metadata);
        return new Metadata(message.CreateMessageMetadata(dateTime), metadata.Context);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Metadata"/> class with new message and context information.
    /// </summary>
    /// <param name="message">The new message object to be included in the metadata.</param>
    /// <param name="userId">The identifier of the user associated with this message.</param>
    /// <param name="partitionId">The identifier of the partition this message belongs to.</param>
    /// <param name="dateTime">The timestamp for the new message.</param>
    /// <returns>A new instance of the <see cref="Metadata"/> class with new message and context information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public static Metadata CreateMetadata(this object message, string userId, string partitionId, DateTimeOffset dateTime)
    {
        ArgumentNullException.ThrowIfNull(message);
        MessageMetadata msgMeta = message.CreateMessageMetadata(dateTime);
        return new Metadata(
            msgMeta,
            new ContextMetadata(msgMeta.Id, userId, partitionId, dateTime, null, null, null, null, []));
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Metadata"/> class with new message, context, and session information.
    /// </summary>
    /// <param name="message">The new message object to be included in the metadata.</param>
    /// <param name="userId">The identifier of the user associated with this message.</param>
    /// <param name="partitionId">The identifier of the partition this message belongs to.</param>
    /// <param name="sessionId">The session identifier associated with this message.</param>
    /// <param name="dateTime">The timestamp for the new message.</param>
    /// <returns>A new instance of the <see cref="Metadata"/> class with new message, context, and session information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public static Metadata CreateNew(object message, string userId, string partitionId, string sessionId, DateTimeOffset dateTime)
    {
        ArgumentNullException.ThrowIfNull(message);
        MessageMetadata msgMeta = message.CreateMessageMetadata(dateTime);
        return new Metadata(
            msgMeta,
            new ContextMetadata(msgMeta.Id, userId, partitionId, dateTime, null, null, null, sessionId, []));
    }
}