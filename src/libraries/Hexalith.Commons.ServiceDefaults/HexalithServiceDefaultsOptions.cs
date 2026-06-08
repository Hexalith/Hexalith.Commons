// <copyright file="HexalithServiceDefaultsOptions.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Hexalith.Commons.ServiceDefaults;

/// <summary>
/// Domain-neutral options for shared Hexalith service defaults.
/// </summary>
public sealed class HexalithServiceDefaultsOptions
{
    /// <summary>
    /// Gets or sets the primary service/resource name. Defaults to the host application name.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets custom activity source names to add after the host application source.
    /// </summary>
    public IList<string> ActivitySourceNames { get; } = [];

    /// <summary>
    /// Gets custom meter names to add to metrics after the shared instrumentation defaults.
    /// </summary>
    public IList<string> MeterNames { get; } = [];

    /// <summary>
    /// Gets or sets the aggregate health endpoint path.
    /// </summary>
    public string HealthEndpointPath { get; set; } = "/health";

    /// <summary>
    /// Gets or sets the liveness endpoint path.
    /// </summary>
    public string LivenessEndpointPath { get; set; } = "/alive";

    /// <summary>
    /// Gets or sets the readiness endpoint path.
    /// </summary>
    public string ReadinessEndpointPath { get; set; } = "/ready";

    /// <summary>
    /// Gets or sets the tag used to select liveness checks.
    /// </summary>
    public string LivenessTag { get; set; } = "live";

    /// <summary>
    /// Gets or sets the tag used to select readiness checks.
    /// </summary>
    public string ReadinessTag { get; set; } = "ready";

    /// <summary>
    /// Gets or sets a value indicating whether the shared self check is registered.
    /// </summary>
    public bool RegisterDefaultSelfCheck { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the shared self check is both live and ready.
    /// </summary>
    public bool DefaultSelfCheckIncludesReadiness { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry log records include formatted messages.
    /// </summary>
    public bool IncludeFormattedLogMessage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry log records include scopes.
    /// </summary>
    public bool IncludeLogScopes { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether JSON console logging is registered.
    /// </summary>
    public bool AddJsonConsoleLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the OTLP exporter is registered when the OTLP endpoint is configured.
    /// </summary>
    public bool UseOtlpExporterWhenConfigured { get; set; } = true;

    /// <summary>
    /// Gets or sets the development response writer used for the aggregate health and readiness endpoints.
    /// </summary>
    public Func<HttpContext, HealthReport, Task>? DevelopmentHealthResponseWriter { get; set; }
        = HexalithServiceDefaults.WriteDevelopmentHealthJsonResponseAsync;

    /// <summary>
    /// Gets custom health check registration hooks. Shared defaults run before these hooks.
    /// </summary>
    public IList<Action<IHealthChecksBuilder>> ConfigureHealthChecks { get; } = [];

    /// <summary>
    /// Gets custom logging hooks. Shared defaults run before these hooks.
    /// </summary>
    public IList<Action<OpenTelemetryLoggerOptions>> ConfigureLogging { get; } = [];

    /// <summary>
    /// Gets custom metrics hooks. Shared defaults run before these hooks.
    /// </summary>
    public IList<Action<MeterProviderBuilder>> ConfigureMetrics { get; } = [];

    /// <summary>
    /// Gets custom tracing hooks. Shared defaults run before these hooks.
    /// </summary>
    public IList<Action<TracerProviderBuilder>> ConfigureTracing { get; } = [];

    /// <summary>
    /// Creates a configured options instance.
    /// </summary>
    /// <param name="configure">The optional configuration delegate.</param>
    /// <returns>The configured options.</returns>
    public static HexalithServiceDefaultsOptions Create(Action<HexalithServiceDefaultsOptions>? configure)
    {
        HexalithServiceDefaultsOptions options = new();
        configure?.Invoke(options);
        options.Validate();
        return options;
    }

    private void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(HealthEndpointPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(LivenessEndpointPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(ReadinessEndpointPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(LivenessTag);
        ArgumentException.ThrowIfNullOrWhiteSpace(ReadinessTag);
        if (ActivitySourceNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Activity source names cannot be null, empty, or whitespace.", nameof(ActivitySourceNames));
        }

        if (MeterNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Meter names cannot be null, empty, or whitespace.", nameof(MeterNames));
        }
    }
}
