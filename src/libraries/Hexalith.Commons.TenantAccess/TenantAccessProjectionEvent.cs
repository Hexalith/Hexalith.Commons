// <copyright file="TenantAccessProjectionEvent.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Domain-neutral view of a tenant event used by the generic projection handler.
/// </summary>
/// <param name="Kind">The normalized event kind.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="MessageId">The message identifier.</param>
/// <param name="SequenceNumber">The event sequence number.</param>
/// <param name="Timestamp">The event timestamp.</param>
/// <param name="PrincipalId">The principal identifier for membership events.</param>
/// <param name="Role">The role token for membership events.</param>
/// <param name="ConfigurationKey">The configuration key for configuration events.</param>
/// <param name="PayloadFingerprint">A bounded payload fingerprint used for replay-conflict detection.</param>
public sealed record TenantAccessProjectionEvent(
    TenantAccessProjectionEventKind Kind,
    string? TenantId,
    string? MessageId,
    long SequenceNumber,
    DateTimeOffset Timestamp,
    string? PrincipalId = null,
    string? Role = null,
    string? ConfigurationKey = null,
    string? PayloadFingerprint = null);
