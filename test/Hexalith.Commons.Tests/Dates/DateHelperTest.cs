// <copyright file="DateHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Dates;

using System;

using Hexalith.Commons.Dates;

using Shouldly;

/// <summary>
/// Unit tests for the DateHelper class.
/// </summary>
public class DateHelperTest
{
    /// <summary>
    /// Tests that nullable WaitTime overload works correctly.
    /// </summary>
    [Fact]
    public void NullableWaitTimeOverloadShouldWorkCorrectly()
    {
        // Arrange
        DateTimeOffset? from = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        DateTimeOffset? to = new DateTimeOffset(2024, 1, 1, 11, 30, 0, TimeSpan.Zero);

        // Act
        TimeSpan result = from.WaitTime(to);

        // Assert
        result.TotalMinutes.ShouldBe(90);
    }

    /// <summary>
    /// Tests that ToLocalTime converts DateOnly to DateTimeOffset with correct offset.
    /// </summary>
    [Fact]
    public void ToLocalTimeShouldConvertWithCorrectOffset()
    {
        // Arrange
        DateOnly date = new(2024, 6, 15);
        var offset = TimeSpan.FromHours(2);

        // Act
        DateTimeOffset result = date.ToLocalTime(offset);

        // Assert
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(6);
        result.Day.ShouldBe(15);
        result.Hour.ShouldBe(0);
        result.Minute.ShouldBe(0);
        result.Second.ShouldBe(0);
        result.Offset.ShouldBe(offset);
    }

    /// <summary>
    /// Tests that ToLocalTime with negative offset works correctly.
    /// </summary>
    [Fact]
    public void ToLocalTimeWithNegativeOffsetShouldWork()
    {
        // Arrange
        DateOnly date = new(2024, 1, 1);
        var offset = TimeSpan.FromHours(-5);

        // Act
        DateTimeOffset result = date.ToLocalTime(offset);

        // Assert
        result.Offset.ShouldBe(offset);
    }

    /// <summary>
    /// Tests that ToUniversalTime converts DateOnly to UTC DateTimeOffset.
    /// </summary>
    [Fact]
    public void ToUniversalTimeShouldConvertToUtc()
    {
        // Arrange
        DateOnly date = new(2024, 12, 25);

        // Act
        DateTimeOffset result = date.ToUniversalTime();

        // Assert
        result.Year.ShouldBe(2024);
        result.Month.ShouldBe(12);
        result.Day.ShouldBe(25);
        result.Offset.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that WaitTime returns zero when from is null.
    /// </summary>
    [Fact]
    public void WaitTimeFromNullShouldReturnZero()
    {
        // Arrange
        DateTimeOffset? from = null;
        DateTimeOffset? to = DateTimeOffset.UtcNow;

        // Act
        TimeSpan result = from.WaitTime(to);

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that WaitTime returns positive duration when to is after from.
    /// </summary>
    [Fact]
    public void WaitTimeShouldReturnPositiveDurationWhenToIsAfterFrom()
    {
        // Arrange
        DateTimeOffset from = new(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        DateTimeOffset to = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        TimeSpan result = from.WaitTime(to);

        // Assert
        result.TotalHours.ShouldBe(2);
    }

    /// <summary>
    /// Tests that WaitTime returns zero when from and to are equal.
    /// </summary>
    [Fact]
    public void WaitTimeShouldReturnZeroWhenFromEqualsTo()
    {
        // Arrange
        DateTimeOffset time = new(2024, 6, 15, 14, 30, 0, TimeSpan.Zero);

        // Act
        TimeSpan result = time.WaitTime(time);

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that WaitTime returns zero when to is before from.
    /// </summary>
    [Fact]
    public void WaitTimeShouldReturnZeroWhenToIsBeforeFrom()
    {
        // Arrange
        DateTimeOffset from = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset to = new(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        // Act
        TimeSpan result = from.WaitTime(to);

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }

    /// <summary>
    /// Tests that WaitTime returns zero when to is null.
    /// </summary>
    [Fact]
    public void WaitTimeToNullShouldReturnZero()
    {
        // Arrange
        DateTimeOffset from = DateTimeOffset.UtcNow;
        DateTimeOffset? to = null;

        // Act
        TimeSpan result = from.WaitTime(to);

        // Assert
        result.ShouldBe(TimeSpan.Zero);
    }
}