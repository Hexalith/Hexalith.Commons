// <copyright file="HttpCorrelationTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using Hexalith.Commons.Metadatas;

using Microsoft.AspNetCore.Http;

using Shouldly;

/// <summary>
/// Tests bounded HTTP correlation propagation.
/// </summary>
public sealed class HttpCorrelationTest
{
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
