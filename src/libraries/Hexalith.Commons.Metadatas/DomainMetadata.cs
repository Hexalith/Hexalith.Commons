// <copyright file="DomainMetadata.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Metadatas;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the metadata of a domain aggregate.
/// </summary>
/// <param name="Id">The identifier of the domain aggregate.</param>
/// <param name="Name">The name of the domain aggregate.</param>
[DataContract]
[method: JsonConstructor]
public record DomainMetadata(
    [property:DataMember(Order = 1)]
    [property:JsonPropertyOrder(1)]
    string Id,
    [property:DataMember(Order = 2)]
    [property:JsonPropertyOrder(2)]
    string Name)
{
    /// <summary>
    /// Gets an empty instance of <see cref="DomainMetadata"/>.
    /// </summary>
    public static DomainMetadata Empty => new(string.Empty, string.Empty);
}