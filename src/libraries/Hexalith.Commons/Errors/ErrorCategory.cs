// <copyright file="ErrorCategory.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Errors;

/// <summary>
/// Enum ErrorType.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Unknown error type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Business error type.
    /// </summary>
    Business = 1,

    /// <summary>
    /// Technical error type.
    /// </summary>
    Technical = 2,
}