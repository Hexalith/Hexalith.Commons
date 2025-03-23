﻿// <copyright file="AreSameTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using Hexalith.Commons.Objects;

using Shouldly;

public class AreSameTest
{
    [Fact]
    public void EmbeddedEquatableAreSameShouldReturnTrue()
    {
        DummyEmbeddedEquatable a = new();
        DummyEmbeddedEquatable b = new() { Property4 = a.Property4 };
        a.AreSame(b).ShouldBeTrue();
    }

    [Fact]
    public void EquatableAreSameShouldReturnFalse()
    {
        DummyEquatable a = new();
        DummyEquatable b = new() { Property2 = "Hello" };
        a.AreSame(b).ShouldBeFalse();
    }

    [Fact]
    public void EquatableAreSameShouldReturnTrue()
    {
        DummyEquatable a = new();
        DummyEquatable b = new();
        a.AreSame(b).ShouldBeTrue();
    }

    [Fact]
    public void NonEquatableAreSameShouldReturnFalse()
    {
        DummyNonEquatable a = new();
        DummyNonEquatable b = new();
        a.AreSame(b).ShouldBeFalse();
    }

    [Fact]
    public void NonEquatableAreSameShouldReturnTrue()
    {
        DummyNonEquatable a = new();
        a.AreSame(a).ShouldBeTrue();
    }
}