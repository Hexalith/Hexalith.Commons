# Story 2.1: Implement Sortable Unique ID Generation

Status: done

## Story

As a Hexalith module developer,
I want a `GenerateSortableUniqueStringId()` method that produces ULID-based identifiers,
so that my events, aggregates, and projections sort chronologically without custom comparers.

## Acceptance Criteria

1. **Given** the `UniqueIdHelper` static class
   **When** a developer calls `GenerateSortableUniqueStringId()`
   **Then** it returns a 26-character Crockford Base32 string conforming to the ULID specification
   **And** no ByteAether types appear in the public API — only `string` is returned (per ADR-005)
   **And** a `_ulidOptions` static readonly field configures monotonic ordering (per ADR-007, ADR-008)

2. **Given** 1,000 sequential calls to `GenerateSortableUniqueStringId()`
   **When** the results are sorted via `string.Compare`
   **Then** lexicographic order matches chronological generation order

3. **Given** 100 parallel tasks each calling `GenerateSortableUniqueStringId()`
   **When** all tasks complete
   **Then** all 100 IDs are unique (zero duplicates)

4. **Given** multiple calls within the same millisecond
   **When** IDs are generated rapidly
   **Then** monotonic ordering is maintained (each ID is greater than the previous)

5. **Given** all changes are complete
   **When** `dotnet build` and `dotnet test` are executed
   **Then** zero warnings, zero errors, and all existing + new tests pass

## Tasks / Subtasks

- [x] Task 1: Add type aliases, `_ulidOptions` field, AND `GenerateSortableUniqueStringId()` method together (AC: #1)
  - [x] Add `using BaUlid = ByteAether.Ulid.Ulid;` type alias (no `using static` — not used in this codebase)
  - [x] Add `_ulidOptions` field with XML doc (SA1214: place after `_dateTimeLock`, before `_previous`)
  - [x] Add `GenerateSortableUniqueStringId()` method with XML doc (alphabetical: after `GenerateDateTimeId()`, before `GenerateUniqueStringId()`)
  - [x] Run `dotnet build` — zero warnings, zero errors (field + method added together avoids CS0414 unused field warning)
- [x] Task 2: Add ULID format and length test (AC: #1)
  - [x] Add test: `GenerateSortableUniqueStringIdProduces26CharCrockfordBase32String`
  - [x] Assert: 26 chars, matches Crockford Base32 regex `^[0-9A-HJKMNP-TV-Z]{26}$`
  - [x] Run `dotnet test` — all tests pass
- [x] Task 3: Add sortability test with 1,000 IDs (AC: #2)
  - [x] Add test: `GetAThousandSortableUniqueIdStringInChronologicalOrder`
  - [x] Generate 1,000 sequential IDs, assert lexicographic order matches generation order
  - [x] Run `dotnet test` — all tests pass
- [x] Task 4: Add concurrency test with 100 parallel tasks (AC: #3)
  - [x] Add test: `GetAHundredConcurrentSortableUniqueIdStringWithoutAnyDuplicatesAsync`
  - [x] 100 parallel `Task.Run` calls, assert all 100 IDs unique
  - [x] Run `dotnet test` — all tests pass
  - [x] **If duplicates found:** Apply thread-safety contingency (see Dev Notes), then re-run
- [x] Task 5: Add monotonic ordering test (AC: #4)
  - [x] Add test: `GenerateSortableUniqueStringIdProducesMonotonicallyIncreasingIds`
  - [x] Generate 100 rapid sequential IDs, assert each > previous via `StringComparer.Ordinal.Compare`
  - [x] Run `dotnet test` — all tests pass
- [x] Task 6: Add FR14 coexistence verification test (AC: #5)
  - [x] Add test: `AllThreeIdStrategiesCoexistIndependently`
  - [x] Call all three methods (`GenerateDateTimeId`, `GenerateUniqueStringId`, `GenerateSortableUniqueStringId`) in sequence, assert each returns non-empty and correct length (17, 22, 26)
  - [x] Run `dotnet test` — all tests pass
- [x] Task 7: Final verification (AC: #5)
  - [x] `dotnet build Hexalith.Commons.sln` — zero warnings, zero errors
  - [x] `dotnet test Hexalith.Commons.sln` — all 157 existing tests MUST still pass (regression) + ~5 new tests = ~162 total

## Dev Notes

### Architecture Context

This story implements **Architecture step 9** from the 12-step test-gated implementation sequence (with step 6 — `GenerationOptions` field — folded in as Task 1). This is the core capability of the project: adding ULID-based sortable ID generation.

**ADR-005 (No ByteAether in Public API):** Return `string` only. ByteAether types (`BaUlid`, `BaUlid.GenerationOptions`) are internal implementation details hidden behind type aliases.

**ADR-007 (Default Monotonic Ordering):** ByteAether's default `MonotonicIncrement` mode satisfies FR5 (within-millisecond monotonic ordering). We still set it explicitly in `_ulidOptions` for clarity and safety against library default changes.

**ADR-008 (Static Readonly GenerationOptions):** CLR guarantees thread-safe initialization of static readonly fields. ByteAether's `GenerationOptions` manages its own monotonic state internally — do NOT add a lock or track `_previousUlid`.

**ADR-002 (Separate Locks):** `GenerateSortableUniqueStringId()` does NOT use `_dateTimeLock`. Monotonic synchronization is delegated entirely to ByteAether's `GenerationOptions` internals. Each ID strategy has isolated state.

### Verified ByteAether.Ulid v1.3.5 API

**CRITICAL: These are the exact API names verified from ByteAether.Ulid documentation — do NOT guess or invent alternatives.**

| API                                                               | Usage                                                                         |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `ByteAether.Ulid.Ulid`                                            | The ULID struct. Type alias: `using BaUlid = ByteAether.Ulid.Ulid;`           |
| `ByteAether.Ulid.Ulid.GenerationOptions`                          | Nested class for generation config. Access via `BaUlid.GenerationOptions`     |
| `BaUlid.GenerationOptions.MonotonicityOptions.MonotonicIncrement` | Enum value. Access via fully qualified path through alias (no `using static`) |
| `BaUlid.New(options)`                                             | Generate ULID with options. Returns `Ulid` struct                             |
| `.ToString()`                                                     | Returns 26-char Crockford Base32 string (uppercase: `0-9A-HJKMNP-TV-Z`)       |
| `BaUlid.Parse(string)`                                            | Parse ULID from string. Throws on invalid format                              |

**WARNING — Architecture doc inaccuracies (corrected here):**

- Architecture says `MonotonicityMode.Monotonic` → ACTUAL: `BaUlid.GenerationOptions.MonotonicityOptions.MonotonicIncrement`
- Architecture says `using BaGenerationOptions = ByteAether.Ulid.GenerationOptions;` → ACTUAL: `GenerationOptions` is nested inside `Ulid`, so access via `BaUlid.GenerationOptions` (through type alias)
- Architecture suggests `using static` for `MonotonicityOptions` → NOT USED: this codebase has no `using static` directives; use fully qualified path instead

### Exact Code Changes

**File: `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs`**

Add type alias at the top of the file (after existing `using` directives, before `namespace`):

```csharp
using BaUlid = ByteAether.Ulid.Ulid;
```

**Note:** No `using static` — this codebase has no `using static` directives. Instead, access `MonotonicityOptions` via the fully qualified path through the type alias: `BaUlid.GenerationOptions.MonotonicityOptions.MonotonicIncrement`.

Add static readonly field. **SA1214 ordering:** `static readonly` fields come BEFORE `static` (non-readonly) fields. Since `_dateTimeLock` and `_ulidOptions` are both `static readonly`, and `_previous` is `static` (mutable), the correct order is: `_dateTimeLock`, `_ulidOptions`, then `_previous`.

```csharp
/// <summary>
/// Generation options for ULID with monotonic increment to ensure within-millisecond ordering.
/// </summary>
private static readonly BaUlid.GenerationOptions _ulidOptions = new()
{
    Monotonicity = BaUlid.GenerationOptions.MonotonicityOptions.MonotonicIncrement,
};
```

Add public method (alphabetical ordering — after `GenerateDateTimeId()`, before `GenerateUniqueStringId()`):

```csharp
/// <summary>
/// Generates a sortable unique 26-character ID string based on the ULID specification.
/// ULIDs are chronologically sortable and distributed-safe, making them ideal for
/// event sourcing, aggregate identifiers, and any use case requiring natural ordering.
/// </summary>
/// <returns>A 26-character Crockford Base32 encoded ULID string.</returns>
public static string GenerateSortableUniqueStringId()
    => BaUlid.New(_ulidOptions).ToString();
```

**That's it — 3 additions to the source file. No lock, no `_previousUlid`, no validation logic.**

**Thread-Safety Decision (RESOLVED):** Start lock-free. ByteAether's `GenerationOptions` is designed for shared-instance usage with internal monotonic state management. Implement the one-liner first, then run the concurrency test (Task 5). If and ONLY if the concurrency test fails with duplicate IDs, apply this fix:

1. Add field: `private static readonly Lock _ulidLock = new();` (SA1214: between `_dateTimeLock` and `_ulidOptions`)
2. Change method body to: `using (_ulidLock.EnterScope()) { return BaUlid.New(_ulidOptions).ToString(); }`
3. Re-run ALL tests to confirm fix

This is a **contingency**, not an alternative design. Do NOT add the lock preemptively — it adds unnecessary contention if ByteAether is already thread-safe.

**File: `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`**

Add ~4 new test methods. Follow existing patterns exactly.

### Test Implementation Guidance

**Crockford Base32 character set:** `0123456789ABCDEFGHJKMNPQRSTVWXYZ` (32 chars — excludes I, L, O, U)
**Regex:** `^[0-9A-HJKMNP-TV-Z]{26}$` — expects uppercase per ULID spec canonical form. ByteAether's `.ToString()` outputs uppercase.

The test class is already `partial` (from Story 1.2's `[GeneratedRegex]`). Add a second `[GeneratedRegex]` for Crockford Base32. **Alphabetical ordering:** `CrockfordBase32Pattern()` sorts BEFORE existing `Base64UrlPattern()` — insert it first among the `partial` methods:

```csharp
[GeneratedRegex("^[0-9A-HJKMNP-TV-Z]{26}$")]
private static partial Regex CrockfordBase32Pattern();
```

**Format test:**

```csharp
/// <summary>
/// Tests that a generated sortable unique string ID is exactly 26 characters
/// and contains only valid Crockford Base32 characters conforming to the ULID specification.
/// </summary>
[Fact]
public void GenerateSortableUniqueStringIdProduces26CharCrockfordBase32String()
{
    HashSet<string> ids = [];
    Regex pattern = CrockfordBase32Pattern();
    for (int i = 0; i < 1_000; i++)
    {
        string id = UniqueIdHelper.GenerateSortableUniqueStringId();
        id.Length.ShouldBe(26);
        pattern.IsMatch(id).ShouldBeTrue($"ID '{id}' contains invalid Crockford Base32 characters");
        _ = ids.Add(id);
    }

    ids.Count.ShouldBe(1_000);
}
```

**Monotonic ordering test:**

```csharp
/// <summary>
/// Tests that 100 sequential sortable unique IDs are monotonically increasing,
/// verifying the monotonic increment behavior within the same millisecond window.
/// </summary>
[Fact]
public void GenerateSortableUniqueStringIdProducesMonotonicallyIncreasingIds()
{
    string previous = UniqueIdHelper.GenerateSortableUniqueStringId();
    for (int i = 0; i < 99; i++)
    {
        string current = UniqueIdHelper.GenerateSortableUniqueStringId();
        StringComparer.Ordinal.Compare(current, previous).ShouldBeGreaterThan(0);
        previous = current;
    }
}
```

Use strict `ShouldBeGreaterThan(0)` — `MonotonicIncrement` guarantees strictly increasing values within the same millisecond, and across milliseconds the timestamp component increases. If this test fails, it reveals a real monotonicity bug that must be investigated.

**Out of scope:** Clock skew (system clock jumping backward) and ULID timestamp overflow behavior are delegated entirely to ByteAether and are NOT in scope for this story's tests. Do not attempt to write clock-skew tests or handling.

**Sortability test (1,000 IDs):**

```csharp
/// <summary>
/// Tests that 1,000 sequentially generated sortable unique IDs maintain chronological
/// order when sorted lexicographically, verifying the ULID specification's sortability guarantee.
/// </summary>
[Fact]
public void GetAThousandSortableUniqueIdStringInChronologicalOrder()
{
    List<string> ids = [];
    for (int i = 0; i < 1_000; i++)
    {
        ids.Add(UniqueIdHelper.GenerateSortableUniqueStringId());
    }

    List<string> sorted = [.. ids.OrderBy(id => id, StringComparer.Ordinal)];
    ids.ShouldBe(sorted);
}
```

**Concurrency test (100 parallel tasks):**

```csharp
/// <summary>
/// Tests that concurrent generation of 100 sortable unique IDs
/// produces unique values without any duplicates, verifying
/// thread-safety of the ULID generation process.
/// </summary>
/// <returns>A task that represents the asynchronous operation.</returns>
[Fact]
public async Task GetAHundredConcurrentSortableUniqueIdStringWithoutAnyDuplicatesAsync()
{
    List<Task<string>> tasks = [];
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(UniqueIdHelper.GenerateSortableUniqueStringId));
    }

    string[] result = await Task.WhenAll(tasks);
    result.Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
}
```

**FR14 coexistence test:**

```csharp
/// <summary>
/// Tests that all three ID strategies (DateTime, Base64URL, ULID) coexist independently
/// without interfering with each other, verifying FR14 incremental adoption guarantee.
/// </summary>
[Fact]
public void AllThreeIdStrategiesCoexistIndependently()
{
    string dateTimeId = UniqueIdHelper.GenerateDateTimeId();
    string uniqueId = UniqueIdHelper.GenerateUniqueStringId();
    string sortableId = UniqueIdHelper.GenerateSortableUniqueStringId();

    dateTimeId.Length.ShouldBe(17);
    uniqueId.Length.ShouldBe(22);
    sortableId.Length.ShouldBe(26);
}
```

### Anti-Patterns — DO NOT DO THESE

```csharp
// 1. WRONG: Adding a lock for ULID generation — ByteAether handles synchronization
using (_ulidLock.EnterScope()) { BaUlid.New(_ulidOptions); }  // NO!

// 2. WRONG: Tracking previous ULID — ByteAether owns monotonicity
private static BaUlid _previousUlid;  // NO!

// 3. WRONG: Exposing ByteAether types in public API
public static ByteAether.Ulid.Ulid GenerateUlid()  // NO — return string

// 4. WRONG: Using namespace import (type name collision)
using ByteAether.Ulid;  // NO — "Ulid" collides with namespace
```

### Analyzer Compliance

- **StyleCop SA1201:** Fields and methods must be alphabetically ordered within visibility groups
  - **SA1214:** `static readonly` fields before `static` non-readonly: `_dateTimeLock`, `_ulidOptions` (both readonly), then `_previous` (mutable)
  - Methods order: `GenerateDateTimeId()`, `GenerateSortableUniqueStringId()`, `GenerateUniqueStringId()` (alphabetical ✓)
- **XML documentation:** Every new method/field needs `<summary>` XML docs
- **No new namespaces** — stay in `Hexalith.Commons.UniqueIds`
- **No new files** — modify existing `UniqueIdHelper.cs` and `UniqueHelperTest.cs`
- **MIT copyright header** — already present, do not duplicate
- **Test methods:** Sentence-style PascalCase, `[Fact]` for single-behavior tests

### Branch & Commit Strategy

**Branch name:** `feat/2-1-implement-sortable-unique-id-generation` (created from `main` AFTER Story 1.2 is merged)

This story requires **1 commit** (source + tests together since they are a single feature):

- Message: `feat(unique-ids): add sortable ULID generation` (47 chars — under 50-char limit)
- This is a `feat` because it adds a new public API method (triggers minor version bump per semantic-release)
- **CRITICAL:** Do NOT commit until ALL tasks (1-7) pass. Complete all code changes, run `dotnet build` and `dotnet test` to verify zero warnings/errors and all tests green, THEN commit once.

### Previous Story Intelligence (Story 1.2)

- ByteAether.Ulid v1.3.5 is already added to `Directory.Packages.props` and `Hexalith.Commons.UniqueIds.csproj` (Story 1.1)
- `_lock` already renamed to `_dateTimeLock` (Story 1.2) — per-strategy isolation is in place
- Test class is already `partial` (for `[GeneratedRegex]` source generator) — can add more `[GeneratedRegex]` patterns
- 157 tests currently passing (155 original + 2 from Story 1.2)
- Used `StringComparer.Ordinal.Compare` per Roslynator RCS1235 — continue this pattern
- Used `_ = ids.Add(id)` to discard return value — continue this pattern

### Git Intelligence

Recent commits on branch `feat/1-2-lock-existing-behavior-and-refactor-lock-strategy`:

- `0b695a7` refactor(unique-ids): rename \_lock to \_dateTimeLock for per-strategy isolation
- `a354cc3` test(unique-ids): add characterization and regression tests for existing ID methods

Files changed: `UniqueIdHelper.cs` (4 lines), `UniqueHelperTest.cs` (43 lines added)

**Note:** Story 2.1 should be implemented on a **new branch** from `main` (after Story 1.2 is merged), not on the current 1.2 branch.

### Project Structure Notes

Files to modify (3 files, 0 new):

| File                                                                         | Change                                                                            |
| ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` | Add `ByteAether.Ulid` package reference                                           |
| `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs`                 | Add type aliases, `_ulidOptions` field, `GenerateSortableUniqueStringId()` method |
| `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`                  | Add ~5 new test methods + 1 `[GeneratedRegex]` pattern                            |

### References

- [Source: _bmad-output/planning-artifacts/architecture.md — ADR-005, ADR-007, ADR-008, Implementation step 9]
- [Source: _bmad-output/planning-artifacts/architecture.md — Anti-patterns, Enforcement rules, Naming patterns]
- [Source: _bmad-output/planning-artifacts/epics.md — Story 2.1 acceptance criteria, FR1/FR4/FR5/FR14]
- [Source: _bmad-output/implementation-artifacts/1-2-lock-existing-behavior-and-refactor-lock-strategy.md — Previous story learnings]
- [Source: ByteAether.Ulid v1.3.5 documentation — Verified API names: Ulid.New, GenerationOptions, MonotonicityOptions]
- [Source: src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs — Current source]
- [Source: test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs — Current test patterns]
- [Source: _bmad-output/project-context.md — 56 project rules]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

### Completion Notes List

- This is a feature story — adds 1 new public API method (`GenerateSortableUniqueStringId`)
- The method is a one-liner delegating to `BaUlid.New(_ulidOptions).ToString()`
- NO lock needed — ByteAether handles monotonic synchronization internally (concurrency test confirmed)
- NO `_previousUlid` field — ByteAether owns monotonic state
- After this story, `UniqueIdHelper` has 3 independent ID strategies: DateTime (17-char), Base64URL (22-char), ULID (26-char)
- All 162 tests pass (157 existing + 5 new), zero build warnings
- Thread-safety contingency was NOT needed — ByteAether `GenerationOptions` is thread-safe out of the box
- Epic 3 (Stories 3.1-3.2) will add `ExtractTimestamp`, `ToGuid`, and `ToSortableUniqueId` using the same ByteAether dependency

### File List

| File                                                                         | Change                                                                            |
| ---------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` | Added `ByteAether.Ulid` package reference (4 lines)                               |
| `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs`                 | Added type alias, `_ulidOptions` field, `GenerateSortableUniqueStringId()` method |
| `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`                  | Added 5 test methods + `CrockfordBase32Pattern()` generated regex                 |

### Change Log

- 2026-03-14: Implemented Story 2.1 — Added `GenerateSortableUniqueStringId()` public API method returning 26-char ULID strings, with 5 comprehensive tests covering format, sortability, concurrency, monotonicity, and strategy coexistence
- 2026-03-14: Senior Developer Review (AI) completed — implementation validated, but story/git reality traceability issues require follow-up before marking done
- 2026-03-14: Addressed review findings — updated File List to include `.csproj` (finding #1); working tree is clean on main branch (finding #2 resolved)
- 2026-03-14: Senior Developer Review (AI) re-run completed — no remaining implementation or traceability issues; story marked done and sprint status synced

## Senior Developer Review (AI)

**Reviewer:** GitHub Copilot
**Date:** 2026-03-14
**Outcome:** Changes Requested

### What I validated

- Acceptance Criteria 1-5 are implemented in code and covered by tests
- `dotnet build Hexalith.Commons.sln` succeeded with zero warnings and zero errors
- Full test run passed: 162/162 tests
- ULID dependency state is present as expected in `Hexalith.Builds/Props/Directory.Packages.props:77` and `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj:8`

### Findings

1. **Medium — Story file list does not match git reality**

- The story still claims `Files to modify (2 files, 0 new)` and the Dev Agent Record lists only `UniqueIdHelper.cs` and `UniqueHelperTest.cs`.
- However, git shows `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` as modified too, and that file contains the active `ByteAether.Ulid` package reference at line 8.
- Evidence: story notes at lines 303, 322, and 359 vs git status plus `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj:8`.
- Impact: implementation is correct, but review traceability is incomplete, so the story should not be marked `done` yet.

2. **Medium — Working tree includes additional undocumented changes during review**

- Git status also shows an uncommitted `Hexalith.Builds` submodule change while Story 2.1 is under review.
- Impact: this makes it unclear whether the review is being performed from a clean Story 2.1 branch state, which the story itself says should be based on a fresh branch from `main`.

### Decision

- Code quality: **Approved**
- Story/review traceability: **Changes Requested**
- Final story status: **in-progress** until the story record matches the actual branch contents

### Review Follow-up Resolution (2026-03-14)

- [x] Finding #1 resolved: File List updated to include `Hexalith.Commons.UniqueIds.csproj` with 3 files total
- [x] Finding #2 resolved: Working tree is clean on main branch, no undocumented submodule changes

### Re-review (2026-03-14)

**Reviewer:** GitHub Copilot
**Outcome:** Approved

### What I validated

- Acceptance Criteria 1-5 remain fully implemented in `UniqueIdHelper.cs` and `UniqueHelperTest.cs`
- `dotnet build Hexalith.Commons.sln` succeeded with zero warnings and zero errors
- `dotnet test Hexalith.Commons.sln` passed with 162/162 tests green
- `Hexalith.Builds/Props/Directory.Packages.props` still pins `ByteAether.Ulid` to `1.3.5`
- The remaining story traceability inconsistency in Project Structure Notes was corrected to match the three-file implementation footprint

### Findings

- No HIGH, MEDIUM, or LOW issues remain

### Decision

- Code quality: **Approved**
- Story/review traceability: **Approved**
- Final story status: **done**
