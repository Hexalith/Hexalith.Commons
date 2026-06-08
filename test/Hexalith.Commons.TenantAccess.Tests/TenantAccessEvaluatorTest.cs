// <copyright file="TenantAccessEvaluatorTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Hexalith.Commons.TenantAccess;

using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

namespace Hexalith.Commons.TenantAccess.Tests;

/// <summary>
/// Verifies the shared fail-closed evaluator preserves the Conversations denial surface.
/// </summary>
public sealed class TenantAccessEvaluatorTest
{
    private const string Tenant = "tenant-a";
    private const string Caller = "user-a";

    /// <summary>
    /// Missing and malformed inputs deny before projection lookup.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="caller">The caller principal.</param>
    /// <param name="expected">The expected denial.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData(null, Caller, TenantAccessDenialKind.MissingTenant)]
    [InlineData("tenant-a:rogue", Caller, TenantAccessDenialKind.MalformedTenant)]
    [InlineData(Tenant, null, TenantAccessDenialKind.MissingCaller)]
    [InlineData(Tenant, " user-a", TenantAccessDenialKind.MissingCaller)]
    public async Task EvaluateAsyncShouldDenyUnsafeInputsBeforeStoreLookup(
        string? tenantId,
        string? caller,
        TenantAccessDenialKind expected)
    {
        StubTenantAccessStateStore store = new(ActiveState());

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [tenantId],
            caller,
            store,
            new StaticHealthProvider(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(expected);
        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// Contradictory tenant bindings deny before projection lookup.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldDenyTenantMismatchesBeforeStoreLookup()
    {
        StubTenantAccessStateStore store = new(ActiveState());

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Write,
            [Tenant, "tenant-b"],
            Caller,
            store,
            new StaticHealthProvider(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.TenantMismatch);
        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// Projection-health failures deny before stored state is trusted.
    /// </summary>
    /// <param name="health">The health record.</param>
    /// <param name="expected">The expected denial.</param>
    /// <param name="retryable">Whether the denial is retryable.</param>
    /// <returns>A task.</returns>
    [Theory]
    [MemberData(nameof(HealthDenials))]
    public async Task EvaluateAsyncShouldDenyUnsafeProjectionHealth(
        TenantAccessProjectionHealth? health,
        TenantAccessDenialKind expected,
        bool retryable)
    {
        StubTenantAccessStateStore store = new(ActiveState());

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [Tenant],
            Caller,
            store,
            new StaticHealthProvider(health),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(expected);
        decision.IsRetryable.ShouldBe(retryable);
        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// Projection store exceptions become retryable unavailable denials.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldClassifyStoreExceptionsAsRetryableUnavailable()
    {
        StubTenantAccessStateStore store = new(ActiveState())
        {
            OnGet = static () => throw new InvalidOperationException("raw upstream payload"),
        };

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [Tenant],
            Caller,
            store,
            new StaticHealthProvider(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.TenantAccessUnavailable);
        decision.IsRetryable.ShouldBeTrue();
        store.GetCount.ShouldBe(1);
    }

    /// <summary>
    /// Unsafe projection states deny closed.
    /// </summary>
    /// <param name="state">The stored tenant state.</param>
    /// <param name="expected">The expected denial.</param>
    /// <returns>A task.</returns>
    [Theory]
    [MemberData(nameof(ProjectionDenials))]
    public async Task EvaluateAsyncShouldDenyUnsafeProjectionStates(
        TenantAccessState? state,
        TenantAccessDenialKind expected)
    {
        StubTenantAccessStateStore store = new(state);

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [Tenant],
            Caller,
            store,
            new StaticHealthProvider(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(expected);
        store.GetCount.ShouldBe(1);
    }

    /// <summary>
    /// Valid roles map through the module-supplied permission predicate.
    /// </summary>
    /// <param name="role">The tenant role.</param>
    /// <param name="requirement">The requirement.</param>
    /// <param name="expectedAllowed">Whether access is expected.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData(TestRole.Reader, TestRequirement.Read, true)]
    [InlineData(TestRole.Reader, TestRequirement.Write, false)]
    [InlineData(TestRole.Contributor, TestRequirement.Write, true)]
    [InlineData(TestRole.Contributor, TestRequirement.Admin, false)]
    [InlineData(TestRole.Owner, TestRequirement.Admin, true)]
    public async Task EvaluateAsyncShouldMapRolesThroughRequirementPredicate(
        TestRole role,
        TestRequirement requirement,
        bool expectedAllowed)
    {
        StubTenantAccessStateStore store = new(ActiveState(role));

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            requirement,
            [Tenant],
            Caller,
            store,
            new StaticHealthProvider(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBe(expectedAllowed);
        decision.DenialKind.ShouldBe(expectedAllowed ? TenantAccessDenialKind.None : TenantAccessDenialKind.InsufficientRole);
    }

    /// <summary>
    /// Gets projection health denial cases.
    /// </summary>
    public static TheoryData<TenantAccessProjectionHealth?, TenantAccessDenialKind, bool> HealthDenials() => new()
    {
        { null, TenantAccessDenialKind.TenantAccessUnavailable, true },
        { new(1, "tenant-a:1", IsStale: true, HasGap: false, HasRollback: false, IsPoisoned: false), TenantAccessDenialKind.TenantAccessStale, true },
        { new(1, "tenant-a:1", IsStale: false, HasGap: true, HasRollback: false, IsPoisoned: false), TenantAccessDenialKind.TenantAccessGapDetected, true },
        { new(1, "tenant-a:1", IsStale: false, HasGap: false, HasRollback: true, IsPoisoned: false), TenantAccessDenialKind.TenantAccessRolledBack, true },
        { new(1, "tenant-a:1", IsStale: false, HasGap: false, HasRollback: false, IsPoisoned: true), TenantAccessDenialKind.TenantProjectionPoisoned, false },
    };

    /// <summary>
    /// Gets projection state denial cases.
    /// </summary>
    public static TheoryData<TenantAccessState?, TenantAccessDenialKind> ProjectionDenials() => new()
    {
        { null, TenantAccessDenialKind.UnknownTenant },
        { ActiveState(tenantId: "tenant-a:bad"), TenantAccessDenialKind.MalformedProjection },
        { ActiveState(tenantId: "tenant-b"), TenantAccessDenialKind.TenantMismatch },
        { ActiveState(status: TestStatus.Disabled), TenantAccessDenialKind.TenantDisabled },
        { ActiveState(status: TestStatus.Unknown), TenantAccessDenialKind.UnmappedStatus },
        { ActiveState(role: TestRole.Reader, caller: "other-user"), TenantAccessDenialKind.MissingMember },
        { ActiveState(role: (TestRole)999), TenantAccessDenialKind.UnmappedRole },
        { PoisonedMemberState(), TenantAccessDenialKind.TenantProjectionPoisoned },
    };

    private static ValueTask<TenantAccessEvaluation<TestRequirement>> EvaluateAsync(
        TestRequirement requirement,
        IEnumerable<string?> tenantIds,
        string? caller,
        ITenantAccessStateStore store,
        ITenantAccessProjectionHealthProvider health,
        CancellationToken cancellationToken)
        => TenantAccessEvaluator.EvaluateAsync(
            requirement,
            tenantIds,
            caller,
            store,
            health,
            static requirement => Enum.IsDefined(requirement),
            static status => Enum.IsDefined((TestStatus)status),
            static status => (TestStatus)status == TestStatus.Active,
            static status => (TestStatus)status == TestStatus.Disabled,
            static role => Enum.IsDefined((TestRole)role),
            static (role, requirement) => (TestRole)role switch
            {
                TestRole.Reader => requirement == TestRequirement.Read,
                TestRole.Contributor => requirement is TestRequirement.Read or TestRequirement.Write,
                TestRole.Owner => requirement is TestRequirement.Read or TestRequirement.Write or TestRequirement.Admin,
                _ => false,
            },
            NullLogger.Instance,
            cancellationToken);

    private static TenantAccessState ActiveState(
        TestRole role = TestRole.Owner,
        TestStatus status = TestStatus.Active,
        string tenantId = Tenant,
        string caller = Caller)
        => new(tenantId, (int)status, new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [caller] = (int)role,
        });

    private static TenantAccessState PoisonedMemberState()
        => new(Tenant, (int)TestStatus.Active, new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [" user-a"] = (int)TestRole.Owner,
        });

    public enum TestRequirement
    {
        Read,
        Write,
        Admin,
    }

    public enum TestStatus
    {
        Unknown,
        Active,
        Disabled,
    }

    public enum TestRole
    {
        Reader,
        Contributor,
        Owner,
    }

    private sealed class StaticHealthProvider(TenantAccessProjectionHealth? health = null) : ITenantAccessProjectionHealthProvider
    {
        private readonly TenantAccessProjectionHealth? _health = health ?? new(1, "tenant-a:1", false, false, false, false);

        public ValueTask<TenantAccessProjectionHealth?> GetProjectionHealthAsync(
            string tenantId,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_health);
    }

    private sealed class StubTenantAccessStateStore(TenantAccessState? state) : ITenantAccessStateStore
    {
        public Func<TenantAccessState?>? OnGet { get; init; }

        public int GetCount { get; private set; }

        public Task<TenantAccessState?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            GetCount++;
            return Task.FromResult(OnGet?.Invoke() ?? state);
        }
    }
}
