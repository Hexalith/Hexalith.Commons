// <copyright file="ICorrelationContextAccessor.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http;

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
