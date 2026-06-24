// <copyright file="AspireDaprDomainModuleTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Aspire.Tests;

using CommunityToolkit.Aspire.Hosting.Dapr;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

using Hexalith.Commons.Aspire;

using Shouldly;

public sealed class AspireDaprDomainModuleTest
{
    [Fact]
    public void AddAspireDaprDomainModuleShouldRejectMissingInputs()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<ProjectResource> project = AddProject(builder, "module");
        AspireDaprSharedComponents shared = AddSharedComponents(builder);

        _ = Should.Throw<ArgumentNullException>(() =>
            AspireDaprDomainModuleExtensions.AddAspireDaprDomainModule(null!, new AspireDaprDomainModuleOptions("module", AspireDaprInfrastructureMode.Shared)
            {
                SharedComponents = shared,
            }));
        _ = Should.Throw<ArgumentNullException>(() => project.AddAspireDaprDomainModule(null!));
        _ = Should.Throw<ArgumentException>(() => project.AddAspireDaprDomainModule(new AspireDaprDomainModuleOptions(" ", AspireDaprInfrastructureMode.Shared)
        {
            SharedComponents = shared,
        }));
        _ = Should.Throw<InvalidOperationException>(() => project.AddAspireDaprDomainModule(new AspireDaprDomainModuleOptions("module", AspireDaprInfrastructureMode.Shared)));
    }

    [Fact]
    public void SharedModeShouldAttachSidecarAndReferenceSharedComponents()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<ProjectResource> upstream = AddProject(builder, "eventstore");
        IResourceBuilder<ProjectResource> project = AddProject(builder, "module");
        AspireDaprSharedComponents shared = AddSharedComponents(builder);

        AspireDaprDomainModuleResource resource = project.AddAspireDaprDomainModule(new AspireDaprDomainModuleOptions("module", AspireDaprInfrastructureMode.Shared)
        {
            SharedComponents = shared,
            Config = "DaprComponents/accesscontrol.yaml",
            ResourcesPaths = ["DaprComponents"],
            References = [upstream],
            WaitFor = [upstream],
            PlacementHostAddress = "localhost:50005",
            SchedulerHostAddress = "localhost:50006",
            AppHealthCheckPath = "/alive",
            EnableAppHealthCheck = true,
            DaprHttpPort = 3510,
        });

        resource.Project.ShouldBeSameAs(project);
        resource.AppId.ShouldBe("module");
        resource.InfrastructureMode.ShouldBe(AspireDaprInfrastructureMode.Shared);

        DaprSidecarOptions options = GetSidecarOptions(project.Resource);
        options.AppId.ShouldBe("module");
        options.Config.ShouldBe("DaprComponents/accesscontrol.yaml");
        options.ResourcesPaths.ShouldContain("DaprComponents");
        options.PlacementHostAddress.ShouldBe("localhost:50005");
        options.SchedulerHostAddress.ShouldBe("localhost:50006");
        options.AppHealthCheckPath.ShouldBe("/alive");
        options.EnableAppHealthCheck.ShouldBe(true);
        options.DaprHttpPort.ShouldBe(3510);

        ResourceNamesReferencedBySidecar(project.Resource).ShouldContain("statestore");
        ResourceNamesReferencedBySidecar(project.Resource).ShouldContain("pubsub");
        ResourceNamesReferencedBy(project.Resource).ShouldContain("eventstore");
        ResourceNamesWaitedOnBy(project.Resource).ShouldContain("eventstore");
    }

    [Fact]
    public void IsolatedModeShouldLoadResourcesPathWithoutSharedComponents()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<ProjectResource> project = AddProject(builder, "module");
        _ = AddSharedComponents(builder);

        _ = project.AddAspireDaprDomainModule(new AspireDaprDomainModuleOptions("module", AspireDaprInfrastructureMode.Isolated)
        {
            Config = "DaprComponents/accesscontrol.yaml",
            ResourcesPaths = ["DaprComponents/empty"],
        });

        DaprSidecarOptions options = GetSidecarOptions(project.Resource);
        options.AppId.ShouldBe("module");
        options.ResourcesPaths.ShouldBe(["DaprComponents/empty"], ignoreOrder: true);

        ResourceNamesReferencedBySidecar(project.Resource).ShouldNotContain("statestore");
        ResourceNamesReferencedBySidecar(project.Resource).ShouldNotContain("pubsub");
    }

    [Fact]
    public void DomainModuleResourceShouldExposeProjectAppIdAndMode()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder();
        IResourceBuilder<ProjectResource> project = AddProject(builder, "module");
        AspireDaprSharedComponents shared = AddSharedComponents(builder);

        AspireDaprDomainModuleResource resource = project.AddAspireDaprDomainModule(new AspireDaprDomainModuleOptions("module", AspireDaprInfrastructureMode.Shared)
        {
            SharedComponents = shared,
        });

        resource.Project.Resource.Name.ShouldBe("module");
        resource.AppId.ShouldBe("module");
        resource.InfrastructureMode.ShouldBe(AspireDaprInfrastructureMode.Shared);
    }

    private static IResourceBuilder<ProjectResource> AddProject(IDistributedApplicationBuilder builder, string name)
        => builder.AddProject(
            name,
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Hexalith.Commons.Aspire.Tests.csproj")));

    private static AspireDaprSharedComponents AddSharedComponents(IDistributedApplicationBuilder builder)
        => new(
            builder.AddDaprComponent("statestore", "state.redis"),
            builder.AddDaprPubSub("pubsub"));

    private static DaprSidecarOptions GetSidecarOptions(ProjectResource resource)
        => resource.Annotations
            .OfType<DaprSidecarAnnotation>()
            .SelectMany(static sidecar => sidecar.Sidecar.Annotations.OfType<DaprSidecarOptionsAnnotation>())
            .Select(static annotation => annotation.Options)
            .Single();

    private static string[] ResourceNamesReferencedBySidecar(ProjectResource resource)
        => [.. resource.Annotations
            .OfType<DaprSidecarAnnotation>()
            .SelectMany(static annotation => annotation.Sidecar.Annotations.OfType<DaprComponentReferenceAnnotation>())
            .Select(static annotation => annotation.Component.Name)
            .Order(StringComparer.Ordinal)];

    private static string[] ResourceNamesReferencedBy(ProjectResource resource)
        => [.. resource.Annotations
            .OfType<ResourceRelationshipAnnotation>()
            .Select(static annotation => annotation.Resource.Name)
            .Order(StringComparer.Ordinal)];

    private static string[] ResourceNamesWaitedOnBy(ProjectResource resource)
        => [.. resource.Annotations
            .OfType<WaitAnnotation>()
            .Select(static annotation => annotation.Resource.Name)
            .Order(StringComparer.Ordinal)];
}
