// <copyright file="AspireDaprLocalServiceEndpointsTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Aspire.Tests;

using Hexalith.Commons.Aspire;

using Shouldly;

public sealed class AspireDaprLocalServiceEndpointsTest
{
    [Fact]
    public void ResolveShouldReturnTrimmedConfiguredAddresses()
    {
        (string? placement, string? scheduler) = AspireDaprLocalServiceEndpoints.Resolve(
            "  local-placement:1234 ",
            " local-scheduler:5678 ",
            _ => false);

        placement.ShouldBe("local-placement:1234");
        scheduler.ShouldBe("local-scheduler:5678");
    }

    [Fact]
    public void ResolveShouldReturnPreferredAddressesWhenPreferredPortsAreReachable()
    {
        (string? placement, string? scheduler) = AspireDaprLocalServiceEndpoints.Resolve(
            null,
            null,
            static port => port is 6050 or 6060);

        placement.ShouldBe("localhost:6050");
        scheduler.ShouldBe("localhost:6060");
    }

    [Fact]
    public void ResolveShouldReturnNativeAddressesWhenOnlyNativePortsAreReachable()
    {
        (string? placement, string? scheduler) = AspireDaprLocalServiceEndpoints.Resolve(
            null,
            null,
            static port => port is 50005 or 50006);

        placement.ShouldBe("localhost:50005");
        scheduler.ShouldBe("localhost:50006");
    }

    [Fact]
    public void ResolveShouldProbeOnlyTheMissingAddress()
    {
        (string? placement, string? scheduler) = AspireDaprLocalServiceEndpoints.Resolve(
            "localhost:12345",
            null,
            static port => port == 50006);

        placement.ShouldBe("localhost:12345");
        scheduler.ShouldBe("localhost:50006");
    }

    [Fact]
    public void ResolveShouldReturnNullAddressesWhenNoCandidateIsReachable()
    {
        (string? placement, string? scheduler) = AspireDaprLocalServiceEndpoints.Resolve(
            null,
            null,
            _ => false);

        placement.ShouldBeNull();
        scheduler.ShouldBeNull();
    }
}
