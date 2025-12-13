# Hexalith.Commons.Configurations

> Type-safe configuration management with FluentValidation support for .NET applications.

[![NuGet](https://img.shields.io/nuget/v/Hexalith.Commons.Configurations.svg)](https://www.nuget.org/packages/Hexalith.Commons.Configurations)

---

## Overview

**Hexalith.Commons.Configurations** simplifies configuration management in .NET applications by providing:

- Type-safe settings with the `ISettings` interface
- Integration with Microsoft's Options pattern
- FluentValidation support for configuration validation
- Detailed exception handling for configuration errors

---

## Installation

```bash
dotnet add package Hexalith.Commons.Configurations
```

---

## Quick Start

### 1. Define Settings Class

```csharp
using Hexalith.Commons.Configurations;

public class EmailSettings : ISettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;

    // Configuration section name
    public static string ConfigurationName() => "Email";
}
```

### 2. Add Configuration

**appsettings.json:**
```json
{
  "Email": {
    "SmtpServer": "smtp.example.com",
    "Port": 587,
    "Username": "notifications@example.com",
    "Password": "secret",
    "UseSsl": true
  }
}
```

### 3. Register Settings

```csharp
// Program.cs
using Hexalith.Commons.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSettings<EmailSettings>(builder.Configuration);
```

### 4. Inject and Use

```csharp
public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.SmtpServer, _settings.Port);
        // ...
    }
}
```

---

## API Reference

### ISettings Interface

Contract for configuration classes. Requires implementing the configuration section name.

```csharp
public interface ISettings
{
    /// <summary>
    /// Returns the configuration section name.
    /// </summary>
    /// <returns>Section name in appsettings.json (e.g., "Database", "Email")</returns>
    static abstract string ConfigurationName();
}
```

**Implementation:**
```csharp
public class DatabaseSettings : ISettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;

    public static string ConfigurationName() => "Database";
}
```

**Nested sections:**
```csharp
public class AzureStorageSettings : ISettings
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountKey { get; set; } = string.Empty;

    // Nested configuration: Azure:Storage
    public static string ConfigurationName() => "Azure:Storage";
}
```

---

### SettingsHelper

Extension methods for retrieving settings from configuration.

#### GetSettings<TSettings>

```csharp
public static TSettings GetSettings<TSettings>(this IConfiguration configuration)
    where TSettings : class, ISettings, new();
```

Retrieves typed settings from the configuration section.

**Usage:**
```csharp
IConfiguration configuration = // ...

// Retrieve settings directly
EmailSettings settings = configuration.GetSettings<EmailSettings>();
```

---

### ConfigureSettings Extension

Registers settings with dependency injection and the Options pattern.

```csharp
// Register for IOptions<T> injection
services.ConfigureSettings<EmailSettings>(configuration);

// Now inject anywhere
public class MyService
{
    public MyService(IOptions<EmailSettings> options) { }
}
```

---

### SettingsException

Custom exception for configuration errors with detailed context.

```csharp
public class SettingsException : Exception
{
    public string SettingsName { get; }
    public string? PropertyName { get; }
}

public class SettingsException<TSettings> : SettingsException
    where TSettings : ISettings
{
    public static void ThrowIfUndefined(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? propertyName = null);
}
```

#### ThrowIfUndefined

Validates that required settings are not null or empty.

```csharp
public class OrderService
{
    private readonly string _apiKey;

    public OrderService(IOptions<ApiSettings> options)
    {
        // Throws SettingsException<ApiSettings> if ApiKey is null/empty
        SettingsException<ApiSettings>.ThrowIfUndefined(options.Value.ApiKey);
        _apiKey = options.Value.ApiKey;
    }
}
```

**Exception message format:**
```
The setting 'ApiKey' in configuration section 'Api' is required but was not provided.
Settings type: ApiSettings
```

---

### FluentValidation Integration

Validate settings using FluentValidation rules during application startup.

#### FluentValidateOptions<TOptions>

Implements `IValidateOptions<TOptions>` for FluentValidation integration.

```csharp
public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    public FluentValidateOptions(string? name, IServiceProvider provider);
    public ValidateOptionsResult Validate(string? name, TOptions options);
}
```

#### Complete Validation Example

**1. Define validator:**
```csharp
using FluentValidation;

public class EmailSettingsValidator : AbstractValidator<EmailSettings>
{
    public EmailSettingsValidator()
    {
        RuleFor(x => x.SmtpServer)
            .NotEmpty()
            .WithMessage("SMTP server is required");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.Username)
            .NotEmpty()
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Username must be a valid email when password is provided");
    }
}
```

**2. Register validator:**
```csharp
// Program.cs
builder.Services.ConfigureSettings<EmailSettings>(builder.Configuration);

// Register validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<EmailSettingsValidator>();

// Or register manually
builder.Services.AddScoped<IValidator<EmailSettings>, EmailSettingsValidator>();
```

**3. Validation triggers:**
- On first access to `IOptions<T>.Value`
- Throws `OptionsValidationException` if validation fails

---

## Advanced Patterns

### Multiple Configuration Sources

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load from multiple sources
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

// Settings resolve from all sources with override precedence
builder.Services.ConfigureSettings<DatabaseSettings>(builder.Configuration);
```

### Environment Variable Override

Settings can be overridden using environment variables:

```bash
# Override Email:SmtpServer
export Email__SmtpServer="smtp.production.com"

# Override nested settings Azure:Storage:AccountName
export Azure__Storage__AccountName="prodaccount"
```

### Conditional Settings

```csharp
public class FeatureSettings : ISettings
{
    public bool EnableNewCheckout { get; set; }
    public bool EnableBetaFeatures { get; set; }
    public int MaxConcurrentOperations { get; set; } = 10;

    public static string ConfigurationName() => "Features";
}

// Validator with conditional rules
public class FeatureSettingsValidator : AbstractValidator<FeatureSettings>
{
    public FeatureSettingsValidator()
    {
        RuleFor(x => x.MaxConcurrentOperations)
            .GreaterThanOrEqualTo(50)
            .When(x => x.EnableBetaFeatures)
            .WithMessage("Beta features require at least 50 concurrent operations");
    }
}
```

### Named Options

```csharp
// Register named options
services.Configure<DatabaseSettings>("Primary", configuration.GetSection("Database:Primary"));
services.Configure<DatabaseSettings>("Replica", configuration.GetSection("Database:Replica"));

// Inject named options
public class DataService
{
    public DataService(IOptionsSnapshot<DatabaseSettings> options)
    {
        var primary = options.Get("Primary");
        var replica = options.Get("Replica");
    }
}
```

---

## Dependencies

- **FluentValidation.DependencyInjectionExtensions**
- **Microsoft.Extensions.Configuration.Binder**
- **Microsoft.Extensions.Options**
- **Microsoft.Extensions.Options.ConfigurationExtensions**
- **Microsoft.Extensions.Options.DataAnnotations**

---

## License

MIT License - See [LICENSE](../../../LICENSE) for details.

---

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Main Documentation](../../../README.md)
- [NuGet Package](https://www.nuget.org/packages/Hexalith.Commons.Configurations)
