# Hexalith.Commons.UniqueIds

> Three ID strategies for .NET — DateTime, Base64URL, and ULID — pick the right one in 30 seconds.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.UniqueIds.svg)](https://www.nuget.org/packages/Hexalith.Commons.UniqueIds)

---

## Overview

**Hexalith.Commons.UniqueIds** provides three unique identifier strategies via the static `UniqueIdHelper` class:

| Method                             | Format                | Length   | Sortable            | Distributed-Safe    | Thread-Safe     | Best For                           |
| ---------------------------------- | --------------------- | -------- | ------------------- | ------------------- | --------------- | ---------------------------------- |
| `GenerateDateTimeId()`             | `yyyyMMddHHmmssfff`   | 17 chars | Yes (chronological) | No (single machine) | Yes (locked)    | Log entries, file names            |
| `GenerateUniqueStringId()`         | Base64URL GUID        | 22 chars | No                  | Yes                 | Yes (stateless) | Legacy keys, session tokens        |
| `GenerateSortableUniqueStringId()` | Crockford Base32 ULID | 26 chars | Yes (chronological) | Yes                 | Yes (monotonic) | **Event sourcing, DDD aggregates** |

---

## Installation

```bash
dotnet add package Hexalith.Commons.UniqueIds
```

---

## Quick Start

```csharp
using Hexalith.Commons.UniqueIds;

// Sortable + distributed (event sourcing, DDD)
string ulidId = UniqueIdHelper.GenerateSortableUniqueStringId();
// "01HYX7QS3NP8M4KQJR5A7CVWKM" — 26-char ULID

// Distributed (legacy keys, session tokens)
string base64Id = UniqueIdHelper.GenerateUniqueStringId();
// "gZOW2EgVrEq5SBJLegYcVA" — 22-char Base64URL

// Human-readable (single machine, logs)
string dateId = UniqueIdHelper.GenerateDateTimeId();
// "20260314143052789" — 17-char timestamp
```

---

## Method Reference

### GenerateSortableUniqueStringId()

Generates a 26-character ULID — sortable, distributed-safe, ideal for event sourcing.

```csharp
string id = UniqueIdHelper.GenerateSortableUniqueStringId();
// "01HYX7QS3NP8M4KQJR5A7CVWKM"

// ULIDs sort chronologically as plain strings
string[] ids = Enumerable.Range(0, 5)
    .Select(_ => UniqueIdHelper.GenerateSortableUniqueStringId())
    .ToArray();
// ids are already in creation order — no OrderBy needed
```

### GenerateUniqueStringId()

Generates a 22-character Base64URL string from a GUID. Distributed-safe but not sortable.

```csharp
string id = UniqueIdHelper.GenerateUniqueStringId();
// "gZOW2EgVrEq5SBJLegYcVA"

// URL-safe — use directly in REST routes
// GET /api/orders/gZOW2EgVrEq5SBJLegYcVA
```

### GenerateDateTimeId()

Generates a 17-character UTC timestamp ID. Human-readable and sortable, but single-machine only.

```csharp
string id = UniqueIdHelper.GenerateDateTimeId();
// "20260314143052789"

// Format breakdown: yyyyMMddHHmmssfff
// Thread-safe: same-millisecond calls auto-increment
```

---

## Conversion Utilities

### ExtractTimestamp(string ulid)

Need to know when an entity was created? Extract the embedded UTC timestamp from any ULID.

```csharp
string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();
DateTimeOffset created = UniqueIdHelper.ExtractTimestamp(ulid);
// Returns the exact UTC time the ULID was generated
```

### ToGuid(string ulid)

Your ERP only accepts GUIDs? Convert your internal ULID while preserving the 128-bit identity.

```csharp
string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();
Guid guid = UniqueIdHelper.ToGuid(ulid);
// Use guid for external systems that require System.Guid
```

> **Note:** `ToGuid` preserves identity but NOT lexicographic sort order. Two ULIDs that sort correctly as strings may not sort the same way as Guids due to byte-order differences.

### ToSortableUniqueId(Guid value)

Converting back from a Guid to a ULID string? Round-trip is lossless for Guids originally derived from ULIDs.

```csharp
// Round-trip: ULID → Guid → ULID
string original = UniqueIdHelper.GenerateSortableUniqueStringId();
Guid guid = UniqueIdHelper.ToGuid(original);
string restored = UniqueIdHelper.ToSortableUniqueId(guid);
// restored == original (case-insensitive)
```

> **Note:** Converting a non-ULID Guid (e.g., `Guid.NewGuid()`) produces a valid ULID string, but its embedded timestamp is meaningless.

---

## Decision Guide

| Use Case                              | Recommended Method                                              |
| ------------------------------------- | --------------------------------------------------------------- |
| **Event sourcing / DDD aggregates**   | `GenerateSortableUniqueStringId()` — sortable + distributed     |
| **Distributed keys / session tokens** | `GenerateUniqueStringId()` — compact + GUID-backed              |
| **Log entries / file names**          | `GenerateDateTimeId()` — human-readable timestamps              |
| **Migrating Base64URL to ULID**       | Generate ULID for new records; keep old Base64URL IDs unchanged |
| **Interop with Guid-only systems**    | `ToGuid()` / `ToSortableUniqueId()` — lossless round-trip       |

---

## Combined Usage

```csharp
public class AuditLog
{
    // ULID: sortable, distributed-safe event ID
    public string EventId { get; } = UniqueIdHelper.GenerateSortableUniqueStringId();

    // Base64URL: globally unique correlation for tracing
    public string CorrelationId { get; set; }

    public string Action { get; set; }
    public string UserId { get; set; }
}

var log = new AuditLog
{
    CorrelationId = UniqueIdHelper.GenerateUniqueStringId(),
    Action = "OrderPlaced",
    UserId = "user-42"
};
```

---

## Thread Safety

All three methods are fully thread-safe:

```csharp
var ids = new ConcurrentBag<string>();

Parallel.For(0, 10_000, _ =>
{
    ids.Add(UniqueIdHelper.GenerateSortableUniqueStringId());
});

// All 10,000 IDs are unique and in monotonic order within each millisecond
Debug.Assert(ids.Distinct().Count() == 10_000);
```

---

## License

MIT License — See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons.UniqueIds)
