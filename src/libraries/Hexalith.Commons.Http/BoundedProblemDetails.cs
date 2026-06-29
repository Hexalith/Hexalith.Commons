// <copyright file="BoundedProblemDetails.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http;

/// <summary>
/// Bounded, domain-neutral ProblemDetails-compatible fields.
/// </summary>
/// <param name="Status">The HTTP or problem status code.</param>
/// <param name="Title">The bounded title.</param>
/// <param name="Type">The bounded problem type.</param>
/// <param name="Detail">The bounded detail.</param>
/// <param name="CorrelationId">The optional correlation identifier.</param>
public sealed record BoundedProblemDetails(
    int Status,
    string? Title,
    string? Type,
    string? Detail,
    string? CorrelationId);
