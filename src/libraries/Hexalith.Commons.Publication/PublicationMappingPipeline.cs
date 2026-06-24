// <copyright file="PublicationMappingPipeline.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication;

/// <summary>
/// Generic fail-closed mechanics for mapping persisted candidates to publishable events.
/// </summary>
public static class PublicationMappingPipeline
{
    /// <summary>
    /// Maps a persisted candidate through module-owned payload and metadata adapters.
    /// </summary>
    public static PublicationMappingDecision<TDiagnostic> TryMap<TOutcome, TTenant, TMetadata, TDiagnostic>(
        PersistedPublicationCandidate<TOutcome, TTenant> candidate,
        Func<TOutcome, bool> isSuccessOutcome,
        Func<TOutcome, TDiagnostic> createOutcomeDiagnostic,
        Func<object, object?> mapPayload,
        Func<object, TMetadata?> getMetadata,
        Func<TMetadata, TTenant> getTenant,
        Func<TMetadata, bool> isSchemaSupported,
        Func<object, TMetadata, bool> eventTypeMatches,
        Func<TDiagnostic> createUnsupportedPayloadDiagnostic,
        Func<TMetadata, TDiagnostic> createTenantMismatchDiagnostic,
        Func<TMetadata, TDiagnostic> createUnsupportedSchemaDiagnostic,
        Func<TMetadata, TDiagnostic> createEventTypeMismatchDiagnostic)
        where TMetadata : class
        where TDiagnostic : class
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(isSuccessOutcome);
        ArgumentNullException.ThrowIfNull(createOutcomeDiagnostic);
        ArgumentNullException.ThrowIfNull(mapPayload);
        ArgumentNullException.ThrowIfNull(getMetadata);
        ArgumentNullException.ThrowIfNull(getTenant);
        ArgumentNullException.ThrowIfNull(isSchemaSupported);
        ArgumentNullException.ThrowIfNull(eventTypeMatches);
        ArgumentNullException.ThrowIfNull(createUnsupportedPayloadDiagnostic);
        ArgumentNullException.ThrowIfNull(createTenantMismatchDiagnostic);
        ArgumentNullException.ThrowIfNull(createUnsupportedSchemaDiagnostic);
        ArgumentNullException.ThrowIfNull(createEventTypeMismatchDiagnostic);

        if (!isSuccessOutcome(candidate.Outcome))
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createOutcomeDiagnostic(candidate.Outcome));
        }

        object? publicEvent = mapPayload(candidate.Payload);
        if (publicEvent is null)
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createUnsupportedPayloadDiagnostic());
        }

        TMetadata? metadata = getMetadata(publicEvent);
        if (metadata is null)
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createUnsupportedPayloadDiagnostic());
        }

        if (!EqualityComparer<TTenant>.Default.Equals(candidate.Tenant, getTenant(metadata)))
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createTenantMismatchDiagnostic(metadata));
        }

        if (!isSchemaSupported(metadata))
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createUnsupportedSchemaDiagnostic(metadata));
        }

        if (!eventTypeMatches(publicEvent, metadata))
        {
            return PublicationMappingDecision<TDiagnostic>.Rejected(createEventTypeMismatchDiagnostic(metadata));
        }

        return PublicationMappingDecision<TDiagnostic>.Published(publicEvent);
    }
}
