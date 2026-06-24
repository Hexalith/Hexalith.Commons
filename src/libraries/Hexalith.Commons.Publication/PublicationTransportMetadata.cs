// <copyright file="PublicationTransportMetadata.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Safe transport-visible publication metadata.
/// </summary>
/// <param name="Topic">The topic name.</param>
/// <param name="Type">The transport event type.</param>
/// <param name="Source">The transport source.</param>
/// <param name="Subject">The transport subject.</param>
/// <param name="Headers">The safe transport headers or extensions.</param>
public sealed record PublicationTransportMetadata(
    string Topic,
    string Type,
    string Source,
    string Subject,
    IReadOnlyDictionary<string, string> Headers);
