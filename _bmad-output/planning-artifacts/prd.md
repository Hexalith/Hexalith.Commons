---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish']
inputDocuments: ['project-context.md']
workflowType: 'prd'
classification:
  projectType: 'developer_tool'
  domain: 'general'
  complexity: 'low'
  projectContext: 'brownfield'
decisions:
  - 'Use Cysharp/Ulid as external dependency'
  - 'Verify Base64URL encoding change is backward-compatible'
  - 'New method: GenerateSortableUniqueStringId() returns 26-char ULID'
documentCounts:
  briefs: 0
  research: 0
  projectDocs: 1
---

# Product Requirements Document - Hexalith.Commons.UniqueIds

**Author:** JeromePiquot
**Date:** 2026-03-14

## Executive Summary

Hexalith.Commons.UniqueIds is the shared identifier generation library for the Hexalith ecosystem — used by both internal modules and external consumers. Today it offers two ID strategies: human-readable DateTime IDs (sortable, single-machine) and Base64-encoded GUID IDs (distributed-safe, unsorted). This enhancement fills a critical gap by adding ULID-based sortable unique identifiers that are both chronologically ordered and distributed-safe — directly addressing event ordering requirements in Hexalith's DDD/CQRS architecture. The existing GUID-based method is also updated to use proper Base64URL encoding, replacing the current manual character substitution hack.

### What Makes This Special

Three ID strategies under one static helper, each purpose-built for a specific use case — no external decision-making required. The ULID addition eliminates the forced trade-off between sortability and distributed uniqueness. Events, aggregate snapshots, and projections get natural chronological ordering at the ID layer, so every downstream system — event stores, read models, logs — benefits without additional sorting logic. Built on the battle-tested `Cysharp/Ulid` library rather than a custom implementation, keeping the solution reliable and the codebase lean.

### Project Classification

- **Project Type:** Developer tool (NuGet library)
- **Domain:** General — shared infrastructure utility
- **Complexity:** Low — standard ULID spec via proven library, focused enhancement
- **Project Context:** Brownfield — extending existing `Hexalith.Commons.UniqueIds` package

## Success Criteria

### User Success

- Developer identifies the right ID method in < 30 seconds from the API surface
- ULID IDs sort correctly as strings — lexicographic order matches chronological order
- Conversion utilities (timestamp extraction, Ulid ↔ Guid) are discoverable and intuitive
- Zero behavior change for `GenerateDateTimeId()` consumers

### Business Success

- Adopted as the default ID strategy for event sourcing across Hexalith modules
- Single package covers all ID generation needs — no reason to look elsewhere

### Technical Success

- Thread-safe concurrent generation with zero duplicates
- ULID monotonic ordering within the same millisecond
- All existing tests pass unchanged
- New tests cover: uniqueness, sortability, length (26 chars), concurrency, timestamp extraction, Guid conversion

### Measurable Outcomes

- 100% of existing tests pass after Base64URL update
- Sortability verified: 1000 sequential IDs sort correctly via `string.Compare`
- Concurrency verified: 100 parallel generations produce 100 unique IDs
- Round-trip verified: `Ulid → Guid → Ulid` produces identical values

## User Journeys

### Journey 1: Marco, the Hexalith Module Developer (Primary — Success Path)

Marco is building a new `Hexalith.Orders` module. He needs aggregate events to sort chronologically across distributed services. He opens `UniqueIdHelper`, sees three methods with clear XML docs, and picks `GenerateSortableUniqueStringId()`. His events now sort naturally in the event store — no custom comparers, no timestamp columns. When debugging a production issue, he copies a ULID from the logs, extracts the timestamp, and immediately knows when the event was created.

**Reveals:** Clear API naming, XML documentation, timestamp extraction utility.

### Journey 2: Priya, the External NuGet Consumer (Primary — Discovery Path)

Priya finds `Hexalith.Commons.UniqueIds` on NuGet while searching for a lightweight ULID generator for her ASP.NET API. She reads the README, sees the comparison table of three ID strategies, and understands which to use in 30 seconds. She installs the package, calls one static method, and has sortable IDs in her API responses. No configuration, no DI registration, no ceremony.

**Reveals:** README quality, comparison table, zero-config static API, NuGet discoverability.

### Journey 3: Sam, the Migrating Developer (Edge Case — Migration Path)

Sam's team has been using `GenerateUniqueStringId()` for aggregate IDs across three Hexalith modules. Event replay is painful — events are randomly ordered and they're constantly sorting by a separate timestamp field. Sam wants to switch new aggregates to ULID while existing data stays untouched. He uses `GenerateSortableUniqueStringId()` for new aggregates and keeps the old method for existing ones. For interop, he uses the Guid conversion utility when bridging old and new systems. No big-bang migration needed.

**Reveals:** Coexistence of ID strategies, Guid conversion utility, incremental adoption path.

### Journey 4: Li, the Integration Developer (Edge Case — Interop Path)

Li integrates Hexalith with a third-party ERP that only accepts GUIDs. She generates a ULID for internal event ordering, then converts it to a Guid for the ERP API call. When the ERP sends a response with the Guid, she converts back to ULID to correlate with internal events. The round-trip is lossless. She also extracts the timestamp from a ULID during debugging to pinpoint when a failed integration event was created.

**Reveals:** Ulid → Guid → Ulid round-trip, timestamp extraction for debugging, lossless conversion.

### Journey Requirements Summary

| Capability | Journeys | Priority |
|-----------|----------|----------|
| `GenerateSortableUniqueStringId()` | Marco, Priya, Sam | MVP Core |
| Proper Base64URL on existing method | Sam | MVP Core |
| Timestamp extraction from ULID | Marco, Li | MVP |
| Ulid ↔ Guid conversion | Sam, Li | MVP |
| Clear XML documentation | Marco, Priya | MVP |
| README with comparison table | Priya | MVP |
| Coexistence of all three ID strategies | Sam | MVP |

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Problem-solving — deliver the minimum that solves the event ordering gap via ULID, plus conversion utilities.
**Resource Requirements:** Single developer, 1-2 days of implementation + testing.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:** Marco (event ordering), Sam (migration/interop), Li (Guid conversion), Priya (discovery)

**Must-Have Capabilities:**
1. `GenerateSortableUniqueStringId()` — ULID generation via `Cysharp/Ulid`
2. `GenerateUniqueStringId()` — updated to proper Base64URL encoding
3. `ExtractTimestamp(string ulid)` — timestamp extraction from ULID
4. `ToGuid(string ulid)` — ULID to Guid conversion
5. `ToSortableUniqueId(Guid guid)` — Guid to ULID conversion
6. Tests: uniqueness, sortability, concurrency, round-trip, timestamp extraction
7. README: comparison table + code examples for all methods

### Post-MVP Features

**Phase 2 (Growth):**
- `SortableUniqueId` value object with parsing, comparison operators, implicit conversions
- Integration with Hexalith DDD abstractions (aggregate root ID base type)

**Phase 3 (Expansion):**
- ULID-based correlation ID middleware for distributed tracing
- ID generation telemetry and diagnostics

### Risk Mitigation Strategy

**Technical Risks:** Low — `Cysharp/Ulid` is battle-tested. Only risk is Base64URL output compatibility with current method. Mitigation: unit test that verifies character set is identical (`A-Za-z0-9-_`).
**Market Risks:** None — internal infrastructure library with known consumers.
**Resource Risks:** Minimal — single-developer scope. If constrained, drop README update and ship docs later.

## Developer Tool Specific Requirements

### Project-Type Overview

NuGet library targeting .NET 10+ / C# 14+. Single static helper class (`UniqueIdHelper`) with zero-config API. Consumed via `Hexalith.Commons.UniqueIds` package. External dependency on `Cysharp/Ulid` managed through centralized `Directory.Packages.props`.

### API Surface

| Method | Behavior | Returns |
|--------|----------|---------|
| `GenerateDateTimeId()` | Unchanged | 17-char datetime string |
| `GenerateUniqueStringId()` | Updated to proper Base64URL encoding | 22-char Base64URL string |
| `GenerateSortableUniqueStringId()` | New — generates ULID | 26-char Crockford Base32 string |
| `ExtractTimestamp(string ulid)` | New — extracts creation time from ULID | `DateTimeOffset` |
| `ToGuid(string ulid)` | New — converts ULID string to Guid | `Guid` |
| `ToSortableUniqueId(Guid guid)` | New — converts Guid to ULID string | 26-char string |

### Technical Architecture Considerations

- All methods remain static on `UniqueIdHelper` — no DI, no interfaces, no ceremony
- Thread safety via `Lock` for methods requiring monotonic guarantees
- `Cysharp/Ulid` handles ULID internals (timestamp encoding, Crockford Base32, monotonic ordering)
- Conversion utilities are thin wrappers over `Ulid` struct methods

### Documentation Requirements

- XML documentation on all public methods with `<summary>`, `<param>`, `<returns>`
- README with code examples for every method
- Comparison table of all three ID strategies (format, length, sortable, distributed-safe)
- Installation instructions via NuGet

### Implementation Considerations

- Package version for `Cysharp/Ulid` added to `Directory.Packages.props` in `Hexalith.Builds` submodule — requires approval
- No migration guide needed — developers simply adopt new methods
- Existing `GenerateUniqueStringId()` output should remain functionally identical after Base64URL update

## Functional Requirements

### ID Generation

- **FR1:** Developer can generate a sortable unique string identifier that is chronologically ordered and distributed-safe
- **FR2:** Developer can generate a non-sortable unique string identifier encoded in Base64URL format
- **FR3:** Developer can generate a datetime-based unique string identifier (existing, unchanged)
- **FR4:** Developer can generate sortable IDs concurrently from multiple threads without duplicates
- **FR5:** Developer can generate sortable IDs within the same millisecond that maintain monotonic ordering

### ID Conversion

- **FR6:** Developer can extract the creation timestamp from a ULID string as a `DateTimeOffset`
- **FR7:** Developer can convert a ULID string to a `Guid`
- **FR8:** Developer can convert a `Guid` to a ULID string
- **FR9:** Developer can round-trip between ULID and Guid without data loss

### API Discoverability

- **FR10:** Developer can discover all ID generation methods via IntelliSense with XML documentation
- **FR11:** Developer can determine which ID method to use from a README comparison table
- **FR12:** Developer can see code examples for each method in the README

### Coexistence

- **FR13:** Developer can use all three ID strategies (DateTime, Base64URL, ULID) independently within the same application
- **FR14:** Developer can adopt ULID for new code without affecting existing code that uses other ID methods

## Non-Functional Requirements

### Performance

- ID generation must not become a bottleneck: `GenerateSortableUniqueStringId()` completes in < 1 microsecond per call (matching `Cysharp/Ulid` benchmarks)
- Zero heap allocations on the generation hot path beyond the returned string
- Lock contention under concurrent load must not degrade throughput by more than 10% vs single-threaded
- `GenerateUniqueStringId()` Base64URL update must not degrade performance compared to current `Replace`-based implementation

### Compatibility

- .NET 10+ target framework
- No platform-specific dependencies — runs on Windows, Linux, macOS
- `Cysharp/Ulid` version pinned in centralized `Directory.Packages.props`
- ULID output conforms to the [ULID specification](https://github.com/ulid/spec) — interoperable with any ULID implementation in any language
