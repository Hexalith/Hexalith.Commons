# Story 3.2: Bidirectional ULID-Guid Conversion

Status: ready-for-dev

## Story

As a Hexalith module developer,
I want to convert between ULID strings and Guids,
so that I can interoperate with external systems that require Guid identifiers while maintaining ULID-based ordering internally.

## Acceptance Criteria

1. Given a valid ULID string
   - When a developer calls `ToGuid(string ulid)`
   - Then it returns a `Guid` that preserves the ULID's identity (per ADR-004)
   - And no ByteAether types appear in the method signature (per ADR-005)

2. Given a `Guid` value
   - When a developer calls `ToSortableUniqueId(Guid guid)`
   - Then it returns a 26-character ULID string in Crockford Base32 format

3. Given a ULID string converted to Guid via `ToGuid()` then back via `ToSortableUniqueId()`
   - When the round-trip completes
   - Then the original ULID string is returned identically (lossless round-trip per FR9)

4. Given a null, empty, or whitespace string passed to `ToGuid()`
   - When the method is called
   - Then an `ArgumentException` is thrown (per ADR-006)

5. Given an invalid ULID format string passed to `ToGuid()`
   - When the method is called
   - Then a `FormatException` is thrown with a descriptive message (per ADR-006)

6. Given a non-ULID Guid (e.g., `Guid.NewGuid()`) passed to `ToSortableUniqueId()`
   - When the method is called
   - Then it returns a valid 26-character ULID string (does not throw)
   - And `ExtractTimestamp()` on the result returns a `DateTimeOffset` (does not throw)

7. All existing tests continue to pass (162+ tests as of Story 2.1, plus Story 3.1 tests)

## Prerequisites

- **Story 3.1 (Extract Timestamp from ULID) must be merged to `main` before starting this story.** Task 6.2 and AC #6 depend on `ExtractTimestamp()` which is implemented in Story 3.1. Without it, the edge case test will not compile.

## Tasks / Subtasks

- [ ] Task 0: Verify prerequisite (BLOCKING)
  - [ ] 0.1 Confirm Story 3.1 is merged to `main` and `ExtractTimestamp()` exists in `UniqueIdHelper.cs`
  - [ ] 0.2 Run `dotnet build` to verify baseline compiles

- [ ] Task 1: Implement `ToGuid(string ulid)` method (AC: #1, #4, #5)
  - [ ] 1.1 Add method to `UniqueIdHelper.cs` — alphabetical placement per SA1201 (after `GenerateUniqueStringId()`, before private fields or at correct alphabetical position among public methods)
  - [ ] 1.2 Validate input with `ArgumentException.ThrowIfNullOrWhiteSpace(ulid)`
  - [ ] 1.3 Parse ULID and convert: `BaUlid.Parse(ulid).ToGuid()`
  - [ ] 1.4 Wrap non-ArgumentException in `FormatException` (same pattern as `ExtractTimestamp`)
  - [ ] 1.5 Add XML documentation including ADR-004 caveat: conversion preserves identity NOT sort order

- [ ] Task 2: Implement `ToSortableUniqueId(Guid guid)` method (AC: #2, #6)
  - [ ] 2.1 Add method to `UniqueIdHelper.cs` — alphabetical placement per SA1201 (after `ToGuid`)
  - [ ] 2.2 Implementation: `BaUlid.New(guid).ToString()`
  - [ ] 2.3 Add XML documentation noting: non-ULID Guids produce valid ULID strings with meaningless timestamps

- [ ] Task 3: Add round-trip conversion tests (AC: #3)
  - [ ] 3.1 `ConvertSortableUniqueIdToGuidAndBackShouldReturnOriginalValue` — [Fact]
  - [ ] 3.2 `ConvertAHundredSortableUniqueIdsToGuidAndBackShouldAllReturnOriginalValues` — [Fact], bulk round-trip test generating 100 ULIDs to catch any state-dependent conversion bugs

- [ ] Task 4: Add ToGuid validation tests (AC: #4, #5)
  - [ ] 4.1 `ToGuidFromNullOrWhiteSpaceThrowsArgumentException` — [Theory] with `[InlineData(null)]`, `[InlineData("")]`, `[InlineData("   ")]`
  - [ ] 4.2 `ToGuidFromInvalidFormatThrowsFormatException` — [Theory] with invalid ULID strings

- [ ] Task 5: Add ToGuid positive test (AC: #1)
  - [ ] 5.1 `ToGuidFromValidUlidReturnsNonEmptyGuid` — [Fact], verify `guid.ShouldNotBe(Guid.Empty)`

- [ ] Task 6: Add ToSortableUniqueId tests (AC: #2, #6)
  - [ ] 6.1 `ToSortableUniqueIdFromGuidReturns26CharCrockfordBase32String` — [Fact], verify format with `CrockfordBase32Pattern()`
  - [ ] 6.2 `ToSortableUniqueIdFromRandomGuidProducesValidUlidWithExtractableTimestamp` — [Fact], edge case: `Guid.NewGuid()` produces valid ULID and `ExtractTimestamp()` does not throw
  - [ ] 6.3 `ToSortableUniqueIdFromEmptyGuidReturnsAllZerosUlid` — [Fact], edge case: `Guid.Empty` produces valid 26-char all-zeros ULID string

- [ ] Task 7: Build and test (AC: #7)
  - [ ] 7.1 Run `dotnet build` — zero warnings, zero errors
  - [ ] 7.2 Run `dotnet test` — all tests green (existing 162+ plus new ~8 tests)

- [ ] Task 8: Commit
  - [ ] 8.1 Branch: `feat/3-2-bidirectional-ulid-guid-conversion` from `main`
  - [ ] 8.2 Single commit: `feat(unique-ids): add bidirectional ULID-Guid conversion`
  - [ ] 8.3 CRITICAL: All tasks 1-7 must pass before commit

## Dev Notes

### Architecture Compliance

**ADR-004 (Identity not sort order):** `ToGuid()` preserves the 128-bit identity of a ULID but the resulting Guid does NOT maintain lexicographic sort order. This MUST be documented in XML docs.

**ADR-005 (No ByteAether in public API):** Method signatures use only `string`, `Guid`, `DateTimeOffset`. ByteAether is purely internal. Type alias `using BaUlid = ByteAether.Ulid.Ulid;` already in place.

**ADR-006 (Exception-based error handling):**
- Null/empty/whitespace → `ArgumentException.ThrowIfNullOrWhiteSpace()`
- Invalid ULID format → `FormatException` wrapping ByteAether parse failure
- `ToSortableUniqueId(Guid)` does NOT throw — any Guid produces a valid ULID string

### Implementation Patterns

**ToGuid implementation — follows ExtractTimestamp pattern exactly:**
```csharp
/// <summary>
/// Converts a ULID string to a <see cref="Guid"/>.
/// </summary>
/// <param name="ulid">A 26-character Crockford Base32 ULID string.</param>
/// <returns>A <see cref="Guid"/> preserving the ULID's 128-bit identity.</returns>
/// <remarks>
/// The resulting Guid preserves identity but NOT lexicographic sort order.
/// Two ULIDs that sort correctly as strings may not sort the same way as Guids
/// due to Guid byte-order differences.
/// </remarks>
/// <exception cref="ArgumentException">Thrown when <paramref name="ulid"/> is null, empty, or whitespace.</exception>
/// <exception cref="FormatException">Thrown when <paramref name="ulid"/> is not a valid ULID string.</exception>
public static Guid ToGuid(string ulid)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(ulid);
    try
    {
        return BaUlid.Parse(ulid).ToGuid();
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
        throw new FormatException($"The value '{ulid}' is not a valid ULID string.", ex);
    }
}
```

**ToSortableUniqueId implementation — simple, no validation needed:**
```csharp
/// <summary>
/// Converts a <see cref="Guid"/> to a 26-character ULID string.
/// </summary>
/// <param name="guid">The Guid to convert.</param>
/// <returns>A 26-character Crockford Base32 ULID string.</returns>
/// <remarks>
/// When called with a Guid not originally derived from a ULID (e.g., <see cref="Guid.NewGuid()"/>),
/// the result is a valid ULID string but its embedded timestamp is meaningless.
/// </remarks>
public static string ToSortableUniqueId(Guid guid)
{
    return BaUlid.New(guid).ToString();
}
```

**Thread safety:** Both methods are stateless — no locks, no shared state. Pure conversion operations.

### ByteAether.Ulid v1.3.5 API (Verified)

| API Call | Purpose |
|----------|---------|
| `BaUlid.Parse(string)` | Parse ULID from 26-char Crockford Base32 string; throws on invalid |
| `.ToGuid()` | Instance method converting ULID struct to `System.Guid` |
| `BaUlid.New(Guid)` | Create ULID from Guid bytes (identity-preserving) |
| `.ToString()` | Returns 26-char Crockford Base32 string |

Round-trip verified in ByteAether docs: `Ulid.New() → .ToGuid() → Ulid.New(guid) → equal to original`

### Method Alphabetical Placement (SA1201)

Current public method order in `UniqueIdHelper.cs`:
1. `ExtractTimestamp(string ulid)` — added by Story 3.1
2. `GenerateDateTimeId()`
3. `GenerateSortableUniqueStringId()`
4. `GenerateUniqueStringId()`

After Story 3.2, the order becomes:
1. `ExtractTimestamp(string ulid)`
2. `GenerateDateTimeId()`
3. `GenerateSortableUniqueStringId()`
4. `GenerateUniqueStringId()`
5. **`ToGuid(string ulid)`** ← NEW
6. **`ToSortableUniqueId(Guid guid)`** ← NEW

### Files to Modify (2 files, 0 new)

| File | Action |
|------|--------|
| `src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs` | Add `ToGuid()` and `ToSortableUniqueId()` methods |
| `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs` | Add ~8 test methods |

### Test Implementation Guidance

**Test class:** `UniqueHelperTest` (already `partial`, already has `CrockfordBase32Pattern()` regex)

**Test methods to add (~8 tests):**

```csharp
[Fact]
public void ConvertSortableUniqueIdToGuidAndBackShouldReturnOriginalValue()
{
    string original = UniqueIdHelper.GenerateSortableUniqueStringId();
    Guid guid = UniqueIdHelper.ToGuid(original);
    string roundTripped = UniqueIdHelper.ToSortableUniqueId(guid);
    roundTripped.ShouldBe(original);
}

[Fact]
public void ConvertAHundredSortableUniqueIdsToGuidAndBackShouldAllReturnOriginalValues()
{
    for (int i = 0; i < 100; i++)
    {
        string original = UniqueIdHelper.GenerateSortableUniqueStringId();
        Guid guid = UniqueIdHelper.ToGuid(original);
        string roundTripped = UniqueIdHelper.ToSortableUniqueId(guid);
        roundTripped.ShouldBe(original);
    }
}

[Fact]
public void ToGuidFromValidUlidReturnsNonEmptyGuid()
{
    string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();
    Guid guid = UniqueIdHelper.ToGuid(ulid);
    guid.ShouldNotBe(Guid.Empty);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void ToGuidFromNullOrWhiteSpaceThrowsArgumentException(string? ulid)
{
    Should.Throw<ArgumentException>(() => UniqueIdHelper.ToGuid(ulid!));
}

[Theory]
[InlineData("short")]
[InlineData("THIS_IS_NOT_A_VALID_ULID!!")]
[InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA!")]
public void ToGuidFromInvalidFormatThrowsFormatException(string ulid)
{
    Should.Throw<FormatException>(() => UniqueIdHelper.ToGuid(ulid));
}

[Fact]
public void ToSortableUniqueIdFromGuidReturns26CharCrockfordBase32String()
{
    Guid guid = UniqueIdHelper.ToGuid(UniqueIdHelper.GenerateSortableUniqueStringId());
    string result = UniqueIdHelper.ToSortableUniqueId(guid);
    result.Length.ShouldBe(26);
    CrockfordBase32Pattern().IsMatch(result).ShouldBeTrue();
}

[Fact]
public void ToSortableUniqueIdFromRandomGuidProducesValidUlidWithExtractableTimestamp()
{
    Guid randomGuid = Guid.NewGuid();
    string ulid = UniqueIdHelper.ToSortableUniqueId(randomGuid);
    ulid.Length.ShouldBe(26);
    CrockfordBase32Pattern().IsMatch(ulid).ShouldBeTrue();
    // Should not throw — edge case per architecture
    _ = UniqueIdHelper.ExtractTimestamp(ulid);
}

[Fact]
public void ToSortableUniqueIdFromEmptyGuidReturnsAllZerosUlid()
{
    string ulid = UniqueIdHelper.ToSortableUniqueId(Guid.Empty);
    ulid.Length.ShouldBe(26);
    CrockfordBase32Pattern().IsMatch(ulid).ShouldBeTrue();
}
```

### Anti-Patterns to Avoid

1. **Do NOT manually validate ULID format** — delegate entirely to `BaUlid.Parse()`
2. **Do NOT expose `BaUlid` in method signatures** — return `Guid` and `string` only
3. **Do NOT add null check on `ToSortableUniqueId(Guid guid)`** — `Guid` is a value type, cannot be null
4. **Do NOT add a lock** — both methods are stateless pure conversions
5. **Do NOT create new files** — add to existing `UniqueIdHelper.cs` and `UniqueHelperTest.cs`
6. **Do NOT use ByteAether implicit conversion operators** — use explicit `BaUlid.Parse(ulid).ToGuid()` and `BaUlid.New(guid).ToString()`, not `Guid guid = ulid;` or `Ulid u = guid;`. Implicit operators may throw different exception types than what tests expect, bypassing the `FormatException` wrapping

### Previous Story Intelligence

**Story 3.1 (Extract Timestamp)** established:
- Validation + try/catch pattern for ULID string input methods
- `FormatException` wrapping with descriptive message
- ByteAether `BaUlid.Parse()` as the single validation gate
- Theory test pattern with `[InlineData]` for error cases
- Same invalid ULID test data can be reused: `"short"`, `"THIS_IS_NOT_A_VALID_ULID!!"`, `"01ARZ3NDEKTSV4RRFFQ69G5FA!"`

**Story 2.1 (Sortable ULID Generation)** established:
- Type alias `using BaUlid = ByteAether.Ulid.Ulid;` already in place
- `CrockfordBase32Pattern()` regex already in test class
- ByteAether.Ulid v1.3.5 already in `Directory.Packages.props`
- 162 tests passing baseline

### Architecture Sequence Position

This story corresponds to **Step 11** of the 12-step test-gated implementation sequence defined in the architecture document. After this story, only Step 12 (Epic 4 documentation) remains.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Epic 3 Story 3.2]
- [Source: _bmad-output/planning-artifacts/architecture.md#ADR-004, ADR-005, ADR-006]
- [Source: _bmad-output/planning-artifacts/prd.md#FR7, FR8, FR9]
- [Source: _bmad-output/planning-artifacts/prd.md#User Journey 4 (Li, Integration Developer)]
- [Source: ByteAether.Ulid docs — ToGuid(), Ulid.New(Guid) API]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
