// <copyright file="StringHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Strings;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// String helper class.
/// </summary>
public static partial class StringHelper
{
    private const string _dash = "-";

    /// <summary>
    /// Formats the with named placeholders.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="value">The value.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the formatting operation fails due to invalid arguments or placeholders.
    /// </exception>
    public static string FormatWithNamedPlaceholders(IFormatProvider provider, string value, IEnumerable<object>? args)
    {
        string format = ReplacePlaceholderNamesByIndex(value);
        if (args is null)
        {
            return value;
        }

        try
        {
            object[] arguments = [.. args];
            if (arguments.Length > 0)
            {
                return string.Format(
                    provider,
                    format,
                    arguments);
            }

            return value;
        }
        catch (Exception e)
        {
            IEnumerable<string> argValues = args.Select(p => $"{p?.GetType().Name ?? "null"}:{p ?? "null"}");
            throw new InvalidOperationException(
                $"Could not format :\nOriginal={value}\nIndexed={format}\nValues={string.Join("\n", argValues)}", e);
        }
    }

    /// <summary>
    /// Determines whether the specified hostname is RFC 1123 compliant.
    /// </summary>
    /// <param name="hostname">The hostname to check.</param>
    /// <returns><c>true</c> if the specified hostname is RFC 1123 compliant; otherwise, <c>false</c>.</returns>
    public static bool IsRfc1123Compliant(this string? hostname)
    {
        if (string.IsNullOrEmpty(hostname))
        {
            return false;
        }

        // RFC 1123 compliant hostname must:
        // - Contain only a-z, A-Z, 0-9, hyphen, and periods
        // - Cannot start or end with hyphen
        // - Cannot exceed 255 characters in total length
        // - Each label (part between dots) cannot exceed 63 characters
        // - Cannot be empty
        if (hostname.Length > 255)
        {
            return false;
        }

        // Check each label between dots
        foreach (string label in hostname.Split('.'))
        {
            // Each label must be between 1-63 characters
            if (label.Length is < 1 or > 63)
            {
                return false;
            }

            // Cannot start or end with hyphen
            if (label.StartsWith(_dash, StringComparison.Ordinal) || label.EndsWith(_dash, StringComparison.Ordinal))
            {
                return false;
            }

            // Can only contain alphanumeric characters and hyphens
            if (!AlphanumericHyphenRegex().IsMatch(label))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the Jaro-Winkler similarity between two strings using invariant case-insensitive character comparison.
    /// </summary>
    /// <param name="value">The first value.</param>
    /// <param name="other">The second value.</param>
    /// <returns>A similarity score from 0 to 1.</returns>
    public static double JaroWinklerSimilarity(string? value, string? other)
    {
        if (string.Equals(value, other, StringComparison.Ordinal))
        {
            return 1.0;
        }

        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(other))
        {
            return 0.0;
        }

        int matchWindow = Math.Max(0, (Math.Max(value.Length, other.Length) / 2) - 1);

        bool[] valueMatched = new bool[value.Length];
        bool[] otherMatched = new bool[other.Length];

        int matches = 0;
        int transpositions = 0;

        for (int i = 0; i < value.Length; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end = Math.Min(i + matchWindow + 1, other.Length);

            for (int j = start; j < end; j++)
            {
                if (otherMatched[j] || !char.Equals(char.ToLowerInvariant(value[i]), char.ToLowerInvariant(other[j])))
                {
                    continue;
                }

                valueMatched[i] = true;
                otherMatched[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0)
        {
            return 0.0;
        }

        int k = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (!valueMatched[i])
            {
                continue;
            }

            while (!otherMatched[k])
            {
                k++;
            }

            if (!char.Equals(char.ToLowerInvariant(value[i]), char.ToLowerInvariant(other[k])))
            {
                transpositions++;
            }

            k++;
        }

        double jaro = (((double)matches / value.Length)
            + ((double)matches / other.Length)
            + ((double)(matches - (transpositions / 2)) / matches))
            / 3.0;

        int prefixLength = 0;
        for (int i = 0; i < Math.Min(4, Math.Min(value.Length, other.Length)); i++)
        {
            if (char.Equals(char.ToLowerInvariant(value[i]), char.ToLowerInvariant(other[i])))
            {
                prefixLength++;
            }
            else
            {
                break;
            }
        }

        return jaro + (prefixLength * 0.1 * (1.0 - jaro));
    }

    /// <summary>
    /// Replaces the placeholder names by their index.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>System.String.</returns>
    public static string ReplacePlaceholderNamesByIndex(string value)
    {
        int i = 0;
        return EmptyJson().Replace(value, _ => "{" + i++ + "}");
    }

    /// <summary>
    /// Removes non-spacing diacritic marks from a string.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The value without non-spacing diacritic marks, or an empty string for null input.</returns>
    public static string StripDiacritics(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string normalized = value.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new(normalized.Length);

        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                _ = builder.Append(c);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Convert an invariant culture string to a decimal number.
    /// </summary>
    /// <param name="value">The number string.</param>
    /// <returns>The number.</returns>
    public static decimal ToDecimal(this string value)
        => decimal.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert an invariant culture string to a long integer number.
    /// </summary>
    /// <param name="value">The number string.</param>
    /// <returns>The number.</returns>
    public static double ToDouble(this string value)
        => double.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert an invariant culture string to an integer number.
    /// </summary>
    /// <param name="value">The number string.</param>
    /// <returns>The number.</returns>
    public static int ToInteger(this string value)
        => int.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert double float number to invariant culture string.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The value as string.</returns>
    public static string ToInvariantString(this double value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert long number to invariant culture string.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The value as string.</returns>
    public static string ToInvariantString(this long value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert decimal number to invariant culture string.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The value as string.</returns>
    public static string ToInvariantString(this decimal value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert number to invariant culture string.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The value as string.</returns>
    public static string ToInvariantString(this int value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Convert an invariant culture string to a long integer number.
    /// </summary>
    /// <param name="value">The number string.</param>
    /// <returns>The number.</returns>
    public static long ToLong(this string value)
        => long.Parse(value, CultureInfo.InvariantCulture);

    [GeneratedRegex(@"\{\w+\}")]
    private static partial Regex EmptyJson();

    [GeneratedRegex(@"^[a-zA-Z0-9\-]+$")]
    private static partial Regex AlphanumericHyphenRegex();
}
