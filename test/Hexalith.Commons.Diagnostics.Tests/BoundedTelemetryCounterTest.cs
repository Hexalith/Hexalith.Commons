// <copyright file="BoundedTelemetryCounterTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Diagnostics.Tests;

using System.Diagnostics.Metrics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

/// <summary>
/// Verifies the bounded telemetry counter helper.
/// </summary>
public sealed class BoundedTelemetryCounterTest
{
    private enum TestStatus
    {
        None = 0,
        Pass = 1,
        InfrastructureFailure = 2,
    }

    /// <summary>
    /// Constructor guard clauses reject missing meter factory and meter name.
    /// </summary>
    [Fact]
    public void ConstructorShouldRejectMissingMeterFactoryOrName()
    {
        _ = Should.Throw<ArgumentNullException>(() => new BoundedTelemetryMeter(null!, "Hexalith.Test"));

        using FakeMeterFactory factory = new();
        _ = Should.Throw<ArgumentException>(() => new BoundedTelemetryMeter(factory, string.Empty));
    }

    /// <summary>
    /// Counter creation uses the supplied meter name and the declared counter name.
    /// </summary>
    [Fact]
    public void CreateCounterShouldUseMeterFactoryAndCounterDefinition()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryMeter meter = new(factory, "Hexalith.Test");
        BoundedTelemetryCounter counter = meter.CreateCounter(
            new BoundedTelemetryCounterDefinition(
                "hexalith.test.outcomes",
                "Test outcomes",
                "status_class",
                "retryable"));

        List<MeasurementRecord<long>> captured = [];
        using MeterListener listener = StartListening<long>("Hexalith.Test", "hexalith.test.outcomes", captured);

        counter.AddOne(
            BoundedMetricDimension.EnumToken("status_class", TestStatus.Pass),
            BoundedMetricDimension.BooleanToken("retryable", false));

        factory.CreatedMeters.ShouldBe(1);
        captured.Count.ShouldBe(1);
        captured[0].Value.ShouldBe(1L);
        captured[0].TagKeys.ShouldBe(["status_class", "retryable"]);
        captured[0].TagValue("status_class").ShouldBe("pass");
        captured[0].TagValue("retryable").ShouldBe("false");
    }

    /// <summary>
    /// Enum token formatting is lowercase invariant and rejects the conventional None sentinel.
    /// </summary>
    [Fact]
    public void EnumTokenShouldFormatLowercaseInvariantAndRejectNoneSentinel()
    {
        BoundedMetricDimension.EnumToken("status_class", TestStatus.InfrastructureFailure)
            .Value.ShouldBe("infrastructurefailure");

        _ = Should.Throw<ArgumentException>(() =>
            BoundedMetricDimension.EnumToken("status_class", TestStatus.None, "statusClass"));
    }

    /// <summary>
    /// The helper validates the declared dimension key order before recording.
    /// </summary>
    [Fact]
    public void AddOneShouldRejectUnexpectedDimensionKeyOrder()
    {
        using FakeMeterFactory factory = new();
        BoundedTelemetryCounter counter = new BoundedTelemetryMeter(factory, "Hexalith.Test")
            .CreateCounter(new BoundedTelemetryCounterDefinition(
                "hexalith.test.outcomes",
                "Test outcomes",
                "status_class",
                "blocking"));

        _ = Should.Throw<InvalidOperationException>(() =>
            counter.AddOne(
                BoundedMetricDimension.BooleanToken("blocking", true),
                BoundedMetricDimension.EnumToken("status_class", TestStatus.Pass)));
    }

    /// <summary>
    /// Safe string tokens reject empty and control-character values to avoid accidental free-text dimensions.
    /// </summary>
    [Fact]
    public void SafeTokenShouldRejectEmptyOrControlCharacterValues()
    {
        _ = Should.Throw<ArgumentException>(() => BoundedMetricDimension.SafeToken("gate_id", string.Empty, "gateId"));
        _ = Should.Throw<ArgumentException>(() => BoundedMetricDimension.SafeToken("gate_id", "gate\nraw", "gateId"));

        BoundedMetricDimension.SafeToken("gate_id", "tenant-isolation").Value.ShouldBe("tenant-isolation");
    }

    /// <summary>
    /// The logging hook guards null inputs and delegates safe templates to ILogger.
    /// </summary>
    [Fact]
    public void InformationLogShouldGuardInputsAndEmitSafeTemplate()
    {
        CapturingLogger logger = new();

        _ = Should.Throw<ArgumentNullException>(() => BoundedTelemetryLog.Information(null!, "Event {Value}", "safe"));
        _ = Should.Throw<ArgumentException>(() => BoundedTelemetryLog.Information(logger, string.Empty));

        BoundedTelemetryLog.Information(logger, "TelemetryEvent value={Value}", "bounded");

        logger.Messages.Single().ShouldBe("TelemetryEvent value=bounded");
    }

    private static MeterListener StartListening<T>(
        string meterName,
        string instrumentName,
        List<MeasurementRecord<T>> captured)
        where T : struct
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
        listener.SetMeasurementEventCallback<T>((instrument, measurement, tags, _) =>
        {
            if (instrument.Meter.Name == meterName && instrument.Name == instrumentName)
            {
                captured.Add(new MeasurementRecord<T>(measurement, tags.ToArray()));
            }
        });
        listener.Start();
        return listener;
    }

    private sealed record MeasurementRecord<T>(T Value, KeyValuePair<string, object?>[] Tags)
        where T : struct
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

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NullLogger.Instance.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
}
