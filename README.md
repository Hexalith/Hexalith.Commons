# Hexalith.Commons

A comprehensive .NET utility library providing common functionality for Hexalith projects.

[![License: MIT](https://img.shields.io/github/license/hexalith/hexalith.commons)](https://github.com/hexalith/hexalith/blob/main/LICENSE)
[![Discord](https://img.shields.io/discord/1063152441819942922?label=Discord&logo=discord&logoColor=white&color=d82679)](https://discordapp.com/channels/1102166958918610994/1102166958918610997)

[![Coverity Scan Build Status](https://scan.coverity.com/projects/27051/badge.svg)](https://scan.coverity.com/projects/hexalith-commons)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/11d3f1af6b0f4d168552c2626d588294)](https://app.codacy.com/gh/Hexalith/Hexalith.Commons/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.Commons&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.Commons)

## Build Status

Stable (version tag):

[![Build status](https://github.com/Hexalith/Hexalith.Commons/actions/workflows/packages.yml/badge.svg)](https://github.com/Hexalith/Hexalith.Commons/actions)
[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.svg)](https://www.nuget.org/packages/Hexalith.Commons)
[![Preview](https://img.shields.io/github/v/packages/Hexalith/Hexalith.Commons/Hexalith.Commons?style=flat-square)](https://github.com/Hexalith/Hexalith.Commons/pkgs/nuget/Hexalith.Commons)

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

- ISettings interface for strongly-typed settings
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

```shell
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

### Configuration utilities

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

```shell
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
