// <copyright file="ContextMetadata.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// The context metadata.
/// </summary>
[DataContract]
public record ContextMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContextMetadata"/> class.
    /// </summary>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="partitionId">The partition identifier.</param>
    /// <param name="receivedDate">The received date.</param>
    [JsonConstructor]
    public ContextMetadata(
        string correlationId,
        string userId,
        string partitionId,
        DateTimeOffset? receivedDate)
    {
        CorrelationId = correlationId;
        UserId = userId;
        PartitionId = partitionId;
        ReceivedDate = receivedDate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextMetadata"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="receivedDate">The received date.</param>
    /// <exception cref="ArgumentNullException">context is null.</exception>
    public ContextMetadata(ContextMetadata context, DateTimeOffset receivedDate)
        : this(
              (context ?? throw new ArgumentNullException(nameof(context))).CorrelationId,
              context.UserId,
              context.PartitionId,
              receivedDate)
    {
        TimeToLive = context.TimeToLive;
        SequenceNumber = context.SequenceNumber;
        Etag = context.Etag;
        SessionId = context.SessionId;
        Scopes = context.Scopes;
    }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    [DataMember(Order = 1)]
    [JsonPropertyOrder(1)]
    public string CorrelationId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    [DataMember(Order = 2)]
    [JsonPropertyOrder(2)]
    public string UserId { get; init; }

    /// <summary>
    /// Gets the partition identifier.
    /// </summary>
    [DataMember(Order = 3)]
    [JsonPropertyOrder(3)]
    public string PartitionId { get; init; }

    /// <summary>
    /// Gets the received date.
    /// </summary>
    [DataMember(Order = 4)]
    [JsonPropertyOrder(4)]
    public DateTimeOffset? ReceivedDate { get; init; }

    /// <summary>
    /// Gets the time to live.
    /// </summary>
    [DataMember(Order = 5)]
    [JsonPropertyOrder(5)]
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Gets the sequence number.
    /// </summary>
    [DataMember(Order = 6)]
    [JsonPropertyOrder(6)]
    public long? SequenceNumber { get; init; }

    /// <summary>
    /// Gets the etag.
    /// </summary>
    [DataMember(Order = 7)]
    [JsonPropertyOrder(7)]
    public string? Etag { get; init; }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    [DataMember(Order = 8)]
    [JsonPropertyOrder(8)]
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets the scopes.
    /// </summary>
    [DataMember(Order = 9)]
    [JsonPropertyOrder(9)]
    public IEnumerable<string> Scopes { get; init; } = [];
}
