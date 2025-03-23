// <copyright file="DummyEmbeddedEquatable.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Common;

using System.Collections.Generic;

using Hexalith.Commons.Objects;

public class DummyEmbeddedEquatable : IEquatableObject
{
    public string Property1 { get; set; } = "Hi";

    public string Property2 { get; set; } = "Ho";

    public int Property3 { get; set; } = 1230;

    public DummyNonEquatable Property4 { get; set; } = new DummyNonEquatable();

    public DummyEquatable Property5 { get; set; } = new DummyEquatable();

    /// <inheritdoc/>
    public IEnumerable<object> GetEqualityComponents()
                            => [Property1, Property2, Property3, Property4, Property5];
}