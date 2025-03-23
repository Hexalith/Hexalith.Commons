// <copyright file="DummyEquatable.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using System.Collections.Generic;

using Hexalith.Commons.Objects;

/// <summary>
/// A dummy class that implements the <see cref="IEquatableObject"/> interface.
/// </summary>
public class DummyEquatable : IEquatableObject
{
    /// <summary>
    /// Gets or sets the first property.
    /// </summary>
    public string Property1 { get; set; } = "Prop1";

    /// <summary>
    /// Gets or sets the second property.
    /// </summary>
    public string Property2 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the third property.
    /// </summary>
    public int Property3 { get; set; } = 123;

    /// <inheritdoc/>
    public IEnumerable<object> GetEqualityComponents()
                    => new object[] { Property1, Property2, Property3 };
}