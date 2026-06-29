// <copyright file="PagedResult.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Paging;

/// <summary>
/// Domain-neutral paged result shape for internal adapters.
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
