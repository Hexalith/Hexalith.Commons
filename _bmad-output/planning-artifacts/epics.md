---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-final-validation']
inputDocuments: ['prd.md', 'architecture.md']
---

# Hexalith.Commons - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for Hexalith.Commons, decomposing the requirements from the PRD and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Developer can generate a sortable unique string identifier that is chronologically ordered and distributed-safe
FR2: Developer can generate a non-sortable unique string identifier encoded in Base64URL format
FR3: Developer can generate a datetime-based unique string identifier (existing, unchanged)
FR4: Developer can generate sortable IDs concurrently from multiple threads without duplicates
FR5: Developer can generate sortable IDs within the same millisecond that maintain monotonic ordering
FR6: Developer can extract the creation timestamp from a ULID string as a `DateTimeOffset`
FR7: Developer can convert a ULID string to a `Guid`
FR8: Developer can convert a `Guid` to a ULID string
FR9: Developer can round-trip between ULID and Guid without data loss
FR10: Developer can discover all ID generation methods via IntelliSense with XML documentation
FR11: Developer can determine which ID method to use from a README comparison table
FR12: Developer can see code examples for each method in the README
FR13: Developer can use all three ID strategies (DateTime, Base64URL, ULID) independently within the same application
FR14: Developer can adopt ULID for new code without affecting existing code that uses other ID methods

### NonFunctional Requirements

NFR1: `GenerateSortableUniqueStringId()` completes in < 1 microsecond per call (matching ByteAether.Ulid benchmarks)
NFR2: Zero heap allocations on the generation hot path beyond the returned string
NFR3: Lock contention under concurrent load must not degrade throughput by more than 10% vs single-threaded
NFR4: `GenerateUniqueStringId()` Base64URL update must not degrade performance compared to current `Replace`-based implementation
NFR5: .NET 10+ target framework
NFR6: No platform-specific dependencies — runs on Windows, Linux, macOS
NFR7: ByteAether.Ulid version pinned in centralized `Directory.Packages.props`
NFR8: ULID output conforms to the ULID specification — interoperable with any ULID implementation in any language

### Additional Requirements

- ADR-001: Use ByteAether.Ulid (not Cysharp/Ulid as PRD specified) — overflow safety, .NET 10/C# 14 alignment
- ADR-002: Separate locks per ID strategy — rename existing `_lock` to `_dateTimeLock`, no cross-strategy contention
- ADR-003: Characterization test required for Base64URL backward compatibility (assert character set `A-Za-z0-9-_` and length 22 chars across 10,000 IDs)
- ADR-004: `ToGuid()` preserves identity not sort order — must document caveat in XML docs
- ADR-005: No ByteAether types in public API — use `string`, `Guid`, `DateTimeOffset` only
- ADR-006: Exception-based error handling — `ThrowIfNullOrWhiteSpace` + `FormatException` for invalid ULID format
- ADR-007: Default monotonic ordering — ULID spec compliant
- ADR-008: Static readonly field for `GenerationOptions` — CLR thread-safe init
- Add `ByteAether.Ulid` to `Directory.Packages.props` in `Hexalith.Builds` submodule (requires submodule approval)
- Follow 12-step test-gated implementation sequence from Architecture
- No new files — all changes in 4 existing files only
- Use type aliases (`BaUlid`, `BaGenerationOptions`) to avoid namespace collision
- Match existing test naming style (sentence-style PascalCase)

### UX Design Requirements

N/A — This is a NuGet library with no UI component.

### FR Coverage Map

| FR | Epic | Description |
|----|------|-------------|
| FR1 | Epic 2 | Sortable unique ID generation |
| FR2 | Epic 1 | Base64URL encoding verified/preserved |
| FR3 | Epic 1 | DateTime ID unchanged |
| FR4 | Epic 2 | Concurrent generation without duplicates |
| FR5 | Epic 2 | Monotonic ordering within same millisecond |
| FR6 | Epic 3 | Timestamp extraction from ULID |
| FR7 | Epic 3 | ULID to Guid conversion |
| FR8 | Epic 3 | Guid to ULID conversion |
| FR9 | Epic 3 | Lossless round-trip |
| FR10 | Epic 4 | XML documentation on all methods |
| FR11 | Epic 4 | README comparison table |
| FR12 | Epic 4 | Code examples in README |
| FR13 | Epic 1 | All three strategies coexist independently |
| FR14 | Epic 2 | Incremental ULID adoption |

## Epic List

### Epic 1: Foundation & Backward Compatibility
Developers can trust that existing ID methods (`GenerateDateTimeId()`, `GenerateUniqueStringId()`) continue to work identically after the ByteAether.Ulid dependency is added, the lock strategy is refactored, and Base64URL output is verified.
**FRs covered:** FR2, FR3, FR13

### Epic 2: Sortable Unique ID Generation
Developers can generate chronologically sortable, distributed-safe ULID identifiers — the core capability for event sourcing, aggregate snapshots, and any use case requiring natural ordering.
**FRs covered:** FR1, FR4, FR5, FR14

### Epic 3: ID Conversion & Interoperability
Developers can convert between ULID strings and Guids, extract timestamps from ULIDs, and bridge old/new ID systems with lossless round-trip conversion.
**FRs covered:** FR6, FR7, FR8, FR9

### Epic 4: API Documentation & Discoverability
Developers can discover all ID strategies via IntelliSense XML docs and a README comparison table with code examples, enabling 30-second decision-making.
**FRs covered:** FR10, FR11, FR12

## Epic 1: Foundation & Backward Compatibility

Developers can trust that existing ID methods (`GenerateDateTimeId()`, `GenerateUniqueStringId()`) continue to work identically after the ByteAether.Ulid dependency is added, the lock strategy is refactored, and Base64URL output is verified.

### Story 1.1: Add ByteAether.Ulid Package Dependency

As a Hexalith module developer,
I want the ByteAether.Ulid package added to the project infrastructure,
So that ULID generation capabilities are available for implementation.

**Acceptance Criteria:**

**Given** the `Hexalith.Builds` submodule contains `Directory.Packages.props`
**When** a developer checks the package configuration
**Then** `ByteAether.Ulid` is listed with a pinned version (1.3.2 or later, supporting monotonic generation options)
**And** `Hexalith.Commons.UniqueIds.csproj` includes a `<PackageReference Include="ByteAether.Ulid" />` (no version — centralized)
**And** the project builds successfully with no warnings or errors
**And** all existing tests pass unchanged
**And** the submodule PR includes a justification that adding a `Directory.Packages.props` entry does not add transitive references to other projects

### Story 1.2: Lock Existing Behavior and Refactor Lock Strategy

As a Hexalith module developer,
I want regression tests that lock down existing behavior and the lock strategy refactored for isolation,
So that any future changes are proven safe by test coverage and each ID strategy has independent synchronization.

**Acceptance Criteria:**

**Given** the existing `UniqueHelperTest.cs` test file
**When** the regression tests run
**Then** `GenerateDateTimeId()` produces a 17-character datetime string
**And** `GenerateUniqueStringId()` produces a 22-character string using only `A-Za-z0-9-_` characters (Base64URL character set)
**And** the characterization test validates character set and length across 10,000 generated IDs (per ADR-003)
**And** all new test methods follow the existing sentence-style PascalCase naming convention

**Given** the existing `_lock` field in `UniqueIdHelper.cs`
**When** the field is renamed to `_dateTimeLock`
**Then** only `GenerateDateTimeId()` uses `_dateTimeLock`
**And** `GenerateUniqueStringId()` remains stateless (no lock)
**And** the lock rename is a standalone commit for clean `git bisect` (per Architecture)
**And** all existing and new regression tests pass after the rename

## Epic 2: Sortable Unique ID Generation

Developers can generate chronologically sortable, distributed-safe ULID identifiers — the core capability for event sourcing, aggregate snapshots, and any use case requiring natural ordering.

### Story 2.1: Implement Sortable Unique ID Generation

As a Hexalith module developer,
I want a `GenerateSortableUniqueStringId()` method that produces ULID-based identifiers,
So that my events, aggregates, and projections sort chronologically without custom comparers.

**Acceptance Criteria:**

**Given** the `UniqueIdHelper` static class
**When** a developer calls `GenerateSortableUniqueStringId()`
**Then** it returns a 26-character Crockford Base32 string conforming to the ULID specification
**And** no ByteAether types appear in the public API — only `string` is returned (per ADR-005)
**And** a `_ulidOptions` static readonly field configures monotonic ordering (per ADR-007, ADR-008)
**And** the exact ByteAether.Ulid API names for generation options and monotonicity configuration are verified before implementation

**Given** 1,000 sequential calls to `GenerateSortableUniqueStringId()`
**When** the results are sorted via `string.Compare`
**Then** lexicographic order matches chronological generation order

**Given** 100 parallel tasks each calling `GenerateSortableUniqueStringId()`
**When** all tasks complete
**Then** all 100 IDs are unique (zero duplicates)

**Given** multiple calls within the same millisecond
**When** IDs are generated rapidly
**Then** monotonic ordering is maintained (each ID is greater than the previous)

**And** all existing tests continue to pass

**Note:** Performance benchmarking (NFR1 — sub-microsecond per call) is deferred to post-MVP per Architecture decision. Coexistence (FR13/FR14) is structurally guaranteed by isolated per-strategy state (ADR-002) and verified by the regression AC.

## Epic 3: ID Conversion & Interoperability

Developers can convert between ULID strings and Guids, extract timestamps from ULIDs, and bridge old/new ID systems with lossless round-trip conversion.

### Story 3.1: Extract Timestamp from ULID

As a Hexalith module developer,
I want to extract the creation timestamp from a ULID string,
So that I can determine when an event or entity was created directly from its ID without querying additional metadata.

**Acceptance Criteria:**

**Given** a valid ULID string generated by `GenerateSortableUniqueStringId()`
**When** a developer calls `ExtractTimestamp(string ulid)`
**Then** it returns a `DateTimeOffset` representing the ULID's creation time
**And** the returned `DateTimeOffset` represents UTC time (ULID timestamps are Unix epoch milliseconds)
**And** the returned timestamp is within 1 millisecond of the actual generation time

**Given** a null, empty, or whitespace string
**When** a developer calls `ExtractTimestamp()`
**Then** an `ArgumentException` is thrown (per ADR-006)

**Given** an invalid ULID format string (e.g., "short", "THIS_IS_NOT_A_VALID_ULID!!")
**When** a developer calls `ExtractTimestamp()`
**Then** a `FormatException` is thrown with a descriptive message (per ADR-006)

**And** all existing tests continue to pass

### Story 3.2: Bidirectional ULID-Guid Conversion

As a Hexalith module developer,
I want to convert between ULID strings and Guids,
So that I can interoperate with external systems that require Guid identifiers while maintaining ULID-based ordering internally.

**Acceptance Criteria:**

**Given** a valid ULID string
**When** a developer calls `ToGuid(string ulid)`
**Then** it returns a `Guid` that preserves the ULID's identity (per ADR-004)
**And** no ByteAether types appear in the method signature (per ADR-005)

**Given** a `Guid` value
**When** a developer calls `ToSortableUniqueId(Guid guid)`
**Then** it returns a 26-character ULID string

**Given** a ULID string converted to Guid via `ToGuid()` then back via `ToSortableUniqueId()`
**When** the round-trip completes
**Then** the original ULID string is returned identically (lossless round-trip per FR9)

**Given** a null, empty, or whitespace string passed to `ToGuid()`
**When** the method is called
**Then** an `ArgumentException` is thrown (per ADR-006)

**Given** an invalid ULID format string passed to `ToGuid()`
**When** the method is called
**Then** a `FormatException` is thrown (per ADR-006)

**Given** a non-ULID Guid (e.g., `Guid.NewGuid()`) passed to `ToSortableUniqueId()`
**When** the method is called
**Then** it returns a valid ULID string (does not throw)
**And** `ExtractTimestamp()` on the result returns a `DateTimeOffset` (does not throw — per Architecture edge case test)

**And** all existing tests continue to pass

## Epic 4: API Documentation & Discoverability

Developers can discover all ID strategies via IntelliSense XML docs and a README comparison table with code examples, enabling 30-second decision-making.

### Story 4.1: Complete XML Documentation on All Public Methods

As a Hexalith module developer,
I want complete XML documentation on all `UniqueIdHelper` public methods,
So that I can discover and understand each ID strategy directly via IntelliSense without leaving my editor.

**Acceptance Criteria:**

**Given** all public methods on `UniqueIdHelper`
**When** a developer hovers over any method in their IDE
**Then** they see `<summary>`, `<param>`, and `<returns>` XML documentation
**And** `ToGuid()` includes a `<remarks>` warning that conversion preserves identity but not sort order (per ADR-004)
**And** `ToSortableUniqueId(Guid)` includes a `<remarks>` warning that non-ULID Guids produce a valid string but with meaningless timestamp (per ADR-004)
**And** existing methods (`GenerateDateTimeId`, `GenerateUniqueStringId`) are reviewed and updated if their XML documentation is incomplete or inconsistent with new methods
**And** the project builds with no documentation warnings

### Story 4.2: Create README with Comparison Table and Code Examples

As a NuGet consumer evaluating ID generation libraries,
I want a README with a comparison table and code examples for all methods,
So that I can choose the right ID strategy in under 30 seconds.

**Acceptance Criteria:**

**Given** the `Hexalith.Commons.UniqueIds` package README
**When** a developer reads the documentation
**Then** it contains a comparison table showing all three ID strategies with columns: Method, Format, Length, Sortable, Distributed-Safe
**And** it contains code examples for every public method (`GenerateDateTimeId`, `GenerateUniqueStringId`, `GenerateSortableUniqueStringId`, `ExtractTimestamp`, `ToGuid`, `ToSortableUniqueId`)
**And** it contains NuGet installation instructions
**And** the comparison table makes the right choice obvious for common use cases (event sourcing → ULID, legacy compatibility → Base64URL, single-machine → DateTime)
**And** the NuGet package description includes searchable terms (ULID, sortable IDs, Base64URL)
**And** the README is referenced in `Hexalith.Commons.UniqueIds.csproj` via `<PackageReadmeFile>` for NuGet package display
