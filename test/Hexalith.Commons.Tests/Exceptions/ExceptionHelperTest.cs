// <copyright file="ExceptionHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Exceptions;

using System;

using Hexalith.Commons.Errors;

using Shouldly;

/// <summary>
/// Unit tests for the ExceptionHelper class.
/// </summary>
public class ExceptionHelperTest
{
    /// <summary>
    /// Tests that retrieving the full message from an exception with inner exceptions succeeds.
    /// </summary>
    [Fact]
    public void RetrieveMessageFromExceptionWithInnerShouldSucceed()
    {
        // Create a chain of nested exceptions with different messages
        Exception ex = new("Test 1", new Exception("Test 2", new Exception("Test 3")));

        // Verify that all exception messages are concatenated in order
        ex.FullMessage().ShouldBe("Test 1 Test 2 Test 3");
    }

    /// <summary>
    /// Tests that retrieving the full message from a null exception returns an empty string.
    /// </summary>
    [Fact]
    public void RetrieveMessageFromNullExceptionShouldReturnEmpty()
    {
        // Test null exception handling
        const Exception ex = null!;

        // Verify that null exception returns empty string
        ex.FullMessage().ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that retrieving the full message from a simple exception succeeds.
    /// </summary>
    [Fact]
    public void RetrieveMessageFromSimpleExceptionShouldSucceed()
    {
        // Create a simple exception with a single message
        Exception ex = new("Test");

        // Verify that the message is returned as is
        ex.FullMessage().ShouldBe("Test");
    }
}