// <copyright file="PublicationDeduplicationSet.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Tracks publication identities already applied by an idempotent local consumer.
/// </summary>
public sealed class PublicationDeduplicationSet
{
    private readonly HashSet<string> _identities = new(StringComparer.Ordinal);

    /// <summary>
    /// Records a publication identity if it has not been seen.
    /// </summary>
    /// <param name="identity">The stable publication identity.</param>
    /// <returns><c>true</c> when the identity is new; otherwise <c>false</c>.</returns>
    public bool TryApply(string identity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        return _identities.Add(identity);
    }
}
