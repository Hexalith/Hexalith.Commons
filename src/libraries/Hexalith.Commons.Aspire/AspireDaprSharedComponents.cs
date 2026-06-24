// <copyright file="AspireDaprSharedComponents.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Aspire;

using global::Aspire.Hosting.ApplicationModel;

using CommunityToolkit.Aspire.Hosting.Dapr;

/// <summary>
/// Contains shared Dapr component builders a domain module can reference from its sidecar.
/// </summary>
public sealed class AspireDaprSharedComponents
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AspireDaprSharedComponents"/> class.
    /// </summary>
    /// <param name="stateStore">The shared Dapr state-store component.</param>
    /// <param name="pubSub">The shared Dapr pub/sub component.</param>
    public AspireDaprSharedComponents(
        IResourceBuilder<IDaprComponentResource> stateStore,
        IResourceBuilder<IDaprComponentResource> pubSub)
    {
        ArgumentNullException.ThrowIfNull(stateStore);
        ArgumentNullException.ThrowIfNull(pubSub);

        StateStore = stateStore;
        PubSub = pubSub;
    }

    /// <summary>
    /// Gets the shared Dapr state-store component.
    /// </summary>
    public IResourceBuilder<IDaprComponentResource> StateStore { get; }

    /// <summary>
    /// Gets the shared Dapr pub/sub component.
    /// </summary>
    public IResourceBuilder<IDaprComponentResource> PubSub { get; }
}
