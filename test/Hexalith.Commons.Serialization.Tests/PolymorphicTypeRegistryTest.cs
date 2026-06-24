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

    /// <summary>
    /// Exact-then-suffix resolution must prefer an exact ordinal match over a registered key that is merely a
    /// suffix of the requested discriminator. This pins the load-bearing AC-5 invariant: a discriminator that is
    /// simultaneously an exact key and the suffix of a longer name resolves to the exact registration, while a
    /// qualified name that only ends with a shorter key still falls through to the suffix match.
    /// </summary>
    [Fact]
    public void RegistryShouldPreferExactMatchOverSuffixMatch()
    {
        var registry = PolymorphicTypeRegistry.Create(
            [
                new PolymorphicTypeRegistration("Appended", typeof(Appended)),
                new PolymorphicTypeRegistration("MessageAppended", typeof(MessageAppended)),
            ]);

        // "MessageAppended" ends with the shorter "Appended" key, but the exact match must win.
        registry.TryResolveExactThenSuffix("MessageAppended", out Type? exact).ShouldBeTrue();
        exact.ShouldBe(typeof(MessageAppended));

        // A qualified name with no exact match falls through to the suffix match on the shorter key.
        registry.TryResolveExactThenSuffix("Some.Qualified.Appended", out Type? suffix).ShouldBeTrue();
        suffix.ShouldBe(typeof(Appended));
    }

    /// <summary>
    /// Resolution is ordinal (case-sensitive) on both the exact and suffix passes; a case-mismatched discriminator
    /// must not resolve, so a wire value cannot be silently coerced to a type it does not name exactly.
    /// </summary>
    [Fact]
    public void RegistryShouldResolveOrdinalCaseSensitively()
    {
        var registry = PolymorphicTypeRegistry.FromTypeNames([typeof(Created)]);

        registry.TryResolve("created", out _).ShouldBeFalse();
        registry.TryResolveExactThenSuffix("created", out _).ShouldBeFalse();
        registry.TryResolveExactThenSuffix("Some.Qualified.created", out _).ShouldBeFalse();
    }

    /// <summary>
    /// <see cref="PolymorphicTypeRegistry.Create"/> rejects every malformed registration collection input: a null
    /// collection, an empty collection, and a collection containing a null entry.
    /// </summary>
    [Fact]
    public void CreateShouldRejectInvalidRegistrationCollections()
    {
        _ = Should.Throw<ArgumentNullException>(() => PolymorphicTypeRegistry.Create(null!));
        _ = Should.Throw<ArgumentException>(() => PolymorphicTypeRegistry.Create([]));
        _ = Should.Throw<ArgumentException>(() => PolymorphicTypeRegistry.Create([null!]));
    }

    /// <summary>
    /// Open generic types cannot be registered because they have no closed runtime identity to decode into.
    /// </summary>
    [Fact]
    public void CreateShouldRejectOpenGenericTypes()
        => Should.Throw<ArgumentException>(() =>
            PolymorphicTypeRegistry.Create([new PolymorphicTypeRegistration("OpenList", typeof(List<>))]));

    /// <summary>
    /// Discriminators may contain ASCII letters, digits, and the bounded punctuation set ('.', '_', '-'); such a
    /// discriminator is accepted and resolves by exact match.
    /// </summary>
    [Fact]
    public void CreateShouldAcceptDottedUnderscoreAndHyphenDiscriminators()
    {
        var registry = PolymorphicTypeRegistry.Create(
            [new PolymorphicTypeRegistration("v1.event_name-2", typeof(Created))]);

        registry.TryResolve("v1.event_name-2", out Type? resolved).ShouldBeTrue();
        resolved.ShouldBe(typeof(Created));
    }

    /// <summary>
    /// <see cref="PolymorphicTypeRegistry.FromTypeNames"/> rejects a null input and otherwise keys each type by its
    /// simple (unqualified) type name.
    /// </summary>
    [Fact]
    public void FromTypeNamesShouldRejectNullInputAndKeyBySimpleTypeName()
    {
        _ = Should.Throw<ArgumentNullException>(() => PolymorphicTypeRegistry.FromTypeNames(null!));

        var registry = PolymorphicTypeRegistry.FromTypeNames([typeof(Created), typeof(MessageAppended)]);

        registry.Entries.Keys.Order(StringComparer.Ordinal).ShouldBe(["Created", "MessageAppended"], ignoreOrder: false);
        registry.Entries["Created"].ShouldBe(typeof(Created));
    }

    private sealed record Created(string Value);

    private sealed record MessageAppended(string Value);

    private sealed record Appended(string Value);
}
