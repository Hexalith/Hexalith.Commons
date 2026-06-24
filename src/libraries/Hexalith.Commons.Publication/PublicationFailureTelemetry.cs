// <copyright file="PublicationFailureTelemetry.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Emits module-owned telemetry for rejected publication decisions.
/// </summary>
public static class PublicationFailureTelemetry
{
    /// <summary>
    /// Records a failure when a publication decision was rejected.
    /// </summary>
    public static void RecordRejected<TDiagnostic, TFailureClass>(
        bool isPublished,
        TDiagnostic? diagnostic,
        Func<TDiagnostic, TFailureClass> classify,
        Action<TFailureClass, string> recordFailure,
        string? correlationId = null,
        Func<string>? createCorrelationId = null)
        where TDiagnostic : class
    {
        ArgumentNullException.ThrowIfNull(classify);
        ArgumentNullException.ThrowIfNull(recordFailure);

        if (isPublished || diagnostic is null)
        {
            return;
        }

        string safeCorrelationId = correlationId
            ?? createCorrelationId?.Invoke()
            ?? Guid.NewGuid().ToString("N")[..8];
        recordFailure(classify(diagnostic), safeCorrelationId);
    }
}
