// <copyright file="PagedResult.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http;

/// <summary>
/// Domain-neutral paged result shape for HTTP adapters.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Gets the current page items.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total matching item count.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total matching page count.
    /// </summary>
    public int TotalPages { get; init; }
}
