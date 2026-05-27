# Hexalith.Commons Libraries

> Source code for the Hexalith.Commons NuGet packages.

---

## Package Overview

| Package | Description |
|---------|-------------|
| [Hexalith.Commons](./Hexalith.Commons/) | Core utilities |
| [Hexalith.Commons.Configurations](./Hexalith.Commons.Configurations/) | Type-safe settings |
| [Hexalith.Commons.StringEncoders](./Hexalith.Commons.StringEncoders/) | RFC1123 encoding |
| [Hexalith.Commons.UniqueIds](./Hexalith.Commons.UniqueIds/) | ID generation |
| [Hexalith.Commons.Metadatas](./Hexalith.Commons.Metadatas/) | Message metadata |

---

## Hexalith.Commons

**Core utility library** providing foundational helpers for .NET applications.

### Key Namespaces

| Namespace | Purpose |
|-----------|---------|
| `Hexalith.Extensions.Helpers` | String manipulation, culture-invariant conversions |
| `Hexalith.Commons.Errors` | Structured errors, `ValueOrError<T>` railway pattern |
| `Hexalith.Commons.Objects` | Deep equality, object description, difference detection |
| `Hexalith.Commons.Reflections` | Type discovery, type-name mapping |
| `Hexalith.Commons.Dates` | Timezone-aware date operations |
| `Hexalith.Commons.Assemblies` | Version information retrieval |
| `Hexalith.Commons.Helpers` | Structured logging for errors |

### Quick Example

```csharp
using Hexalith.Extensions.Helpers;
using Hexalith.Commons.Errors;
using Hexalith.Commons.Objects;

// String utilities
string result = "Hello {name}".FormatWithNamedPlaceholders(
    new Dictionary<string, object> { ["name"] = "World" }
);

// Railway-oriented error handling
ValueOrError<User> user = await GetUserAsync(id);
if (user.HasError)
    return BadRequest(user.Error.GetDetailMessage());

// Deep equality
bool equal = EquatableHelper.AreSame(obj1, obj2);
```

[Full Documentation](./Hexalith.Commons/README.md)

---

## Hexalith.Commons.Configurations

**Configuration management** with Microsoft Options pattern and FluentValidation.

### Features

- `ISettings` interface for type-safe configuration
- `ConfigureSettings<T>` extension for DI registration
- `SettingsException<T>` for validation with context
- `FluentValidateOptions<T>` for validation integration

### Quick Example

```csharp
using Hexalith.Commons.Configurations;

// Define settings
public class ApiSettings : ISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public static string ConfigurationName() => "Api";
}

// Register
builder.Services.ConfigureSettings<ApiSettings>(builder.Configuration);

// Validate
SettingsException<ApiSettings>.ThrowIfUndefined(settings.ApiKey);
```

[Full Documentation](./Hexalith.Commons.Configurations/README.md)

---

## Hexalith.Commons.StringEncoders

**RFC1123 string encoding** for restricted character contexts.

### Features

- `ToRFC1123()` - Encode any string to RFC1123 format
- `FromRFC1123()` - Decode back to original
- Full Unicode and emoji support
- Lossless round-trip guarantee

### Quick Example

```csharp
using Hexalith.Commons.StringEncoders;

string encoded = "Hello World!".ToRFC1123();  // "Hello_20World_21"
string decoded = encoded.FromRFC1123();        // "Hello World!"

// Unicode support
string chinese = "你好".ToRFC1123();  // "_E4_BD_A0_E5_A5_BD"
```

[Full Documentation](./Hexalith.Commons.StringEncoders/README.md)

---

## Hexalith.Commons.UniqueIds

**Unique identifier generation** for different scenarios.

### Methods

| Method | Format | Length | Use Case |
|--------|--------|--------|----------|
| `GenerateDateTimeId()` | `yyyyMMddHHmmssfff` | 17 | Sortable, human-readable |
| `GenerateUniqueStringId()` | Base64 URL-safe | 22 | Distributed systems |

### Quick Example

```csharp
using Hexalith.Commons.UniqueIds;

// Sortable timestamp ID
string timeId = UniqueIdHelper.GenerateDateTimeId();
// "20240615143052789"

// Distributed-safe GUID ID
string uniqueId = UniqueIdHelper.GenerateUniqueStringId();
// "gZOW2EgVrEq5SBJLegYcVA"
```

[Full Documentation](./Hexalith.Commons.UniqueIds/README.md)

---

## Hexalith.Commons.Metadatas

**Message metadata** for tracking and correlation in distributed systems.

### Records

- `DomainMetadata` - Aggregate identification (Id, Name)
- `MessageMetadata` - Message info (Id, Name, Version, Domain, CreatedDate)
- `ContextMetadata` - Execution context (CorrelationId, UserId, PartitionId, etc.)
- `Metadata` - Composite with helper methods

### Quick Example

```csharp
using Hexalith.Commons.Metadatas;

var metadata = new Metadata(
    Message: new MessageMetadata(
        Id: UniqueIdHelper.GenerateUniqueStringId(),
        Name: "OrderCreated",
        Version: 1,
        Domain: new DomainMetadata("ORD-123", "Order"),
        CreatedDate: DateTimeOffset.UtcNow
    ),
    Context: new ContextMetadata(
        CorrelationId: correlationId,
        UserId: userId,
        PartitionId: tenantId,
        ReceivedDate: DateTimeOffset.UtcNow,
        SequenceNumber: 1,
        SessionId: null,
        Scopes: null
    )
);

string globalId = metadata.DomainGlobalId;
// "tenantId-Order-ORD-123"
```

[Full Documentation](./Hexalith.Commons.Metadatas/README.md)

---

## Tests

Unit tests are located in [../test/Hexalith.Commons.Tests/](../test/Hexalith.Commons.Tests/).

Run tests:
```bash
dotnet test
```

---

## Building

```bash
# Build all libraries
dotnet build

# Build specific library
dotnet build src/libraries/Hexalith.Commons/Hexalith.Commons.csproj
```

---

## License

MIT License - See [LICENSE](../../LICENSE) for details.
