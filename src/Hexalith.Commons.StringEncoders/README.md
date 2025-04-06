# Hexalith.Commons.StringEncoders

## Overview

Hexalith.Commons.StringEncoders is a .NET library that provides utilities for encoding and decoding strings to be compliant with RFC 1123 format. This library ensures that strings can be used in contexts with restricted character sets while preserving the original data for later reversal.

The main purpose of this library is to provide a reversible mechanism for encoding arbitrary strings using a restricted character set often associated with RFC 1123 contexts (like headers or identifiers).

## Features

- Encode strings to be RFC 1123 compliant (using only A-Z, a-z, 0-9, '-', '.')
- Decode RFC 1123 compliant strings back to their original form
- Lossless round-trip encoding/decoding for any string, including those with special characters or Unicode
- Proper handling of UTF-8 encoding for international characters

## Installation

### Package Manager Console

```
Install-Package Hexalith.Commons.StringEncoders
```

### .NET CLI

```
dotnet add package Hexalith.Commons.StringEncoders
```

## Usage

### Basic Usage

```csharp
using Hexalith.Commons.StringEncoders;

// Encoding a string to be RFC 1123 compliant
string original = "Hello World! 你好世界";
string encoded = original.ToRFC1123();
// Result: "Hello_20World_21_20_E4_BD_A0_E5_A5_BD_E4_B8_96_E7_95_8C"

// Decoding back to the original string
string decoded = encoded.FromRFC1123();
// Result: "Hello World! 你好世界"
```

### Handling Special Characters

The library automatically escapes characters that are not in the allowed set:
- Spaces become `_20`
- Underscores are escaped as `__` (double underscore)
- Other special characters and Unicode are escaped as `_XX` where XX is the hex representation of the UTF-8 byte

```csharp
string withSpecialChars = "user@example.com";
string encoded = withSpecialChars.ToRFC1123();
// Result: "user_40example.com"

string decoded = encoded.FromRFC1123();
// Result: "user@example.com"
```

## API Reference

### ToRFC1123

```csharp
public static string ToRFC1123(this string input)
```

Converts a string into a representation where characters not in the allowed set (A-Z, a-z, 0-9, '-', '.') are escaped using a reversible mechanism (`_XX` for UTF-8 bytes). The escape character itself (`_`) is escaped as `__`.

**Parameters:**
- `input`: The string to convert.

**Returns:**
- The escaped string, or the original string if null or empty.

### FromRFC1123

```csharp
public static string FromRFC1123(this string input)
```

Converts a string previously escaped using ToRFC1123 back to its original form. It reverses the `_XX` (UTF-8 byte hex) and `__` (literal underscore) escaping.

**Parameters:**
- `input`: The escaped string.

**Returns:**
- The original, decoded string, or the original string if null or empty.

**Exceptions:**
- `FormatException`: Thrown if the input string contains invalid escape sequences.

## Use Cases

- Creating file names or paths from arbitrary strings
- Generating URL-safe identifiers
- Storing data in systems with character restrictions
- Creating headers or identifiers that need to be RFC 1123 compliant

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file in the project root for details.

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Project Website](https://github.com/Hexalith/Hexalith.Commons)