// <copyright file="TenantAccessProjectionState.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Base state for module-owned tenant-access projections.
/// </summary>
public class TenantAccessProjectionState
{
    /// <summary>Gets or sets the tenant identifier.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the tenant is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the highest applied sequence number.</summary>
    public long Watermark { get; set; }

    /// <summary>Gets or sets the last applied event timestamp.</summary>
    public DateTimeOffset? LastEventTimestamp { get; set; }

    /// <summary>Gets or sets the projection watermark token.</summary>
    public string? ProjectionWatermark { get; set; }

    /// <summary>Gets or sets a value indicating whether malformed evidence was observed.</summary>
    public bool MalformedEvidence { get; set; }

    /// <summary>Gets or sets a value indicating whether divergent duplicate delivery was observed.</summary>
    public bool ReplayConflict { get; set; }

    /// <summary>Gets the processed message evidence keyed by message id.</summary>
    public Dictionary<string, TenantAccessProjectionEvidence> ProcessedMessages { get; } = new(StringComparer.Ordinal);

    /// <summary>Gets the projected principals keyed by principal id.</summary>
    public Dictionary<string, TenantAccessPrincipalEvidence> Principals { get; } = new(StringComparer.Ordinal);

    /// <summary>Gets active module configuration keys.</summary>
    public HashSet<string> ConfigurationKeys { get; } = new(StringComparer.Ordinal);

    /// <summary>Gets removed module configuration keys.</summary>
    public HashSet<string> RemovedConfigurationKeys { get; } = new(StringComparer.Ordinal);
}
