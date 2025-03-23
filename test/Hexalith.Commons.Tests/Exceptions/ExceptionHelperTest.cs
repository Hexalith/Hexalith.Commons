// <copyright file="ExceptionHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Exceptions;

using System;

using Hexalith.Commons.Errors;

using Shouldly;

public class ExceptionHelperTest
{
    [Fact]
    public void RetreiveMessageFromExceptionWithInnerShouldSucceed()
    {
        Exception ex = new("Test 1", new Exception("Test 2", new Exception("Test 3")));
        ex.FullMessage().ShouldBe("Test 1 Test 2 Test 3");
    }

    [Fact]
    public void RetreiveMessageFromNullExceptionShouldReturnEmpty()
    {
        const Exception ex = null!;
        ex.FullMessage().ShouldBeEmpty();
    }

    [Fact]
    public void RetreiveMessageFromSimpleExceptionShouldSucceed()
    {
        Exception ex = new("Test");
        ex.FullMessage().ShouldBe("Test");
    }
}