---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
lastStep: 8
status: 'complete'
completedAt: '2026-03-14'
inputDocuments: ['prd.md', 'project-context.md']
workflowType: 'architecture'
project_name: 'Hexalith.Commons'
user_name: 'JeromePiquot'
date: '2026-03-14'
decisions:
  - 'ADR-001: Use ByteAether.Ulid (not Cysharp/Ulid) — overflow safety, .NET 10/C# 14 alignment'
  - 'ADR-002: Separate locks per ID strategy — no cross-strategy contention'
  - 'ADR-003: Characterization test required for Base64URL backward compatibility'
  - 'ADR-004: ToGuid() preserves identity not sort order — document in XML docs'
  - 'ADR-005: No ByteAether types in public API — use string, Guid, DateTimeOffset only'
  - 'ADR-006: Exception-based error handling — ThrowIfNullOrWhiteSpace + FormatException'
  - 'ADR-007: Default monotonic ordering — ULID spec compliant'
  - 'ADR-008: Static readonly field for GenerationOptions — CLR thread-safe init'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
14 requirements across 4 categories. The core is straightforward — add ULID generation via ByteAether.Ulid and expose conversion utilities. The architectural weight is light: no new classes, no new abstractions, no new patterns. Everything extends the existing `UniqueIdHelper` static class.

- **ID Generation (FR1-FR5):** Three coexisting strategies — DateTime (unchanged), Base64URL GUID (updated encoding), ULID (new). Monotonic ordering within the same millisecond is the most architecturally significant requirement.
- **ID Conversion (FR6-FR9):** Thin wrappers over `Ulid` struct methods. Lossless round-trip (Ulid ↔ Guid) must be verified.
- **API Discoverability (FR10-FR12):** XML docs and README. No architectural impact, but shapes the public API design.
- **Coexistence (FR13-FR14):** All three ID strategies must be independently usable. No shared state between strategies except the class itself.

**Non-Functional Requirements:**

- **Performance:** Sub-microsecond per call, zero heap allocations on hot path, < 10% lock contention degradation.
- **Compatibility:** .NET 10+, cross-platform, ULID spec conformance (Crockford Base32). Base64URL output character set must match current output (`A-Za-z0-9-_`).

**Scale & Complexity:**

- Primary domain: Library/API (NuGet package)
- Complexity level: Low
- Estimated architectural components: 1 (UniqueIdHelper class extension)

### Technical Constraints & Dependencies

- `ByteAether.Ulid` version must be pinned in centralized `Directory.Packages.props` within the `Hexalith.Builds` submodule — requires approval since changes propagate to all Hexalith repos
- All code must pass 5 static analyzers (SonarAnalyzer, StyleCop, Roslynator, Roslynator.Formatting, Threading Analyzers)
- `sealed` classes, primary constructors, nullable reference types, XML documentation, MIT copyright headers — all enforced
- No nested namespaces — `Hexalith.Commons.UniqueIds` is the flat namespace
- Test file naming: `UniqueIdHelperTest.cs` (singular, no "s")

### Cross-Cutting Concerns Identified

- **Thread safety:** Monotonic ULID generation requires synchronization. Must not affect existing `GenerateDateTimeId()` or `GenerateUniqueStringId()` methods.
- **Backward compatibility:** Current Base64URL implementation may already be correct — characterization test will verify and lock behavior regardless.
- **Submodule dependency management:** Adding `ByteAether.Ulid` to `Directory.Packages.props` is a cross-repo change. Note: adding an entry only centralizes version management — it does not add a transitive reference to other projects.

## Starter Template Evaluation

### Primary Technology Domain

**.NET NuGet library** — brownfield enhancement to existing `Hexalith.Commons.UniqueIds` package. No starter template needed; the project structure, build configuration, and conventions are fully established.

### Existing Foundation (Serves as "Starter")

**Language & Runtime:**
- .NET 10.0 / C# latest (`LangVersion=latest`)
- Nullable reference types enabled, implicit usings enabled
- Documentation file generation enforced

**Build Tooling:**
- Centralized `Directory.Packages.props` in `Hexalith.Builds` submodule
- 5 analyzers enforced on every build
- Semantic-release CI/CD via GitHub Actions

**Testing Framework:**
- XUnit 2.9.3 + Shouldly 4.3.0, coverlet 8.0.0 for coverage
- Test project: `test/Hexalith.Commons.Tests/`

**Code Organization:**
- Single source file: `UniqueIdHelper.cs` in `src/libraries/Hexalith.Commons.UniqueIds/`
- Flat namespace: `Hexalith.Commons.UniqueIds`
- Static helper class pattern (no DI, no interfaces)

### Starter Options Considered

| Option | Verdict |
|--------|---------|
| Create new project/package | **Rejected** — PRD explicitly specifies extending existing `UniqueIdHelper` class |
| Separate `Hexalith.Commons.Ulids` package | **Rejected** — scored 93/140 vs 122/140; splits cohesive API, contradicts PRD |
| New `SortableUniqueId` value object | **Rejected** — scored 88/140; Phase 2 scope per PRD |
| Extend existing `UniqueIdHelper` | **Selected** — scored 122/140; highest PRD alignment, API cohesion, and simplicity |

### Selected Approach: Extend Existing Project

**Rationale:** Comparative analysis across 8 weighted criteria confirms extending `UniqueIdHelper` is the strongest option. The only trade-offs (dependency isolation, future extensibility) are manageable given ByteAether.Ulid's zero transitive dependencies and the explicit Phase 2 value object plan in the PRD.

### Key Architectural Decisions from Elicitation

**ADR-001: Dependency Selection — ByteAether.Ulid (not Cysharp/Ulid)**
- PRD specified `Cysharp/Ulid`, but First Principles analysis revealed ByteAether.Ulid is the better fit
- Overflow prevention (auto-increments timestamp) aligns with "zero duplicates" requirement
- Granular `MonotonicityOptions` for anti-enumeration security
- Native .NET 10 / C# 14 `field` keyword support
- SIMD-optimized Base32 operations
- Pin exact version in `Directory.Packages.props`

**ADR-002: Lock Strategy — Separate Concerns**
- Rename existing `_lock` → `_dateTimeLock` (guards DateTime monotonicity only)
- ULID synchronization: delegate to ByteAether's `MonotonicityOptions` if thread-safe, otherwise dedicated `_ulidLock`
- `GenerateUniqueStringId()` needs no lock (stateless `Guid.NewGuid()`)
- Verify ByteAether's thread safety model during implementation

**ADR-003: Backward Compatibility — Characterization Testing**
- Characterization test locks current `GenerateUniqueStringId()` behavior regardless of whether implementation changes
- Assert character set (`A-Za-z0-9-_`) and length (22 chars) across 10,000 generated IDs
- Current manual `Replace` approach may already be correct Base64URL — test verifies and documents this

**ADR-004: Guid Conversion Caveat**
- `ToGuid()` preserves **identity** but **not sort order** (Guid mixed-endian byte layout)
- `ToSortableUniqueId(Guid)` with a non-ULID Guid produces a valid ULID string but with meaningless timestamp
- Must document both caveats clearly in XML docs

### Pre-mortem Risk Mitigations

| Risk | Mitigation |
|------|------------|
| Base64URL breaks consumers | Characterization test suite locks current behavior |
| Lock contention at scale | Separate locks per strategy |
| ByteAether API instability | Pin version, wrap behind public API |
| Guid sort order confusion | XML doc warning on `ToGuid()` |
| Non-ULID Guid conversion confusion | XML doc warning on `ToSortableUniqueId(Guid)` |
| Submodule PR rejection | Clarify: `Directory.Packages.props` entry ≠ transitive reference |

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- ADR-001: ByteAether.Ulid as dependency
- ADR-002: Separate locks per strategy
- ADR-005: No ByteAether types in public API — use `string`, `Guid`, `DateTimeOffset` only
- ADR-006: Exception-based error handling — `ArgumentException.ThrowIfNullOrWhiteSpace()` + `FormatException` for invalid ULID format

**Important Decisions (Shape Architecture):**
- ADR-003: Characterization testing for Base64URL behavior
- ADR-004: Guid conversion caveats documented in XML docs
- ADR-007: Default monotonic ordering (ULID spec compliant)
- ADR-008: Static readonly field for `GenerationOptions` — CLR-guaranteed thread-safe init

**Deferred Decisions (Post-MVP / Phase 2):**
- `SortableUniqueId` value object with operators, parsing, implicit conversions
- `TryExtractTimestamp` / Try-pattern overloads
- Anti-enumeration monotonicity options (expose as configurable)

### API Design

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Public API types | `string`, `Guid`, `DateTimeOffset` — no ByteAether types | Zero coupling to dependency. Consumers never reference ByteAether. Wrapping absorbs future API changes. |
| ByteAether dependency | Internal implementation detail | Hidden behind public API surface. |
| Method signatures | Match PRD | `GenerateSortableUniqueStringId()`, `ExtractTimestamp(string)`, `ToGuid(string)`, `ToSortableUniqueId(Guid)` |

### Error Handling

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Null/empty input | `ArgumentException.ThrowIfNullOrWhiteSpace()` | Project-context rule. Fail-fast at boundary. |
| Invalid ULID format | `FormatException` | Consistent with `Guid.Parse`, `int.Parse` conventions. Programmer error, not expected failure. |
| Result pattern | Not used for MVP | Invalid ULID input is a programmer error. `ValueOrError<T>` reserved for business logic. |

### Internal Configuration

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Monotonicity strategy | Default monotonic | ULID spec compliant. Enumeration not a concern for infrastructure IDs. |
| GenerationOptions init | Static readonly field | CLR-guaranteed thread-safe initialization. Matches existing `Lock` field pattern. |
| Generator lifetime | Singleton (static class) | One `GenerationOptions` instance for application lifetime. Must verify ByteAether maintains per-instance monotonic state. |

### Base64URL Implementation Decision

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Current `Replace` approach | Likely correct — verify, don't change | Current manual `Replace("/", "_").Replace("+", "-")` IS proper Base64URL encoding. Changing to a framework method (e.g., `WebEncoders.Base64UrlEncode`) adds an ASP.NET Core dependency for no benefit. Characterization test locks behavior regardless. |

### Decision Impact Analysis

**Implementation Sequence (test-gated):**
1. Add `ByteAether.Ulid` to `Directory.Packages.props` (requires submodule approval)
2. Add package reference to `Hexalith.Commons.UniqueIds.csproj`
3. Add regression test for existing `GenerateDateTimeId()` behavior
4. Rename `_lock` → `_dateTimeLock` (separate commit — clean refactoring)
5. Verify regression test still passes after rename
6. Add `GenerationOptions` static readonly field
7. Add characterization test for `GenerateUniqueStringId()` Base64URL output
8. Verify characterization test passes with current implementation — if it does, no code change needed
9. Implement `GenerateSortableUniqueStringId()` with tests (uniqueness, sortability, concurrency, monotonic ordering)
10. Implement `ExtractTimestamp(string)` with tests (valid ULID, invalid input, null/empty)
11. Implement `ToGuid(string)` and `ToSortableUniqueId(Guid)` with round-trip test
12. Add edge case test: `ToSortableUniqueId(Guid.NewGuid())` → `ExtractTimestamp()` returns a `DateTimeOffset` (doesn't throw)

**Cross-Component Dependencies:**
- Steps 1-2 block all other steps (dependency must be available)
- Step 4 (lock rename) must be a separate commit from feature work for clean `git bisect`
- Steps 3 and 5 gate the lock rename safety
- Step 7 gates any potential Base64URL implementation change
- Steps 9-12 depend on step 6 (GenerationOptions field)

## Implementation Patterns & Consistency Rules

### Enforcement Rules (Tiered)

**CRITICAL (broken or incorrect code if violated):**

1. **Never expose ByteAether types in public API** — method signatures, return types, and XML documentation must use only `string`, `Guid`, `DateTimeOffset`. ByteAether is an internal implementation detail.
2. **Never create `_previousUlid` or any field to track ULID monotonic state** — ByteAether owns monotonicity internally via `GenerationOptions`. Duplicating this logic introduces subtle ordering bugs.
3. **Delegate format validation to ByteAether** — do not duplicate ULID parsing, length checks, or Crockford Base32 character validation. Catch ByteAether's exceptions and wrap in `FormatException`.
4. **Initialize `_ulidOptions` with explicit monotonicity mode** — if the default is non-monotonic, FR5 (within-millisecond ordering) fails silently. Verify the exact ByteAether API during implementation and specify monotonicity explicitly:
```csharp
private static readonly BaGenerationOptions _ulidOptions = new()
{
    Monotonicity = MonotonicityMode.Monotonic  // verify exact API name
};
```

**SHOULD (inconsistent but functional if violated):**

5. **Match existing test naming style** — sentence-style PascalCase: `GetAThousandSortableUniqueIdStringWithoutAnyDuplicates`. Check `UniqueHelperTest.cs` for examples before writing new tests.
6. **Use `[Theory]` + `[InlineData]` for error/boundary tests** — `[Fact]` for single-case behavior (generation, round-trip, length). `[Theory]` for multiple error inputs (null, empty, whitespace, invalid format).
7. **Self-contained tests** — no constructor, no `IDisposable`, no shared state. Each test creates its own data. Matches existing `UniqueHelperTest` class structure.

### Naming Patterns

**Private Field Names:**

| Field | Name | Used By |
|-------|------|---------|
| DateTime lock | `_dateTimeLock` | `GenerateDateTimeId()` only |
| DateTime previous | `_previous` (existing) | `GenerateDateTimeId()` only |
| ULID options | `_ulidOptions` | `GenerateSortableUniqueStringId()` only |

**ByteAether Type Alias (convention, not enforced by analyzer):**

```csharp
using BaUlid = ByteAether.Ulid.Ulid;
using BaGenerationOptions = ByteAether.Ulid.GenerationOptions;
```

Do not use `using ByteAether.Ulid;` as a namespace import — the type `Ulid` collides with the namespace. Fully qualified `ByteAether.Ulid.Ulid` is acceptable but noisy.

### Structure Patterns

**Source:** All new code in existing `UniqueIdHelper.cs`. No new source files.

**Tests:** All new tests in existing `test/Hexalith.Commons.Tests/UniqueIds/UniqueHelperTest.cs`. Same namespace: `Hexalith.Commons.Tests.UniqueIds`. Do not create new namespaces. Update the class-level `<summary>` to include ULID test coverage.

**Method Ordering:** StyleCop SA1201 enforces alphabetical ordering within visibility groups. Do not attempt to group by feature.

```csharp
// Alphabetical ordering (enforced by analyzer):
private static readonly Lock _dateTimeLock = new();
private static readonly DateTime _previous = DateTime.MinValue;
private static readonly BaGenerationOptions _ulidOptions = new() { ... };

public static DateTimeOffset ExtractTimestamp(string ulid) { ... }
public static string GenerateDateTimeId() { ... }
public static string GenerateSortableUniqueStringId() { ... }
public static string GenerateUniqueStringId() { ... }
public static Guid ToGuid(string ulid) { ... }
public static string ToSortableUniqueId(Guid guid) { ... }
```

**Field Ownership — No Cross-Strategy State:**
- `_dateTimeLock` and `_previous` → ONLY `GenerateDateTimeId()`
- `_ulidOptions` → ONLY `GenerateSortableUniqueStringId()`
- `GenerateUniqueStringId()` → NO shared state

### Process Patterns

**Validation Pattern for String Input Methods:**

```csharp
public static DateTimeOffset ExtractTimestamp(string ulid)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(ulid);
    try
    {
        return BaUlid.Parse(ulid).Time;
    }
    catch (Exception ex) when (ex is not ArgumentException)
    {
        throw new FormatException($"The value '{ulid}' is not a valid ULID string", ex);
    }
}
```

Notes:
- String interpolation in the `FormatException` message is correct — this is the throw path, not the hot path
- ULID operations are culture-invariant (Crockford Base32 is ASCII-only). Do not add `CultureInfo` parameters.

**Generation Method Exceptions:**
`GenerateSortableUniqueStringId()` has no input. Any ByteAether exception (system clock issue, library bug) is truly exceptional — let it propagate raw. Do not wrap. Document in `<remarks>` XML tag if applicable.

**Round-Trip Test Pattern:**

```csharp
[Fact]
public void ConvertSortableUniqueIdToGuidAndBackShouldReturnOriginalValue()
{
    string original = UniqueIdHelper.GenerateSortableUniqueStringId();
    Guid guid = UniqueIdHelper.ToGuid(original);
    string roundTripped = UniqueIdHelper.ToSortableUniqueId(guid);
    roundTripped.ShouldBe(original, "round-trip conversion should preserve the original ULID string");
}
```

Note: Guid → Ulid → Guid round-trip direction needs verification during implementation — may or may not preserve the original Guid due to endianness.

**Theory Test Pattern:**

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void ExtractTimestampFromNullOrWhiteSpaceThrowsArgumentException(string? ulid)
{
    Should.Throw<ArgumentException>(() => UniqueIdHelper.ExtractTimestamp(ulid!));
}

[Theory]
[InlineData("short")]
[InlineData("THIS_IS_NOT_A_VALID_ULID!!")]
[InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA!")]
public void ExtractTimestampFromInvalidFormatThrowsFormatException(string ulid)
{
    Should.Throw<FormatException>(() => UniqueIdHelper.ExtractTimestamp(ulid));
}
```

### Anti-Patterns (Top 3 Most Likely Agent Mistakes)

```csharp
// 1. WRONG: ByteAether type leaking into public API
public static ByteAether.Ulid.Ulid GenerateUlid()  // NO — return string

// 2. WRONG: Duplicating ByteAether's monotonicity logic
private static BaUlid _previousUlid;  // NO — ByteAether owns this
public static string GenerateSortableUniqueStringId()
{
    using (_ulidLock.EnterScope())
    {
        var ulid = BaUlid.New(_ulidOptions);
        while (ulid <= _previousUlid) { ... }  // NO!
    }
}

// 3. WRONG: Duplicating validation that ByteAether already does
if (ulid.Length != 26) throw new FormatException(...);  // NO — let ByteAether validate
if (!IsValidCrockford(ulid)) throw new FormatException(...);  // NO — don't duplicate
```

## Project Structure & Boundaries

### Complete Project Directory Structure

```
Hexalith.Commons/
├── Hexalith.Builds/                          # Submodule (shared across repos)
│   └── Directory.Packages.props              # ✏️ MODIFY: add ByteAether.Ulid version
├── src/
│   └── libraries/
│       └── Hexalith.Commons.UniqueIds/
│           ├── Hexalith.Commons.UniqueIds.csproj  # ✏️ MODIFY: add PackageReference
│           └── UniqueIdHelper.cs                  # ✏️ MODIFY: add 4 methods, rename lock
├── test/
│   └── Hexalith.Commons.Tests/
│       ├── Hexalith.Commons.Tests.csproj          # (unchanged)
│       └── UniqueIds/
│           └── UniqueHelperTest.cs                # ✏️ MODIFY: add ~10 test methods
└── (all other files unchanged)
```

**Legend:** ✏️ = modified file. No new files created.

### Files Modified — Detailed

| File | Change | FR Coverage |
|------|--------|-------------|
| `Hexalith.Builds/Directory.Packages.props` | Add `<PackageVersion Include="ByteAether.Ulid" Version="x.x.x" />` | Prerequisite for all FRs |
| `src/.../Hexalith.Commons.UniqueIds.csproj` | Add `<PackageReference Include="ByteAether.Ulid" />` (no version — centralized) | Prerequisite for all FRs |
| `src/.../UniqueIdHelper.cs` | Rename `_lock` → `_dateTimeLock`, add `_ulidOptions` field, add 4 public methods | FR1-FR9 |
| `test/.../UniqueHelperTest.cs` | Add ~10 test methods covering all new functionality | FR4, FR5, FR6-FR9 verification |

### Architectural Boundaries

**Public API Boundary:**
```
Consumer code
    ↓ calls
UniqueIdHelper (public static methods — string, Guid, DateTimeOffset)
    ↓ delegates internally
ByteAether.Ulid (private, never exposed)
```

The boundary is the `UniqueIdHelper` class surface. Everything behind it is an implementation detail. Consumers reference `Hexalith.Commons.UniqueIds` only — never `ByteAether.Ulid`.

**Package Dependency Boundary:**
```
Hexalith.Commons.UniqueIds.csproj
    → ByteAether.Ulid (runtime dependency, version in Directory.Packages.props)

Hexalith.Commons.Tests.csproj
    → Hexalith.Commons.UniqueIds (project reference, existing)
    → (no direct ByteAether reference needed in tests)
```

**Thread Safety Boundary:**
```
GenerateDateTimeId()              → _dateTimeLock + _previous      (self-contained)
GenerateSortableUniqueStringId()  → _ulidOptions (ByteAether sync) (self-contained)
GenerateUniqueStringId()          → stateless                      (no synchronization)
ExtractTimestamp()                → stateless parse                (no synchronization)
ToGuid()                          → stateless conversion            (no synchronization)
ToSortableUniqueId()              → stateless conversion            (no synchronization)
```

Each ID strategy is an isolated island — no shared mutable state between strategies.

### Requirements to Structure Mapping

| FR | Method | File |
|----|--------|------|
| FR1 (Sortable unique ID) | `GenerateSortableUniqueStringId()` | `UniqueIdHelper.cs` |
| FR2 (Base64URL unique ID) | `GenerateUniqueStringId()` | `UniqueIdHelper.cs` (existing, verify) |
| FR3 (DateTime ID) | `GenerateDateTimeId()` | `UniqueIdHelper.cs` (existing, unchanged) |
| FR4 (Concurrent generation) | Tested via async concurrent test | `UniqueHelperTest.cs` |
| FR5 (Monotonic ordering) | Configured via `_ulidOptions` field | `UniqueIdHelper.cs` |
| FR6 (Timestamp extraction) | `ExtractTimestamp()` | `UniqueIdHelper.cs` |
| FR7 (ULID to Guid) | `ToGuid()` | `UniqueIdHelper.cs` |
| FR8 (Guid to ULID) | `ToSortableUniqueId()` | `UniqueIdHelper.cs` |
| FR9 (Round-trip) | Tested via round-trip conversion test | `UniqueHelperTest.cs` |
| FR10-FR12 (Discoverability) | XML docs | `UniqueIdHelper.cs` + README |
| FR13-FR14 (Coexistence) | Independent state per strategy | `UniqueIdHelper.cs` |

### Data Flow

```
ULID Generation:
Ulid.New(_ulidOptions) → ByteAether.Ulid.Ulid struct → .ToString() → 26-char string → consumer

Timestamp Extraction:
string → BaUlid.Parse(ulid) → ByteAether.Ulid.Ulid struct → .Time → DateTimeOffset → consumer

ULID to Guid:
string → BaUlid.Parse(ulid) → ByteAether.Ulid.Ulid struct → .ToGuid() → System.Guid → consumer

Guid to ULID:
System.Guid → new BaUlid(guid.ToByteArray()) → ByteAether.Ulid.Ulid struct → .ToString() → string → consumer
```

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:** All 8 ADRs are mutually compatible. One verification item: confirm ByteAether's default monotonicity behavior during implementation.

**Pattern Consistency:** All patterns align with decisions, project-context rules, and existing codebase conventions. No contradictions found.

**Structure Alignment:** 4-file modification footprint aligns with all decisions. No structural conflicts.

### Requirements Coverage ✅

| Category | Coverage |
|----------|----------|
| Functional Requirements (FR1-FR14) | 14/14 covered (FR11-FR12 are documentation tasks) |
| Non-Functional Requirements | All covered — performance, compatibility, ULID spec conformance |
| User Journeys (Marco, Priya, Sam, Li) | All supported by the API surface and conversion utilities |

### Implementation Readiness ✅

| Dimension | Status |
|-----------|--------|
| Decision documentation | 8 ADRs with rationale and priority tiers |
| Code patterns | Exact code examples for validation, generation, testing |
| Structure mapping | All FRs mapped to specific files |
| Enforcement rules | 4 CRITICAL + 3 SHOULD — tiered for agent guidance |
| Anti-patterns | Top 3 most likely agent mistakes documented |

### Gap Analysis

**Items to Verify During Implementation:**
1. ByteAether 1.3.2 exact API names (`MonotonicityMode`, `GenerationOptions`, `Parse`, `.Time`, `.ToGuid()`)
2. ByteAether thread safety model — determines whether `_ulidLock` is needed
3. Guid → Ulid → Guid round-trip preservation

**Deferred (Post-MVP):**
- README comparison table and code examples (FR11-FR12)
- Performance benchmark test
- `SortableUniqueId` value object (Phase 2)

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] Project context thoroughly analyzed (56 rules loaded)
- [x] Scale and complexity assessed (Low)
- [x] Technical constraints identified (5 analyzers, centralized packages, submodule)
- [x] Cross-cutting concerns mapped (thread safety, backward compat, dependency management)

**✅ Architectural Decisions**
- [x] 8 ADRs documented with rationale and priority tiers
- [x] Technology stack fully specified (ByteAether.Ulid, .NET 10, C# latest)
- [x] Dependency strategy defined (pin version, wrap behind API)
- [x] Performance considerations addressed (separate locks, ByteAether benchmarks)

**✅ Implementation Patterns**
- [x] Naming conventions established (fields, parameters, test methods)
- [x] Validation patterns defined with code examples
- [x] Test patterns specified (Fact, Theory, async, round-trip)
- [x] 4 CRITICAL + 3 SHOULD enforcement rules tiered

**✅ Project Structure**
- [x] Complete file modification list (4 files, 0 new)
- [x] Architectural boundaries defined (API, package, thread safety)
- [x] All 14 FRs mapped to specific files and methods
- [x] Data flow documented for all operations

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** High — low-complexity enhancement to a well-understood codebase with comprehensive patterns and a 12-step test-gated implementation sequence.

**Key Strengths:**
- Single class, 4 files — minimal blast radius
- ByteAether.Ulid handles the hard parts (monotonicity, Crockford Base32, overflow prevention)
- Tiered enforcement rules prevent the most costly agent mistakes
- Test-gated implementation sequence catches regressions at each step

**Areas for Future Enhancement (Phase 2+):**
- `SortableUniqueId` value object with operators, parsing, implicit conversions
- `TryExtractTimestamp` / Try-pattern overloads
- Anti-enumeration monotonicity options
- ULID-based correlation ID middleware

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all CRITICAL enforcement rules exactly — violations produce broken code
- Verify ByteAether API names against actual 1.3.2 package before implementing
- Follow the 12-step implementation sequence — each step is gated by tests
- Refer to this document for all architectural questions

**First Implementation Priority:**
Add `ByteAether.Ulid` to `Directory.Packages.props` in `Hexalith.Builds` submodule (requires user approval for submodule modification).
