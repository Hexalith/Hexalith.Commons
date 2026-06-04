// <copyright file="MarkerHandler.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using System.Net.Http;

/// <summary>
/// No-op delegating handler used to prove the returned builder supports handler chaining.
/// </summary>
internal sealed class MarkerHandler : DelegatingHandler;
