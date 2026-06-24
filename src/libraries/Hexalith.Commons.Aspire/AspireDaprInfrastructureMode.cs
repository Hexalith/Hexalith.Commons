// <copyright file="AspireDaprInfrastructureMode.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Aspire;

/// <summary>
/// Defines how a domain-module Dapr sidecar binds infrastructure components.
/// </summary>
public enum AspireDaprInfrastructureMode
{
    /// <summary>
    /// The sidecar references shared state-store and pub/sub components.
    /// </summary>
    Shared = 0,

    /// <summary>
    /// The sidecar loads only supplied resources paths and does not reference shared components.
    /// </summary>
    Isolated = 1,
}
