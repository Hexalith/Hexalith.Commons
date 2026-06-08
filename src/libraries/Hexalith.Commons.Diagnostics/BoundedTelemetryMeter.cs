// <copyright file="BoundedTelemetryMeter.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System.Diagnostics.Metrics;

namespace Hexalith.Commons.Diagnostics;

/// <summary>
/// Creates bounded telemetry counters from an <see cref="IMeterFactory"/>-owned meter.
/// </summary>
public sealed class BoundedTelemetryMeter
{
    private readonly Meter _meter;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundedTelemetryMeter"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory registered by the host.</param>
    /// <param name="meterName">The meter name.</param>
    public BoundedTelemetryMeter(IMeterFactory meterFactory, string meterName)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(meterName);

        MeterName = meterName;
        _meter = meterFactory.Create(meterName);
    }

    /// <summary>
    /// Gets the meter name.
    /// </summary>
    public string MeterName { get; }

    /// <summary>
    /// Creates a bounded counter from a counter definition.
    /// </summary>
    /// <param name="definition">The counter definition.</param>
    /// <returns>The bounded counter.</returns>
    public BoundedTelemetryCounter CreateCounter(BoundedTelemetryCounterDefinition definition)
        => new(_meter, definition);
}
