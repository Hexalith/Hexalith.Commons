// <copyright file="Metadata.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the metadata of a message, including both message-specific and context-related information.
/// </summary>
/// <param name="Message">The message-specific metadata.</param>
/// <param name="Context">The context-related metadata.</param>
[DataContract]
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Metadata is the appropriate name for this domain concept; System.Runtime.Remoting is obsolete")]
[method: JsonConstructor]
public record Metadata(
    [property: DataMember(Order = 1)]
    [property: JsonPropertyOrder(1)]
    MessageMetadata Message,
    [property: DataMember(Order = 2)]
    [property: JsonPropertyOrder(2)]
    ContextMetadata Context)
{
    /// <summary>
    /// Gets the partition key, which is a combination of the partition ID, aggregate name, and aggregate ID.
    /// </summary>
    /// <remarks>
    /// The partition key is used to determine how data is distributed across partitions in a distributed system.
    /// It is constructed by escaping the combination of PartitionId, Domain Name, and Domain Id.
    /// </remarks>
    public string DomainGlobalId => CreateDomainGlobalId(Message.Domain.Id);

    /// <summary>
    /// Creates a global identifier for an aggregate by combining the partition ID, aggregate name, and aggregate ID.
    /// </summary>
    /// <param name="partitionId">The identifier of the partition.</param>
    /// <param name="aggregateName">The name of the aggregate.</param>
    /// <param name="aggregateId">The identifier of the aggregate.</param>
    /// <returns>A string representing the global identifier for the aggregate.</returns>
    public static string CreateDomainGlobalId(string partitionId, string aggregateName, string aggregateId) => $"{partitionId}-{aggregateName}-{aggregateId}";

    /// <summary>
    /// Creates a global identifier for an aggregate by combining the partition ID, aggregate name, and aggregate ID.
    /// </summary>
    /// <param name="aggregateId">The identifier of the aggregate.</param>
    /// <returns>A string representing the global identifier for the aggregate.</returns>
    public string CreateDomainGlobalId(string aggregateId)
        => CreateDomainGlobalId(Context.PartitionId, Message.Domain.Name, aggregateId);

    /// <summary>
    /// Converts the metadata to a log string.
    /// </summary>
    /// <returns>A string representation of the metadata for logging purposes.</returns>
    public string ToLogString()
        => $"MessageName={Message.Name}; DomainGlobalId={DomainGlobalId}; " +
            $"MessageId={Message.Id}; CorrelationId={Context.CorrelationId}; UserId={Context.UserId}";
}