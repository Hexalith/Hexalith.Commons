// <copyright file="Rfc1123StringExtensionsTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.StringEncoders;

using System;

using Hexalith.Commons.StringEncoders;

using Shouldly;

/// <summary>
/// Tests for the <see cref="Rfc1123StringExtensions"/> class.
/// </summary>
public class Rfc1123StringExtensionsTest
{
    /// <summary>
    /// Tests that ToRFC1123 returns null when input is null.
    /// </summary>
    [Fact]
    public void ToRFC1123_NullInput_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        string? result = input!.ToRFC1123();

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests that ToRFC1123 returns empty string when input is empty.
    /// </summary>
    [Fact]
    public void ToRFC1123_EmptyInput_ReturnsEmptyString()
    {
        // Arrange
        string input = string.Empty;

        // Act
        string result = input.ToRFC1123();

        // Assert
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that ToRFC1123 returns the same string when input contains only allowed characters.
    /// </summary>
    [Fact]
    public void ToRFC1123_AllowedCharsOnly_ReturnsSameString()
    {
        // Arrange
        string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-.";

        // Act
        string result = input.ToRFC1123();

        // Assert
        result.ShouldBe(input);
    }

    /// <summary>
    /// Tests that ToRFC1123 escapes characters that are not in the allowed set.
    /// </summary>
    [Fact]
    public void ToRFC1123_DisallowedChars_EscapesCharacters()
    {
        // Arrange
        string input = "Hello World!";

        // Act
        string result = input.ToRFC1123();

        // Assert
        // Space is encoded as _20 (UTF-8 hex for space)
        // ! is encoded as _21 (UTF-8 hex for !)
        result.ShouldBe("Hello_20World_21");
    }

    /// <summary>
    /// Tests that ToRFC1123 escapes the escape character itself.
    /// </summary>
    [Fact]
    public void ToRFC1123_EscapeCharInInput_DoublesEscapeChar()
    {
        // Arrange
        string input = "Hello_World";

        // Act
        string result = input.ToRFC1123();

        // Assert
        // _ is escaped as __
        result.ShouldBe("Hello__World");
    }

    /// <summary>
    /// Tests that ToRFC1123 correctly handles Unicode characters.
    /// </summary>
    [Fact]
    public void ToRFC1123_UnicodeChars_EscapesUTF8Bytes()
    {
        // Arrange
        string input = "Café"; // é is a multi-byte UTF-8 character

        // Act
        string result = input.ToRFC1123();

        // Assert
        // é is encoded as _C3_A9 (UTF-8 hex for é)
        result.ShouldBe("Caf_C3_A9");
    }

    /// <summary>
    /// Tests that FromRFC1123 returns null when input is null.
    /// </summary>
    [Fact]
    public void FromRFC1123_NullInput_ReturnsNull()
    {
        // Arrange
        string? input = null;

        // Act
        string? result = input!.FromRFC1123();

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests that FromRFC1123 returns empty string when input is empty.
    /// </summary>
    [Fact]
    public void FromRFC1123_EmptyInput_ReturnsEmptyString()
    {
        // Arrange
        string input = string.Empty;

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that FromRFC1123 correctly decodes escaped characters.
    /// </summary>
    [Fact]
    public void FromRFC1123_EscapedChars_DecodesCorrectly()
    {
        // Arrange
        string input = "Hello_20World_21";

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBe("Hello World!");
    }

    /// <summary>
    /// Tests that FromRFC1123 correctly decodes escaped escape characters.
    /// </summary>
    [Fact]
    public void FromRFC1123_EscapedEscapeChar_DecodesCorrectly()
    {
        // Arrange
        string input = "Hello__World";

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBe("Hello_World");
    }

    /// <summary>
    /// Tests that FromRFC1123 correctly decodes Unicode characters.
    /// </summary>
    [Fact]
    public void FromRFC1123_EscapedUnicodeChars_DecodesCorrectly()
    {
        // Arrange
        string input = "Caf_C3_A9";

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBe("Café");
    }

    /// <summary>
    /// Tests that FromRFC1123 throws FormatException for invalid escape sequences.
    /// </summary>
    [Fact]
    public void FromRFC1123_InvalidEscapeSequence_ThrowsFormatException()
    {
        // Arrange
        string input = "Hello_";

        // Act & Assert
        Should.Throw<FormatException>(() => input.FromRFC1123());
    }

    /// <summary>
    /// Tests that FromRFC1123 throws FormatException for incomplete escape sequences.
    /// </summary>
    [Fact]
    public void FromRFC1123_IncompleteEscapeSequence_ThrowsFormatException()
    {
        // Arrange
        string input = "Hello_2";

        // Act & Assert
        Should.Throw<FormatException>(() => input.FromRFC1123());
    }

    /// <summary>
    /// Tests that FromRFC1123 throws FormatException for invalid hex in escape sequences.
    /// </summary>
    [Fact]
    public void FromRFC1123_InvalidHexInEscapeSequence_ThrowsFormatException()
    {
        // Arrange
        string input = "Hello_ZZ";

        // Act & Assert
        Should.Throw<FormatException>(() => input.FromRFC1123());
    }

    /// <summary>
    /// Tests that a string can be round-trip converted (ToRFC1123 followed by FromRFC1123).
    /// </summary>
    [Fact]
    public void RoundTrip_VariousCharacters_PreservesOriginalString()
    {
        // Arrange
        string[] testStrings =
        [
            "Hello World!",
            "Café au lait",
            "123_456",
            "Special chars: !@#$%^&*()",
            "Unicode: 你好, こんにちは, 안녕하세요",
            "Mixed: ABC-123.xyz_"
        ];

        foreach (string original in testStrings)
        {
            // Act
            string encoded = original.ToRFC1123();
            string decoded = encoded.FromRFC1123();

            // Assert
            decoded.ShouldBe(original, $"Failed round-trip for: {original}");
        }
    }

    /// <summary>
    /// Tests that FromRFC1123 correctly handles mixed content with both escaped and non-escaped characters.
    /// </summary>
    [Fact]
    public void FromRFC1123_MixedContent_DecodesCorrectly()
    {
        // Arrange
        string input = "Hello_20__World_21";

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBe("Hello _World!");
    }

    /// <summary>
    /// Tests that FromRFC1123 correctly handles consecutive escaped bytes.
    /// </summary>
    [Fact]
    public void FromRFC1123_ConsecutiveEscapedBytes_DecodesCorrectly()
    {
        // Arrange - "你" is encoded as E4 BD A0 in UTF-8
        string input = "Hello_E4_BD_A0World";

        // Act
        string result = input.FromRFC1123();

        // Assert
        result.ShouldBe("Hello你World");
    }
}