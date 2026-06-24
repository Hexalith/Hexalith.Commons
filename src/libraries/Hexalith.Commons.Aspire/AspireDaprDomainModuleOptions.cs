// <copyright file="AspireDaprDomainModuleOptions.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Aspire;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Options used to attach a Dapr sidecar and resource relationships to an Aspire project resource.
/// </summary>
/// <param name="appId">The stable Dapr application id.</param>
/// <param name="infrastructureMode">The shared or isolated infrastructure mode.</param>
public sealed class AspireDaprDomainModuleOptions(
    string appId,
    AspireDaprInfrastructureMode infrastructureMode)
{
    /// <summary>
    /// Gets the stable Dapr application id.
    /// </summary>
    public string AppId { get; } = appId;

    /// <summary>
    /// Gets the shared or isolated infrastructure mode.
    /// </summary>
    public AspireDaprInfrastructureMode InfrastructureMode { get; } = infrastructureMode;

    /// <summary>
    /// Gets or sets the shared state-store/pub-sub components used by shared mode.
    /// </summary>
    public AspireDaprSharedComponents? SharedComponents { get; set; }

    /// <summary>
    /// Gets or sets the optional Dapr Configuration CRD path.
    /// </summary>
    public string? Config { get; set; }

    /// <summary>
    /// Gets or sets the optional Dapr resources directories.
    /// </summary>
    public IReadOnlyCollection<string> ResourcesPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets project resources this module references.
    /// </summary>
    public IReadOnlyCollection<IResourceBuilder<ProjectResource>> References { get; set; } = [];

    /// <summary>
    /// Gets or sets project resources this module waits for.
    /// </summary>
    public IReadOnlyCollection<IResourceBuilder<ProjectResource>> WaitFor { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional app health-check path passed to the Dapr sidecar.
    /// </summary>
    public string? AppHealthCheckPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Dapr app health checks are enabled.
    /// </summary>
    public bool EnableAppHealthCheck { get; set; }

    /// <summary>
    /// Gets or sets the optional Dapr placement service address.
    /// </summary>
    public string? PlacementHostAddress { get; set; }

    /// <summary>
    /// Gets or sets the optional Dapr scheduler service address.
    /// </summary>
    public string? SchedulerHostAddress { get; set; }

    /// <summary>
    /// Gets or sets the optional fixed Dapr HTTP port.
    /// </summary>
    public int? DaprHttpPort { get; set; }
}
