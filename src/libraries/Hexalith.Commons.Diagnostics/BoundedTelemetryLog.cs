// <copyright file="BoundedTelemetryLog.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Hexalith.Commons.Diagnostics;

/// <summary>
/// Provides a narrow logging hook for telemetry wrappers that already supply content-safe message templates and values.
/// </summary>
public static class BoundedTelemetryLog
{
    /// <summary>
    /// Emits an informational log from a domain-supplied safe message template.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="safeMessageTemplate">The content-safe message template.</param>
    /// <param name="args">The bounded template arguments.</param>
    public static void Information(ILogger logger, string safeMessageTemplate, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(safeMessageTemplate);
        logger.LogInformation(safeMessageTemplate, args);
    }
}
