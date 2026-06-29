// <copyright file="BoundedProblemDetailsReaderTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using System.Net;
using System.Text;

using Shouldly;

/// <summary>
/// Tests bounded ProblemDetails parsing from HTTP responses.
/// </summary>
public sealed class BoundedProblemDetailsReaderTest
{
    /// <summary>
    /// Reads problem details values from string fields in a problem response.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ReadAsyncShouldUseProblemStatusAndStringFields()
    {
        using HttpResponseMessage response = new(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(
                "{\"status\":422,\"title\":\"Validation failed\",\"type\":\"urn:test\",\"detail\":\"Safe detail\",\"correlationId\":\"corr-1\"}",
                Encoding.UTF8,
                "application/problem+json"),
        };

        BoundedProblemDetails details = await BoundedProblemDetailsReader
            .ReadAsync(response, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        details.Status.ShouldBe(422);
        details.Title.ShouldBe("Validation failed");
        details.Type.ShouldBe("urn:test");
        details.Detail.ShouldBe("Safe detail");
        details.CorrelationId.ShouldBe("corr-1");
    }

    /// <summary>
    /// Ignores problem details fields that are not strings.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ReadAsyncShouldIgnoreNonStringProblemFields()
    {
        using HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(
                "{\"status\":503,\"title\":1,\"type\":false,\"detail\":null,\"correlationId\":99}",
                Encoding.UTF8,
                "application/problem+json"),
        };

        BoundedProblemDetails details = await BoundedProblemDetailsReader
            .ReadAsync(response, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        details.Status.ShouldBe(503);
        details.Title.ShouldBe("Service Unavailable");
        details.Type.ShouldBeNull();
        details.Detail.ShouldBeNull();
        details.CorrelationId.ShouldBeNull();
    }

    /// <summary>
    /// Falls back to the HTTP status when the response body is malformed JSON.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task ReadAsyncShouldFallbackToHttpStatusWhenJsonIsMalformed()
    {
        using HttpResponseMessage response = new(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("{", Encoding.UTF8, "application/problem+json"),
        };

        BoundedProblemDetails details = await BoundedProblemDetailsReader
            .ReadAsync(response, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        details.Status.ShouldBe(502);
        details.Title.ShouldBe("Bad Gateway");
        details.Type.ShouldBeNull();
        details.Detail.ShouldBeNull();
        details.CorrelationId.ShouldBeNull();
    }
}
