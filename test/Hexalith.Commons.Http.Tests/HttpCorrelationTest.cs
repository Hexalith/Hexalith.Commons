// <copyright file="HttpCorrelationTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using Microsoft.AspNetCore.Http;

using Shouldly;

/// <summary>
/// Tests bounded HTTP correlation propagation.
/// </summary>
public sealed class HttpCorrelationTest
{
    /// <summary>
    /// Propagates a valid inbound correlation header and restores the previous ambient value.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task InvokeAsyncShouldPropagateGuidHeaderAndRestoreAmbientValue()
    {
        const string previous = "previous-correlation";
        const string incoming = "5dc89ec7-f725-4520-9229-6b084e9d5ba0";
        CorrelationContextAccessor accessor = new() { CorrelationId = previous };
        DefaultHttpContext context = new();
        context.Request.Headers["X-Correlation-ID"] = incoming;
        string? observed = null;

        await HttpCorrelation.InvokeAsync(
            context,
            accessor,
            _ =>
            {
                observed = accessor.CorrelationId;
                return Task.CompletedTask;
            },
            "X-Correlation-ID",
            "CorrelationId").ConfigureAwait(true);

        observed.ShouldBe(incoming);
        context.Items["CorrelationId"].ShouldBe(incoming);
        context.Response.Headers["X-Correlation-ID"].ToString().ShouldBe(incoming);
        accessor.CorrelationId.ShouldBe(previous);
    }

    /// <summary>
    /// Generates a new correlation identifier when the inbound header is malformed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task InvokeAsyncShouldGenerateGuidForMalformedHeader()
    {
        CorrelationContextAccessor accessor = new();
        DefaultHttpContext context = new();
        context.Request.Headers["X-Correlation-ID"] = "not-a-guid";

        await HttpCorrelation.InvokeAsync(
            context,
            accessor,
            _ => Task.CompletedTask,
            "X-Correlation-ID",
            "CorrelationId").ConfigureAwait(true);

        string generated = context.Response.Headers["X-Correlation-ID"].ToString();
        Guid.TryParse(generated, out _).ShouldBeTrue();
        generated.ShouldNotBe("not-a-guid");
    }
}
