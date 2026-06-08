// <copyright file="TenantAccessProjectionHandlerOptions.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Options for the generic tenant-access projection handler.
/// </summary>
public sealed class TenantAccessProjectionHandlerOptions
{
    /// <summary>
    /// Gets or sets how many times retryable persistence failures are retried.
    /// </summary>
    public int ConcurrencyRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the tolerated future clock skew for tenant-event timestamps.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the module configuration-key filter.
    /// </summary>
    public Func<string, bool>? AcceptsConfigurationKey { get; set; }

    /// <summary>
    /// Gets or sets the retryable persistence exception classifier.
    /// </summary>
    public Func<Exception, bool>? IsRetryablePersistenceException { get; set; }
}
