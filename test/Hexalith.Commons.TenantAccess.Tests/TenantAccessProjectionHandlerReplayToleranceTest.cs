// <copyright file="TenantAccessProjectionHandlerReplayToleranceTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Hexalith.Commons.TenantAccess;

using Shouldly;

namespace Hexalith.Commons.TenantAccess.Tests;

/// <summary>
/// Covers replay, revocation, and persistence-failure tolerance gaps for the promoted
/// <see cref="TenantAccessProjectionHandler{TEvent, TProjection}"/> (Story 3.2, AC-5).
/// </summary>
public sealed class TenantAccessProjectionHandlerReplayToleranceTest
{
    private static readonly DateTimeOffset EventTimestamp = new(2026, 6, 8, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Event-driven revocation removes the projected principal so a former member is no longer authorized.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task UserRemovedFromTenantShouldRevokeProjectedPrincipal()
    {
        CountingStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-1", 1), cancellationToken);
        await handler.HandleAsync(
            Event(
                TenantAccessProjectionEventKind.UserAddedToTenant,
                "tenant-a",
                "message-2",
                2,
                new TestTenantEventOptions { PrincipalId = "user-a", Role = "TenantOwner" }),
            cancellationToken);
        await handler.HandleAsync(
            Event(
                TenantAccessProjectionEventKind.UserRemovedFromTenant,
                "tenant-a",
                "message-3",
                3,
                new TestTenantEventOptions { PrincipalId = "user-a" }),
            cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.Principals.ShouldNotContainKey("user-a");
        projection.Watermark.ShouldBe(3);
    }

    /// <summary>
    /// A role change updates the metadata-only role evidence for an existing principal.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task UserRoleChangedShouldUpdateProjectedRoleEvidence()
    {
        CountingStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-10", 1), cancellationToken);
        await handler.HandleAsync(
            Event(
                TenantAccessProjectionEventKind.UserAddedToTenant,
                "tenant-a",
                "message-11",
                2,
                new TestTenantEventOptions { PrincipalId = "user-a", Role = "TenantReader" }),
            cancellationToken);
        await handler.HandleAsync(
            Event(
                TenantAccessProjectionEventKind.UserRoleChanged,
                "tenant-a",
                "message-12",
                3,
                new TestTenantEventOptions { PrincipalId = "user-a", Role = "TenantOwner" }),
            cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.Principals["user-a"].Role.ShouldBe("TenantOwner");
    }

    /// <summary>
    /// Tenant enable/disable events toggle the projected enabled flag in delivery order.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task TenantDisabledThenEnabledShouldToggleEnabledFlag()
    {
        CountingStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-20", 1), cancellationToken);
        (await store.GetAsync("tenant-a", cancellationToken)).ShouldNotBeNull().Enabled.ShouldBeTrue();

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantDisabled, "tenant-a", "message-21", 2), cancellationToken);
        (await store.GetAsync("tenant-a", cancellationToken)).ShouldNotBeNull().Enabled.ShouldBeFalse();

        await handler.HandleAsync(Event(TenantAccessProjectionEventKind.TenantEnabled, "tenant-a", "message-22", 3), cancellationToken);
        (await store.GetAsync("tenant-a", cancellationToken)).ShouldNotBeNull().Enabled.ShouldBeTrue();
    }

    /// <summary>
    /// An event with a missing tenant id is dropped before any store interaction.
    /// </summary>
    /// <param name="tenantId">The unsafe tenant id.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EventWithMissingTenantIdShouldBeNoOpWithoutTouchingStore(string tenantId)
    {
        CountingStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store);

        await handler.HandleAsync(
            Event(TenantAccessProjectionEventKind.TenantCreated, tenantId, "message-30", 1),
            CancellationToken.None);

        store.GetCount.ShouldBe(0);
        store.SaveCount.ShouldBe(0);
    }

    /// <summary>
    /// Non-positive sequence numbers are treated as malformed evidence and never advance the projection.
    /// </summary>
    /// <param name="sequenceNumber">The unsafe sequence number.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public async Task NonPositiveSequenceNumberShouldMarkMalformedEvidence(long sequenceNumber)
    {
        CountingStore store = new();
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store);
        CancellationToken cancellationToken = CancellationToken.None;

        await handler.HandleAsync(
            Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-40", sequenceNumber),
            cancellationToken);

        TestProjection? projection = await store.GetAsync("tenant-a", cancellationToken);

        projection.ShouldNotBeNull();
        projection.MalformedEvidence.ShouldBeTrue();
        projection.Watermark.ShouldBe(0);
        projection.Enabled.ShouldBeFalse();
    }

    /// <summary>
    /// When every bounded retry attempt fails with a retryable persistence error, the failure surfaces
    /// rather than being silently swallowed, and the projection is never partially committed.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ExhaustingRetryablePersistenceFailuresShouldRethrowWithoutPartialCommit()
    {
        AlwaysFailingStore store = new(static () => new TimeoutException("transient timeout"));
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, retryAttempts: 2);

        await Should.ThrowAsync<TimeoutException>(async () =>
            await handler.HandleAsync(
                Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-50", 1),
                CancellationToken.None).ConfigureAwait(false));

        store.SaveAttempts.ShouldBe(2);
        (await store.GetAsync("tenant-a", CancellationToken.None)).ShouldBeNull();
    }

    /// <summary>
    /// A non-retryable persistence error propagates immediately without consuming retry budget.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task NonRetryablePersistenceFailureShouldPropagateImmediately()
    {
        AlwaysFailingStore store = new(static () => new InvalidOperationException("non-retryable"));
        TenantAccessProjectionHandler<TestTenantEvent, TestProjection> handler = CreateHandler(store, retryAttempts: 3);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await handler.HandleAsync(
                Event(TenantAccessProjectionEventKind.TenantCreated, "tenant-a", "message-60", 1),
                CancellationToken.None).ConfigureAwait(false));

        store.SaveAttempts.ShouldBe(1);
    }

    private static TenantAccessProjectionHandler<TestTenantEvent, TestProjection> CreateHandler(
        ITenantAccessProjectionStore<TestProjection> store,
        int retryAttempts = 3)
        => new(
            store,
            new FixedClock(EventTimestamp.AddMinutes(1)),
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
        TestTenantEventOptions? options = null)
    {
        options ??= new TestTenantEventOptions();
        return new(new TenantAccessProjectionEvent(
            kind,
            tenantId,
            messageId,
            sequenceNumber,
            EventTimestamp)
        {
            PrincipalId = options.PrincipalId,
            Role = options.Role,
            PayloadFingerprint = tenantId,
        });
    }

    private sealed record TestTenantEvent(TenantAccessProjectionEvent View);

    private sealed record TestTenantEventOptions
    {
        public string? PrincipalId { get; init; }

        public string? Role { get; init; }
    }

    private sealed class TestProjection : TenantAccessProjectionState;

    private sealed class FixedClock(DateTimeOffset now) : ITenantAccessClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private sealed class CountingStore : ITenantAccessProjectionStore<TestProjection>
    {
        private readonly Dictionary<string, TestProjection> _projections = new(StringComparer.Ordinal);

        public int GetCount { get; private set; }

        public int SaveCount { get; private set; }

        public Task<TestProjection?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            GetCount++;
            return Task.FromResult(_projections.GetValueOrDefault(tenantId));
        }

        public Task SaveAsync(TestProjection projection, CancellationToken cancellationToken = default)
        {
            SaveCount++;
            _projections[projection.TenantId] = projection;
            return Task.CompletedTask;
        }
    }

    private sealed class AlwaysFailingStore(Func<Exception> exceptionFactory) : ITenantAccessProjectionStore<TestProjection>
    {
        private readonly Dictionary<string, TestProjection> _projections = new(StringComparer.Ordinal);

        public int SaveAttempts { get; private set; }

        public Task<TestProjection?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(_projections.GetValueOrDefault(tenantId));

        public Task SaveAsync(TestProjection projection, CancellationToken cancellationToken = default)
        {
            SaveAttempts++;
            throw exceptionFactory();
        }
    }
}
