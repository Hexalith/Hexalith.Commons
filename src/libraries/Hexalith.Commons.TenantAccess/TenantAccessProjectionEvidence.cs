// <copyright file="TenantAccessProjectionEvidence.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Bounded evidence used to detect divergent duplicate tenant-event delivery.
/// </summary>
/// <param name="MessageId">The event message identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="Kind">The normalized tenant event kind.</param>
/// <param name="SequenceNumber">The tenant event sequence number.</param>
/// <param name="Timestamp">The tenant event timestamp.</param>
/// <param name="PayloadFingerprint">The bounded payload fingerprint.</param>
public sealed record TenantAccessProjectionEvidence(
    string MessageId,
    string TenantId,
    string Kind,
    long SequenceNumber,
    DateTimeOffset Timestamp,
    string? PayloadFingerprint);
