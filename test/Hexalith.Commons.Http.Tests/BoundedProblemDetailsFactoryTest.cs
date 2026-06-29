// <copyright file="BoundedProblemDetailsFactoryTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using Shouldly;

/// <summary>
/// Tests bounded ProblemDetails creation.
/// </summary>
public sealed class BoundedProblemDetailsFactoryTest
{
    /// <summary>
    /// Creates the requested bounded fields and correlation extension.
    /// </summary>
    [Fact]
    public void CreateShouldPopulateBoundedFieldsAndCorrelationExtension()
    {
        Microsoft.AspNetCore.Mvc.ProblemDetails details = BoundedProblemDetailsFactory.Create(
            503,
            "Dependency Unavailable",
            "urn:test:dependency",
            "Retry after recovery.",
            "/process",
            "corr-1");

        details.Status.ShouldBe(503);
        details.Title.ShouldBe("Dependency Unavailable");
        details.Type.ShouldBe("urn:test:dependency");
        details.Detail.ShouldBe("Retry after recovery.");
        details.Instance.ShouldBe("/process");
        details.Extensions["correlationId"].ShouldBe("corr-1");
    }

    /// <summary>
    /// Omits blank correlation identifiers instead of emitting unusable metadata.
    /// </summary>
    [Fact]
    public void CreateShouldOmitBlankCorrelationExtension()
    {
        Microsoft.AspNetCore.Mvc.ProblemDetails details = BoundedProblemDetailsFactory.Create(
            500,
            "Internal Server Error",
            "urn:test:error",
            "A bounded detail.",
            "/process",
            " ");

        details.Extensions.ShouldNotContainKey("correlationId");
    }
}
