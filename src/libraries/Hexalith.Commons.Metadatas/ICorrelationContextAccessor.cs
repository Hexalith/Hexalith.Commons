// <copyright file="ICorrelationContextAccessor.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Metadatas;

/// <summary>
/// Provides ambient access to the current correlation identifier across async flows.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// Gets or sets the current ambient correlation identifier.
    /// </summary>
    string? CorrelationId { get; set; }
}
