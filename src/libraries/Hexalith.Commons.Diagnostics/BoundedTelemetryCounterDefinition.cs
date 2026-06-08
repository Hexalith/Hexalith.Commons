// <copyright file="BoundedTelemetryCounterDefinition.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Diagnostics;

/// <summary>
/// Defines a bounded telemetry counter and the exact dimension-key sequence it accepts.
/// </summary>
public sealed class BoundedTelemetryCounterDefinition
{
    private readonly string[] _dimensionKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundedTelemetryCounterDefinition"/> class.
    /// </summary>
    /// <param name="name">The counter name.</param>
    /// <param name="description">The counter description.</param>
    /// <param name="dimensionKeys">The exact dimension keys emitted by the counter, in order.</param>
    public BoundedTelemetryCounterDefinition(
        string name,
        string description,
        params string[] dimensionKeys)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(dimensionKeys);

        for (int i = 0; i < dimensionKeys.Length; i++)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dimensionKeys[i], nameof(dimensionKeys));
        }

        if (dimensionKeys.Distinct(StringComparer.Ordinal).Count() != dimensionKeys.Length)
        {
            throw new ArgumentException("Telemetry dimension keys must be unique.", nameof(dimensionKeys));
        }

        Name = name;
        Description = description;
        _dimensionKeys = dimensionKeys.ToArray();
    }

    /// <summary>
    /// Gets the counter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the counter description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the exact dimension-key sequence.
    /// </summary>
    public IReadOnlyList<string> DimensionKeys => _dimensionKeys;
}
