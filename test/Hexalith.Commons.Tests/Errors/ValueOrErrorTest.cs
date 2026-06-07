// <copyright file="ValueOrErrorTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Errors;

using System;

using Hexalith.Commons.Errors;

using Shouldly;

/// <summary>
/// Unit tests for the ValueOrError and ValueOrError{T} classes.
/// </summary>
public class ValueOrErrorTest
{
    /// <summary>
    /// Tests that accessing Error on a value instance throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void AccessingErrorOnValueInstanceShouldThrowInvalidOperationException()
    {
        // Arrange
        ValueOrError<int> valueOrError = new(42);

        // Act & Assert
        _ = Should.Throw<InvalidOperationException>(() => _ = valueOrError.Error);
    }

    /// <summary>
    /// Tests that accessing Value on an error instance throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void AccessingValueOnErrorInstanceShouldThrowInvalidOperationException()
    {
        // Arrange
        ApplicationError error = new() { Title = "Test Error", Detail = "Test Detail" };
        var valueOrError = ValueOrError.WithError<int>(error);

        // Act & Assert
        _ = Should.Throw<InvalidOperationException>(() => _ = valueOrError.Value);
    }

    /// <summary>
    /// Tests that creating a ValueOrError with an error sets HasValue to false.
    /// </summary>
    [Fact]
    public void CreateWithErrorShouldSetHasValueToFalse()
    {
        // Arrange
        ApplicationError error = new() { Title = "Test Error" };

        // Act
        var valueOrError = ValueOrError.WithError<string>(error);

        // Assert
        valueOrError.HasValue.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that creating a ValueOrError with a value sets HasValue to true.
    /// </summary>
    [Fact]
    public void CreateWithValueShouldSetHasValueToTrue()
    {
        // Arrange & Act
        ValueOrError<int> valueOrError = new(42);

        // Assert
        valueOrError.HasValue.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that Error property returns the correct error.
    /// </summary>
    [Fact]
    public void ErrorPropertyShouldReturnCorrectError()
    {
        // Arrange
        ApplicationError error = new() { Title = "Test Error", Detail = "Test Detail" };
        var valueOrError = ValueOrError.WithError<int>(error);

        // Act
        ApplicationError result = valueOrError.Error;

        // Assert
        result.Title.ShouldBe("Test Error");
        result.Detail.ShouldBe("Test Detail");
    }

    /// <summary>
    /// Tests that ValueOrError works with null values.
    /// </summary>
    [Fact]
    public void ValueOrErrorWithNullValueShouldHaveValue()
    {
        // Arrange & Act
        ValueOrError valueOrError = new(null);

        // Assert
        valueOrError.HasValue.ShouldBeTrue();
        valueOrError.Value.ShouldBeNull();
    }

    /// <summary>
    /// Tests that ValueOrError with reference type returns correct value.
    /// </summary>
    [Fact]
    public void ValueOrErrorWithReferenceTypeShouldReturnCorrectValue()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 123 };
        ValueOrError<object> valueOrError = new(testObject);

        // Act & Assert
        valueOrError.HasValue.ShouldBeTrue();
        valueOrError.Value.ShouldBe(testObject);
    }

    /// <summary>
    /// Tests that Value property returns the correct value.
    /// </summary>
    [Fact]
    public void ValuePropertyShouldReturnCorrectValue()
    {
        // Arrange
        ValueOrError<string> valueOrError = new("Hello World");

        // Act
        string? result = valueOrError.Value;

        // Assert
        result.ShouldBe("Hello World");
    }
}