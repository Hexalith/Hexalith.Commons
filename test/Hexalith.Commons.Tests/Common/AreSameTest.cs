// <copyright file="AreSameTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using Hexalith.Commons.Objects;

using Shouldly;

/// <summary>
/// Unit tests for the AreSame method in various dummy classes.
/// </summary>
public class AreSameTest
{
    /// <summary>
    /// Tests that AreSame returns true for embedded equatable objects with the same properties.
    /// </summary>
    [Fact]
    public void EmbeddedEquatableAreSameShouldReturnTrue()
    {
        DummyEmbeddedEquatable a = new();
        DummyEmbeddedEquatable b = new() { Property4 = a.Property4 };
        a.AreSame(b).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AreSame returns false for equatable objects with different properties.
    /// </summary>
    [Fact]
    public void EquatableAreSameShouldReturnFalse()
    {
        DummyEquatable a = new();
        DummyEquatable b = new() { Property2 = "Hello" };
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSame returns true for equatable objects with the same properties.
    /// </summary>
    [Fact]
    public void EquatableAreSameShouldReturnTrue()
    {
        DummyEquatable a = new();
        DummyEquatable b = new();
        a.AreSame(b).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AreSame returns false for non-equatable objects.
    /// </summary>
    [Fact]
    public void NonEquatableAreSameShouldReturnFalse()
    {
        DummyNonEquatable a = new();
        DummyNonEquatable b = new();
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSame returns true for the same non-equatable object.
    /// </summary>
    [Fact]
    public void NonEquatableAreSameShouldReturnTrue()
    {
        DummyNonEquatable a = new();
        a.AreSame(a).ShouldBeTrue();
    }
}