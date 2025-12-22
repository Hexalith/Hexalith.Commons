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
/// <param name="CorrelationId">The correlation identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="PartitionId">The partition identifier.</param>
/// <param name="ReceivedDate">The received date.</param>
/// <param name="TimeToLive">The time to live.</param>
/// <param name="SequenceNumber">The sequence number.</param>
/// <param name="Etag">The etag.</param>
/// <param name="SessionId">The session identifier.</param>
/// <param name="Scopes">The scopes.</param>
[DataContract]
[method: JsonConstructor]
public record ContextMetadata(
    [property:DataMember(Order = 1)]
    [property:JsonPropertyOrder(1)]
    string CorrelationId,
    [property:DataMember(Order = 2)]
    [property:JsonPropertyOrder(2)]
    string UserId,
    [property:DataMember(Order = 2)]
    [property:JsonPropertyOrder(2)]
    string PartitionId,
    [property:DataMember(Order = 3)]
    [property:JsonPropertyOrder(3)]
    DateTimeOffset? ReceivedDate,
    [property:DataMember(Order = 4)]
    [property:JsonPropertyOrder(4)]
    TimeSpan? TimeToLive,
    [property:DataMember(Order = 5)]
    [property:JsonPropertyOrder(5)]
    long? SequenceNumber,
    [property:DataMember(Order = 6)]
    [property:JsonPropertyOrder(6)]
    string? Etag,
    [property:DataMember(Order = 7)]
    [property:JsonPropertyOrder(7)]
    string? SessionId,
    [property: DataMember(Order = 8)]
    [property: JsonPropertyOrder(8)]
    IEnumerable<string> Scopes)
{
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
              receivedDate,
              context.TimeToLive,
              context.SequenceNumber,
              context.Etag,
              context.SessionId,
              context.Scopes)
    {
    }
}