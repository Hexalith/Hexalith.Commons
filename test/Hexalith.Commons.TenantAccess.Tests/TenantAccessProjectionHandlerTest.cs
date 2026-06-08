// <copyright file="TenantAccessProjectionHandlerTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Hexalith.Commons.TenantAccess;

using Shouldly;

namespace Hexalith.Commons.TenantAccess.Tests;

/// <summary>
/// Verifies the shared tenant-access projection handler behavior promoted from Folders/Projects.
/// </summary>
public sealed class TenantAccessProjectionHandlerTest
{
    private static readonly DateTimeOffset EventTimestamp = new(2026, 6, 8, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Membership and tenant enabled events update metadata-only access evidence.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task UserAddedToTenantShouldProjectMetadataOnlyAccessEvidence()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp.AddMinutes(1));
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-1", 1), cancellationToken);
        await handler.HandleAsync(
            Event(TenantAccessProjectionEventKind.UserAddedToTenant, "tenant-a", "message-2", 2, principalId: "user-a", role: "TenantOwner"),
            cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.TenantId.ShouldBe("tenant-a");
        projection.Enabled.ShouldBeTrue();
        projection.Watermark.ShouldBe(2);
        projection.ProjectionWatermark.ShouldBe("tenant-a:2");
        projection.LastEventTimestamp.ShouldBe(EventTimestamp);
        projection.Principals["user-a"].Role.ShouldBe("TenantOwner");
    }

    /// <summary>
    /// Divergent duplicate messages record replay conflict without advancing watermark.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task DuplicateMessageWithDivergentMetadataShouldRecordReplayConflictWithoutAdvancingWatermark()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp.AddMinutes(1));
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(
            Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-10", 1, payloadFingerprint: "created"),
            cancellationToken);
        await handler.HandleAsync(
            Event(TenantAccessProjectionEventKind.TenantDisabled, "tenant-a", "message-10", 2, payloadFingerprint: "disabled"),
            cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.Watermark.ShouldBe(1);
        projection.ReplayConflict.ShouldBeTrue();
        projection.Enabled.ShouldBeTrue();
    }

    /// <summary>
    /// Identical duplicate messages are no-ops.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task DuplicateMessageWithSameEvidenceShouldBeNoOp()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp.AddMinutes(1));
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-20", 1), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-20", 1), cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.Watermark.ShouldBe(1);
        projection.ReplayConflict.ShouldBeFalse();
        projection.ProcessedMessages.Count.ShouldBe(1);
    }

    /// <summary>
    /// Out-of-order delivery is ignored without poisoning projection health.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task OutOfOrderDeliveryShouldNotAdvanceProjectionOrMarkMalformed()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp.AddMinutes(1));
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-30", 2), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantDisabled, "tenant-a", "message-29", 1), cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.Enabled.ShouldBeTrue();
        projection.Watermark.ShouldBe(2);
        projection.MalformedEvidence.ShouldBeFalse();
        projection.ReplayConflict.ShouldBeFalse();
    }

    /// <summary>
    /// Configuration keys are filtered and removed keys are tombstoned.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ConfigurationKeysShouldBeScopedAndTombstoned()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp.AddMinutes(1));
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-40", 1), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantConfigurationSet, "tenant-a", "message-41", 2, configurationKey: "billing.plan"), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantConfigurationSet, "tenant-a", "message-42", 3, configurationKey: "conversations.enabled"), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantConfigurationRemoved, "tenant-a", "message-43", 4, configurationKey: "conversations.enabled"), cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.ConfigurationKeys.ShouldNotContain("billing.plan");
        projection.ConfigurationKeys.ShouldNotContain("conversations.enabled");
        projection.RemovedConfigurationKeys.ShouldContain("conversations.enabled");
    }

    /// <summary>
    /// Malformed and future events mark projection evidence malformed without advancing.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task MalformedEvidenceShouldFailClosed()
    {
        InMemoryStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, EventTimestamp);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", string.Empty, 1), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.UserAddedToTenant, "tenant-b", "message-50", 1, role: "TenantReader"), cancellationToken);
        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-c", "message-51", 1, timestamp: EventTimestamp.AddMinutes(5)), cancellationToken);

        (await store.GetAsync("tenant-a", cancellationToken)).ShouldNotBeNull().MalformedEvidence.ShouldBeTrue();
        (await store.GetAsync("tenant-b", cancellationToken)).ShouldNotBeNull().MalformedEvidence.ShouldBeTrue();
        (await store.GetAsync("tenant-c", cancellationToken)).ShouldNotBeNull().MalformedEvidence.ShouldBeTrue();
    }

    /// <summary>
    /// Retryable persistence failures are retried within configured attempts.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task RetryablePersistenceFailureShouldRetryWithinConfiguredAttempts()
    {
        FlakyStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(
            store,
            EventTimestamp.AddMinutes(1),
            retryAttempts: 2);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-60", 1), cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);
        projection.ShouldNotBeNull();
        projection.Enabled.ShouldBeTrue();
        store.SaveAttempts.ShouldBe(2);
    }

    private static TenantAccessProjectionHandler<TestTenantEvent, TestProjection> CreateHandler(
        ITenantAccessProjectionStore<TestProjection> store,
        DateTimeOffset now,
        int retryAttempts = 3)
        => new(
            store,
            new FixedClock(now),
            new TenantAccessProjectionHandlerOptions
            {
                ClockSkewTolerance = TimeSpan.FromMinutes(2),
                ConcurrencyRetryAttempts = retryAttempts,
                AcceptsConfigurationKey = static key => key.StartsWith("conversations.", StringComparison.Ordinal),
                IsRetryablePersistenceException = static exception => exception is TimeoutException,
            },
            static @event => @event.View);

    private static TestTenantEvent Event(
        TenantAccessProjectionEventKind kind,
        string tenantId,
        string messageId,
        long sequenceNumber,
        string? principalId = null,
        string? role = null,
        string? configurationKey = null,
        string? payloadFingerprint = null,
        DateTimeOffset? timestamp = null)
        => new(new TenantAccessProjectionEvent(
            kind,
            tenantId,
            messageId,
            sequenceNumber,
            timestamp ?? EventTimestamp,
            principalId,
            role,
            configurationKey,
            payloadFingerprint ?? tenantId));

    private sealed record TestTenantEvent(TenantAccessProjectionEvent View);

    private sealed class TestProjection : TenantAccessProjectionState;

    private sealed class FixedClock(DateTimeOffset now) : ITenantAccessClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private class InMemoryStore : ITenantAccessProjectionStore<TestProjection>
    {
        private readonly Dictionary<string, TestProjection> _projections = new(StringComparer.Ordinal);

        public Task<TestProjection?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(_projections.GetValueOrDefault(tenantId));

        public virtual Task SaveAsync(TestProjection projection, CancellationToken cancellationToken = default)
        {
            _projections[projection.TenantId] = projection;
            return Task.CompletedTask;
        }
    }

    private sealed class FlakyStore : InMemoryStore
    {
        private bool _failed;

        public int SaveAttempts { get; private set; }

        public override Task SaveAsync(TestProjection projection, CancellationToken cancellationToken = default)
        {
            SaveAttempts++;
            if (!_failed)
            {
                _failed = true;
                throw new TimeoutException("transient timeout");
            }

            return base.SaveAsync(projection, cancellationToken);
        }
    }
}
