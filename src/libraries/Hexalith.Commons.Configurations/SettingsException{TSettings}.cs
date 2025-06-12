// <copyright file="SettingsException{TSettings}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Configurations;

using System;

/// <summary>
/// Class SettingsException.
/// Implements the <see cref="SettingsException" />.
/// </summary>
/// <typeparam name="TSettings">The type of the t settings.</typeparam>
/// <seealso cref="ArgumentException" />
public class SettingsException<TSettings> : SettingsException
    where TSettings : ISettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException{TSettings}" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public SettingsException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException{TSettings}"/> class.
    /// </summary>
    public SettingsException()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException{TSettings}"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.
    /// If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
    public SettingsException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException{TSettings}"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.
    /// If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
    public SettingsException(string? message, string? paramName, Exception? innerException)
        : base(message, paramName, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException{TSettings}"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    public SettingsException(string? message, string? paramName)
        : base(message, paramName) { }
}