// <copyright file="MessageMetadata.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the metadata of a message.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Name">The message name.</param>
/// <param name="Version">The message version.</param>
/// <param name="Domain">The domain metadata.</param>
/// <param name="CreatedDate">The message creation date.</param>
[DataContract]
public record MessageMetadata(
    [property: DataMember(Order = 1)]
    [property: JsonPropertyOrder(1)]
    string Id,
    [property: DataMember(Order = 2)]
    [property: JsonPropertyOrder(2)]
    string Name,
    [property: DataMember(Order = 3)]
    [property: JsonPropertyOrder(3)]
    int Version,
    [property: DataMember(Order = 4)]
    [property: JsonPropertyOrder(4)]
    DomainMetadata Domain,
    [property: DataMember(Order = 5)]
    [property: JsonPropertyOrder(5)]
    DateTimeOffset CreatedDate)
{
    /// <summary>
    /// Gets an empty instance of the <see cref="MessageMetadata"/> class.
    /// </summary>
    public static MessageMetadata Empty => new(
        string.Empty,
        string.Empty,
        0,
        DomainMetadata.Empty,
        DateTimeOffset.MinValue);
}