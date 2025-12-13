# Hexalith.Commons.UniqueIds

> Lightweight unique identifier generation for .NET applications.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.UniqueIds.svg)](https://www.nuget.org/packages/Hexalith.Commons.UniqueIds)

---

## Overview

**Hexalith.Commons.UniqueIds** provides two methods for generating unique identifiers, each optimized for different use cases:

| Method | Length | Format | Best For |
|--------|--------|--------|----------|
| `GenerateDateTimeId()` | 17 chars | `yyyyMMddHHmmssfff` | Sortable, human-readable IDs |
| `GenerateUniqueStringId()` | 22 chars | Base64 URL-safe | Distributed systems, high throughput |

---

## Installation

```bash
dotnet add package Hexalith.Commons.UniqueIds
```

---

## Quick Start

```csharp
using Hexalith.Commons.UniqueIds;

// DateTime-based ID (sortable, readable)
string dateId = UniqueIdHelper.GenerateDateTimeId();
// Example: "20240615143052789"

// GUID-based ID (distributed-safe)
string uniqueId = UniqueIdHelper.GenerateUniqueStringId();
// Example: "gZOW2EgVrEq5SBJLegYcVA"
```

---

## DateTime-Based IDs

### GenerateDateTimeId()

Generates a 17-character identifier based on the current UTC timestamp.

```csharp
public static string GenerateDateTimeId()
```

**Returns:** String in format `yyyyMMddHHmmssfff`

**Example:**
```csharp
string id = UniqueIdHelper.GenerateDateTimeId();
// "20240615143052789"
//  ^^^^              Year (2024)
//      ^^            Month (06 = June)
//        ^^          Day (15)
//          ^^        Hour (14 = 2 PM)
//            ^^      Minute (30)
//              ^^    Second (52)
//                ^^^ Millisecond (789)
```

### Characteristics

| Property | Value |
|----------|-------|
| Length | 17 characters |
| Character set | 0-9 |
| Timezone | UTC |
| Sortable | Yes (chronological) |
| Human readable | Yes |
| Thread safe | Yes |
| Max rate | 1 per millisecond |

### Thread Safety

The method is thread-safe and handles concurrent calls:

```csharp
// Thread-safe: automatic increment for same-millisecond calls
var tasks = Enumerable.Range(0, 100)
    .Select(_ => Task.Run(() => UniqueIdHelper.GenerateDateTimeId()));

string[] ids = await Task.WhenAll(tasks);

// All IDs are unique
Debug.Assert(ids.Distinct().Count() == 100);
```

### Use Cases

**Log correlation IDs:**
```csharp
public class LogEntry
{
    public string Id { get; } = UniqueIdHelper.GenerateDateTimeId();
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}

// IDs sort naturally by creation time
var logs = logEntries.OrderBy(l => l.Id);
```

**Sequential order numbers:**
```csharp
public class Order
{
    public string OrderNumber { get; } = UniqueIdHelper.GenerateDateTimeId();
    // OrderNumber: "20240615143052789"
}
```

**File naming with timestamp:**
```csharp
string backupFile = $"backup_{UniqueIdHelper.GenerateDateTimeId()}.zip";
// "backup_20240615143052789.zip"
```

### Limitations

- **Rate limit**: Maximum 1 unique ID per millisecond
- **Single machine**: Not suitable for distributed ID generation
- **Predictable**: Sequential nature may be undesirable for some use cases

---

## GUID-Based IDs

### GenerateUniqueStringId()

Generates a 22-character identifier derived from a GUID using URL-safe Base64 encoding.

```csharp
public static string GenerateUniqueStringId()
```

**Returns:** 22-character URL-safe string

**Example:**
```csharp
string id = UniqueIdHelper.GenerateUniqueStringId();
// "gZOW2EgVrEq5SBJLegYcVA"
```

### Characteristics

| Property | Value |
|----------|-------|
| Length | 22 characters |
| Character set | A-Z, a-z, 0-9, _, - |
| Sortable | No |
| Human readable | No |
| Thread safe | Yes |
| Uniqueness | Globally unique (GUID-based) |
| URL safe | Yes |

### Encoding Details

The method:
1. Generates a new GUID
2. Converts to Base64
3. Replaces `+` with `_` and `/` with `-`
4. Truncates trailing `==` padding

```csharp
// Equivalent to:
Guid guid = Guid.NewGuid();
string base64 = Convert.ToBase64String(guid.ToByteArray());
string urlSafe = base64.Replace('+', '_').Replace('/', '-').TrimEnd('=');
// Result: 22 characters
```

### Use Cases

**Primary keys:**
```csharp
public class Entity
{
    public string Id { get; } = UniqueIdHelper.GenerateUniqueStringId();
}

// Short, URL-safe IDs for REST APIs
// GET /api/orders/gZOW2EgVrEq5SBJLegYcVA
```

**Distributed systems:**
```csharp
// Safe to generate across multiple servers
public class DistributedEvent
{
    public string EventId { get; } = UniqueIdHelper.GenerateUniqueStringId();
    public string Payload { get; set; }
}
```

**Session tokens:**
```csharp
string sessionId = UniqueIdHelper.GenerateUniqueStringId();
Response.Cookies.Append("session", sessionId);
```

**Correlation IDs for distributed tracing:**
```csharp
public class RequestContext
{
    public string CorrelationId { get; } = UniqueIdHelper.GenerateUniqueStringId();
}

// Pass through HTTP headers
httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", context.CorrelationId);
```

---

## Comparison

| Feature | GenerateDateTimeId | GenerateUniqueStringId |
|---------|-------------------|----------------------|
| **Length** | 17 chars | 22 chars |
| **Characters** | 0-9 only | Alphanumeric + _ - |
| **Sortable** | Yes | No |
| **Human readable** | Yes | No |
| **Distributed safe** | No | Yes |
| **Rate limit** | 1/ms | Unlimited |
| **Predictable** | Yes | No |
| **URL safe** | Yes | Yes |

### Decision Guide

Use `GenerateDateTimeId()` when:
- IDs need to be sortable by creation time
- Human readability is important
- Single-server deployment
- Low throughput (< 1000/second)

Use `GenerateUniqueStringId()` when:
- Distributed system with multiple servers
- High throughput requirements
- Unpredictable IDs preferred
- Standard GUID uniqueness guarantees needed

---

## Examples

### Combined Usage

```csharp
public class AuditLog
{
    // Sortable, timestamp-based ID for ordering
    public string LogId { get; } = UniqueIdHelper.GenerateDateTimeId();

    // Globally unique correlation for distributed tracing
    public string CorrelationId { get; set; }

    public string Action { get; set; }
    public string UserId { get; set; }
}

// Usage
var log = new AuditLog
{
    CorrelationId = UniqueIdHelper.GenerateUniqueStringId(),
    Action = "UserLogin",
    UserId = "user123"
};
```

### ID Generation Service

```csharp
public interface IIdGenerator
{
    string NewId();
    string NewSortableId();
}

public class HexalithIdGenerator : IIdGenerator
{
    public string NewId() => UniqueIdHelper.GenerateUniqueStringId();
    public string NewSortableId() => UniqueIdHelper.GenerateDateTimeId();
}

// Register in DI
services.AddSingleton<IIdGenerator, HexalithIdGenerator>();
```

---

## Thread Safety

Both methods are fully thread-safe:

```csharp
// Concurrent generation test
var ids = new ConcurrentBag<string>();

Parallel.For(0, 10000, _ =>
{
    ids.Add(UniqueIdHelper.GenerateUniqueStringId());
});

// All 10,000 IDs are unique
Debug.Assert(ids.Distinct().Count() == 10000);
```

---

## License

MIT License - See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons.UniqueIds)
