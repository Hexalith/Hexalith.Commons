// <copyright file="HexalithServiceDefaultsTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using System.Text.Json;

using Hexalith.Commons.ServiceDefaults;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Shouldly;

namespace Hexalith.Commons.ServiceDefaults.Tests;

/// <summary>
/// Verifies the shared ServiceDefaults base and its module hook surface.
/// </summary>
public sealed class HexalithServiceDefaultsTest
{
    [Fact]
    public void AddHexalithServiceDefaultsShouldRejectNullBuilder()
    {
        _ = Should.Throw<ArgumentNullException>(
            static () => HexalithServiceDefaults.AddHexalithServiceDefaults<IHostApplicationBuilder>(null!));
    }

    [Fact]
    public void AddHexalithDefaultHealthChecksShouldRegisterLiveSelfCheckByDefault()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        _ = builder.AddHexalithDefaultHealthChecks();

        HealthCheckServiceOptions options = BuildHealthOptions(builder.Services);
        HealthCheckRegistration registration = options.Registrations.Single(static r => r.Name == "self");
        registration.Tags.ShouldContain("live");
        registration.Tags.ShouldNotContain("ready");
    }

    [Fact]
    public void AddHexalithDefaultHealthChecksShouldSupportReadySelfCheckAndAdditionalChecks()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        _ = builder.AddHexalithDefaultHealthChecks(options =>
        {
            options.DefaultSelfCheckIncludesReadiness = true;
            options.ConfigureHealthChecks.Add(health => health.AddCheck("custom", () => HealthCheckResult.Degraded(), tags: ["custom-ready"]));
        });

        HealthCheckServiceOptions options = BuildHealthOptions(builder.Services);
        HealthCheckRegistration self = options.Registrations.Single(static r => r.Name == "self");
        self.Tags.ShouldContain("live");
        self.Tags.ShouldContain("ready");
        options.Registrations.ShouldContain(static r => r.Name == "custom" && r.Tags.Contains("custom-ready"));
    }

    [Fact]
    public void AddHexalithDefaultHealthChecksShouldAllowModulesToSkipDefaultSelfCheck()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        _ = builder.AddHexalithDefaultHealthChecks(static options => options.RegisterDefaultSelfCheck = false);

        BuildHealthOptions(builder.Services).Registrations.ShouldNotContain(static r => r.Name == "self");
    }

    [Fact]
    public void MapHexalithDefaultEndpointsShouldUseConfiguredPaths()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.AddHexalithDefaultHealthChecks();
        WebApplication app = builder.Build();

        _ = app.MapHexalithDefaultEndpoints(options =>
        {
            options.HealthEndpointPath = "/status";
            options.LivenessEndpointPath = "/health/live";
            options.ReadinessEndpointPath = "/health/ready";
        });

        string[] routes = [.. ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(static source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(static endpoint => endpoint.RoutePattern.RawText ?? string.Empty)];
        routes.ShouldContain("/status");
        routes.ShouldContain("/health/live");
        routes.ShouldContain("/health/ready");
    }

    [Fact]
    public void CreateHealthStatusCodesShouldMapDegradedToServingAndUnhealthyToUnavailable()
    {
        IDictionary<HealthStatus, int> statusCodes = HexalithServiceDefaults.CreateHealthStatusCodes();

        statusCodes[HealthStatus.Healthy].ShouldBe(StatusCodes.Status200OK);
        statusCodes[HealthStatus.Degraded].ShouldBe(StatusCodes.Status200OK);
        statusCodes[HealthStatus.Unhealthy].ShouldBe(StatusCodes.Status503ServiceUnavailable);
    }

    [Theory]
    [InlineData("/health", false)]
    [InlineData("/alive", false)]
    [InlineData("/ready", false)]
    [InlineData("/api/conversations", true)]
    public void ShouldTraceHttpRequestShouldExcludeConfiguredHealthProbePaths(string path, bool expected)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;

        HexalithServiceDefaults.ShouldTraceHttpRequest(context).ShouldBe(expected);
    }

    [Fact]
    public async Task WriteDevelopmentHealthJsonResponseShouldWriteDetailedJsonAndTolerateNonSerializableData()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        HealthReport report = new(
            new Dictionary<string, HealthReportEntry>
            {
                ["self"] = new(
                    HealthStatus.Degraded,
                    "degraded-but-serving",
                    TimeSpan.FromMilliseconds(7),
                    exception: null,
                    data: new Dictionary<string, object> { ["payload"] = new NonSerializableHealthData() }),
            },
            TimeSpan.FromMilliseconds(7));

        await HexalithServiceDefaults.WriteDevelopmentHealthJsonResponseAsync(context, report).ConfigureAwait(true);

        context.Response.ContentType.ShouldBe("application/json; charset=utf-8");
        context.Response.Body.Position = 0;
        using JsonDocument document = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        document.RootElement.GetProperty("status").GetString().ShouldBe("Degraded");
        JsonElement self = document.RootElement.GetProperty("results").GetProperty("self");
        self.GetProperty("status").GetString().ShouldBe("Degraded");
        self.GetProperty("description").GetString().ShouldBe("degraded-but-serving");
        self.GetProperty("data").GetProperty("payload").GetString().ShouldBe("[non-serializable: NonSerializableHealthData]");
    }

    [Fact]
    public void AddHexalithServiceDefaultsShouldRegisterDiscoveryHttpResilienceAndOpenTelemetry()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        _ = builder.AddHexalithServiceDefaults(options =>
        {
            options.ActivitySourceNames.Add("Hexalith.Test.Activity");
            options.MeterNames.Add("Hexalith.Test.Meter");
        });

        string descriptorText = string.Join(Environment.NewLine, builder.Services.Select(static descriptor => descriptor.ToString()));
        descriptorText.ShouldContain("ServiceDiscovery");
        descriptorText.ShouldContain("Resilience");
        descriptorText.ShouldContain("OpenTelemetry");
    }

    [Fact]
    public void AddHexalithServiceDefaultsShouldGateOtlpExporterOnEndpointConfiguration()
    {
        WebApplicationBuilder withoutEndpoint = WebApplication.CreateBuilder();
        _ = withoutEndpoint.AddHexalithServiceDefaults();

        WebApplicationBuilder withEndpoint = WebApplication.CreateBuilder();
        withEndpoint.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://localhost:4317";
        _ = withEndpoint.AddHexalithServiceDefaults();

        CountOtlpDescriptors(withEndpoint.Services).ShouldBeGreaterThan(CountOtlpDescriptors(withoutEndpoint.Services));
    }

    [Fact]
    public void AddHexalithServiceDefaultsShouldExecuteModuleHooksAfterSharedRegistration()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        List<string> calls = [];

        _ = builder.AddHexalithServiceDefaults(options =>
        {
            options.ConfigureHealthChecks.Add(_ => calls.Add("health"));
            options.ConfigureLogging.Add(_ => calls.Add("logging"));
            options.ConfigureMetrics.Add(_ => calls.Add("metrics"));
            options.ConfigureTracing.Add(_ => calls.Add("tracing"));
        });

        calls.ShouldBe(["logging", "metrics", "tracing", "health"]);
    }

    private static HealthCheckServiceOptions BuildHealthOptions(IServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
    }

    private static int CountOtlpDescriptors(IServiceCollection services)
        => services.Count(static descriptor => descriptor.ToString()?.Contains("Otlp", StringComparison.OrdinalIgnoreCase) == true);

    private sealed class NonSerializableHealthData
    {
        public string Value => throw new InvalidOperationException("Data should be guarded.");
    }
}
