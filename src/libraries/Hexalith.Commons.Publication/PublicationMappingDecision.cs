// <copyright file="PublicationMappingDecision.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Represents the generic result of publication candidate mapping.
/// </summary>
/// <typeparam name="TDiagnostic">The module-owned diagnostic type.</typeparam>
/// <param name="PublishedEvent">The publishable event when mapping succeeds.</param>
/// <param name="Diagnostic">The diagnostic when mapping fails closed.</param>
public sealed record PublicationMappingDecision<TDiagnostic>(
    object? PublishedEvent,
    TDiagnostic? Diagnostic)
    where TDiagnostic : class
{
    /// <summary>
    /// Gets a value indicating whether a public event is ready for publication.
    /// </summary>
    public bool IsPublished => PublishedEvent is not null;

    /// <summary>
    /// Creates a published decision.
    /// </summary>
    /// <param name="publishedEvent">The publishable event.</param>
    /// <returns>The published decision.</returns>
    public static PublicationMappingDecision<TDiagnostic> Published(object publishedEvent)
    {
        ArgumentNullException.ThrowIfNull(publishedEvent);
        return new(publishedEvent, null);
    }

    /// <summary>
    /// Creates a rejected decision.
    /// </summary>
    /// <param name="diagnostic">The rejection diagnostic.</param>
    /// <returns>The rejected decision.</returns>
    public static PublicationMappingDecision<TDiagnostic> Rejected(TDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        return new(null, diagnostic);
    }
}
