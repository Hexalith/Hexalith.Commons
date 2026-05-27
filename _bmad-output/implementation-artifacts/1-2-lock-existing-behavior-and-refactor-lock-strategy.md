# Story 1.2: Lock Existing Behavior and Refactor Lock Strategy

Status: done

## Story

As a Hexalith module developer,
I want regression tests that lock down existing behavior and the lock strategy refactored for isolation,
so that any future changes are proven safe by test coverage and each ID strategy has independent synchronization.

## Acceptance Criteria

1. **Given** the existing `UniqueHelperTest.cs` test file
   **When** the regression tests run
   **Then** `GenerateDateTimeId()` produces a 17-character datetime string matching format `yyyyMMddHHmmssfff`
   **And** sequential IDs are monotonically increasing (each > previous)

2. **Given** 10,000 calls to `GenerateUniqueStringId()`
   **When** the characterization test validates the output
   **Then** every ID is exactly 22 characters long
   **And** every ID contains only Base64URL characters: `A-Za-z0-9-_`
   **And** all 10,000 IDs are unique (per ADR-003)

3. **Given** the existing `_lock` field in `UniqueIdHelper.cs`
   **When** the field is renamed to `_dateTimeLock`
   **Then** only `GenerateDateTimeId()` uses `_dateTimeLock`
   **And** `GenerateUniqueStringId()` remains stateless (no lock)
   **And** the lock rename is a standalone commit for clean `git bisect`

4. **Given** all changes are complete
   **When** `dotnet build` and `dotnet test` are executed
   **Then** zero warnings, zero errors, and all existing + new tests pass

## Tasks / Subtasks

- [x] Task 1: Add characterization test for `GenerateUniqueStringId()` Base64URL output (AC: #2)
  - [x] Add test method `GenerateUniqueStringIdProducesOnly22CharBase64UrlStringsAcrossTenThousandIds` to `UniqueHelperTest.cs`
  - [x] Assert: all 10,000 IDs are 22 chars, match regex `^[A-Za-z0-9_-]{22}$`, and are unique
  - [x] Run `dotnet test` — all tests pass (155 existing + 2 new = 157 total)
- [x] Task 2: Add regression test for `GenerateDateTimeId()` format (AC: #1)
  - [x] Add test method `GenerateDateTimeIdProducesMonotonicallyIncreasingIds` to `UniqueHelperTest.cs`
  - [x] Generate 100 sequential IDs, assert each is > previous via `StringComparer.Ordinal.Compare`
  - [x] Run `dotnet test` — all tests pass (157 total)
- [x] Task 3: Rename `_lock` to `_dateTimeLock` — STANDALONE COMMIT (AC: #3)
  - [x] In `UniqueIdHelper.cs` line 17: rename `_lock` → `_dateTimeLock`
  - [x] In `UniqueIdHelper.cs` line 27: update `_lock.EnterScope()` → `_dateTimeLock.EnterScope()`
  - [x] Run `dotnet build` — zero warnings, zero errors
  - [x] Run `dotnet test` — all 157 tests pass
  - [x] Commit with message: `refactor(unique-ids): rename _lock to _dateTimeLock for per-strategy isolation`
- [x] Task 4: Final verification (AC: #4)
  - [x] `dotnet build Hexalith.Commons.sln` — zero warnings, zero errors
  - [x] `dotnet test Hexalith.Commons.sln` — all 157 tests pass

## Dev Notes

### Architecture Context

This story implements Architecture steps 3-5 and 7-8 from the 12-step test-gated implementation sequence. The lock rename (ADR-002) establishes per-strategy synchronization isolation — a prerequisite for Story 2.1's ULID generation which will add its own `_ulidOptions` field.

**ADR-002 (Separate Locks):** Rename `_lock` → `_dateTimeLock`. Only `GenerateDateTimeId()` uses it. `GenerateUniqueStringId()` is already stateless. Future `GenerateSortableUniqueStringId()` (Story 2.1) will use ByteAether's internal monotonicity via `_ulidOptions`.

**ADR-003 (Characterization Testing):** The characterization test locks current `GenerateUniqueStringId()` output behavior. The current `Replace("/", "_").Replace("+", "-")` approach IS proper Base64URL encoding — the test verifies and documents this. No code change needed if the test passes (it should).

### Exact Code Changes

**File: `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs`**

Only a rename — 2 locations:

```text
Line 17: private static readonly Lock _lock = new();
→        private static readonly Lock _dateTimeLock = new();

Line 27: using (_lock.EnterScope())
→        using (_dateTimeLock.EnterScope())
```

No other code changes. No new methods. No new fields.

**File: `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`**

Add 2 new test methods. Follow existing patterns exactly:

- `[Fact]` attribute (single-case behavior tests)
- Sentence-style PascalCase method names
- Shouldly assertions (`ShouldBe`, `ShouldBeTrue`)
- XML documentation with `<summary>` tags
- MIT copyright header already present (do not duplicate)

### Test Implementation Guidance

**Characterization test (10,000 IDs):**

```csharp
[Fact]
public void GenerateUniqueStringIdProducesOnly22CharBase64UrlStringsAcrossTenThousandIds()
{
    HashSet<string> ids = [];
    System.Text.RegularExpressions.Regex base64UrlPattern = new("^[A-Za-z0-9_-]{22}$");
    for (int i = 0; i < 10_000; i++)
    {
        string id = UniqueIdHelper.GenerateUniqueStringId();
        id.Length.ShouldBe(22);
        base64UrlPattern.IsMatch(id).ShouldBeTrue($"ID '{id}' contains invalid Base64URL characters");
        ids.Add(id);
    }
    ids.Count.ShouldBe(10_000);
}
```

**Monotonic regression test:**

```csharp
[Fact]
public void GenerateDateTimeIdProducesMonotonicallyIncreasingIds()
{
    string previous = UniqueIdHelper.GenerateDateTimeId();
    for (int i = 0; i < 99; i++)
    {
        string current = UniqueIdHelper.GenerateDateTimeId();
        string.Compare(current, previous, StringComparison.Ordinal).ShouldBeGreaterThan(0);
        previous = current;
    }
}
```

**Important:** Use `System.Text.RegularExpressions.Regex` with full namespace or add a `using` — check if the test file already imports it. The existing test file does NOT have this using, so use the fully-qualified name or add `using System.Text.RegularExpressions;`.

### Analyzer Compliance

- **StyleCop SA1201:** Methods must be alphabetically ordered within visibility groups. The new test methods will sort alphabetically among existing methods.
- **XML documentation:** Every new test method needs `<summary>` XML docs (enforced by `GenerateDocumentationFile=true`).
- **No new namespaces** — stay in `Hexalith.Commons.Tests.UniqueIds`.
- **No new files** — add to existing `UniqueHelperTest.cs`.

### Commit Strategy

This story requires **2 commits** (not 1):

1. **First commit:** Add regression + characterization tests (Tasks 1-2)
   - Message: `test(unique-ids): add characterization and regression tests for existing ID methods`
2. **Second commit:** Rename `_lock` → `_dateTimeLock` (Task 3) — MUST be standalone for clean `git bisect`
   - Message: `refactor(unique-ids): rename _lock to _dateTimeLock for per-strategy isolation`

### Previous Story Intelligence (Story 1.1)

- ByteAether.Ulid v1.3.5 successfully added to centralized package management
- Build: 0 warnings, 0 errors after adding dependency
- Tests: 155/155 passed (no regressions from adding the package reference)
- `GenerationOptions` is a **nested class** inside `Ulid` → type alias: `using BaGenerationOptions = ByteAether.Ulid.Ulid.GenerationOptions;`
- `MonotonicityOptions` is nested inside `GenerationOptions` → use `static using` for direct access
- This story does NOT use ByteAether.Ulid at all — it only prepares the lock infrastructure

### Project Structure Notes

Files to modify (2 files, 0 new):

| File                                                         | Change                                           |
| ------------------------------------------------------------ | ------------------------------------------------ |
| `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs` | Rename `_lock` → `_dateTimeLock` (2 occurrences) |
| `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`  | Add 2 new test methods                           |

### References

- [Source: _bmad-output/planning-artifacts/architecture.md — ADR-002: Separate locks per strategy]
- [Source: _bmad-output/planning-artifacts/architecture.md — ADR-003: Characterization testing for Base64URL]
- [Source: _bmad-output/planning-artifacts/architecture.md — Implementation sequence steps 3-5, 7-8]
- [Source: _bmad-output/planning-artifacts/epics.md — Story 1.2 acceptance criteria]
- [Source: _bmad-output/implementation-artifacts/1-1-add-byteaether-ulid-package-dependency.md — Previous story learnings]
- [Source: src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs — Current source (lines 17, 27)]
- [Source: test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs — Current test patterns]
- [Source: _bmad-output/project-context.md — 56 project rules]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Completion Notes List

- This is a test-and-refactor story — no new public API methods
- The characterization test (10,000 IDs) is the most critical deliverable — it gates any future Base64URL changes
- The lock rename MUST be a standalone commit (separate from tests) for clean `git bisect`
- ByteAether.Ulid is NOT used in this story — it was added in Story 1.1 and will be used starting in Story 2.1
- After this story, `_lock` no longer exists — Story 2.1 will add `_ulidOptions` as a separate field with no cross-strategy contention

### Implementation Notes

- **Commit 1** (`test`): Added 2 new test methods to `UniqueHelperTest.cs` — characterization test (10,000 Base64URL IDs) and monotonic regression test (100 datetime IDs). Class made `partial` for `[GeneratedRegex]` source generator. Used `StringComparer.Ordinal.Compare` per Roslynator RCS1235. The datetime regression test now also validates the exact `yyyyMMddHHmmssfff` format. Total: 157 tests passing.
- **Commit 2** (`refactor`): Renamed `_lock` → `_dateTimeLock` in 2 locations in `UniqueIdHelper.cs`. Standalone commit for clean `git bisect`. Zero warnings, zero errors, 157/157 tests pass.
- No new dependencies, no new files, no new public API.

### File List

- `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs` (modify — rename `_lock` → `_dateTimeLock`)
- `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs` (modify — add 2 new test methods)

### Senior Developer Review (AI)

**Reviewer:** JeromePiquot  
**Date:** 2026-03-14  
**Outcome:** Approve

- Git vs story validation: `main..HEAD` contains exactly the two files listed in the story File List (`src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs`, `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`). The promised standalone refactor commit is present as `0b695a7`, and the preceding test-only commit is `a354cc3`.
- Build and test validation passed: `dotnet build Hexalith.Commons.sln -nologo` succeeded with 0 warnings/0 errors, and `dotnet test Hexalith.Commons.sln -nologo --no-build` passed 157/157 tests.
- AC #2 is satisfied by `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs:43` and existing implementation in `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs:54`.
- AC #3 is satisfied by `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs:17` and `:27`; `GenerateUniqueStringId()` remains stateless.
- AC #1 is now satisfied by `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs:27`, which verifies both exact `yyyyMMddHHmmssfff` parsing and monotonic ordering across 100 sequential IDs.
- No remaining HIGH, MEDIUM, or LOW findings after the fix and rerun.

### Change Log

- 2026-03-14: Story created with comprehensive developer context from epics, architecture, previous story, and codebase analysis.
- 2026-03-14: Implementation complete. 2 commits: test additions + lock rename. All 157 tests pass, zero build warnings. Status → review.
- 2026-03-14: Senior developer AI review completed — changes requested. Story moved to in-progress because datetime format regression coverage did not yet prove the exact `yyyyMMddHHmmssfff` output format.
- 2026-03-14: Added exact datetime format assertions to the existing regression test, reran build/tests successfully, and completed the AI review with approval. Status → done.
