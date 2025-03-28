﻿// <copyright file="IIdempotent.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Objects;

/// <summary>
/// Interface IIdempotent.
/// </summary>
public interface IIdempotent
{
    /// <summary>
    /// Gets the idempotency identifier.
    /// </summary>
    /// <value>The idempotency identifier.</value>
    string IdempotencyId { get; }
}