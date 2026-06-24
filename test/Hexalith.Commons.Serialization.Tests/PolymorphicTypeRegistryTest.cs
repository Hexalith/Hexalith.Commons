// <copyright file="PolymorphicTypeRegistryTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization.Tests;

using Shouldly;

/// <summary>
/// Verifies bounded explicit polymorphic type registration.
/// </summary>
public sealed class PolymorphicTypeRegistryTest
{
    /// <summary>
    /// Registries expose explicit name-to-type mappings without requiring marker interfaces or default constructors.
    /// </summary>
    [Fact]
    public void RegistryShouldResolveExplicitTypesWithoutMappableContractRequirements()
    {
        var registry = PolymorphicTypeRegistry.Create(
            [
                new PolymorphicTypeRegistration("Created", typeof(Created)),
                new PolymorphicTypeRegistration("MessageAppended", typeof(MessageAppended)),
            ]);

        registry.TryResolve("Created", out Type? created).ShouldBeTrue();
        created.ShouldBe(typeof(Created));

        registry.TryResolveExactThenSuffix("Hexalith.Tests.MessageAppended", out Type? appended).ShouldBeTrue();
        appended.ShouldBe(typeof(MessageAppended));
    }

    /// <summary>
    /// Duplicate discriminators fail at registration time.
    /// </summary>
    [Fact]
    public void RegistryShouldRejectDuplicateDiscriminators()
        => Should.Throw<InvalidOperationException>(() =>
            PolymorphicTypeRegistry.Create(
                [
                    new PolymorphicTypeRegistration("Created", typeof(Created)),
                    new PolymorphicTypeRegistration("Created", typeof(MessageAppended)),
                ]));

    /// <summary>
    /// Discriminators are bounded content-safe strings.
    /// </summary>
    /// <param name="discriminator">The unsafe discriminator to reject.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Message Appended")]
    [InlineData("Message$Appended")]
    public void RegistryShouldRejectUnsafeDiscriminators(string discriminator)
        => Should.Throw<ArgumentException>(() =>
            PolymorphicTypeRegistry.Create([new PolymorphicTypeRegistration(discriminator, typeof(Created))]));

    /// <summary>
    /// Discriminators over the bounded length are rejected.
    /// </summary>
    [Fact]
    public void RegistryShouldRejectOversizedDiscriminators()
        => Should.Throw<ArgumentException>(() =>
            PolymorphicTypeRegistry.Create(
                [new PolymorphicTypeRegistration(new string('A', PolymorphicTypeRegistry.MaxDiscriminatorLength + 1), typeof(Created))]));

    /// <summary>
    /// The registry does not resolve missing or blank discriminators.
    /// </summary>
    [Fact]
    public void RegistryShouldFailClosedForMissingDiscriminators()
    {
        var registry = PolymorphicTypeRegistry.FromTypeNames([typeof(Created)]);

        registry.TryResolve(null, out _).ShouldBeFalse();
        registry.TryResolve("Unknown", out _).ShouldBeFalse();
        registry.TryResolveExactThenSuffix("UnknownCreatedButNotSuffix", out _).ShouldBeFalse();
    }

    private sealed record Created(string Value);

    private sealed record MessageAppended(string Value);
}
