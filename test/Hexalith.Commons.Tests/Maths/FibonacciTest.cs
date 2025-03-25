// <copyright file="FibonacciTest.cs" company="ITANEO">
//     Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT
//     license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Maths;

using Hexalith.Commons.Maths;

using Shouldly;

/// <summary>
/// Provides unit tests for the Fibonacci sequence calculations.
/// </summary>
public class FibonacciTest
{
    /// <summary>
    /// Checks the first Fibonacci values.
    /// </summary>
    /// <param name="sequence">The position in the Fibonacci sequence.</param>
    /// <param name="value">The expected Fibonacci value at the given position.</param>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(5, 5)]
    [InlineData(6, 8)]
    [InlineData(7, 13)]
    [InlineData(8, 21)]
    [InlineData(9, 34)]
    [InlineData(10, 55)]
    [InlineData(11, 89)]
    public void CheckFirstFibonacciValues(long sequence, long value)
    {
        long result = FibonacciSequence.Number(sequence);
        result.ShouldBe(value);
    }
}