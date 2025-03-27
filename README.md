# Hexalith.Commons

A comprehensive .NET utility library providing common functionality for Hexalith projects.

## Overview

Hexalith.Commons is a collection of utility classes, helpers, and extensions designed to support development across Hexalith projects. This library provides consistent and reusable components for handling common programming tasks.

## Features

The library is organized into several key utility areas:

### Assemblies
Utilities for working with .NET assemblies and reflection-based operations.

### Common
General purpose utilities and helpers that don't fit into specific categories.

### Configuration
Tools and extensions for managing application configuration:
- `ISettings` interface for strongly-typed settings
- Configuration validation using FluentValidation
- Helper methods for registering and validating configuration
- Settings exception handling

### Dates
Date and time manipulation utilities.

### Errors
Error handling, custom exceptions, and error management utilities.

### Loggings
Extensions and helpers for consistent logging across applications.

### Maths
Mathematical utility functions and extensions.

### Objects
Object manipulation, comparison, and conversion utilities.

### Reflections
Reflection-based utilities for working with types, properties, and methods at runtime.

### Strings
String manipulation utilities:
- Format strings with named placeholders
- RFC1123 compliance checking
- Culture-invariant string conversions
- String to number conversions with proper culture handling

### Tests
Testing helpers and utilities to simplify unit and integration tests.

## Getting Started

### Prerequisites
- .NET 8.0 or higher

### Installation

You can add this library to your project using NuGet:

```
dotnet add package Hexalith.Commons
```

## Usage Examples

### String Utilities

```csharp
using Hexalith.Extensions.Helpers;

// Convert a string to an integer using invariant culture
string numberStr = "42";
int number = numberStr.ToInteger();

// Check if a hostname is RFC 1123 compliant
bool isValid = "valid-host.example.com".IsRfc1123Compliant();
```

### Configuration

```csharp
// Define settings class
public class MySettings : ISettings
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public static string ConfigurationName() => "Me";    
}

// In startup code
services.ConfigureSettings<MySettings>(configuration);

// In a service
public class MyService
{
  private readonly IOptions<MySettings> _settings;

  public MyService(IOptions<MySettings> settings)
  {
    _settings = settings;
  }
}
```

## Building and Testing

The project uses standard .NET build tools and practices:

```
dotnet build
dotnet test
```

## CI/CD

The repository includes GitHub Actions workflows for:
- Building and testing the code
- Publishing NuGet packages
- Version management

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome. Please feel free to submit a Pull Request.
