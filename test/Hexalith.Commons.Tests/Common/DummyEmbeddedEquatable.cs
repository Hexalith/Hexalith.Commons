// <copyright file="DummyEmbeddedEquatable.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using System.Collections.Generic;

using Hexalith.Commons.Objects;

/// <summary>
/// Represents a dummy embedded equatable object.
/// </summary>
public class DummyEmbeddedEquatable : IEquatableObject
{
    /// <summary>
    /// Gets or sets the first property.
    /// </summary>
    public string Property1 { get; set; } = "Hi";

    /// <summary>
    /// Gets or sets the second property.
    /// </summary>
    public string Property2 { get; set; } = "Ho";

    /// <summary>
    /// Gets or sets the third property.
    /// </summary>
    public int Property3 { get; set; } = 1230;

    /// <summary>
    /// Gets or sets the fourth property.
    /// </summary>
    public DummyNonEquatable Property4 { get; set; } = new DummyNonEquatable();

    /// <summary>
    /// Gets or sets the fifth property.
    /// </summary>
    public DummyEquatable Property5 { get; set; } = new DummyEquatable();

    /// <inheritdoc/>
    public IEnumerable<object> GetEqualityComponents() =>
        [Property1, Property2, Property3, Property4, Property5];
}