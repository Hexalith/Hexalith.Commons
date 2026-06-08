// <copyright file="BoundedTelemetryHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Diagnostics.Tests;

using System.Diagnostics.Metrics;

using Shouldly;

/// <summary>
/// Verifies the bounded telemetry definition, counter overloads, dimension tokens, and meter reuse
/// that the promoted FR-15 helper exposes to adopting modules.
/// </summary>
public sealed class BoundedTelemetryHelperTest
{
    private enum TestStatus
    {
        None = 0,
        Pass = 1,
        InfrastructureFailure = 2,
    }

    /// <summary>
    /// The counter definition rejects missing name, missing description, and a null dimension-key array.
    /// </summary>
    [Fact]
    public void DefinitionShouldRejectMissingNameDescriptionOrKeys()
    {
        _ = Should.Throw<ArgumentException>(() =>
            new BoundedTelemetryCounterDefinition(string.Empty, "Description", "status_class"));
        _ = Should.Throw<ArgumentException>(() =>
            new BoundedTelemetryCounterDefinition("hexalith.test.outcomes", string.Empty, "status_class"));

        string[] nullKeys = null!;
        _ = Should.Throw<ArgumentNullException>(() =>
            new BoundedTelemetryCounterDefinition("hexalith.test.outcomes", "Description", nullKeys));
    }

    /// <summary>
    /// The counter definition rejects blank dimension keys and duplicate dimension keys.
    /// </summary>
    [Fact]
    public void DefinitionShouldRejectBlankOrDuplicateDimensionKeys()
    {
        _ = Should.Throw<ArgumentException>(() =>
            new BoundedTelemetryCounterDefinition("hexalith.test.outcomes", "Description", "status_class", "  "));
        _ = Should.Throw<ArgumentException>(() =>
            new BoundedTelemetryCounterDefinition("hexalith.test.outcomes", "Description", "status_class", "status_class"));
    }

    /// <summary>
    /// The counter definition exposes the dimension keys in the exact declared order.
    /// </summary>
    [Fact]
    public void DefinitionShouldExposeDimensionKeysInDeclaredOrder()
    {
        BoundedTelemetryCounterDefinition definition = new(
            "hexalith.test.outcomes",
            "Test outcomes",
            "status_class",
            "operation_class",
            "retryable");

        definition.Name.ShouldBe("hexalith.test.outcomes");
        definition.Description.ShouldBe("Test outcomes");
        definition.DimensionKeys.ShouldBe(["status_class", "operation_class", "retryable"]);
    }

    /// <summary>
    /// The single-dimension overload emits exactly one tag in the declared position.
    /// </summary>
    [Fact]
    public void AddOneWithSingleDimensionShouldEmitOneTag()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition("hexalith.test.single", "Single", "rebuild_class"));

        List<MeasurementRecord> captured = [];
        using MeterListener listener = StartListening("Hexalith.Test", "hexalith.test.single", captured);

        counter.AddOne(BoundedMetricDimension.EnumToken("rebuild_class", TestStatus.Pass));

        captured.Single().TagKeys.ShouldBe(["rebuild_class"]);
        captured.Single().TagValue("rebuild_class").ShouldBe("pass");
    }

    /// <summary>
    /// The three-dimension overload emits all three tags in the declared order.
    /// </summary>
    [Fact]
    public void AddOneWithThreeDimensionsShouldEmitAllTagsInOrder()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition(
                "hexalith.test.triple",
                "Triple",
                "status_class",
                "operation_class",
                "retryable"));

        List<MeasurementRecord> captured = [];
        using MeterListener listener = StartListening("Hexalith.Test", "hexalith.test.triple", captured);

        counter.AddOne(
            BoundedMetricDimension.EnumToken("status_class", TestStatus.InfrastructureFailure),
            BoundedMetricDimension.SafeToken("operation_class", "write"),
            BoundedMetricDimension.BooleanToken("retryable", true));

        MeasurementRecord record = captured.Single();
        record.TagKeys.ShouldBe(["status_class", "operation_class", "retryable"]);
        record.TagValue("status_class").ShouldBe("infrastructurefailure");
        record.TagValue("operation_class").ShouldBe("write");
        record.TagValue("retryable").ShouldBe("true");
    }

    /// <summary>
    /// The three-dimension overload validates the declared key order.
    /// </summary>
    [Fact]
    public void AddOneWithThreeDimensionsShouldRejectUnexpectedKeyOrder()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition(
                "hexalith.test.triple",
                "Triple",
                "status_class",
                "operation_class",
                "retryable"));

        _ = Should.Throw<InvalidOperationException>(() =>
            counter.AddOne(
                BoundedMetricDimension.EnumToken("status_class", TestStatus.Pass),
                BoundedMetricDimension.BooleanToken("retryable", true),
                BoundedMetricDimension.SafeToken("operation_class", "write")));
    }

    /// <summary>
    /// The params overload emits the supplied dimensions and rejects a dimension count that does not
    /// match the declared key count.
    /// </summary>
    [Fact]
    public void AddOneParamsShouldEmitDeclaredCountAndRejectMismatch()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition(
                "hexalith.test.params",
                "Params",
                "status_class",
                "retryable"));

        List<MeasurementRecord> captured = [];
        using MeterListener listener = StartListening("Hexalith.Test", "hexalith.test.params", captured);

        BoundedMetricDimension[] dimensions =
        [
            BoundedMetricDimension.EnumToken("status_class", TestStatus.Pass),
            BoundedMetricDimension.BooleanToken("retryable", false),
        ];
        counter.AddOne(dimensions);

        captured.Single().TagKeys.ShouldBe(["status_class", "retryable"]);

        _ = Should.Throw<InvalidOperationException>(() =>
            counter.AddOne(BoundedMetricDimension.EnumToken("status_class", TestStatus.Pass)));
    }

    /// <summary>
    /// The params overload guards against a null dimension array.
    /// </summary>
    [Fact]
    public void AddOneParamsShouldRejectNullDimensionArray()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition("hexalith.test.params", "Params", "status_class"));

        BoundedMetricDimension[] nullDimensions = null!;
        _ = Should.Throw<ArgumentNullException>(() => counter.AddOne(nullDimensions));
    }

    /// <summary>
    /// The boolean token formats both true and false, and every token factory guards a blank key.
    /// </summary>
    [Fact]
    public void DimensionTokensShouldFormatBooleansAndGuardBlankKeys()
    {
        BoundedMetricDimension.BooleanToken("retryable", true).Value.ShouldBe("true");
        BoundedMetricDimension.BooleanToken("retryable", false).Value.ShouldBe("false");

        _ = Should.Throw<ArgumentException>(() => BoundedMetricDimension.BooleanToken(" ", true));
        _ = Should.Throw<ArgumentException>(() => BoundedMetricDimension.EnumToken(string.Empty, TestStatus.Pass));
        _ = Should.Throw<ArgumentException>(() => BoundedMetricDimension.SafeToken(string.Empty, "value"));
    }

    /// <summary>
    /// A single bounded meter is reused across every counter it creates rather than allocating a meter per counter.
    /// </summary>
    [Fact]
    public void MeterShouldBeReusedAcrossCounters()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryMeter meter = new(factory, "Hexalith.Test");

        BoundedTelemetryCounter first = meter.CreateCounter(
            new BoundedTelemetryCounterDefinition("hexalith.test.first", "First", "status_class"));
        BoundedTelemetryCounter second = meter.CreateCounter(
            new BoundedTelemetryCounterDefinition("hexalith.test.second", "Second", "status_class"));

        first.Name.ShouldBe("hexalith.test.first");
        second.Name.ShouldBe("hexalith.test.second");
        meter.MeterName.ShouldBe("Hexalith.Test");
        factory.CreatedMeters.ShouldBe(1);
    }

    private static MeterListener StartListening(
        string meterName,
        string instrumentName,
        List<MeasurementRecord> captured)
    {
        MeterListener listener = new()
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == meterName && instrument.Name == instrumentName)
                {
                    l.EnableMeasurementEvents(instrument, null);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, _) =>
        {
            if (instrument.Meter.Name == meterName && instrument.Name == instrumentName)
            {
                captured.Add(new MeasurementRecord(measurement, tags.ToArray()));
            }
        });
        listener.Start();
        return listener;
    }

    private sealed record MeasurementRecord(long Value, KeyValuePair<string, object?>[] Tags)
    {
        public string[] TagKeys => [.. Tags.Select(static t => t.Key)];

        public string? TagValue(string key)
            => Tags.FirstOrDefault(t => t.Key == key).Value?.ToString();
    }

    private sealed class FakeMeterFactory : IMeterFactory
    {
        private readonly List<Meter> _meters = [];

        public int CreatedMeters => _meters.Count;

        public Meter Create(MeterOptions options)
        {
            Meter meter = new(options);
            _meters.Add(meter);
            return meter;
        }

        public void Dispose()
        {
            foreach (Meter meter in _meters)
            {
                meter.Dispose();
            }

            _meters.Clear();
        }
    }
}
