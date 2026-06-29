// <copyright file="BoundedProblemDetailsFactory.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Creates bounded ProblemDetails responses without domain-specific error semantics.
/// </summary>
public static class BoundedProblemDetailsFactory
{
    /// <summary>
    /// Creates a ProblemDetails instance with bounded fields and optional correlation metadata.
    /// </summary>
    /// <param name="status">The response status.</param>
    /// <param name="title">The bounded title.</param>
    /// <param name="type">The bounded problem type.</param>
    /// <param name="detail">The bounded detail.</param>
    /// <param name="instance">The request instance.</param>
    /// <param name="correlationId">The optional correlation identifier.</param>
    /// <returns>The created ProblemDetails instance.</returns>
    public static ProblemDetails Create(
        int status,
        string title,
        string type,
        string detail,
        string? instance,
        string? correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);

        ProblemDetails details = new()
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance,
        };

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            details.Extensions["correlationId"] = correlationId;
        }

        return details;
    }
}
