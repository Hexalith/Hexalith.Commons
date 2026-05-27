// <copyright file="Rfc1123StringExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.StringEncoders;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Provides methods for encoding and decoding strings to be compliant with RFC 1123.
/// This includes escaping characters that are not allowed in RFC 1123 formats.
/// It also provides a reversible mechanism for encoding arbitrary strings
/// using a restricted character set often associated with RFC 1123 contexts.
/// </summary>
public static class Rfc1123StringExtensions
{
    private const char _escapeChar = '_';

    /// <summary>
    /// Define the set of characters considered "compliant" and won't be escaped.
    /// RFC 1123 itself mostly defines a date format (letters, digits, ',', ':', ' ').
    /// For broader string compatibility (like headers or identifiers potentially
    /// used alongside RFC 1123 dates), a stricter set is often safer.
    /// We'll use Alphanumeric, Hyphen, and Dot as the base "safe" set.
    /// All other characters, including space, comma, colon, control chars,
    /// and Unicode chars will be escaped to ensure lossless round-tripping.
    /// </summary>
    private static readonly HashSet<char> _allowedChars = [.. "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-."];

    /// <summary>
    /// Converts a string previously escaped using ToRFC1123Compliant back to its original form.
    /// It reverses the _XX (UTF-8 byte hex) and __ (literal underscore) escaping.
    /// </summary>
    /// <param name="input">The escaped string.</param>
    /// <returns>The original, decoded string, or the original string if null or empty.</returns>
    /// <exception cref="FormatException">Thrown if the input string contains invalid escape sequences.</exception>
    public static string FromRFC1123(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var resultBuilder = new StringBuilder(input.Length);

        // Use MemoryStream to collect bytes that form multi-byte UTF-8 characters
        using (var byteStream = new MemoryStream())
        {
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];

                if (c == _escapeChar)
                {
                    i = Escape(input, resultBuilder, byteStream, i);
                }
                else
                {
                    // Not an escape character
                    // Decode any pending bytes before appending the literal char
                    DecodePendingBytes(byteStream, resultBuilder);
                    _ = resultBuilder.Append(c);
                    i++; // Consumed the character
                }
            }

            // Decode any remaining bytes at the end of the string
            DecodePendingBytes(byteStream, resultBuilder);
        }

        return resultBuilder.ToString();
    }

    /// <summary>
    /// Converts a string into a representation where characters not in the allowed set
    /// (A-Z, a-z, 0-9, '-', '.') are escaped using a reversible mechanism (_XX for UTF-8 bytes).
    /// The escape character itself (_) is escaped as __.
    /// This ensures the string can be used in contexts with restricted character sets
    /// while preserving the original data for later reversal.
    /// Note: This is NOT strictly producing an RFC 1123 date string, but rather encoding
    /// an arbitrary string using a restricted character set often associated with
    /// RFC 1123 contexts (like headers), ensuring reversibility.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="provider">The format provider to use for formatting byte values.</param>
    /// <returns>The escaped string, or the original string if null or empty.</returns>
    public static string ToRFC1123(this string input, IFormatProvider? provider = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder(input.Length * 2); // Estimate capacity

        foreach (char c in input)
        {
            if (c == _escapeChar)
            {
                _ = sb.Append(_escapeChar).Append(_escapeChar); // Escape the escape char
            }
            else if (_allowedChars.Contains(c))
            {
                _ = sb.Append(c); // Append allowed char directly
            }
            else
            {
                // Character is not allowed, escape its UTF-8 bytes
                foreach (byte b in Encoding.UTF8.GetBytes(c.ToString()))
                {
                    // Append _XX where XX is the uppercase hex representation of the byte
                    _ = sb.Append(_escapeChar).Append(b.ToString("X2", provider));
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Helper to decode collected bytes from the MemoryStream.
    /// </summary>
    /// <param name="byteStream">MemoryStream containing pending bytes to decode.</param>
    /// <param name="resultBuilder">StringBuilder to append the decoded string.</param>
    private static void DecodePendingBytes(MemoryStream byteStream, StringBuilder resultBuilder)
    {
        if (byteStream.Length > 0)
        {
            _ = resultBuilder.Append(Encoding.UTF8.GetString(byteStream.ToArray()));

            // Reset stream for next sequence
            byteStream.SetLength(0);
            byteStream.Position = 0;
        }
    }

    private static int Escape(string input, StringBuilder resultBuilder, MemoryStream byteStream, int i)
    {
        // Potential escape sequence start
        if (i + 1 >= input.Length)
        {
            // Trailing escape char - invalid sequence
            throw new FormatException($"Invalid escape sequence: trailing '{_escapeChar}' at end of string.");
        }

        char nextChar = input[i + 1];

        if (nextChar == _escapeChar)
        {
            // It's '__', representing a literal '_'
            // Decode any pending bytes before appending the literal char
            DecodePendingBytes(byteStream, resultBuilder);
            _ = resultBuilder.Append(_escapeChar);
            i += 2; // Consumed '__'
        }
        else
        {
            // Should be '_XX' (hex byte representation)
            if (i + 2 >= input.Length)
            {
                // Not enough characters for _XX
                throw new FormatException($"Invalid escape sequence: '{_escapeChar}{nextChar}' is incomplete at end of string.");
            }

            string hex = input.Substring(i + 1, 2);
            try
            {
                byte b = Convert.ToByte(hex, 16);
                byteStream.WriteByte(b); // Collect the byte
                i += 3; // Consumed '_XX'
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Invalid hex sequence '{hex}' in escape sequence at index {i}.", ex);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Convert.ToByte can throw this for non-hex chars
                throw new FormatException($"Invalid hex sequence '{hex}' in escape sequence at index {i}.", ex);
            }
        }

        return i;
    }
}