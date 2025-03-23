// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Exceptions;

using Hexalith.Commons.Configuration;

using Microsoft.Extensions.Options;

using Shouldly;

public class SettingsExceptionTest
{
    [Fact]
    public void DefinedSettingsInOptionsShouldNotThrowException()
    {
        IOptions<DummySettings> settings = Options.Create(new DummySettings() { Name = "hello world" });
        Should.NotThrow(() => SettingsException<DummySettings>.ThrowIfUndefined(settings.Value.Name));
    }

    [Fact]
    public void DefinedSettingsShouldNotThrowException()
    {
        DummySettings settings = new() { Name = "hello world" };
        Should.NotThrow(() => SettingsException<DummySettings>.ThrowIfUndefined(settings.Name));
    }

    [Fact]
    public void NullSettingsPropertyInOptionsShouldThrowException()
    {
        IOptions<DummySettings> options = Options.Create(new DummySettings());
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(options.Value.Name));

        ex.ParamName.ShouldBe("options.Value.Name");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Name");
    }

    [Fact]
    public void NullSettingsPropertyShouldThrowException()
    {
        DummySettings settings = new();
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(settings.Name));

        ex.ParamName.ShouldBe("settings.Name");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Name");
    }

    [Fact]
    public void NullSettingsSubPropertyShouldThrowException()
    {
        DummySettings settings = new() { SubConfig = new SubConfiguration() };
        SettingsException<DummySettings> ex = Should.Throw<SettingsException<DummySettings>>(
            () => SettingsException<DummySettings>.ThrowIfUndefined(settings.SubConfig.Hello));

        ex.ParamName.ShouldBe("settings.SubConfig.Hello");
        ex.Message.ShouldContain("Dummy");
        ex.Message.ShouldContain("Hello");
    }

    internal class DummySettings : ISettings
    {
        public string Name { get; set; } = string.Empty;

        public SubConfiguration? SubConfig { get; set; }

        public static string ConfigurationName() => "Dummy";
    }

    internal class SubConfiguration
    {
        public string Hello { get; set; } = string.Empty;
    }
}