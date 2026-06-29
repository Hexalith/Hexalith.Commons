// <copyright file="CorrelationContextAccessor.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http;

using System.Threading;

/// <summary>
/// Async-local implementation of <see cref="ICorrelationContextAccessor"/>.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<string?> s_correlationId = new();

    /// <inheritdoc />
    public string? CorrelationId
    {
        get => s_correlationId.Value;
        set => s_correlationId.Value = value;
    }
}
