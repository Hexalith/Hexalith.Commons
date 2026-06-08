// <copyright file="BoundedTelemetryCounter.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Hexalith.Commons.Diagnostics;

/// <summary>
/// Emits one bounded telemetry counter with a fixed, declared dimension contract.
/// </summary>
public sealed class BoundedTelemetryCounter
{
    private readonly Counter<long> _counter;
    private readonly string[] _dimensionKeys;

    internal BoundedTelemetryCounter(Meter meter, BoundedTelemetryCounterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(meter);
        ArgumentNullException.ThrowIfNull(definition);

        Name = definition.Name;
        _dimensionKeys = definition.DimensionKeys.ToArray();
        _counter = meter.CreateCounter<long>(definition.Name, description: definition.Description);
    }

    /// <summary>
    /// Gets the counter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Emits one counter increment with one dimension.
    /// </summary>
    /// <param name="dimension">The dimension to emit.</param>
    public void AddOne(BoundedMetricDimension dimension)
    {
        TagList tags = new();
        AddDimension(ref tags, dimension, 0);
        ValidateDimensionCount(1);
        _counter.Add(1, tags);
    }

    /// <summary>
    /// Emits one counter increment with two dimensions.
    /// </summary>
    /// <param name="first">The first dimension.</param>
    /// <param name="second">The second dimension.</param>
    public void AddOne(BoundedMetricDimension first, BoundedMetricDimension second)
    {
        TagList tags = new();
        AddDimension(ref tags, first, 0);
        AddDimension(ref tags, second, 1);
        ValidateDimensionCount(2);
        _counter.Add(1, tags);
    }

    /// <summary>
    /// Emits one counter increment with three dimensions.
    /// </summary>
    /// <param name="first">The first dimension.</param>
    /// <param name="second">The second dimension.</param>
    /// <param name="third">The third dimension.</param>
    public void AddOne(BoundedMetricDimension first, BoundedMetricDimension second, BoundedMetricDimension third)
    {
        TagList tags = new();
        AddDimension(ref tags, first, 0);
        AddDimension(ref tags, second, 1);
        AddDimension(ref tags, third, 2);
        ValidateDimensionCount(3);
        _counter.Add(1, tags);
    }

    /// <summary>
    /// Emits one counter increment with the supplied dimensions.
    /// </summary>
    /// <param name="dimensions">The dimensions to emit in the declared order.</param>
    public void AddOne(params BoundedMetricDimension[] dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);

        TagList tags = new();
        for (int i = 0; i < dimensions.Length; i++)
        {
            AddDimension(ref tags, dimensions[i], i);
        }

        ValidateDimensionCount(dimensions.Length);
        _counter.Add(1, tags);
    }

    private void AddDimension(ref TagList tags, BoundedMetricDimension dimension, int index)
    {
        if (index >= _dimensionKeys.Length)
        {
            throw new InvalidOperationException($"Counter '{Name}' expects {_dimensionKeys.Length} dimensions.");
        }

        if (!StringComparer.Ordinal.Equals(_dimensionKeys[index], dimension.Key))
        {
            throw new InvalidOperationException(
                $"Counter '{Name}' expects dimension '{_dimensionKeys[index]}' at position {index}, but received '{dimension.Key}'.");
        }

        tags.Add(dimension.Key, dimension.Value);
    }

    private void ValidateDimensionCount(int actual)
    {
        if (actual != _dimensionKeys.Length)
        {
            throw new InvalidOperationException($"Counter '{Name}' expects {_dimensionKeys.Length} dimensions, but received {actual}.");
        }
    }
}
