// <copyright file="HttpCorrelation.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http;

using Hexalith.Commons.Metadatas;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Domain-neutral HTTP correlation propagation helpers.
/// </summary>
public static class HttpCorrelation
{
    /// <summary>
    /// Applies bounded correlation propagation for one HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="accessor">The ambient correlation accessor.</param>
    /// <param name="next">The next request delegate.</param>
    /// <param name="headerName">The correlation header name.</param>
    /// <param name="httpContextItemKey">The HTTP context item key.</param>
    /// <returns>A task that completes when the request has been processed.</returns>
    public static async Task InvokeAsync(
        HttpContext context,
        ICorrelationContextAccessor accessor,
        RequestDelegate next,
        string headerName,
        string httpContextItemKey)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(next);
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(httpContextItemKey);

        string correlationId = ResolveCorrelationId(context, headerName);
        context.Items[httpContextItemKey] = correlationId;
        context.Response.Headers[headerName] = correlationId;

        string? previousCorrelationId = accessor.CorrelationId;
        accessor.CorrelationId = correlationId;

        try
        {
            await next(context).ConfigureAwait(false);
        }
        finally
        {
            accessor.CorrelationId = previousCorrelationId;
        }
    }

    /// <summary>
    /// Resolves a request correlation identifier, accepting only GUID-shaped inbound header values.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="headerName">The correlation header name.</param>
    /// <returns>The accepted or generated correlation identifier.</returns>
    public static string ResolveCorrelationId(HttpContext context, string headerName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        return context.Request.Headers.TryGetValue(headerName, out StringValues headerValue)
            && Guid.TryParse(headerValue.ToString(), out _)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString();
    }
}
