// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Helpers;

using System.Globalization;

using Hexalith.Extensions.Helpers;

using Shouldly;

/// <summary>
/// Test class for StringHelper functionality.
/// Contains unit tests to verify string conversion, formatting,
/// and placeholder replacement operations across various numeric types
/// and string manipulation scenarios.
/// </summary>
public class StringHelperTest
{
    /// <summary>
    /// Tests that decimal values are correctly converted to their
    /// invariant culture string representation.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    /// <param name="expected">The expected string representation.</param>
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void DecimalIntoInvariantStringShouldBeExpected(decimal value, string expected) => value.ToInvariantString().ShouldBe(expected);

    /// <summary>
    /// Tests that decimal values maintain their value when converted
    /// to string and back to decimal using invariant culture.
    /// </summary>
    /// <param name="value">The decimal value to test round-trip conversion.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(-1001)]
    public void DecimalIntoStringAndBackToNumberShouldBeSame(decimal value) => value.ToInvariantString().ToDecimal().ShouldBe(value);

    /// <summary>
    /// Tests that double values are correctly converted to their
    /// invariant culture string representation.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <param name="expected">The expected string representation.</param>
    [Theory]
    [InlineData(0d, "0")]
    [InlineData(1d, "1")]
    [InlineData(10000d, "10000")]
    [InlineData(-1001d, "-1001")]
    public void DoubleIntoInvariantStringShouldBeExpected(double value, string expected) => value.ToInvariantString().ShouldBe(expected);

    /// <summary>
    /// Tests that double values maintain their value when converted
    /// to string and back to double using invariant culture.
    /// </summary>
    /// <param name="value">The double value to test round-trip conversion.</param>
    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    [InlineData(10000d)]
    [InlineData(-1001d)]
    public void DoubleIntoStringAndBackToNumberShouldBeSame(double value) => value.ToInvariantString().ToDouble().ShouldBe(value);

    /// <summary>
    /// Tests that string formatting with named placeholders correctly
    /// replaces the placeholders with provided values.
    /// </summary>
    [Fact]
    public void FormatStringWithNamedPlaceholdersShouldReturnExpected() => StringHelper
            .FormatWithNamedPlaceholders(CultureInfo.InvariantCulture, "Say {Hello} {Number} times", ["hello world", 11])
            .ShouldBe("Say hello world 11 times");

    /// <summary>
    /// Tests that integer values are correctly converted to their
    /// invariant culture string representation.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <param name="expected">The expected string representation.</param>
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void IntegerIntoInvariantStringShouldBeExpected(int value, string expected) => value.ToInvariantString().ShouldBe(expected);

    /// <summary>
    /// Tests that integer values maintain their value when converted
    /// to string and back to integer using invariant culture.
    /// </summary>
    /// <param name="value">The integer value to test round-trip conversion.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(-1001)]
    public void IntegerIntoStringAndBackToNumberShouldBeSame(int value) => value.ToInvariantString().ToInteger().ShouldBe(value);

    /// <summary>
    /// Tests that string values are correctly converted to their
    /// integer representation using invariant culture.
    /// </summary>
    /// <param name="expected">The expected integer value.</param>
    /// <param name="value">The string value to convert.</param>
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void InvariantStringIntoIntegerShouldBeExpected(int expected, string value) => value.ToInteger().ShouldBe(expected);

    /// <summary>
    /// Tests that long values are correctly converted to their
    /// invariant culture string representation.
    /// </summary>
    /// <param name="value">The long value to convert.</param>
    /// <param name="expected">The expected string representation.</param>
    [Theory]
    [InlineData(0L, "0")]
    [InlineData(1L, "1")]
    [InlineData(10000L, "10000")]
    [InlineData(-1001L, "-1001")]
    public void LongIntoInvariantStringShouldBeExpected(long value, string expected) => value.ToInvariantString().ShouldBe(expected);

    /// <summary>
    /// Tests that long values maintain their value when converted
    /// to string and back to long using invariant culture.
    /// </summary>
    /// <param name="value">The long value to test round-trip conversion.</param>
    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(10000L)]
    [InlineData(-1001L)]
    public void LongIntoStringAndBackToNumberShouldBeSame(long value) => value.ToInvariantString().ToLong().ShouldBe(value);

    /// <summary>
    /// Tests that named placeholders in strings are correctly replaced
    /// with indexed placeholders for string formatting.
    /// </summary>
    /// <param name="value">The input string with named placeholders.</param>
    /// <param name="expected">The expected string with indexed placeholders.</param>
    [Theory]
    [InlineData("{hello}", "{0}")]
    [InlineData("{4}", "{0}")]
    [InlineData("{{double}} {test}", "{{0}} {1}")]
    [InlineData("hello from {me} and {him}", "hello from {0} and {1}")]
    public void ReplaceNamedPlaceholdersShouldReturnExpectedStringWithIndices(string value, string expected) => StringHelper.ReplacePlaceholderNamesByIndex(value).ShouldBe(expected);
}