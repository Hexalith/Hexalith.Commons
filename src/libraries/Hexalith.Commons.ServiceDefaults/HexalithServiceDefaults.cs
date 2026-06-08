// <copyright file="HexalithServiceDefaults.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Hexalith.Commons.ServiceDefaults;

/// <summary>
/// Shared Hexalith service defaults for OpenTelemetry, health endpoints, service discovery, and HTTP resilience.
/// </summary>
public static class HexalithServiceDefaults
{
    /// <summary>
    /// Adds shared service defaults and module hooks.
    /// </summary>
    /// <typeparam name="TBuilder">The host builder type.</typeparam>
    /// <param name="builder">The host builder.</param>
    /// <param name="configure">The optional module-specific options.</param>
    /// <returns>The same builder for chaining.</returns>
    public static TBuilder AddHexalithServiceDefaults<TBuilder>(
        this TBuilder builder,
        Action<HexalithServiceDefaultsOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        HexalithServiceDefaultsOptions options = HexalithServiceDefaultsOptions.Create(configure);
        _ = builder.ConfigureHexalithOpenTelemetry(options);
        _ = builder.AddHexalithDefaultHealthChecks(options);
        _ = builder.Services.AddServiceDiscovery();
        _ = builder.Services.ConfigureHttpClientDefaults(static http =>
        {
            _ = http.AddStandardResilienceHandler();
            _ = http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry logging, metrics, tracing, and optional OTLP export.
    /// </summary>
    /// <typeparam name="TBuilder">The host builder type.</typeparam>
    /// <param name="builder">The host builder.</param>
    /// <param name="configure">The optional module-specific options.</param>
    /// <returns>The same builder for chaining.</returns>
    public static TBuilder ConfigureHexalithOpenTelemetry<TBuilder>(
        this TBuilder builder,
        Action<HexalithServiceDefaultsOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureHexalithOpenTelemetry(HexalithServiceDefaultsOptions.Create(configure));
    }

    /// <summary>
    /// Adds default health checks and module health hooks.
    /// </summary>
    /// <typeparam name="TBuilder">The host builder type.</typeparam>
    /// <param name="builder">The host builder.</param>
    /// <param name="configure">The optional module-specific options.</param>
    /// <returns>The same builder for chaining.</returns>
    public static TBuilder AddHexalithDefaultHealthChecks<TBuilder>(
        this TBuilder builder,
        Action<HexalithServiceDefaultsOptions>? configure = null)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddHexalithDefaultHealthChecks(HexalithServiceDefaultsOptions.Create(configure));
    }

    /// <summary>
    /// Maps the default health, liveness, and readiness endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configure">The optional module-specific options.</param>
    /// <returns>The same application for chaining.</returns>
    public static WebApplication MapHexalithDefaultEndpoints(
        this WebApplication app,
        Action<HexalithServiceDefaultsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        HexalithServiceDefaultsOptions options = HexalithServiceDefaultsOptions.Create(configure);
        IDictionary<HealthStatus, int> statusCodes = CreateHealthStatusCodes();

        HealthCheckOptions healthOptions = new()
        {
            ResultStatusCodes = statusCodes,
        };
        HealthCheckOptions livenessOptions = new()
        {
            Predicate = registration => registration.Tags.Contains(options.LivenessTag),
            ResultStatusCodes = statusCodes,
        };
        HealthCheckOptions readinessOptions = new()
        {
            Predicate = registration => registration.Tags.Contains(options.ReadinessTag),
            ResultStatusCodes = statusCodes,
        };

        if (app.Environment.IsDevelopment() && options.DevelopmentHealthResponseWriter is not null)
        {
            healthOptions.ResponseWriter = options.DevelopmentHealthResponseWriter;
            readinessOptions.ResponseWriter = options.DevelopmentHealthResponseWriter;
        }

        _ = app.MapHealthChecks(options.HealthEndpointPath, healthOptions);
        _ = app.MapHealthChecks(options.LivenessEndpointPath, livenessOptions);
        _ = app.MapHealthChecks(options.ReadinessEndpointPath, readinessOptions);
        return app;
    }

    /// <summary>
    /// Returns the shared health endpoint status-code mapping.
    /// </summary>
    /// <returns>The health status code mapping.</returns>
    public static IDictionary<HealthStatus, int> CreateHealthStatusCodes()
        => new Dictionary<HealthStatus, int>
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        };

    /// <summary>
    /// Returns whether an HTTP request should be traced by the default ASP.NET Core instrumentation.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="configure">The optional module-specific options.</param>
    /// <returns><c>true</c> when the request should be traced; otherwise <c>false</c>.</returns>
    public static bool ShouldTraceHttpRequest(
        HttpContext httpContext,
        Action<HexalithServiceDefaultsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return ShouldTraceHttpRequest(httpContext, HexalithServiceDefaultsOptions.Create(configure));
    }

    /// <summary>
    /// Writes the shared detailed JSON health response for development environments.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="healthReport">The health report.</param>
    /// <returns>A task that completes when the response has been written.</returns>
    public static async Task WriteDevelopmentHealthJsonResponseAsync(
        HttpContext httpContext,
        HealthReport healthReport)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(healthReport);

        httpContext.Response.ContentType = "application/json; charset=utf-8";

        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("status", healthReport.Status.ToString());
            writer.WriteStartObject("results");

            foreach (KeyValuePair<string, HealthReportEntry> entry in healthReport.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteString("description", entry.Value.Description);
                writer.WriteString("duration", entry.Value.Duration.ToString());
                writer.WriteStartObject("data");
                foreach (KeyValuePair<string, object> dataEntry in entry.Value.Data)
                {
                    writer.WritePropertyName(dataEntry.Key);
                    try
                    {
                        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
                            dataEntry.Value,
                            dataEntry.Value?.GetType() ?? typeof(object));
                        writer.WriteRawValue(json);
                    }
                    catch (Exception ex) when (ex is NotSupportedException or JsonException or InvalidOperationException)
                    {
                        writer.WriteStringValue($"[non-serializable: {dataEntry.Value?.GetType().Name ?? "null"}]");
                    }
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        stream.Position = 0;
        await stream.CopyToAsync(httpContext.Response.Body, httpContext.RequestAborted).ConfigureAwait(false);
    }

    private static TBuilder ConfigureHexalithOpenTelemetry<TBuilder>(
        this TBuilder builder,
        HexalithServiceDefaultsOptions options)
        where TBuilder : IHostApplicationBuilder
    {
        _ = builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = options.IncludeFormattedLogMessage;
            logging.IncludeScopes = options.IncludeLogScopes;
            if (!string.IsNullOrWhiteSpace(options.ServiceName))
            {
                logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(options.ServiceName));
            }

            foreach (Action<OpenTelemetry.Logs.OpenTelemetryLoggerOptions> hook in options.ConfigureLogging)
            {
                hook(logging);
            }
        });

        if (options.AddJsonConsoleLogging)
        {
            _ = builder.Logging.AddJsonConsole(static console => console.UseUtcTimestamp = true);
        }

        IOpenTelemetryBuilder telemetry = builder.Services.AddOpenTelemetry();
        if (!string.IsNullOrWhiteSpace(options.ServiceName))
        {
            _ = telemetry.ConfigureResource(resource => resource.AddService(options.ServiceName));
        }

        _ = telemetry
            .WithMetrics(metrics =>
            {
                _ = metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                foreach (string meterName in options.MeterNames)
                {
                    _ = metrics.AddMeter(meterName);
                }

                foreach (Action<MeterProviderBuilder> hook in options.ConfigureMetrics)
                {
                    hook(metrics);
                }
            })
            .WithTracing(tracing =>
            {
                _ = tracing.AddSource(options.ServiceName ?? builder.Environment.ApplicationName);
                foreach (string sourceName in options.ActivitySourceNames)
                {
                    _ = tracing.AddSource(sourceName);
                }

                _ = tracing
                    .AddAspNetCoreInstrumentation(aspNetCore => aspNetCore.Filter = context => ShouldTraceHttpRequest(context, options))
                    .AddHttpClientInstrumentation();

                foreach (Action<TracerProviderBuilder> hook in options.ConfigureTracing)
                {
                    hook(tracing);
                }
            });

        if (options.UseOtlpExporterWhenConfigured && IsOtlpConfigured(builder.Configuration))
        {
            _ = builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    private static TBuilder AddHexalithDefaultHealthChecks<TBuilder>(
        this TBuilder builder,
        HexalithServiceDefaultsOptions options)
        where TBuilder : IHostApplicationBuilder
    {
        IHealthChecksBuilder healthChecks = builder.Services.AddHealthChecks();
        if (options.RegisterDefaultSelfCheck)
        {
            string[] tags = options.DefaultSelfCheckIncludesReadiness
                ? [options.LivenessTag, options.ReadinessTag]
                : [options.LivenessTag];
            _ = healthChecks.AddCheck("self", static () => HealthCheckResult.Healthy(), tags);
        }

        foreach (Action<IHealthChecksBuilder> hook in options.ConfigureHealthChecks)
        {
            hook(healthChecks);
        }

        return builder;
    }

    private static bool ShouldTraceHttpRequest(HttpContext httpContext, HexalithServiceDefaultsOptions options)
        => !httpContext.Request.Path.StartsWithSegments(options.HealthEndpointPath)
            && !httpContext.Request.Path.StartsWithSegments(options.LivenessEndpointPath)
            && !httpContext.Request.Path.StartsWithSegments(options.ReadinessEndpointPath);

    private static bool IsOtlpConfigured(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
}
