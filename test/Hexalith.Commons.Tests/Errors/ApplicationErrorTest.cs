// <copyright file="ApplicationErrorTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Errors;

using System.Globalization;

using Hexalith.Commons.Errors;

using Shouldly;

/// <summary>
/// Unit tests for the ApplicationError class.
/// </summary>
public class ApplicationErrorTest
{
    /// <summary>
    /// Tests that ApplicationError record equality works correctly.
    /// </summary>
    [Fact]
    public void ApplicationErrorRecordEqualityShouldWork()
    {
        // Arrange
        ApplicationError error1 = new()
        {
            Title = "Error",
            Detail = "Detail",
            Category = ErrorCategory.Business,
        };
        ApplicationError error2 = new()
        {
            Title = "Error",
            Detail = "Detail",
            Category = ErrorCategory.Business,
        };

        // Assert
        error1.ShouldBe(error2);
    }

    /// <summary>
    /// Tests that ApplicationError can store and retrieve all properties correctly.
    /// </summary>
    [Fact]
    public void ApplicationErrorShouldStoreAllPropertiesCorrectly()
    {
        // Arrange
        ApplicationError innerError = new() { Title = "Inner Error" };
        ApplicationError error = new()
        {
            Title = "Test Title",
            Detail = "Test Detail",
            TechnicalDetail = "Technical Info",
            Type = "TestType",
            Category = ErrorCategory.Technical,
            InnerError = innerError,
            Arguments = ["arg1", "arg2"],
            TechnicalArguments = ["tech1"],
        };

        // Assert
        error.Title.ShouldBe("Test Title");
        error.Detail.ShouldBe("Test Detail");
        error.TechnicalDetail.ShouldBe("Technical Info");
        error.Type.ShouldBe("TestType");
        error.Category.ShouldBe(ErrorCategory.Technical);
        error.InnerError.ShouldBe(innerError);
        error.Arguments.ShouldBe(["arg1", "arg2"]);
        error.TechnicalArguments.ShouldBe(["tech1"]);
    }

    /// <summary>
    /// Tests that GetDetailMessage formats the message correctly with arguments.
    /// </summary>
    [Fact]
    public void GetDetailMessageShouldFormatWithArguments()
    {
        // Arrange
        ApplicationError error = new()
        {
            Detail = "Error occurred for user {UserId} with code {ErrorCode}",
            Arguments = ["user123", 500],
        };

        // Act
        string result = error.GetDetailMessage(CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBe("Error occurred for user user123 with code 500");
    }

    /// <summary>
    /// Tests that GetDetailMessage returns empty string when Detail is null.
    /// </summary>
    [Fact]
    public void GetDetailMessageShouldReturnEmptyWhenDetailIsNull()
    {
        // Arrange
        ApplicationError error = new() { Detail = null };

        // Act
        string result = error.GetDetailMessage(CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that GetDetailMessage returns empty string when Detail is whitespace.
    /// </summary>
    [Fact]
    public void GetDetailMessageShouldReturnEmptyWhenDetailIsWhitespace()
    {
        // Arrange
        ApplicationError error = new() { Detail = "   " };

        // Act
        string result = error.GetDetailMessage(CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that GetTechnicalMessage formats the message correctly with arguments.
    /// </summary>
    [Fact]
    public void GetTechnicalMessageShouldFormatWithArguments()
    {
        // Arrange
        ApplicationError error = new()
        {
            TechnicalDetail = "Stack trace at {Location} line {LineNumber}",
            TechnicalArguments = ["MyClass.MyMethod", 42],
        };

        // Act
        string? result = error.GetTechnicalMessage(CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBe("Stack trace at MyClass.MyMethod line 42");
    }

    /// <summary>
    /// Tests that GetTechnicalMessage returns empty string when TechnicalDetail is null.
    /// </summary>
    [Fact]
    public void GetTechnicalMessageShouldReturnEmptyWhenTechnicalDetailIsNull()
    {
        // Arrange
        ApplicationError error = new() { TechnicalDetail = null };

        // Act
        string? result = error.GetTechnicalMessage(CultureInfo.InvariantCulture);

        // Assert
        result.ShouldBeEmpty();
    }
}