// <copyright file="BoundedMetricDimension.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System.Globalization;

namespace Hexalith.Commons.Diagnostics;

/// <summary>
/// Represents one bounded metric dimension emitted by a <see cref="BoundedTelemetryCounter"/>.
/// </summary>
public readonly record struct BoundedMetricDimension
{
    private const string NoneSentinel = "None";

    private BoundedMetricDimension(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Gets the dimension key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the bounded dimension token.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a dimension whose value is the lowercase invariant token of an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="key">The dimension key.</param>
    /// <param name="value">The enum value.</param>
    /// <param name="parameterName">The source parameter name used when reporting invalid sentinel values.</param>
    /// <returns>The metric dimension.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is the conventional <c>None</c> sentinel.</exception>
    public static BoundedMetricDimension EnumToken<TEnum>(
        string key,
        TEnum value,
        string parameterName = "value")
        where TEnum : struct, Enum
    {
        ValidateKey(key);

        string token = value.ToString();
        if (StringComparer.Ordinal.Equals(token, NoneSentinel))
        {
            throw new ArgumentException("None is not a valid telemetry dimension token.", parameterName);
        }

        return new BoundedMetricDimension(key, token.ToLower(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Creates a dimension whose value is <c>true</c> or <c>false</c>.
    /// </summary>
    /// <param name="key">The dimension key.</param>
    /// <param name="value">The boolean value.</param>
    /// <returns>The metric dimension.</returns>
    public static BoundedMetricDimension BooleanToken(string key, bool value)
    {
        ValidateKey(key);
        return new BoundedMetricDimension(key, value ? "true" : "false");
    }

    /// <summary>
    /// Creates a dimension whose value is supplied by the domain as an already-approved safe token.
    /// </summary>
    /// <param name="key">The dimension key.</param>
    /// <param name="value">The approved safe token value.</param>
    /// <param name="parameterName">The source parameter name used when reporting invalid values.</param>
    /// <returns>The metric dimension.</returns>
    public static BoundedMetricDimension SafeToken(
        string key,
        string value,
        string parameterName = "value")
    {
        ValidateKey(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Any(char.IsControl))
        {
            throw new ArgumentException("Telemetry dimension tokens must not contain control characters.", parameterName);
        }

        return new BoundedMetricDimension(key, value);
    }

    private static void ValidateKey(string key)
        => ArgumentException.ThrowIfNullOrWhiteSpace(key);
}
