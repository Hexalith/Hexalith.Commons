// <copyright file="SettingsException.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Configurations;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Class SettingsException.
/// Implements the <see cref="ArgumentException" />.
/// </summary>
/// <seealso cref="ArgumentException" />
[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors", Justification = "Base class")]
[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Base class requires settingsName parameter")]
public abstract class SettingsException : ArgumentException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.
    /// If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
    /// <param name="settingsName">Settings name is null or empty.</param>
    protected SettingsException(string? message, string? paramName, Exception? innerException, string settingsName)
        : base(message, paramName, innerException)
    {
        ThrowIfNullOrWhiteSpace(settingsName);
        SettingsName = settingsName;
    }

    /// <summary>
    /// Gets the settings name.
    /// </summary>
    public string SettingsName { get; }

    /// <summary>
    /// Throws if undefined.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="paramName">Name of the parameter.</param>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    public static void ThrowIfUndefined<TSettings>(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
        where TSettings : ISettings
    {
        if (argument is null || (argument is string str && string.IsNullOrWhiteSpace(str)))
        {
            string? settingsName = string.IsNullOrWhiteSpace(paramName)
                ? string.Empty
                : paramName.Split(".").LastOrDefault();
            if (string.IsNullOrWhiteSpace(settingsName))
            {
                settingsName = "Unknown";
            }

            Throw<TSettings>(
                $"The {settingsName} value is undefined in {TSettings.ConfigurationName()} settings. Argument : {paramName}.",
                paramName
            );
        }
    }

    /// <summary>
    /// Throws the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="paramName">Name of the parameter.</param>
    /// <exception cref="SettingsException{TSettings}">Throw settings exception.</exception>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    [DoesNotReturn]
    internal static void Throw<TSettings>(string? message, string? paramName)
        where TSettings : ISettings
        => throw new SettingsException<TSettings>(message, paramName);
}