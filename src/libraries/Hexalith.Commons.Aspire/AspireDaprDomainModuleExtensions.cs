// <copyright file="AspireDaprDomainModuleExtensions.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Aspire;

using System.Collections.Immutable;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

using CommunityToolkit.Aspire.Hosting.Dapr;

/// <summary>
/// Domain-neutral Aspire and Dapr hosting helpers for Hexalith domain modules.
/// </summary>
public static class AspireDaprDomainModuleExtensions
{
    /// <summary>
    /// Attaches project references, wait ordering, and a Dapr sidecar to a domain-module project resource.
    /// </summary>
    /// <param name="project">The domain-module project resource.</param>
    /// <param name="options">The domain-module hosting options.</param>
    /// <returns>A resource record describing the composed domain module.</returns>
    public static AspireDaprDomainModuleResource AddAspireDaprDomainModule(
        this IResourceBuilder<ProjectResource> project,
        AspireDaprDomainModuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.AppId);

        if (options.InfrastructureMode == AspireDaprInfrastructureMode.Shared && options.SharedComponents is null)
        {
            throw new InvalidOperationException("Shared Dapr infrastructure mode requires shared state-store and pub/sub components.");
        }

        foreach (IResourceBuilder<ProjectResource> reference in options.References)
        {
            ArgumentNullException.ThrowIfNull(reference);
            _ = project.WithReference(reference);
        }

        foreach (IResourceBuilder<ProjectResource> waitFor in options.WaitFor)
        {
            ArgumentNullException.ThrowIfNull(waitFor);
            _ = project.WaitFor(waitFor);
        }

        _ = project.WithDaprSidecar(sidecar =>
        {
            IResourceBuilder<IDaprSidecarResource> configured = sidecar.WithOptions(CreateSidecarOptions(options));
            if (options.InfrastructureMode == AspireDaprInfrastructureMode.Shared)
            {
                configured = configured
                    .WithReference(options.SharedComponents!.StateStore)
                    .WithReference(options.SharedComponents.PubSub);
            }
        });

        return new AspireDaprDomainModuleResource(project, options.AppId, options.InfrastructureMode);
    }

    private static DaprSidecarOptions CreateSidecarOptions(AspireDaprDomainModuleOptions options)
    {
        ImmutableHashSet<string> resourcesPaths = options.ResourcesPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToImmutableHashSet(StringComparer.Ordinal);

        return new DaprSidecarOptions
        {
            AppId = options.AppId,
            Config = options.Config,
            ResourcesPaths = resourcesPaths,
            AppHealthCheckPath = options.AppHealthCheckPath,
            EnableAppHealthCheck = options.EnableAppHealthCheck,
            PlacementHostAddress = options.PlacementHostAddress,
            SchedulerHostAddress = options.SchedulerHostAddress,
            DaprHttpPort = options.DaprHttpPort,
        };
    }
}
