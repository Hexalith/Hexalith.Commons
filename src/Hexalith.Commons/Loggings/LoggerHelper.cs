﻿// <copyright file="LoggerHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Helpers;

using System;
using System.Globalization;

using Hexalith.Commons.Errors;
using Hexalith.Extensions.Helpers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Class LoggerHelper.
/// </summary>
public static partial class LoggerHelper
{
    /// <summary>
    /// Logs the application technical error.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="title">The title.</param>
    /// <param name="message">The message.</param>
    /// <param name="technicalMessage">The message.</param>
    [LoggerMessage(2, LogLevel.Error, "{Title}: {Message}\n{TechnicalMessage}")]
    public static partial void LogApplicationError(this ILogger logger, Exception? exception, string? title, string? message, string? technicalMessage);

    /// <summary>
    /// Logs the application error details.
    /// </summary>
    /// <param name="applicationError">The application error.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="exception">The exception.</param>
    /// <exception cref="ArgumentNullException">null.</exception>
    public static void LogApplicationErrorDetails(this ApplicationError applicationError, ILogger logger, Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(applicationError);
        ArgumentNullException.ThrowIfNull(logger);
        string? details = applicationError.Detail == null
            ? null
            : StringHelper.FormatWithNamedPlaceholders(CultureInfo.InvariantCulture, applicationError.Detail, applicationError.Arguments);
        string? technicalDetails = applicationError.TechnicalDetail == null
            ? null
            : StringHelper.FormatWithNamedPlaceholders(CultureInfo.InvariantCulture, applicationError.TechnicalDetail, applicationError.TechnicalArguments);
        logger.LogApplicationError(
            exception,
            applicationError.Title,
            details,
            technicalDetails);

        applicationError.InnerError?.LogApplicationErrorDetails(logger, null);
    }
}