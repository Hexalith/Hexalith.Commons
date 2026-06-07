# Hexalith Commons Source Code

This directory contains the source code for the Hexalith Commons libraries. These libraries provide utility functionality for Hexalith projects.

## Projects

### [Hexalith.Commons](./Hexalith.Commons/README.md)

A comprehensive utility library providing common functionality across Hexalith projects. Contains utilities for:

- Assembly management
- Configuration handling
- Date manipulation
- Error handling
- Logging
- Math operations
- Object manipulation
- Reflection tools
- String utilities
- And more

### [Hexalith.Commons.Configurations](./Hexalith.Commons.Configurations/README.md)

A library for handling configuration settings in .NET applications:

- Interface-based configuration settings with `ISettings`
- Helper methods for retrieving settings from configuration
- FluentValidation integration for validating configuration options
- Exception handling for configuration errors
- Support for Microsoft's Options pattern

### [Hexalith.Commons.StringEncoders](./Hexalith.Commons.StringEncoders/README.md)

A library for encoding and decoding strings to be compliant with RFC 1123 format:

- Encode strings to be RFC 1123 compliant (using only A-Z, a-z, 0-9, '-', '.')
- Decode RFC 1123 compliant strings back to their original form
- Lossless round-trip encoding/decoding for any string, including those with special characters or Unicode
- Proper handling of UTF-8 encoding for international characters

### [Hexalith.Commons.UniqueIds](./Hexalith.Commons.UniqueIds/README.md)

A lightweight library for generating unique identifiers in various formats:

- Date/Time-based IDs (17 characters)
- GUID-based IDs (22 characters, Base64 URL-safe)

## Test Project

The tests for these libraries can be found in the [test/Hexalith.Commons.Tests](../test/Hexalith.Commons.Tests/) directory.
