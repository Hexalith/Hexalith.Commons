// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Helpers;

using System.Globalization;

using Hexalith.Extensions.Helpers;

using Shouldly;

public class StringHelperTest
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void DecimalIntoInvariantStringShouldBeExpected(decimal value, string expected) => value.ToInvariantString().ShouldBe(expected);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(-1001)]
    public void DecimalIntoStringAndBackToNumberShouldBeSame(decimal value) => value.ToInvariantString().ToDecimal().ShouldBe(value);

    [Theory]
    [InlineData(0d, "0")]
    [InlineData(1d, "1")]
    [InlineData(10000d, "10000")]
    [InlineData(-1001d, "-1001")]
    public void DoubleIntoInvariantStringShouldBeExpected(double value, string expected) => value.ToInvariantString().ShouldBe(expected);

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    [InlineData(10000d)]
    [InlineData(-1001d)]
    public void DoubleIntoStringAndBackToNumberShouldBeSame(double value) => value.ToInvariantString().ToDouble().ShouldBe(value);

    [Fact]
    public void FormatStringWithNamedPlaceholdersShouldReturnExpected() => StringHelper
            .FormatWithNamedPlaceholders(CultureInfo.InvariantCulture, "Say {Hello} {Number} times", ["hello world", 11])
            .ShouldBe("Say hello world 11 times");

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void IntegerIntoInvariantStringShouldBeExpected(int value, string expected) => value.ToInvariantString().ShouldBe(expected);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10000)]
    [InlineData(-1001)]
    public void IntegerIntoStringAndBackToNumberShouldBeSame(int value) => value.ToInvariantString().ToInteger().ShouldBe(value);

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(10000, "10000")]
    [InlineData(-1001, "-1001")]
    public void InvariantStringIntoIntegerShouldBeExpected(int expected, string value) => value.ToInteger().ShouldBe(expected);

    [Theory]
    [InlineData(0L, "0")]
    [InlineData(1L, "1")]
    [InlineData(10000L, "10000")]
    [InlineData(-1001L, "-1001")]
    public void LongIntoInvariantStringShouldBeExpected(long value, string expected) => value.ToInvariantString().ShouldBe(expected);

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(10000L)]
    [InlineData(-1001L)]
    public void LongIntoStringAndBackToNumberShouldBeSame(long value) => value.ToInvariantString().ToLong().ShouldBe(value);

    [Theory]
    [InlineData("{hello}", "{0}")]
    [InlineData("{4}", "{0}")]
    [InlineData("{{double}} {test}", "{{0}} {1}")]
    [InlineData("hello from {me} and {him}", "hello from {0} and {1}")]
    public void ReplaceNamedPlaceholdersShouldReturnExpectedStringWithIndices(string value, string expected) => StringHelper.ReplacePlaceholderNamesByIndex(value).ShouldBe(expected);
}