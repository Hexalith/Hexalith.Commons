// <copyright file="TenantAccessProjectionEvent.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Domain-neutral view of a tenant event used by the generic projection handler.
/// </summary>
public sealed record TenantAccessProjectionEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAccessProjectionEvent"/> class.
    /// </summary>
    /// <param name="kind">The normalized event kind.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="sequenceNumber">The event sequence number.</param>
    /// <param name="timestamp">The event timestamp.</param>
    public TenantAccessProjectionEvent(
        TenantAccessProjectionEventKind kind,
        string? tenantId,
        string? messageId,
        long sequenceNumber,
        DateTimeOffset timestamp)
    {
        Kind = kind;
        TenantId = tenantId;
        MessageId = messageId;
        SequenceNumber = sequenceNumber;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the normalized event kind.
    /// </summary>
    public TenantAccessProjectionEventKind Kind { get; init; }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Gets the event sequence number.
    /// </summary>
    public long SequenceNumber { get; init; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets the principal identifier for membership events.
    /// </summary>
    public string? PrincipalId { get; init; }

    /// <summary>
    /// Gets the role token for membership events.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Gets the configuration key for configuration events.
    /// </summary>
    public string? ConfigurationKey { get; init; }

    /// <summary>
    /// Gets a bounded payload fingerprint used for replay-conflict detection.
    /// </summary>
    public string? PayloadFingerprint { get; init; }
}
