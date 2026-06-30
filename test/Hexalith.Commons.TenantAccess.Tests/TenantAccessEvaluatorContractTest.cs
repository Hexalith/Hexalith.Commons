// <copyright file="TenantAccessEvaluatorContractTest.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Hexalith.Commons.TenantAccess;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Shouldly;

namespace Hexalith.Commons.TenantAccess.Tests;

/// <summary>
/// Covers the fail-closed contract surface of <see cref="TenantAccessEvaluator"/> not exercised by the
/// happy-path/denial theories: argument guards, cancellation, identity canonicalization, and the
/// tenant/caller validation boundary (Story 3.2, AC-3 and AC-4).
/// </summary>
public sealed class TenantAccessEvaluatorContractTest
{
    private const string Tenant = "tenant-a";
    private const string Caller = "user-a";

    /// <summary>
    /// A requirement outside the closed-world set fails closed by throwing before any projection lookup.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldThrowForUndefinedRequirement()
    {
        StubStore store = new(ActiveState());

        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await EvaluateAsync((TestRequirement)999, [Tenant], Caller, store, new StaticHealth(), CancellationToken.None).ConfigureAwait(false));

        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// Every required collaborator is guarded so a misconfigured caller cannot silently bypass the evaluator.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldGuardNullArguments()
    {
        IEnumerable<string?> tenantIds = [Tenant];
        ITenantAccessStateStore store = new StubStore(ActiveState());
        ITenantAccessProjectionHealthProvider health = new StaticHealth();
        NullGuardEvaluationRules rules = new();

        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(null!, store, health, rules));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, null!, health, rules));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, null!, rules));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { RequirementPredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { StatusDefinedPredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { StatusActivePredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { StatusDisabledPredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { RoleDefinedPredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { PermissionPredicate = null! }));
        await Should.ThrowAsync<ArgumentNullException>(() => InvokeNullGuardAsync(tenantIds, store, health, rules with { Logger = null! }));
    }

    /// <summary>
    /// An already-cancelled token short-circuits before any tenant state is read.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldThrowWhenCancellationAlreadyRequested()
    {
        StubStore store = new(ActiveState());
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await EvaluateAsync(TestRequirement.Read, [Tenant], Caller, store, new StaticHealth(), cts.Token).ConfigureAwait(false));

        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// A valid active owner is allowed and the evaluation reports the canonical tenant and caller identity.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldAllowValidOwnerAndReportCanonicalIdentity()
    {
        StubStore store = new(ActiveState(TestRole.Owner));

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read, [Tenant], Caller, store, new StaticHealth(), CancellationToken.None);

        decision.IsAllowed.ShouldBeTrue();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.None);
        decision.IsRetryable.ShouldBeFalse();
        decision.TenantId.ShouldBe(Tenant);
        decision.CallerPrincipalId.ShouldBe(Caller);
    }

    /// <summary>
    /// Forbidden tenant-identifier characters fail closed as malformed before any projection lookup,
    /// keeping tenant canonicalization injection-safe.
    /// </summary>
    /// <param name="tenantId">The hostile tenant identifier.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData("ten:ant")]
    [InlineData("ten/ant")]
    [InlineData("ten\\ant")]
    [InlineData("ten|ant")]
    [InlineData("ten#ant")]
    [InlineData("ten?ant")]
    [InlineData("ten&ant")]
    [InlineData("ten%ant")]
    [InlineData("ten,ant")]
    [InlineData("ten;ant")]
    [InlineData("ten<ant")]
    [InlineData("ten>ant")]
    [InlineData("ten\"ant")]
    [InlineData("ten'ant")]
    [InlineData("ten ant")]
    [InlineData(" tenant")]
    [InlineData("tenant ")]
    [InlineData("tenant\u0007")]
    public async Task EvaluateAsyncShouldRejectForbiddenTenantCharacters(string tenantId)
    {
        StubStore store = new(ActiveState());

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read, [tenantId], Caller, store, new StaticHealth(), CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.MalformedTenant);
        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// Unsafe caller principals fail closed as a missing caller before any projection lookup.
    /// </summary>
    /// <param name="caller">The hostile caller principal.</param>
    /// <returns>A task.</returns>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" user-a")]
    [InlineData("user-a ")]
    [InlineData("user\u0007a")]
    public async Task EvaluateAsyncShouldRejectUnsafeCallerPrincipals(string caller)
    {
        StubStore store = new(ActiveState());

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read, [Tenant], caller, store, new StaticHealth(), CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.MissingCaller);
        store.GetCount.ShouldBe(0);
    }

    /// <summary>
    /// A projection with a null member map is malformed, not an implicit grant or empty membership.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldDenyProjectionWithNullMembers()
    {
        StubStore store = new(new TenantAccessState(Tenant, (int)TestStatus.Active, null));

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read, [Tenant], Caller, store, new StaticHealth(), CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.MalformedProjection);
        store.GetCount.ShouldBe(1);
    }

    /// <summary>
    /// Multiple tenant-bearing inputs that all canonicalize to the same value resolve to a single grant.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldResolveMatchingMultiInputTenantBindings()
    {
        StubStore store = new(ActiveState(TestRole.Owner));

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [Tenant, null, Tenant, Tenant, null, Tenant],
            Caller,
            store,
            new StaticHealth(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeTrue();
        decision.TenantId.ShouldBe(Tenant);
    }

    /// <summary>
    /// A contradictory binding in any input position denies as a tenant mismatch before projection lookup.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task EvaluateAsyncShouldDenyContradictoryBindingInLaterInputPosition()
    {
        StubStore store = new(ActiveState(TestRole.Owner));

        TenantAccessEvaluation<TestRequirement> decision = await EvaluateAsync(
            TestRequirement.Read,
            [Tenant, null, Tenant, "tenant-b"],
            Caller,
            store,
            new StaticHealth(),
            CancellationToken.None);

        decision.IsAllowed.ShouldBeFalse();
        decision.DenialKind.ShouldBe(TenantAccessDenialKind.TenantMismatch);
        store.GetCount.ShouldBe(0);
    }

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
            IsRequirementDefined,
            IsStatusDefined,
            IsStatusActive,
            IsStatusDisabled,
            IsRoleDefined,
            HasPermission,
            NullLogger.Instance,
            cancellationToken);

    private static Task<TenantAccessEvaluation<TestRequirement>> InvokeNullGuardAsync(
        IEnumerable<string?> tenantIds,
        ITenantAccessStateStore store,
        ITenantAccessProjectionHealthProvider health,
        NullGuardEvaluationRules rules)
        => TenantAccessEvaluator.EvaluateAsync(
            TestRequirement.Read,
            tenantIds,
            Caller,
            store,
            health,
            rules.RequirementPredicate,
            rules.StatusDefinedPredicate,
            rules.StatusActivePredicate,
            rules.StatusDisabledPredicate,
            rules.RoleDefinedPredicate,
            rules.PermissionPredicate,
            rules.Logger,
            CancellationToken.None).AsTask();

    private static bool IsRequirementDefined(TestRequirement requirement) => Enum.IsDefined(requirement);

    private static bool IsStatusDefined(int status) => Enum.IsDefined((TestStatus)status);

    private static bool IsStatusActive(int status) => (TestStatus)status == TestStatus.Active;

    private static bool IsStatusDisabled(int status) => (TestStatus)status == TestStatus.Disabled;

    private static bool IsRoleDefined(int role) => Enum.IsDefined((TestRole)role);

    private static bool HasPermission(int role, TestRequirement requirement)
        => (TestRole)role switch
        {
            TestRole.Reader => requirement == TestRequirement.Read,
            TestRole.Contributor => requirement is TestRequirement.Read or TestRequirement.Write,
            TestRole.Owner => requirement is TestRequirement.Read or TestRequirement.Write or TestRequirement.Admin,
            _ => false,
        };

    private static TenantAccessState ActiveState(
        TestRole role = TestRole.Owner,
        TestStatus status = TestStatus.Active,
        string tenantId = Tenant,
        string caller = Caller)
        => new(tenantId, (int)status, new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [caller] = (int)role,
        });

    private sealed record NullGuardEvaluationRules
    {
        public Func<TestRequirement, bool> RequirementPredicate { get; init; } = TenantAccessEvaluatorContractTest.IsRequirementDefined;

        public Func<int, bool> StatusDefinedPredicate { get; init; } = TenantAccessEvaluatorContractTest.IsStatusDefined;

        public Func<int, bool> StatusActivePredicate { get; init; } = TenantAccessEvaluatorContractTest.IsStatusActive;

        public Func<int, bool> StatusDisabledPredicate { get; init; } = TenantAccessEvaluatorContractTest.IsStatusDisabled;

        public Func<int, bool> RoleDefinedPredicate { get; init; } = TenantAccessEvaluatorContractTest.IsRoleDefined;

        public Func<int, TestRequirement, bool> PermissionPredicate { get; init; } = TenantAccessEvaluatorContractTest.HasPermission;

        public ILogger Logger { get; init; } = NullLogger.Instance;
    }

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

    private sealed class StaticHealth(TenantAccessProjectionHealth? health = null) : ITenantAccessProjectionHealthProvider
    {
        private readonly TenantAccessProjectionHealth? _health = health ?? new(1, "tenant-a:1", false, false, false, false);

        public ValueTask<TenantAccessProjectionHealth?> GetProjectionHealthAsync(
            string tenantId,
            CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_health);
    }

    private sealed class StubStore(TenantAccessState? state) : ITenantAccessStateStore
    {
        public int GetCount { get; private set; }

        public Task<TenantAccessState?> GetAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            GetCount++;
            return Task.FromResult(state);
        }
    }
}
