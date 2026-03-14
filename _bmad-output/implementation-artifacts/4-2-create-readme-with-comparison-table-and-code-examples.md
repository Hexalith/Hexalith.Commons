# Story 4.2: Create README with Comparison Table and Code Examples

Status: done

## Story

As a NuGet consumer evaluating ID generation libraries,
I want a README with a comparison table and code examples for all methods,
so that I can choose the right ID strategy in under 30 seconds.

## Acceptance Criteria

1. Given the `Hexalith.Commons.UniqueIds` package README
   - When a developer reads the documentation
   - Then it contains a comparison table showing all three ID strategies with columns: Method, Format, Length, Sortable, Distributed-Safe

2. Given the README
   - When a developer looks for usage examples
   - Then it contains code examples for every public method: `GenerateDateTimeId`, `GenerateUniqueStringId`, `GenerateSortableUniqueStringId`, `ExtractTimestamp`, `ToGuid`, `ToSortableUniqueId`

3. Given the README
   - When a developer wants to install the package
   - Then it contains NuGet installation instructions (`dotnet add package Hexalith.Commons.UniqueIds`)

4. Given the comparison table
   - When a developer needs to pick an ID strategy
   - Then the right choice is obvious for common use cases: event sourcing → ULID, legacy compatibility → Base64URL, single-machine → DateTime

5. Given the NuGet package listing
   - When a developer searches NuGet
   - Then the package description includes searchable terms: ULID, sortable IDs, Base64URL

6. Given the NuGet package
   - When a developer views it on nuget.org
   - Then the library-specific README (not the repo root README) is displayed via `<PackageReadmeFile>`

7. All existing tests continue to pass (182 tests as of Story 4.1)

## Tasks / Subtasks

- [x] Task 1: Update the existing README.md (AC: #1, #2, #3, #4)
  - [x] 1.1 Read current README at `src/libraries/Hexalith.Commons.UniqueIds/README.md`
  - [x] 1.2 Update the overview section to describe all THREE ID strategies (not just the original two)
  - [x] 1.3 Update the comparison table to include all three strategies with columns: Method, Format, Length, Sortable, Distributed-Safe, Use Case
  - [x] 1.4 Add a "ULID-Based IDs" section with `GenerateSortableUniqueStringId()` documentation and examples
  - [x] 1.5 Add a "Conversion Utilities" section covering `ExtractTimestamp`, `ToGuid`, `ToSortableUniqueId` with examples
  - [x] 1.6 Update "Decision Guide" to include the ULID strategy with clear recommendation: event sourcing/DDD → ULID
  - [x] 1.7 Update existing sections (Quick Start, Combined Usage, ID Generation Service) to include ULID examples
  - [x] 1.8 Ensure code examples are copy-paste ready and use correct API signatures

- [x] Task 2: Update .csproj for NuGet packaging (AC: #5, #6)
  - [x] 2.1 Add `<Description>` property with searchable terms (ULID, sortable, Base64URL, unique ID generation)
  - [x] 2.2 Add `<None Include="README.md" Pack="true" PackagePath="\" />` to include library-specific README
  - [x] 2.3 Override `<PackageReadmeFile>README.md</PackageReadmeFile>` if needed (may be inherited from Hexalith.Package.props)
  - [x] 2.4 Verify the local README (not repo-root) is packed: run `dotnet pack`, open .nupkg as zip, confirm README.md content matches the library-specific file

- [x] Task 3: Build and test (AC: #7)
  - [x] 3.1 Run `dotnet build` — zero warnings
  - [x] 3.2 Run `dotnet test` — all 183 tests pass unchanged
  - [x] 3.3 Verify NuGet package contains the updated README: run `dotnet pack` and inspect the .nupkg

- [ ] Task 4: Commit
  - [ ] 4.1 Single commit: `docs(unique-ids): add ULID methods to README with comparison table`
  - [ ] 4.2 CRITICAL: Tasks 1-3 must pass before commit

### Review Follow-ups (AI)

- [x] [AI-Review][Critical] Fixed packaging validation for the library project by overriding `IsPackable` and disabling `GeneratePackageOnBuild` during IDE builds so local `dotnet pack` now generates a `.nupkg` in VS Code sessions.
- [x] [AI-Review][High] Reshaped the README comparison table to the required column-oriented layout: `Method`, `Format`, `Length`, `Sortable`, `Distributed-Safe` (plus `Thread-Safe` and `Best For`).

## Dev Notes

### Nature of This Story

**This is a DOCUMENTATION + PACKAGING story.** Two files are modified:

1. `src/libraries/Hexalith.Commons.UniqueIds/README.md` — content update
2. `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` — NuGet metadata

No new methods, no logic changes, no test changes. Tests must pass unchanged.

### Current README State

The README at `src/libraries/Hexalith.Commons.UniqueIds/README.md` already exists with good structure but only covers the **original two methods** (`GenerateDateTimeId` and `GenerateUniqueStringId`). It was written before Epics 2 and 3 added ULID capabilities. Key gaps:

| Section              | Current State                 | Required Update                                                    |
| -------------------- | ----------------------------- | ------------------------------------------------------------------ |
| Overview table       | 2 methods only                | Add `GenerateSortableUniqueStringId` row                           |
| Quick Start          | DateTime + Base64URL examples | Add ULID example                                                   |
| Comparison table     | 2 columns                     | Add ULID column, add "Distributed-Safe" and "Use Case" rows        |
| Decision Guide       | 2 options                     | Add ULID recommendation for event sourcing                         |
| ULID section         | **MISSING**                   | New section with generation, timestamp extraction, conversion      |
| Conversion utilities | **MISSING**                   | New section for `ExtractTimestamp`, `ToGuid`, `ToSortableUniqueId` |
| Combined Usage       | Uses only old methods         | Update to include ULID in examples                                 |
| Thread Safety        | Covers old methods only       | Add ULID concurrency example                                       |

### NuGet Packaging Configuration

**Current state:**

- `Hexalith.Builds/Hexalith.Package.props` sets `<PackageReadmeFile>README.md</PackageReadmeFile>` and packs `$(ProjectRoot)/README.md` (the REPO ROOT README, not the library-specific one)
- The library's `.csproj` has NO `<Description>` override — it inherits the generic Hexalith description from `Hexalith.Package.props`
- The library-specific `README.md` exists in the project directory but is NOT packed into the NuGet package

**Required changes to `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Description>Unique identifier generation for .NET: ULID (sortable, distributed-safe), Base64URL GUID, and DateTime-based IDs with conversion utilities.</Description>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ByteAether.Ulid" />
  </ItemGroup>
  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>
</Project>
```

**CRITICAL:** The `<None Include="README.md" Pack="true" PackagePath="\" />` overrides the repo-root README from `Hexalith.Package.props` because MSBuild uses the LAST matching `PackagePath` for `PackageReadmeFile`. The `<Description>` overrides the generic one. Do NOT modify `Hexalith.Package.props` — that's the shared submodule.

### Comparison Table Design (AC #1, #4)

The table must make the right choice **instantly obvious**:

```markdown
|                      | `GenerateDateTimeId()`  | `GenerateUniqueStringId()`  | `GenerateSortableUniqueStringId()` |
| -------------------- | ----------------------- | --------------------------- | ---------------------------------- |
| **Format**           | `yyyyMMddHHmmssfff`     | Base64URL GUID              | Crockford Base32 ULID              |
| **Length**           | 17 chars                | 22 chars                    | 26 chars                           |
| **Sortable**         | Yes (chronological)     | No                          | Yes (chronological)                |
| **Distributed-Safe** | No (single machine)     | Yes                         | Yes                                |
| **Thread-Safe**      | Yes (locked)            | Yes (stateless)             | Yes (monotonic)                    |
| **Best For**         | Log entries, file names | Legacy keys, session tokens | **Event sourcing, DDD aggregates** |
```

### Code Examples Required (AC #2)

Every public method needs a working code example:

1. **`GenerateDateTimeId()`** — already in README, keep/refresh
2. **`GenerateUniqueStringId()`** — already in README, keep/refresh
3. **`GenerateSortableUniqueStringId()`** — NEW: basic generation + sorting demo
4. **`ExtractTimestamp(string ulid)`** — NEW: extract and use creation time
5. **`ToGuid(string ulid)`** — NEW: convert for external system interop
6. **`ToSortableUniqueId(Guid guid)`** — NEW: convert back from Guid

Examples should show **real-world use cases** from the PRD user journeys:

- Marco: Event sourcing with ULID IDs
- Priya: Quick start discovering the right method
- Sam: Migration from Base64URL to ULID
- Li: Guid conversion for ERP interop

### API Signatures (Verify Before Writing Examples)

From current `UniqueIdHelper.cs` (confirmed):

```csharp
public static string GenerateDateTimeId()
public static string GenerateUniqueStringId()
public static string GenerateSortableUniqueStringId()
public static DateTimeOffset ExtractTimestamp(string ulid)
public static Guid ToGuid(string ulid)
public static string ToSortableUniqueId(Guid value)  // param name is "value", not "guid"
```

### Architecture Compliance

**ADR-004 (Identity not sort order):** The README must mention that `ToGuid()` preserves identity but NOT sort order. This caveat should appear in the conversion utilities section.

**ADR-005 (No ByteAether in public API):** The README must NOT reference ByteAether as a dependency to consumers. Internally it uses ByteAether.Ulid, but the README should refer to "ULID specification" or "Crockford Base32 encoding."

**FR11 (Comparison table):** Must show all three strategies side-by-side with enough information for 30-second decision-making.

**FR12 (Code examples):** Must cover every public method with copy-paste ready code.

### Anti-Patterns to Avoid

1. **Do NOT reference ByteAether.Ulid in the README** — it's an internal dependency. Say "ULID specification" instead
2. **Do NOT modify `UniqueIdHelper.cs`** — this story is documentation only
3. **Do NOT modify test files** — this story adds no new functionality
4. **Do NOT modify `Hexalith.Builds/Hexalith.Package.props`** — never modify the shared submodule without approval
5. **Do NOT use incorrect parameter names in examples** — the Guid conversion method uses `value`, not `guid` (CA1720 compliance from Story 3.2)
6. **Do NOT add encoding detail section for ULID** — replace the old "Encoding Details" Base64URL section with a more useful section. ULID encoding is handled by the library
7. **Do NOT make the README too long** — NuGet renders README on the package page; keep it scannable. Target ~250 lines (current is 328 lines covering only 2 methods — consolidate old per-method deep dives into the comparison table). Focus on the comparison table and quick code examples
8. **Do NOT change the NuGet badge URL** — keep the existing `[![NuGet]...]` badge

### README Structure Recommendation

Suggested section order for the updated README:

1. Title + tagline (update to: "Three ID strategies for .NET — DateTime, Base64URL, and ULID — pick the right one in 30 seconds.")
2. NuGet badge (keep existing)
3. Overview with 3-strategy comparison table
4. Installation
5. Quick Start (three separate code blocks, each with a one-line comment labeling the use case: `// Sortable + distributed (event sourcing)`, `// Distributed (legacy keys)`, `// Human-readable (single machine)` — scanning three labeled blocks is faster than parsing one big block)
6. Method Reference (one subsection per method with example)
7. Conversion Utilities (ExtractTimestamp, ToGuid, ToSortableUniqueId — lead each with a "why" sentence: e.g., "Your ERP only accepts GUIDs? Convert your internal ULID." Frame the utility before showing the code)
8. Decision Guide (when to use each method)
9. Thread Safety
10. License + Links

### Previous Story Intelligence

**Story 4.1 (XML Documentation) established:**

- All 6 public methods now have complete XML docs with `<summary>`, `<param>`, `<returns>`, `<remarks>`, and `<example>` tags
- Class-level summary describes all three strategies
- 182 tests passing baseline
- Zero build warnings

**Story 3.2 (Bidirectional ULID-Guid Conversion) established:**

- `ToGuid(string ulid)` and `ToSortableUniqueId(Guid value)` are implemented and tested
- Parameter name is `value` (not `guid`) for CA1720 compliance
- Round-trip conversion: ULID → Guid → ULID is lossless
- Non-ULID Guids (e.g., `Guid.NewGuid()`) produce valid ULID strings with meaningless timestamps

**Story 3.1 (Extract Timestamp) established:**

- `ExtractTimestamp(string ulid)` returns `DateTimeOffset` in UTC
- Uses Crockford Base32 regex validation before parsing
- Throws `ArgumentException` for null/empty, `FormatException` for invalid format

**Story 2.1 (Sortable ID Generation) established:**

- `GenerateSortableUniqueStringId()` returns 26-char Crockford Base32 string
- Monotonic ordering within same millisecond
- Thread-safe via `BaUlid.GenerationOptions` with monotonic increment

### Git Intelligence

Recent commits show the implementation sequence:

- `feat(unique-ids): add bidirectional ULID-Guid conversion` (Story 3.2)
- `feat(unique-ids): add ULID timestamp extraction` (Story 3.1)
- `feat(unique-ids): add sortable ULID-based unique ID generation` (Story 2.1)

All ULID functionality is now complete. This documentation story is the final deliverable.

### Files to Modify (2 files, 0 new)

| File                                                                         | Action                                                                               |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `src/libraries/Hexalith.Commons.UniqueIds/README.md`                         | Update content: add ULID methods, comparison table, code examples                    |
| `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` | Add `<Description>` and `<None Include="README.md" Pack="true">` for NuGet packaging |

### Build Verification

- Run `dotnet build` — expect 0 warnings, 0 errors (documentation changes only)
- Run `dotnet test` — expect 182/182 passed (no code changes)
- Run `dotnet pack src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` — verify .nupkg contains the updated README.md

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Epic 4 Story 4.2]
- [Source: _bmad-output/planning-artifacts/prd.md#FR11, FR12]
- [Source: _bmad-output/planning-artifacts/prd.md#User Journey 2 (Priya — Discovery Path)]
- [Source: _bmad-output/planning-artifacts/architecture.md#ADR-004, ADR-005]
- [Source: Hexalith.Builds/Hexalith.Package.props — PackageReadmeFile configuration]
- [Source: src/libraries/Hexalith.Commons.UniqueIds/UniqueIdHelper.cs — current API surface]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

- NU5118 warning resolved: repo-root README conflicted with library README in NuGet pack. Fixed by adding `<None Update="$(ProjectRoot)/README.md" Pack="false" />` to exclude the inherited repo-root README.
- Local `dotnet pack` in VS Code initially evaluated `IDEBuild=true`, which kept `GeneratePackageOnBuild=true` and prevented reproducible package generation. Fixed with a project-local IDE override while preserving inherited non-IDE behavior.

### Completion Notes List

- README rewritten from 328 lines (2 methods) to ~170 lines (3 methods + conversion utilities) — more concise and scannable for NuGet page rendering
- Comparison table now uses the required column layout with `Method`, `Format`, `Length`, `Sortable`, and `Distributed-Safe` for instant comparison
- Code examples provided for all 6 public methods with correct API signatures (param name `value` not `guid` for `ToSortableUniqueId`)
- Decision Guide table covers 5 common use cases with clear recommendations
- Conversion Utilities section includes ADR-004 caveat (identity preserved, sort order not)
- No ByteAether references in README (ADR-005 compliant)
- Combined Usage example updated to use ULID for event IDs
- Thread Safety section uses ULID example
- NuGet `<Description>` includes searchable terms: ULID, sortable, Base64URL, unique ID generation
- Local IDE builds now allow reproducible `dotnet pack` verification via project-local `IsPackable` and `GeneratePackageOnBuild` overrides
- Library-specific README correctly packed into `.nupkg` (verified by package inspection and SHA-256 hash match against the project README)
- Build: 0 warnings, 0 errors
- Tests: 183/183 passed (1 more than 182 baseline — no regressions)

### File List

- `src/libraries/Hexalith.Commons.UniqueIds/README.md` — rewritten with all 3 ID strategies, comparison table, code examples for all 6 public methods
- `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj` — added `<Description>`, library-specific README packing, disabled repo-root README packing

### Senior Developer Review (AI)

**Reviewer:** JeromePiquot  
**Date:** 2026-03-14  
**Outcome:** Approved

#### Resolution Summary

- The reviewed packaging issue and README table mismatch have been fixed.
- The story now satisfies AC #1 through AC #7, including reproducible local package verification.
- Story and sprint tracking have been reconciled to the verified completed state.

#### What I Validated

- Git/story alignment for application files is correct: the story file list matches the two changed source files (`src/libraries/Hexalith.Commons.UniqueIds/README.md` and `src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj`).
- `dotnet build d:\Hexalith.Commons\Hexalith.Commons.sln -nologo` succeeded on 2026-03-14 with zero errors.
- `dotnet test d:\Hexalith.Commons\Hexalith.Commons.sln -nologo --no-build` succeeded on 2026-03-14 with 183/183 tests passing.
- `dotnet msbuild d:\Hexalith.Commons\src\libraries\Hexalith.Commons.UniqueIds\Hexalith.Commons.UniqueIds.csproj -getProperty:IsPackable,GeneratePackageOnBuild,PackageReadmeFile,IDEBuild` evaluated `IsPackable=true`, `GeneratePackageOnBuild=false`, `PackageReadmeFile=README.md`, and `IDEBuild=true` after the project-local fix.
- The README overview table in `src/libraries/Hexalith.Commons.UniqueIds/README.md` now uses the required column-oriented comparison layout with methods as rows.
- `dotnet pack d:\Hexalith.Commons\src\libraries\Hexalith.Commons.UniqueIds\Hexalith.Commons.UniqueIds.csproj -nologo` succeeded on 2026-03-14 and generated `src/libraries/Hexalith.Commons.UniqueIds/bin/Release/Hexalith.Commons.UniqueIds.1.0.0.nupkg`.
- The generated package contains `README.md` at the package root, and its SHA-256 hash matches `src/libraries/Hexalith.Commons.UniqueIds/README.md` exactly.

#### Findings

- No blocking implementation issues remain.
- The packaging verification is now reproducible in the current VS Code environment.
- The README comparison table now matches the acceptance criteria.

#### Follow-up

- Story 4.2 is complete and ready to remain closed unless new regressions are introduced.

### Change Log

- 2026-03-14: Added README and NuGet packaging updates for Story 4.2
- 2026-03-14: Senior Developer Review (AI) requested changes after finding a non-reproducible packaging verification and an AC mismatch in the README comparison table
- 2026-03-14: Fixed the README table layout and local IDE packaging behavior; verified package README inclusion and restored Story 4.2 to done
