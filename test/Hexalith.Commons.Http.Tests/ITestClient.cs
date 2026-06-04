// <copyright file="ITestClient.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

/// <summary>
/// Minimal typed-client contract used to exercise the registration helper.
/// </summary>
internal interface ITestClient
{
    /// <summary>
    /// Gets the base address the typed client was registered with.
    /// </summary>
    Uri? BaseAddress { get; }
}
