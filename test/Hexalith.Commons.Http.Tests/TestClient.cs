// <copyright file="TestClient.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using System.Net.Http;

/// <summary>
/// Typed-client implementation constructed by the DI container from the registered <see cref="HttpClient"/>.
/// </summary>
/// <param name="httpClient">The typed <see cref="HttpClient"/> supplied by the container.</param>
internal sealed class TestClient(HttpClient httpClient) : ITestClient
{
    /// <inheritdoc/>
    public Uri? BaseAddress { get; } = httpClient.BaseAddress;
}
