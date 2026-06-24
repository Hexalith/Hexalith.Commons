// <copyright file="PublicationTransportMetadataComposer.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Creates safe transport metadata from module-owned values.
/// </summary>
public static class PublicationTransportMetadataComposer
{
    /// <summary>
    /// Copies safe transport metadata values into an immutable shape.
    /// </summary>
    public static PublicationTransportMetadata Compose(
        string topic,
        string type,
        string source,
        string subject,
        IReadOnlyDictionary<string, string> headers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(headers);

        return new PublicationTransportMetadata(
            topic,
            type,
            source,
            subject,
            new Dictionary<string, string>(headers, StringComparer.Ordinal));
    }
}
