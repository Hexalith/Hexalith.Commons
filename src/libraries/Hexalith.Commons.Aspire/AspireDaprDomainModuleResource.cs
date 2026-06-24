// <copyright file="AspireDaprDomainModuleResource.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Aspire;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Describes a domain-module project resource after Dapr sidecar composition.
/// </summary>
/// <param name="Project">The project resource builder.</param>
/// <param name="AppId">The stable Dapr application id.</param>
/// <param name="InfrastructureMode">The shared or isolated infrastructure mode.</param>
public sealed record AspireDaprDomainModuleResource(
    IResourceBuilder<ProjectResource> Project,
    string AppId,
    AspireDaprInfrastructureMode InfrastructureMode);
