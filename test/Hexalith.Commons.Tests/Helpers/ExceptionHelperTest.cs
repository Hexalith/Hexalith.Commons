// <copyright file="ExceptionHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Helpers;

using System;

using Hexalith.Commons.Errors;

using Shouldly;

/// <summary>
/// Test class for ExceptionHelper functionality.
/// Contains unit tests to verify the behavior of exception message
/// concatenation and formatting, particularly for nested exceptions.
/// </summary>
public class ExceptionHelperTest
{
    /// <summary>
    /// Tests that the FullMessage extension method correctly concatenates
    /// all exception messages in a nested exception chain, preserving
    /// the order from outer to inner exceptions and maintaining proper
    /// spacing between messages.
    /// </summary>
    [Fact]
    public void GetFullMessageOnNestedExceptionsShouldReturnAllMessages()
    {
        const string msg1 = "Error message 1";
        const string msg2 = "Error 2";
        const string msg3 = "Message :\n Error 3";
        const string msg4 = "Hello 4";
        Exception ex = new(
            msg1,
            new Exception(
                msg2,
                new Exception(
                    msg3,
                    new Exception(msg4))));
        string message = ex.FullMessage();
        message.ShouldBe($"{msg1} {msg2} {msg3} {msg4}");
    }
}