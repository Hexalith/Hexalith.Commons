// <copyright file="Month.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Dates;

using System.Text.Json.Serialization;

/// <summary>
/// Enum Month.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Month
{
    /// <summary>
    /// The undefined.
    /// </summary>
    None = 0,

    /// <summary>
    /// The January.
    /// </summary>
    January = 1,

    /// <summary>
    /// The February.
    /// </summary>
    February = 2,

    /// <summary>
    /// The march.
    /// </summary>
    March = 3,

    /// <summary>
    /// The April.
    /// </summary>
    April = 4,

    /// <summary>
    /// The may.
    /// </summary>
    May = 5,

    /// <summary>
    /// The June.
    /// </summary>
    June = 6,

    /// <summary>
    /// The July.
    /// </summary>
    July = 7,

    /// <summary>
    /// The August.
    /// </summary>
    August = 8,

    /// <summary>
    /// The September.
    /// </summary>
    September = 9,

    /// <summary>
    /// The October.
    /// </summary>
    October = 10,

    /// <summary>
    /// The November.
    /// </summary>
    November = 11,

    /// <summary>
    /// The December.
    /// </summary>
    December = 12,
}
