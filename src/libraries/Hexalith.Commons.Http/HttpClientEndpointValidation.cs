// <copyright file="HttpClientEndpointValidation.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http;

/// <summary>
/// Controls when the typed-HttpClient endpoint configured through
/// <see cref="HttpClientRegistration"/> is validated.
/// </summary>
public enum HttpClientEndpointValidation
{
    /// <summary>
    /// Validate the endpoint lazily, the first time the typed client (or its options) is resolved,
    /// using <c>IOptions&lt;TOptions&gt;.Validate</c>. This mirrors the Folders/Projects client shape.
    /// </summary>
    OnResolve = 0,

    /// <summary>
    /// Validate the endpoint eagerly, at registration time, throwing immediately when the configured
    /// endpoint is rejected. This mirrors the Conversations client shape and is the stronger guarantee.
    /// </summary>
    OnRegistration = 1,
}
