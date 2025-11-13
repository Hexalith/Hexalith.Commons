// <copyright file="TestHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides helper methods and constants for tests.
/// </summary>
[SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "TestHelper is a common name for test utilities")]
public static class TestHelper
{
    /// <summary>
    /// Justification for the test.
    /// </summary>
    public const string TestJustification = "This test is justified because it is a test.";
}
