# Hexalith.Commons

> A modular .NET utility library providing essential building blocks for enterprise applications.

[![License: MIT](https://img.shields.io/github/license/hexalith/hexalith.commons)](https://github.com/hexalith/hexalith/blob/main/LICENSE)
[![Discord](https://img.shields.io/discord/1063152441819942922?label=Discord&logo=discord&logoColor=white&color=d82679)](https://discordapp.com/channels/1102166958918610994/1102166958918610997)
[![Build status](https://github.com/Hexalith/Hexalith.Commons/actions/workflows/build-release.yml/badge.svg)](https://github.com/Hexalith/Hexalith.Commons/actions)
[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.svg)](https://www.nuget.org/packages/Hexalith.Commons)

---

## Overview

**Hexalith.Commons** is a collection of focused .NET libraries that provide reusable utilities for common programming tasks. Each package is designed to be lightweight, well-tested, and easy to integrate.

### Key Capabilities

| Package | Purpose | Key Features |
|---------|---------|--------------|
| [Hexalith.Commons](#hexalithcommons-core) | Core utilities | String helpers, error handling, reflection, logging |
| [Hexalith.Commons.Configurations](#hexalithcommonsconfigurations) | Configuration management | Type-safe settings, FluentValidation integration |
| [Hexalith.Commons.StringEncoders](#hexalithcommonsstringencoders) | String encoding | RFC1123 encoding/decoding for restricted contexts |
| [Hexalith.Commons.UniqueIds](#hexalithcommonsuniqueids) | ID generation | DateTime-based and GUID-based unique identifiers |
| [Hexalith.Commons.Metadatas](#hexalithcommonsmetadatas) | Message metadata | Context tracking for distributed systems |

---

## Requirements

- **.NET 10.0** or later
- Compatible with ASP.NET Core, Console, Worker Services, and library projects

---

## Installation

Install packages via NuGet:

```bash
# Core utilities
dotnet add package Hexalith.Commons

# Configuration management
dotnet add package Hexalith.Commons.Configurations

# String encoding
dotnet add package Hexalith.Commons.StringEncoders

# Unique ID generation
dotnet add package Hexalith.Commons.UniqueIds

# Message metadata
dotnet add package Hexalith.Commons.Metadatas
```

---

## Hexalith.Commons (Core)

The core library provides essential utilities organized into focused namespaces.

### String Utilities

**Namespace:** `Hexalith.Extensions.Helpers`

```csharp
using Hexalith.Extensions.Helpers;

// Format strings with named placeholders
string template = "Hello {name}, your order #{orderId} is ready";
string result = template.FormatWithNamedPlaceholders(
    new Dictionary<string, object> { ["name"] = "John", ["orderId"] = 12345 }
);
// Result: "Hello John, your order #12345 is ready"

// Culture-invariant number conversions
string number = "42.5";
decimal value = number.ToDecimal();  // Works regardless of system culture

// RFC1123 hostname validation
bool isValid = "my-server.example.com".IsRfc1123Compliant();  // true
bool isInvalid = "my_server".IsRfc1123Compliant();            // false
```

### Error Handling

**Namespace:** `Hexalith.Commons.Errors`

Structured error handling with railway-oriented programming support.

```csharp
using Hexalith.Commons.Errors;

// Create structured errors
var error = new ApplicationError
{
    Title = "Validation Failed",
    Detail = "The field {fieldName} is required",
    Category = ErrorCategory.Validation,
    Arguments = new object[] { "Email" }
};

string message = error.GetDetailMessage();
// Result: "The field Email is required"

// Railway-oriented error handling with ValueOrError<T>
ValueOrError<User> result = await GetUserAsync(userId);

if (result.HasError)
{
    // Handle error
    logger.LogError(result.Error.GetDetailMessage());
}
else
{
    // Use the value
    User user = result.Value;
}
```

### Object Utilities

**Namespace:** `Hexalith.Commons.Objects`

Deep equality comparison and object introspection.

```csharp
using Hexalith.Commons.Objects;

// Deep equality comparison (supports nested objects, collections, dictionaries)
bool areEqual = EquatableHelper.AreSame(object1, object2);

// Attribute-based object description
var description = ObjectDescriptionHelper.Describe(typeof(MyClass));
// Returns: Name, DisplayName, Description from attributes

// Implement custom equality
public class Order : IEquatableObject
{
    public string Id { get; set; }
    public decimal Total { get; set; }

    public IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Total;
    }
}
```

### Reflection Utilities

**Namespace:** `Hexalith.Commons.Reflections`

Type discovery and mapping utilities.

```csharp
using Hexalith.Commons.Reflections;

// Find all implementations of an interface
IEnumerable<Type> handlers = ReflectionHelper.GetInstantiableTypesOf<ICommandHandler>();

// Create instances of discovered types
IEnumerable<ICommandHandler> instances = ReflectionHelper.GetInstantiableObjectsOf<ICommandHandler>();

// Type name mapping
var mapper = new TypeMapper();
mapper.Register<OrderCreatedEvent>("order-created");
Type eventType = mapper.GetType("order-created");
```

### Date Utilities

**Namespace:** `Hexalith.Commons.Dates`

Timezone-aware date operations.

```csharp
using Hexalith.Commons.Dates;

// Convert DateOnly to DateTimeOffset with timezone
DateOnly date = new(2024, 1, 15);
TimeSpan offset = TimeSpan.FromHours(-5); // EST
DateTimeOffset result = DateHelper.ToLocalTime(date, offset);

// Convert to UTC
DateTimeOffset utc = DateHelper.ToUniversalTime(date);

// Calculate wait time between dates
TimeSpan waitTime = DateHelper.WaitTime(targetDate, currentDate);
```

### Assembly Utilities

**Namespace:** `Hexalith.Commons.Assemblies`

Version information retrieval.

```csharp
using Hexalith.Commons.Assemblies;

// Get entry assembly version
string? version = VersionHelper.EntryProductVersion();

// Get version from specific assembly
string? assemblyVersion = typeof(MyClass).Assembly.GetAssemblyVersion();
```

### Logging Helpers

**Namespace:** `Hexalith.Commons.Helpers`

Structured logging for application errors.

```csharp
using Hexalith.Commons.Helpers;

// Log application errors with full context
logger.LogApplicationError(applicationError);
```

---

## Hexalith.Commons.Configurations

Type-safe configuration management with validation support.

### Defining Settings

```csharp
using Hexalith.Commons.Configurations;

public class DatabaseSettings : ISettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;

    // Configuration section name in appsettings.json
    public static string ConfigurationName() => "Database";
}
```

**appsettings.json:**
```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=MyApp",
    "CommandTimeout": 60,
    "MaxRetryCount": 5
  }
}
```

### Registration and Usage

```csharp
// Program.cs - Register settings
builder.Services.ConfigureSettings<DatabaseSettings>(builder.Configuration);

// Service class - Inject and use
public class DataService
{
    private readonly DatabaseSettings _settings;

    public DataService(IOptions<DatabaseSettings> options)
    {
        _settings = options.Value;

        // Validate required settings
        SettingsException<DatabaseSettings>.ThrowIfUndefined(_settings.ConnectionString);
    }
}
```

### FluentValidation Integration

```csharp
using FluentValidation;

public class DatabaseSettingsValidator : AbstractValidator<DatabaseSettings>
{
    public DatabaseSettingsValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("Database connection string is required");

        RuleFor(x => x.CommandTimeout)
            .InclusiveBetween(1, 300)
            .WithMessage("Command timeout must be between 1 and 300 seconds");
    }
}

// Registration with validation
services.ConfigureSettings<DatabaseSettings>(configuration);
services.AddValidatorsFromAssemblyContaining<DatabaseSettingsValidator>();
```

---

## Hexalith.Commons.StringEncoders

Reversible string encoding for RFC1123-compliant contexts.

### Encoding Rules

| Character | Encoded Form | Description |
|-----------|--------------|-------------|
| A-Z, a-z, 0-9, -, . | Unchanged | Allowed characters |
| `_` (underscore) | `__` | Escaped as double underscore |
| Space | `_20` | UTF-8 hex encoding |
| Other characters | `_XX` | UTF-8 byte hex encoding |

### Usage Examples

```csharp
using Hexalith.Commons.StringEncoders;

// Basic encoding
string encoded = "Hello World!".ToRFC1123();
// Result: "Hello_20World_21"

// Unicode support
string chinese = "你好".ToRFC1123();
// Result: "_E4_BD_A0_E5_A5_BD"

// Email addresses
string email = "user@example.com".ToRFC1123();
// Result: "user_40example.com"

// Decoding
string original = "Hello_20World_21".FromRFC1123();
// Result: "Hello World!"

// Round-trip guarantee
string input = "Any string with émojis 🎉!";
string roundTrip = input.ToRFC1123().FromRFC1123();
Assert.Equal(input, roundTrip);  // Always true
```

### Use Cases

- **File system paths**: Generate safe filenames from user input
- **URL identifiers**: Create URL-safe slugs from arbitrary text
- **Message headers**: Encode values for protocols with character restrictions
- **Database keys**: Create compliant identifiers from any string

---

## Hexalith.Commons.UniqueIds

Unique identifier generation for different scenarios.

### DateTime-Based IDs

17-character identifiers based on UTC timestamp. Useful for sortable, human-readable IDs.

```csharp
using Hexalith.Commons.UniqueIds;

string id = UniqueIdHelper.GenerateDateTimeId();
// Example: "20240115143052789"
// Format: yyyyMMddHHmmssfff

// Thread-safe - automatically increments for same-millisecond calls
string id1 = UniqueIdHelper.GenerateDateTimeId();
string id2 = UniqueIdHelper.GenerateDateTimeId();
// id2 will be greater than id1 even if called in same millisecond
```

**Characteristics:**
- Length: 17 characters
- Format: `yyyyMMddHHmmssfff`
- Thread-safe with automatic increment
- Sortable chronologically
- One unique ID per millisecond maximum

### GUID-Based IDs

22-character URL-safe identifiers derived from GUIDs. Ideal for distributed systems.

```csharp
string id = UniqueIdHelper.GenerateUniqueStringId();
// Example: "gZOW2EgVrEq5SBJLegYcVA"
```

**Characteristics:**
- Length: 22 characters
- Characters: A-Z, a-z, 0-9, _, -
- URL-safe (no encoding needed)
- Globally unique (GUID-based)
- Suitable for distributed systems

### Comparison

| Feature | GenerateDateTimeId | GenerateUniqueStringId |
|---------|-------------------|----------------------|
| Length | 17 chars | 22 chars |
| Sortable | Yes (chronological) | No |
| Rate limit | 1 per millisecond | Unlimited |
| Distributed | No | Yes |
| Human readable | Yes (datetime) | No |

---

## Hexalith.Commons.Metadatas

Metadata structures for message tracking in distributed systems.

### Metadata Structure

```
Metadata
├── MessageMetadata
│   ├── Id          (string)    - Unique message identifier
│   ├── Name        (string)    - Message type name
│   ├── Version     (int)       - Message schema version
│   ├── CreatedDate (DateTimeOffset)
│   └── Domain      (DomainMetadata)
│       ├── Id      (string)    - Aggregate identifier
│       └── Name    (string)    - Aggregate type name
└── ContextMetadata
    ├── CorrelationId  (string) - Request correlation
    ├── UserId         (string) - User performing action
    ├── PartitionId    (string) - Partition for distribution
    ├── SessionId      (string) - User session
    ├── SequenceNumber (long)   - Message ordering
    ├── ReceivedDate   (DateTimeOffset)
    └── Scopes         (IEnumerable<string>)
```

### Usage Example

```csharp
using Hexalith.Commons.Metadatas;

// Create message metadata
var messageMetadata = new MessageMetadata(
    Id: UniqueIdHelper.GenerateUniqueStringId(),
    Name: "OrderCreated",
    Version: 1,
    Domain: new DomainMetadata(Id: "ORD-12345", Name: "Order"),
    CreatedDate: DateTimeOffset.UtcNow
);

// Create context metadata
var contextMetadata = new ContextMetadata(
    CorrelationId: correlationId,
    UserId: currentUser.Id,
    PartitionId: tenantId,
    SessionId: sessionId,
    SequenceNumber: 1,
    ReceivedDate: DateTimeOffset.UtcNow,
    Scopes: new[] { "orders", "write" }
);

// Combine into complete metadata
var metadata = new Metadata(messageMetadata, contextMetadata);

// Generate domain global identifier
string globalId = metadata.DomainGlobalId;
// Format: "{partitionId}-{aggregateName}-{aggregateId}"

// Logging-friendly representation
string logEntry = metadata.ToLogString();
```

### Use Cases

- **Event sourcing**: Track event origin and context
- **Message routing**: Route messages based on partition and domain
- **Audit trails**: Complete traceability of all operations
- **Correlation**: Link related messages across services
- **Ordering**: Maintain message sequence within partitions

---

## Quality Metrics

[![Coverity Scan Build Status](https://scan.coverity.com/projects/27051/badge.svg)](https://scan.coverity.com/projects/hexalith-commons)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/11d3f1af6b0f4d168552c2626d588294)](https://app.codacy.com/gh/Hexalith/Hexalith.Commons/dashboard)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)

---

## Building from Source

```bash
# Clone the repository
git clone https://github.com/Hexalith/Hexalith.Commons.git
cd Hexalith.Commons

# Build
dotnet build

# Run tests
dotnet test
```

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [NuGet Packages](https://www.nuget.org/packages?q=Hexalith.Commons)
- [Discord Community](https://discordapp.com/channels/1102166958918610994/1102166958918610997)
- [Issue Tracker](https://github.com/Hexalith/Hexalith.Commons/issues)
