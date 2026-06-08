// <copyright file="ITenantAccessClock.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Provides UTC time for tenant-access projection validation.
/// </summary>
public interface ITenantAccessClock
{
    /// <summary>
    /// Gets the current UTC instant.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
