# Hexalith.Commons.Configurations

## Overview

Hexalith.Commons.Configurations is a .NET library that provides utilities for handling configuration settings in .NET applications. It simplifies the process of retrieving, validating, and managing configuration settings using Microsoft's configuration and options patterns.

## Features

- Interface-based configuration settings with `ISettings`
- Helper methods for retrieving settings from configuration
- FluentValidation integration for validating configuration options
- Exception handling for configuration errors
- Support for Microsoft's Options pattern

## Installation

### Package Manager Console

```
Install-Package Hexalith.Commons.Configurations
```

### .NET CLI

```
dotnet add package Hexalith.Commons.Configurations
```

## Usage

### Defining Settings

```csharp
using Hexalith.Commons.Configurations;

public class MyAppSettings : ISettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    
    public static string ConfigurationName() => "MyApp";
}
```

### Retrieving Settings

```csharp
using Hexalith.Commons.Configurations;
using Microsoft.Extensions.Configuration;

// Get configuration from appsettings.json
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Get settings using the helper method
MyAppSettings settings = configuration.GetSettings<MyAppSettings>();

// Use the settings
Console.WriteLine($"Connection String: {settings.ConnectionString}");
Console.WriteLine($"Timeout: {settings.Timeout}");
```

### Validating Settings with FluentValidation

```csharp
using FluentValidation;
using Hexalith.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Create a validator for your settings
public class MyAppSettingsValidator : AbstractValidator<MyAppSettings>
{
    public MyAppSettingsValidator()
    {
        RuleFor(x => x.ConnectionString).NotEmpty();
        RuleFor(x => x.Timeout).GreaterThan(0);
    }
}

// Register services
var services = new ServiceCollection();

// Register configuration
services.AddSingleton<IConfiguration>(configuration);

// Register validator
services.AddSingleton<IValidator<MyAppSettings>, MyAppSettingsValidator>();

// Register settings with validation
services.AddOptions<MyAppSettings>()
    .Bind(configuration.GetSection(MyAppSettings.ConfigurationName()))
    .ValidateFluentValidation()
    .ValidateOnStart();

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get validated options
var options = serviceProvider.GetRequiredService<IOptions<MyAppSettings>>().Value;
```

## API Reference

### ISettings Interface

```csharp
public interface ISettings
{
    static abstract string ConfigurationName();
}
```

The `ISettings` interface defines a contract for configuration settings classes. It requires implementing a static `ConfigurationName()` method that returns the name of the configuration section.

### SettingsHelper Class

```csharp
public static class SettingsHelper
{
    public static TSettings GetSettings<TSettings>(this IConfiguration configuration)
        where TSettings : class, ISettings, new();
}
```

The `SettingsHelper` class provides extension methods for retrieving settings from configuration.

### FluentValidateOptions Class

```csharp
public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    public FluentValidateOptions(string? name, IServiceProvider provider);
    public ValidateOptionsResult Validate(string? name, TOptions options);
}
```

The `FluentValidateOptions` class implements `IValidateOptions<TOptions>` to provide FluentValidation integration for the Options pattern.

## Dependencies

- FluentValidation.DependencyInjectionExtensions
- Microsoft.Extensions.Configuration.Binder
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options
- Microsoft.Extensions.Options.ConfigurationExtensions
- Microsoft.Extensions.Options.DataAnnotations

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file in the project root for details.

## Links

- [GitHub Repository](https://github.com/Hexalith/Hexalith.Commons)
- [Project Website](https://github.com/Hexalith/Hexalith.Commons)
