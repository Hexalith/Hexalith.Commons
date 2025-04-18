# Hexalith.Commons.UniqueIds

A lightweight C# library for generating unique identifiers in various formats. Part of the Hexalith framework.

## Features

- **Date/Time-based IDs**: Generate unique 17-character IDs based on the current UTC time
- **GUID-based IDs**: Generate unique 22-character Base64 URL-safe IDs

## Installation

```bash
dotnet add package Hexalith.Commons.UniqueIds
```

## Usage

```csharp
using Hexalith.Commons.UniqueIds;

// Generate a unique ID based on current UTC date/time (format: yyyyMMddHHmmssfff)
string dateTimeId = UniqueIdHelper.GenerateDateTimeId();
// Example: "20240401152237385"

// Generate a unique 22-character ID based on a GUID
string uniqueId = UniqueIdHelper.GenerateUniqueStringId();
// Example: "gZOW2EgVrEq5SBJLegYcVA"
```

## Details

### `GenerateDateTimeId()`

Generates a unique 17-character ID based on UTC date/time with format "yyyyMMddHHmmssfff". 
This method ensures uniqueness even when called multiple times within the same millisecond by incrementing the time if necessary.

Caution : Only one ID can be generated per millisecond.

### `GenerateUniqueStringId()`

Generates a unique 22-character ID string derived from a GUID encoded in Base64 URL-safe format.
This produces shorter, URL-friendly unique identifiers compared to standard GUIDs.

## Github Project

[Hexalith.Commons](https://github.com/Hexalith/Hexalith.Commons) - A comprehensive .NET utility library providing common functionality for Hexalith projects.
