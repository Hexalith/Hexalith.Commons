// <copyright file="PersistedPublicationCandidate.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Carries a persisted outcome and payload into a publication mapping pipeline.
/// </summary>
/// <typeparam name="TOutcome">The module-owned persistence outcome type.</typeparam>
/// <typeparam name="TTenant">The module-owned tenant identifier type.</typeparam>
public sealed record PersistedPublicationCandidate<TOutcome, TTenant>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersistedPublicationCandidate{TOutcome, TTenant}"/> class.
    /// </summary>
    /// <param name="outcome">The persistence outcome.</param>
    /// <param name="tenant">The validated tenant scope.</param>
    /// <param name="payload">The durable payload candidate.</param>
    public PersistedPublicationCandidate(TOutcome outcome, TTenant tenant, object payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        Outcome = outcome;
        Tenant = tenant;
        Payload = payload;
    }

    /// <summary>
    /// Gets the persistence outcome.
    /// </summary>
    public TOutcome Outcome { get; init; }

    /// <summary>
    /// Gets the validated tenant scope.
    /// </summary>
    public TTenant Tenant { get; init; }

    /// <summary>
    /// Gets the durable payload candidate.
    /// </summary>
    public object Payload { get; init; }
}
