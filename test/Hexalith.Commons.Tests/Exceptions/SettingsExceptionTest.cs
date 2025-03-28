// <copyright file="SettingsExceptionTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Exceptions;

using Hexalith.Commons.Configuration;

using Microsoft.Extensions.Options;

using Shouldly;

/// <summary>
/// Contains unit tests for the <see cref="SettingsException{T}"/> class.
/// </summary>
public class SettingsExceptionTest
{
    /// <summary>
    /// Tests that defined settings in options do not throw an exception.
    /// </summary>
    [Fact]
    public void DefinedSettingsInOptionsShouldNotThrowException()
    {
        // Create options with valid settings (non-empty name)
        IOptions<DummySettings> settings = Options.Create(
            new DummySettings() { Name = "hello world" }
        );

        // Verify that no exception is thrown for defined settings
        Should.NotThrow(
            () => SettingsException<DummySettings>.ThrowIfUndefined(settings.Value.Name)
        );
    }

    /// <summary>
    /// Tests that defined settings do not throw an exception.
    /// </summary>
    [Fact]
    public void DefinedSettingsShouldNotThrowException()
    {
        DummySettings settings = new() { Name = "hello world" };
        Should.NotThrow(() => SettingsException<DummySettings>.ThrowIfUndefined(settings.Name));
    }

    /// <summary>
    /// Tests that null settings property in options throws an exception.
    /// </summary>
    [Fact]
    public void NullSettingsPropertyInOptionsShouldThrowException()
    {
        // Create options with default settings (empty name)
        IOptions<DummySettings> options = Options.Create(new DummySettings());

        // Verify that appropriate exception is thrown with correct parameter name and message
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(options.Value.Name)
        );

        // Assert the exception details
        ex.ParamName.ShouldBe("options.Value.Name");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Name");
    }

    /// <summary>
    /// Tests that null settings property throws an exception.
    /// </summary>
    [Fact]
    public void NullSettingsPropertyShouldThrowException()
    {
        DummySettings settings = new();
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(settings.Name)
        );

        ex.ParamName.ShouldBe("settings.Name");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Name");
    }

    /// <summary>
    /// Tests that null settings sub-property throws an exception.
    /// </summary>
    [Fact]
    public void NullSettingsSubPropertyShouldThrowException()
    {
        // Create settings with empty sub-configuration
        DummySettings settings = new() { SubConfig = new SubConfiguration() };

        // Verify that appropriate exception is thrown for undefined sub-property
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(settings.SubConfig.Hello)
        );

        // Assert the exception details
        ex.ParamName.ShouldBe("settings.SubConfig.Hello");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Hello");
    }

    /// <summary>
    /// Represents dummy settings for testing purposes.
    /// </summary>
    internal class DummySettings : ISettings
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sub-configuration.
        /// </summary>
        public SubConfiguration? SubConfig { get; set; }

        /// <summary>
        /// Gets the configuration name.
        /// </summary>
        /// <returns>The configuration name.</returns>
        public static string ConfigurationName() => "Dummy";
    }

    /// <summary>
    /// Represents a sub-configuration for testing purposes.
    /// </summary>
    internal class SubConfiguration
    {
        /// <summary>
        /// Gets or sets the hello property.
        /// </summary>
        public string Hello { get; set; } = string.Empty;
    }
}