# Hexalith.Commons

> Core utility library providing essential helpers for .NET applications.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.svg)](https://www.nuget.org/packages/Hexalith.Commons)

---

## Overview

**Hexalith.Commons** is the foundational utility library of the Hexalith framework.
It provides helpers, extensions, and patterns for common programming tasks
including string manipulation, error handling, reflection, and object comparison.

---

## Installation

```bash
dotnet add package Hexalith.Commons
```

---

## Namespaces

| Namespace | Purpose |
|-----------|---------|
| `Hexalith.Extensions.Helpers` | String manipulation and conversion utilities |
| `Hexalith.Commons.Errors` | Structured error handling and railway-oriented programming |
| `Hexalith.Commons.Objects` | Object equality, description, and comparison |
| `Hexalith.Commons.Reflections` | Type discovery and mapping |
| `Hexalith.Commons.Dates` | Date and timezone utilities |
| `Hexalith.Commons.Assemblies` | Assembly version information |
| `Hexalith.Commons.Helpers` | Logging extensions |
| `Hexalith.Commons.Maths` | Mathematical utilities |

---

## String Utilities

**Namespace:** `Hexalith.Extensions.Helpers`

### StringHelper

Comprehensive string manipulation and conversion utilities.

#### Named Placeholder Formatting

```csharp
using Hexalith.Extensions.Helpers;

string template = "Order {orderId} for {customer} is {status}";
var values = new Dictionary<string, object>
{
    ["orderId"] = "ORD-001",
    ["customer"] = "John Doe",
    ["status"] = "shipped"
};

string result = template.FormatWithNamedPlaceholders(values);
// Result: "Order ORD-001 for John Doe is shipped"
```

#### Culture-Invariant Conversions

```csharp
// String to number (culture-invariant)
int intValue = "42".ToInteger();
long longValue = "9999999999".ToLong();
decimal decimalValue = "123.45".ToDecimal();
double doubleValue = "3.14159".ToDouble();

// Number to string (culture-invariant)
string str = 123.45m.ToInvariantString();
```

#### RFC1123 Compliance Checking

```csharp
// Validate hostnames
bool valid = "my-server.example.com".IsRfc1123Compliant();  // true
bool invalid = "my_server".IsRfc1123Compliant();            // false (underscore not allowed)
bool invalid2 = "-invalid".IsRfc1123Compliant();            // false (starts with hyphen)
```

#### Placeholder Conversion

```csharp
// Convert named placeholders to indexed format
string template = "Hello {name}, your {item} is ready";
string indexed = template.ReplacePlaceholderNamesByIndex();
// Result: "Hello {0}, your {1} is ready"
```

---

## Error Handling

**Namespace:** `Hexalith.Commons.Errors`

### ApplicationError

Structured error representation with template-based formatting.

```csharp
using Hexalith.Commons.Errors;

var error = new ApplicationError
{
    Title = "Validation Error",
    Category = ErrorCategory.Validation,
    Type = "https://errors.example.com/validation",
    Detail = "The {fieldName} field must be between {min} and {max}",
    TechnicalDetail = "Received value: {value}",
    Arguments = new object[] { "Age", 0, 120, -5 }
};

// Get formatted messages
string userMessage = error.GetDetailMessage();
// Result: "The Age field must be between 0 and 120"

string techMessage = error.GetTechnicalMessage();
// Result: "Received value: -5"
```

### ErrorCategory Enum

```csharp
public enum ErrorCategory
{
    Technical,      // Infrastructure/system errors
    Business,       // Business rule violations
    Validation,     // Input validation failures
    Security,       // Authentication/authorization errors
    NotFound        // Resource not found
}
```

### ValueOrError<T> (Railway-Oriented Programming)

Pattern for handling operations that can fail without exceptions.

```csharp
using Hexalith.Commons.Errors;

// Success case
ValueOrError<User> success = new User { Id = 1, Name = "John" };

// Error case
ValueOrError<User> failure = new ApplicationError
{
    Title = "User Not Found",
    Category = ErrorCategory.NotFound
};

// Usage
public async Task<ValueOrError<Order>> CreateOrderAsync(OrderRequest request)
{
    var userResult = await GetUserAsync(request.UserId);
    if (userResult.HasError)
        return userResult.Error;  // Propagate error

    var user = userResult.Value;
    // Continue with order creation...
    return new Order { /* ... */ };
}

// Consuming the result
var result = await CreateOrderAsync(request);
if (result.HasError)
{
    logger.LogError(result.Error.GetDetailMessage());
    return BadRequest(result.Error);
}

var order = result.Value;
```

### ApplicationErrorException

Wrap ApplicationError in an exception when needed.

```csharp
var error = new ApplicationError { Title = "Critical Error" };
throw new ApplicationErrorException(error);
```

### ExceptionHelper

Extension methods for exception handling.

```csharp
try { /* ... */ }
catch (Exception ex)
{
    string fullMessage = ex.FullMessage();
    // Returns complete message including all inner exceptions
}
```

---

## Object Utilities

**Namespace:** `Hexalith.Commons.Objects`

### EquatableHelper

Deep equality comparison supporting nested objects, collections, and dictionaries.

```csharp
using Hexalith.Commons.Objects;

// Compare any two objects
bool equal = EquatableHelper.AreSame(obj1, obj2);

// Compare collections
var list1 = new List<int> { 1, 2, 3 };
var list2 = new List<int> { 1, 2, 3 };
bool equalLists = EquatableHelper.AreSameEnumeration(list1, list2);  // true

// Compare dictionaries
var dict1 = new Dictionary<string, int> { ["a"] = 1 };
var dict2 = new Dictionary<string, int> { ["a"] = 1 };
bool equalDicts = EquatableHelper.AreSameDictionary(dict1, dict2);  // true
```

### IEquatableObject Interface

Implement custom equality based on specific components.

```csharp
using Hexalith.Commons.Objects;

public class OrderLine : IEquatableObject
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return Quantity;
        yield return UnitPrice;
    }
}

// Objects are equal if all components match
var line1 = new OrderLine { ProductId = "P1", Quantity = 2, UnitPrice = 10.00m };
var line2 = new OrderLine { ProductId = "P1", Quantity = 2, UnitPrice = 10.00m };
bool equal = EquatableHelper.AreSame(line1, line2);  // true
```

### ObjectDescriptionHelper

Extract metadata from types using attributes.

```csharp
using Hexalith.Commons.Objects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

[Display(Name = "Customer Order", Description = "Represents a customer purchase")]
public class Order { }

var (name, displayName, description) = ObjectDescriptionHelper.Describe(typeof(Order));
// name: "Order"
// displayName: "Customer Order"
// description: "Represents a customer purchase"

// Property description
[Display(Name = "Order Total")]
[Description("The sum of all line items")]
[Required]
[DefaultValue(0)]
public decimal Total { get; set; }

var propInfo = ObjectDescriptionHelper.Describe(typeof(Order).GetProperty("Total")!);
```

### ObjectDifferenceHelper

Find differences between object instances.

```csharp
var order1 = new Order { Id = 1, Status = "Pending" };
var order2 = new Order { Id = 1, Status = "Shipped" };

var differences = ObjectDifferenceHelper.GetDifferences(order1, order2);
// Returns list of property differences
```

### ExampleHelper

Generate example values for testing.

```csharp
using Hexalith.Commons.Objects;

// Generate example values for a type
var example = ExampleHelper.GetExample<Order>();
```

---

## Reflection Utilities

**Namespace:** `Hexalith.Commons.Reflections`

### ReflectionHelper

Discover and instantiate types at runtime.

```csharp
using Hexalith.Commons.Reflections;

// Find all concrete implementations of an interface
IEnumerable<Type> handlerTypes = ReflectionHelper.GetInstantiableTypesOf<ICommandHandler>();

// Find and instantiate all implementations
IEnumerable<ICommandHandler> handlers = ReflectionHelper.GetInstantiableObjectsOf<ICommandHandler>();

// Exclude specific types
var types = ReflectionHelper.GetInstantiableTypesOf<IService>(
    excludeTypes: new[] { typeof(MockService), typeof(TestService) }
);
```

### TypeMapper

Map types to string names and vice versa.

```csharp
using Hexalith.Commons.Reflections;

var mapper = new TypeMapper();

// Register type mappings
mapper.Register<OrderCreatedEvent>("OrderCreated");
mapper.Register<OrderShippedEvent>("OrderShipped");

// Resolve type from name
Type eventType = mapper.GetType("OrderCreated");

// Get name from type
string name = mapper.GetName(typeof(OrderCreatedEvent));
```

### IMappableType Interface

Allow types to define their own mapping names.

```csharp
public class OrderCreatedEvent : IMappableType
{
    public static string TypeMapName => "OrderCreated";
}
```

### NameTypeMapper<TMappable>

Generic mapper for types implementing IMappableType.

```csharp
var mapper = new NameTypeMapper<IEvent>();
// Automatically discovers all IEvent implementations with IMappableType
```

---

## Date Utilities

**Namespace:** `Hexalith.Commons.Dates`

### DateHelper

Timezone-aware date operations.

```csharp
using Hexalith.Commons.Dates;

// Convert DateOnly to DateTimeOffset with timezone offset
DateOnly date = new(2024, 6, 15);
TimeSpan estOffset = TimeSpan.FromHours(-5);
DateTimeOffset localTime = DateHelper.ToLocalTime(date, estOffset);

// Convert to UTC (GMT+0)
DateTimeOffset utcTime = DateHelper.ToUniversalTime(date);

// Calculate wait time between dates
DateTimeOffset target = DateTimeOffset.Now.AddHours(2);
TimeSpan waitTime = DateHelper.WaitTime(target);
// Returns TimeSpan until target time
```

### Month Enum

Month enumeration for business logic.

```csharp
using Hexalith.Commons.Dates;

Month currentMonth = Month.June;
```

---

## Assembly Utilities

**Namespace:** `Hexalith.Commons.Assemblies`

### VersionHelper

Retrieve version information from assemblies.

```csharp
using Hexalith.Commons.Assemblies;

// Get entry assembly product version
string? version = VersionHelper.EntryProductVersion();
// Example: "1.2.3"

// Get version from file path
string? fileVersion = VersionHelper.FileProductVersion("/path/to/assembly.dll");

// Extension methods for Assembly and Type
string? assemblyVersion = typeof(MyClass).Assembly.GetAssemblyVersion();
string? typeVersion = typeof(MyClass).GetAssemblyVersion();
```

---

## Logging Utilities

**Namespace:** `Hexalith.Commons.Helpers`

### LoggerHelper

Structured logging extensions for ApplicationError.

```csharp
using Hexalith.Commons.Helpers;
using Microsoft.Extensions.Logging;

public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public void ProcessOrder(Order order)
    {
        var error = new ApplicationError
        {
            Title = "Order Processing Failed",
            Detail = "Failed to process order {orderId}",
            Arguments = new object[] { order.Id }
        };

        // Log with structured data
        _logger.LogApplicationError(error);
        // Logs title, detail, technical detail, and nested errors
    }
}
```

---

## Mathematical Utilities

**Namespace:** `Hexalith.Commons.Maths`

### FibonacciSequence

Fibonacci number generation.

```csharp
using Hexalith.Commons.Maths;

// Generate Fibonacci numbers
var fibonacci = FibonacciSequence.Generate(10);
// Result: [0, 1, 1, 2, 3, 5, 8, 13, 21, 34]
```

---

## Dependencies

- **CompareNETObjects**: Deep object comparison
- **FluentValidation**: Validation framework
- **Humanizer**: String humanization
- **Microsoft.Extensions.Logging.Abstractions**: Logging interfaces
- **Microsoft.Extensions.Options.ConfigurationExtensions**: Options pattern

---

## License

MIT License - See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons)
