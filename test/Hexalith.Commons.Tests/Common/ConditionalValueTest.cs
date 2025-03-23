// <copyright file="ConditionalValueTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using System;

using Hexalith.Commons.Errors;

using Shouldly;

using Xunit;

/// <summary>
/// Class ConditionalValueTests.
/// </summary>
public class ConditionalValueTest
{
    /// <summary>
    /// Defines the test method to check if an empty conditional value HasValue property is false.
    /// </summary>
    [Fact]
    public void CheckHasValueIsFalseWhenNoValue()
    {
        // Arrange
        ConditionalValue<int> conditionalValue = new();

        // Act and Assert
        conditionalValue.HasValue.ShouldBeFalse();
    }

    /// <summary>
    /// Defines the test method to check that the HasValue property is true when a value is set.
    /// </summary>
    [Fact]
    public void CheckHasValueIsTrueWhenValue()
    {
        // Arrange
        ConditionalValue<int> conditionalValue = new(10);

        // Act and Assert
        conditionalValue.HasValue.ShouldBeTrue();
    }

    /// <summary>
    /// Defines the test method to check getting the value on an empty object is an invalid operation.
    /// </summary>
    [Fact]
    public void GetValueOnNoValueShouldThrowInvalidOperationException()
    {
        // Arrange
        ConditionalValue<int> value = new();

        // Act and Assert
        _ = Should.Throw<InvalidOperationException>(() => _ = value.Value);
    }

    /// <summary>
    /// Defines the test method to check if we retrieve the correct value.
    /// </summary>
    [Fact]
    public void GetValueShouldReturnTheCorrectValue()
    {
        // Arrange
        ConditionalValue<int> conditionalValue = new(5);

        // Assert
        conditionalValue.Value.ShouldBe(5);
    }
}