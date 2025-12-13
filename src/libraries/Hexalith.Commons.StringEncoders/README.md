# Hexalith.Commons.StringEncoders

> Reversible string encoding for RFC1123-compliant contexts with full Unicode support.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.StringEncoders.svg)](https://www.nuget.org/packages/Hexalith.Commons.StringEncoders)

---

## Overview

**Hexalith.Commons.StringEncoders** provides a mechanism to encode arbitrary strings into an RFC1123-compliant format and decode them back to the original. This is essential when working with systems that have restricted character sets.

### Key Features

- **Lossless encoding**: Any string can be encoded and decoded without data loss
- **Unicode support**: Full UTF-8 support including emojis and international characters
- **RFC1123 compliant**: Output uses only allowed characters (A-Z, a-z, 0-9, -, .)
- **Reversible**: Perfect round-trip encoding/decoding

---

## Installation

```bash
dotnet add package Hexalith.Commons.StringEncoders
```

---

## Quick Start

```csharp
using Hexalith.Commons.StringEncoders;

// Encode any string to RFC1123 format
string encoded = "Hello World!".ToRFC1123();
// Result: "Hello_20World_21"

// Decode back to original
string decoded = encoded.FromRFC1123();
// Result: "Hello World!"
```

---

## Encoding Rules

### Allowed Characters (Unchanged)

| Characters | Description |
|------------|-------------|
| `A-Z` | Uppercase letters |
| `a-z` | Lowercase letters |
| `0-9` | Digits |
| `-` | Hyphen |
| `.` | Period |

### Escaped Characters

| Input | Output | Description |
|-------|--------|-------------|
| `_` | `__` | Underscore escapes to double underscore |
| ` ` (space) | `_20` | Space becomes `_20` (hex for 0x20) |
| `@` | `_40` | At sign becomes `_40` (hex for 0x40) |
| `!` | `_21` | Exclamation becomes `_21` (hex for 0x21) |
| Unicode | `_XX_YY...` | UTF-8 bytes as hex sequences |

### Encoding Algorithm

1. Scan each character in the input string
2. If character is allowed (A-Z, a-z, 0-9, -, .), keep as-is
3. If character is underscore (`_`), output `__`
4. Otherwise, convert character to UTF-8 bytes and output each byte as `_XX`

---

## API Reference

### ToRFC1123

Encodes a string to RFC1123-compliant format.

```csharp
public static string ToRFC1123(this string input)
```

**Parameters:**
- `input`: The string to encode

**Returns:**
- RFC1123-compliant string, or original if null/empty

**Example:**
```csharp
"user@example.com".ToRFC1123()      // "user_40example.com"
"file name.txt".ToRFC1123()          // "file_20name.txt"
"price_100".ToRFC1123()              // "price__100"
"".ToRFC1123()                       // ""
```

---

### FromRFC1123

Decodes an RFC1123-encoded string back to original.

```csharp
public static string FromRFC1123(this string input)
```

**Parameters:**
- `input`: The RFC1123-encoded string

**Returns:**
- Original decoded string, or input if null/empty

**Exceptions:**
- `FormatException`: Invalid escape sequence in input

**Example:**
```csharp
"user_40example.com".FromRFC1123()   // "user@example.com"
"file_20name.txt".FromRFC1123()      // "file name.txt"
"price__100".FromRFC1123()           // "price_100"
```

---

## Usage Examples

### File System Paths

Generate safe filenames from user input.

```csharp
string userInput = "Report Q1 2024 (Final).xlsx";
string safeName = userInput.ToRFC1123();
// Result: "Report_20Q1_202024_20_28Final_29.xlsx"

string filePath = Path.Combine(outputDir, safeName);
File.WriteAllBytes(filePath, data);

// Later, decode to display original name
string displayName = Path.GetFileName(filePath).FromRFC1123();
// Result: "Report Q1 2024 (Final).xlsx"
```

### URL-Safe Identifiers

Create URL-safe slugs from arbitrary text.

```csharp
string productName = "Café Espresso (Large)";
string urlSlug = productName.ToRFC1123().ToLower();
// Result: "caf_c3_a9_20espresso_20_28large_29"

string url = $"https://shop.example.com/products/{urlSlug}";
```

### Database Keys

Create compliant identifiers for systems with character restrictions.

```csharp
string tenantName = "Acme Corp & Partners";
string tenantKey = tenantName.ToRFC1123();
// Result: "Acme_20Corp_20_26_20Partners"

// Use as partition key, row key, etc.
var entity = new TableEntity(tenantKey, orderId);
```

### Message Headers

Encode values for protocols with character restrictions.

```csharp
string customHeader = "Données utilisateur: été 2024";
string encodedHeader = customHeader.ToRFC1123();
// Result: "Donn_C3_A9es_20utilisateur_3A_20_C3_A9t_C3_A9_202024"

httpClient.DefaultRequestHeaders.Add("X-Custom-Data", encodedHeader);

// Decode on receiving end
string original = Request.Headers["X-Custom-Data"].FromRFC1123();
```

### Unicode and Emoji Support

Full support for international characters and emojis.

```csharp
// Chinese characters
"你好世界".ToRFC1123()
// Result: "_E4_BD_A0_E5_A5_BD_E4_B8_96_E7_95_8C"

// Japanese
"こんにちは".ToRFC1123()
// Result: "_E3_81_93_E3_82_93_E3_81_AB_E3_81_A1_E3_81_AF"

// Emojis
"Hello 🌍!".ToRFC1123()
// Result: "Hello_20_F0_9F_8C_8D_21"

// All decode back perfectly
"_E4_BD_A0_E5_A5_BD_E4_B8_96_E7_95_8C".FromRFC1123()
// Result: "你好世界"
```

---

## Round-Trip Guarantee

The encoding is completely reversible for any string.

```csharp
string[] testStrings = new[]
{
    "Simple text",
    "With spaces and punctuation!",
    "user@domain.com",
    "path/to/file.txt",
    "Ümlauts and açcénts",
    "日本語テキスト",
    "Emoji: 🎉🚀💻",
    "Mixed: Hello 世界! @user_name",
    ""  // Empty string
};

foreach (var original in testStrings)
{
    string encoded = original.ToRFC1123();
    string decoded = encoded.FromRFC1123();
    Debug.Assert(original == decoded);  // Always true
}
```

---

## Error Handling

### Invalid Escape Sequences

`FromRFC1123` throws `FormatException` for invalid input.

```csharp
try
{
    // Invalid: underscore followed by non-hex character
    "hello_XY".FromRFC1123();
}
catch (FormatException ex)
{
    // "Invalid escape sequence at position 5"
}

try
{
    // Invalid: incomplete escape sequence
    "hello_2".FromRFC1123();
}
catch (FormatException ex)
{
    // "Incomplete escape sequence at position 5"
}
```

### Safe Decoding Pattern

```csharp
public static bool TryFromRFC1123(string input, out string? result)
{
    try
    {
        result = input.FromRFC1123();
        return true;
    }
    catch (FormatException)
    {
        result = null;
        return false;
    }
}

// Usage
if (TryFromRFC1123(encodedValue, out var decoded))
{
    Console.WriteLine($"Decoded: {decoded}");
}
else
{
    Console.WriteLine("Invalid RFC1123 encoded string");
}
```

---

## Performance Considerations

- **Memory efficient**: Uses `StringBuilder` and `MemoryStream` internally
- **No allocations for passthrough**: Characters not requiring encoding pass through directly
- **UTF-8 aware**: Properly handles multi-byte sequences

### Benchmarks (Approximate)

| Operation | String Length | Time |
|-----------|---------------|------|
| Encode (ASCII only) | 100 chars | ~1 μs |
| Encode (with escapes) | 100 chars | ~5 μs |
| Decode | 100 chars | ~3 μs |
| Unicode encode | 100 chars | ~10 μs |

---

## Related

- [RFC 1123](https://datatracker.ietf.org/doc/html/rfc1123) - Requirements for Internet Hosts
- [Hexalith.Commons](../Hexalith.Commons/README.md) - Core utilities including `IsRfc1123Compliant()`

---

## License

MIT License - See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons.StringEncoders)
