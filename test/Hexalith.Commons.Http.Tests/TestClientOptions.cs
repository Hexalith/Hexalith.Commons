// <copyright file="TestClientOptions.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

/// <summary>
/// Endpoint-carrying options double mirroring the Conversations options shape.
/// </summary>
internal sealed record TestClientOptions
{
    /// <summary>
    /// Gets or sets the transport endpoint.
    /// </summary>
    public Uri? Endpoint { get; set; }
}
