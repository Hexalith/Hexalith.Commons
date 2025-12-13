# Hexalith.Commons.Metadatas

> Structured metadata for message tracking and correlation in distributed systems.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.Metadatas.svg)](https://www.nuget.org/packages/Hexalith.Commons.Metadatas)

---

## Overview

**Hexalith.Commons.Metadatas** provides a standardized set of record types for tracking messages as they flow through distributed systems. These metadata structures enable:

- **Message identification**: Unique IDs and versioning
- **Domain context**: Aggregate tracking for DDD/Event Sourcing
- **Request correlation**: Trace requests across services
- **Audit trails**: User, session, and timestamp tracking
- **Message ordering**: Sequence numbers for ordering guarantees

---

## Installation

```bash
dotnet add package Hexalith.Commons.Metadatas
```

---

## Architecture

```
Metadata
├── Message (MessageMetadata)
│   ├── Id            - Unique message identifier
│   ├── Name          - Message type name
│   ├── Version       - Schema version
│   ├── CreatedDate   - Creation timestamp
│   └── Domain (DomainMetadata)
│       ├── Id        - Aggregate identifier
│       └── Name      - Aggregate type name
└── Context (ContextMetadata)
    ├── CorrelationId  - Request correlation
    ├── UserId         - Acting user
    ├── PartitionId    - Distribution partition
    ├── SessionId      - User session
    ├── SequenceNumber - Message order
    ├── ReceivedDate   - Receipt timestamp
    └── Scopes         - Authorization scopes
```

---

## Quick Start

```csharp
using Hexalith.Commons.Metadatas;
using Hexalith.Commons.UniqueIds;

// Create complete metadata for a message
var metadata = new Metadata(
    Message: new MessageMetadata(
        Id: UniqueIdHelper.GenerateUniqueStringId(),
        Name: "OrderCreated",
        Version: 1,
        Domain: new DomainMetadata(Id: "ORD-12345", Name: "Order"),
        CreatedDate: DateTimeOffset.UtcNow
    ),
    Context: new ContextMetadata(
        CorrelationId: correlationId,
        UserId: "user-001",
        PartitionId: "tenant-acme",
        SessionId: sessionId,
        SequenceNumber: 1,
        ReceivedDate: DateTimeOffset.UtcNow,
        Scopes: new[] { "orders:write" }
    )
);

// Get globally unique domain identifier
string globalId = metadata.DomainGlobalId;
// Format: "tenant-acme-Order-ORD-12345"
```

---

## API Reference

### DomainMetadata

Identifies the domain aggregate associated with a message.

```csharp
public record DomainMetadata(
    string Id,      // Aggregate identifier (e.g., "ORD-12345")
    string Name     // Aggregate type name (e.g., "Order")
);
```

**Example:**
```csharp
var domain = new DomainMetadata(
    Id: "ORD-12345",
    Name: "Order"
);
```

**Use cases:**
- Event sourcing: Associate events with aggregates
- CQRS: Route commands to correct aggregate
- Auditing: Track which entity was modified

---

### MessageMetadata

Contains information specific to the message itself.

```csharp
public record MessageMetadata(
    string Id,                    // Unique message identifier
    string Name,                  // Message type name
    int Version,                  // Schema version
    DomainMetadata Domain,        // Associated domain aggregate
    DateTimeOffset CreatedDate    // When message was created
);
```

**Example:**
```csharp
var messageMetadata = new MessageMetadata(
    Id: UniqueIdHelper.GenerateUniqueStringId(),
    Name: "OrderShipped",
    Version: 2,
    Domain: new DomainMetadata("ORD-12345", "Order"),
    CreatedDate: DateTimeOffset.UtcNow
);
```

**Properties explained:**

| Property | Purpose |
|----------|---------|
| `Id` | Deduplicate messages, idempotency |
| `Name` | Route to correct handler, serialization |
| `Version` | Schema evolution, backward compatibility |
| `Domain` | Aggregate association |
| `CreatedDate` | Ordering, debugging, audit |

---

### ContextMetadata

Captures the execution context when a message was created or received.

```csharp
public record ContextMetadata(
    string? CorrelationId,              // Request correlation ID
    string? UserId,                     // User performing the action
    string? PartitionId,                // Partition for distribution
    DateTimeOffset? ReceivedDate,       // When message was received
    long SequenceNumber,                // Message sequence number
    string? SessionId,                  // User session ID
    IEnumerable<string>? Scopes         // Authorization scopes
);
```

**Example:**
```csharp
var contextMetadata = new ContextMetadata(
    CorrelationId: "req-abc123",
    UserId: "user-001",
    PartitionId: "tenant-acme",
    ReceivedDate: DateTimeOffset.UtcNow,
    SequenceNumber: 42,
    SessionId: "sess-xyz789",
    Scopes: new[] { "orders:read", "orders:write" }
);
```

**Properties explained:**

| Property | Purpose |
|----------|---------|
| `CorrelationId` | Trace requests across services |
| `UserId` | Audit who performed action |
| `PartitionId` | Multi-tenancy, sharding |
| `ReceivedDate` | Processing timestamps |
| `SequenceNumber` | Ordering within partition |
| `SessionId` | User session tracking |
| `Scopes` | Authorization context |

---

### Metadata

Composite record combining message and context metadata.

```csharp
public record Metadata(
    MessageMetadata Message,
    ContextMetadata Context
)
{
    // Generate globally unique domain identifier
    public string DomainGlobalId { get; }

    // Create domain global ID from components
    public static string CreateDomainGlobalId(
        string? partitionId,
        string? aggregateName,
        string? aggregateId);

    // Logging-friendly representation
    public string ToLogString();
}
```

#### DomainGlobalId

Generates a globally unique identifier for the domain aggregate.

```csharp
var metadata = new Metadata(messageMetadata, contextMetadata);

string globalId = metadata.DomainGlobalId;
// Format: "{partitionId}-{aggregateName}-{aggregateId}"
// Example: "tenant-acme-Order-ORD-12345"
```

#### CreateDomainGlobalId (Static)

Create domain global ID without full metadata object.

```csharp
string globalId = Metadata.CreateDomainGlobalId(
    partitionId: "tenant-acme",
    aggregateName: "Order",
    aggregateId: "ORD-12345"
);
// Result: "tenant-acme-Order-ORD-12345"
```

#### ToLogString

Get a concise representation for logging.

```csharp
string logEntry = metadata.ToLogString();
// Example: "[OrderCreated] Order:ORD-12345 (tenant-acme) CorrelationId:req-abc123"
```

---

## Usage Patterns

### Event Sourcing

```csharp
public interface IEvent
{
    Metadata Metadata { get; }
}

public record OrderCreatedEvent : IEvent
{
    public Metadata Metadata { get; init; }
    public string CustomerId { get; init; }
    public List<OrderLine> Lines { get; init; }
}

// Create event with metadata
var @event = new OrderCreatedEvent
{
    Metadata = new Metadata(
        Message: new MessageMetadata(
            Id: UniqueIdHelper.GenerateUniqueStringId(),
            Name: nameof(OrderCreatedEvent),
            Version: 1,
            Domain: new DomainMetadata(order.Id, "Order"),
            CreatedDate: DateTimeOffset.UtcNow
        ),
        Context: currentContext
    ),
    CustomerId = customerId,
    Lines = orderLines
};
```

### Message Envelope

```csharp
public record MessageEnvelope<T>
{
    public Metadata Metadata { get; init; }
    public T Payload { get; init; }
}

// Usage
var envelope = new MessageEnvelope<CreateOrderCommand>
{
    Metadata = metadata,
    Payload = new CreateOrderCommand { /* ... */ }
};

await messageBus.PublishAsync(envelope);
```

### Distributed Tracing

```csharp
public class CorrelationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Extract or create correlation ID
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? UniqueIdHelper.GenerateUniqueStringId();

        // Create context metadata
        var contextMetadata = new ContextMetadata(
            CorrelationId: correlationId,
            UserId: context.User?.Identity?.Name,
            PartitionId: context.Request.Headers["X-Tenant-Id"].FirstOrDefault(),
            ReceivedDate: DateTimeOffset.UtcNow,
            SequenceNumber: 0,
            SessionId: context.Session?.Id,
            Scopes: context.User?.Claims
                .Where(c => c.Type == "scope")
                .Select(c => c.Value)
        );

        // Store in request context
        context.Items["ContextMetadata"] = contextMetadata;

        // Propagate to response
        context.Response.Headers.Add("X-Correlation-Id", correlationId);

        await next(context);
    }
}
```

### Multi-Tenant Partitioning

```csharp
public class TenantAwareRepository
{
    public async Task SaveEventAsync(IEvent @event)
    {
        // Use domain global ID for partition key
        string partitionKey = @event.Metadata.DomainGlobalId;

        await eventStore.AppendAsync(
            partitionKey,
            @event.Metadata.Context.SequenceNumber,
            @event
        );
    }

    public async Task<IEnumerable<IEvent>> GetEventsAsync(
        string tenantId,
        string aggregateName,
        string aggregateId)
    {
        string partitionKey = Metadata.CreateDomainGlobalId(
            tenantId,
            aggregateName,
            aggregateId
        );

        return await eventStore.ReadAsync(partitionKey);
    }
}
```

### Audit Logging

```csharp
public class AuditLogger
{
    public void Log(Metadata metadata, string action, object details)
    {
        var auditEntry = new
        {
            Timestamp = DateTimeOffset.UtcNow,
            MessageId = metadata.Message.Id,
            MessageType = metadata.Message.Name,
            AggregateType = metadata.Message.Domain.Name,
            AggregateId = metadata.Message.Domain.Id,
            UserId = metadata.Context.UserId,
            TenantId = metadata.Context.PartitionId,
            CorrelationId = metadata.Context.CorrelationId,
            SessionId = metadata.Context.SessionId,
            Action = action,
            Details = details
        };

        logger.LogInformation("Audit: {@AuditEntry}", auditEntry);
    }
}
```

---

## Serialization

All metadata types are records and serialize cleanly to JSON:

```json
{
  "Message": {
    "Id": "gZOW2EgVrEq5SBJLegYcVA",
    "Name": "OrderCreated",
    "Version": 1,
    "Domain": {
      "Id": "ORD-12345",
      "Name": "Order"
    },
    "CreatedDate": "2024-06-15T14:30:52.789Z"
  },
  "Context": {
    "CorrelationId": "req-abc123",
    "UserId": "user-001",
    "PartitionId": "tenant-acme",
    "ReceivedDate": "2024-06-15T14:30:52.800Z",
    "SequenceNumber": 42,
    "SessionId": "sess-xyz789",
    "Scopes": ["orders:read", "orders:write"]
  }
}
```

---

## License

MIT License - See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons.Metadatas)
