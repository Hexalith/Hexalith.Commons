// <copyright file="PublicationHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Publication.Tests;

using Shouldly;

public sealed class PublicationHelperTest
{
    [Fact]
    public void MappingPipelineShouldPublishValidCandidate()
    {
        PersistedPublicationCandidate<bool, string> candidate = new(true, "tenant-a", new DurableEvent("conversation-started"));

        PublicationMappingDecision<string> decision = Map(candidate);

        decision.IsPublished.ShouldBeTrue();
        decision.Diagnostic.ShouldBeNull();
        PublicEvent published = decision.PublishedEvent.ShouldBeOfType<PublicEvent>();
        published.Metadata.Tenant.ShouldBe("tenant-a");
        published.Metadata.SchemaVersion.ShouldBe(1);
        published.Metadata.EventType.ShouldBe("conversation-started");
    }

    [Theory]
    [InlineData(false, "conversation-started", "tenant-a", 1, "conversation-started", "outcome-failed")]
    [InlineData(true, "unsupported", "tenant-a", 1, "unsupported", "unsupported-payload")]
    [InlineData(true, "conversation-started", "tenant-b", 1, "conversation-started", "tenant-mismatch")]
    [InlineData(true, "conversation-started", "tenant-a", 2, "conversation-started", "unsupported-schema")]
    [InlineData(true, "conversation-started", "tenant-a", 1, "message-posted", "event-type-mismatch")]
    public void MappingPipelineShouldFailClosedForInvalidCandidates(
        bool succeeded,
        string payloadType,
        string metadataTenant,
        int schemaVersion,
        string metadataEventType,
        string expectedDiagnostic)
    {
        PersistedPublicationCandidate<bool, string> candidate = new(
            succeeded,
            "tenant-a",
            new DurableEvent(payloadType, new EventMetadata(metadataTenant, schemaVersion, metadataEventType)));

        PublicationMappingDecision<string> decision = Map(candidate);

        decision.IsPublished.ShouldBeFalse();
        decision.PublishedEvent.ShouldBeNull();
        decision.Diagnostic.ShouldBe(expectedDiagnostic);
    }

    [Fact]
    public void TransportMetadataComposerShouldCopySafeValuesAndHeaders()
    {
        Dictionary<string, string> headers = new(StringComparer.Ordinal)
        {
            ["traceparent"] = "00-safe",
        };

        PublicationTransportMetadata metadata = PublicationTransportMetadataComposer.Compose(
            "conversations.events",
            "conversation.started.v1",
            "/conversations",
            "conversation-1",
            headers);
        headers["traceparent"] = "mutated";

        metadata.Topic.ShouldBe("conversations.events");
        metadata.Type.ShouldBe("conversation.started.v1");
        metadata.Source.ShouldBe("/conversations");
        metadata.Subject.ShouldBe("conversation-1");
        metadata.Headers["traceparent"].ShouldBe("00-safe");
    }

    [Fact]
    public void DeduplicationSetShouldOnlyApplyIdentityOnce()
    {
        PublicationDeduplicationSet set = new();

        set.TryApply("event-1").ShouldBeTrue();
        set.TryApply("event-1").ShouldBeFalse();
        set.TryApply("event-2").ShouldBeTrue();
    }

    [Fact]
    public void FailureTelemetryShouldRecordOnlyRejectedDecisions()
    {
        List<(string FailureClass, string CorrelationId)> recorded = [];

        PublicationFailureTelemetry.RecordRejected<string, string>(
            isPublished: false,
            diagnostic: "tenant-mismatch",
            classify: static diagnostic => diagnostic == "tenant-mismatch" ? "tenant" : "other",
            recordFailure: (failureClass, correlationId) => recorded.Add((failureClass, correlationId)),
            createCorrelationId: static () => "corr-1");
        PublicationFailureTelemetry.RecordRejected<string, string>(
            isPublished: true,
            diagnostic: null,
            classify: static diagnostic => diagnostic == "tenant-mismatch" ? "tenant" : "other",
            recordFailure: (failureClass, correlationId) => recorded.Add((failureClass, correlationId)),
            createCorrelationId: static () => "corr-2");

        recorded.ShouldBe([("tenant", "corr-1")]);
    }

    private static PublicationMappingDecision<string> Map(PersistedPublicationCandidate<bool, string> candidate)
        => PublicationMappingPipeline.TryMap(
            candidate,
            static outcome => outcome,
            static _ => "outcome-failed",
            static payload => payload is DurableEvent { EventType: not "unsupported" } durable
                ? new PublicEvent(
                    durable.EventType,
                    durable.Metadata ?? new EventMetadata("tenant-a", 1, durable.EventType))
                : null,
            static publicEvent => (publicEvent as PublicEvent)?.Metadata,
            static metadata => metadata.Tenant,
            static metadata => metadata.SchemaVersion == 1,
            static (publicEvent, metadata) => publicEvent is PublicEvent @event && @event.EventType == metadata.EventType,
            static () => "unsupported-payload",
            static _ => "tenant-mismatch",
            static _ => "unsupported-schema",
            static _ => "event-type-mismatch");

    private sealed record DurableEvent(string EventType, EventMetadata? Metadata = null);

    private sealed record PublicEvent(string EventType, EventMetadata Metadata);

    private sealed record EventMetadata(string Tenant, int SchemaVersion, string EventType);
}
