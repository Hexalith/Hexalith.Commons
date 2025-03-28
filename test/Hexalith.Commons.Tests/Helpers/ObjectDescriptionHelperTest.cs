// <copyright file="ObjectDescriptionHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Helpers;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

using Hexalith.Commons.Objects;
using Hexalith.Commons.Reflections;

using Shouldly;

/// <summary>
/// Test class for ObjectDescriptionHelper functionality.
/// Contains unit tests to verify the behavior of object description extraction
/// from various attribute decorations and type implementations.
/// </summary>
public class ObjectDescriptionHelperTest
{
    /// <summary>
    /// Tests that when a type implements IMappableType, the helper returns
    /// the correct mapped type name from the implementation.
    /// </summary>
    [Fact]
    public void MappableTypeObjectShouldReturnMapperValue()
    {
        (string type, string name, string description) = ObjectDescriptionHelper.Describe(typeof(MappableTypeTestV2));
        type.ShouldBe("MappableTest");
        name.ShouldBe("Mappable test");
        description.ShouldBe("Mappable test");
    }

    /// <summary>
    /// Tests that when a type is decorated with Description attribute,
    /// the helper correctly extracts the description value.
    /// </summary>
    [Fact]
    public void ObjectWithDescriptionAttributeShouldReturnDefinedValue()
    {
        (string type, string name, string description) = ObjectDescriptionHelper.Describe(typeof(DescriptionAttributeTest));
        type.ShouldBe(nameof(DescriptionAttributeTest));
        name.ShouldBe("Description attribute test");
        description.ShouldBe("This class is used to test a class with a description attribute");
    }

    /// <summary>
    /// Tests that when a type is decorated with Display attribute,
    /// the helper correctly extracts both the name and description values.
    /// </summary>
    [Fact]
    public void ObjectWithDisplayAttributeShouldReturnDefinedValue()
    {
        (string type, string name, string description) = ObjectDescriptionHelper.Describe(typeof(DisplayAttributeTest));
        type.ShouldBe(nameof(DisplayAttributeTest));
        name.ShouldBe("Display attribute example");
        description.ShouldBe("Example of using the display attribute to defined name and description");
    }

    /// <summary>
    /// Tests that when a type is decorated with DisplayName attribute,
    /// the helper correctly extracts the display name and uses it for both name and description.
    /// </summary>
    [Fact]
    public void ObjectWithDisplayNameAttributeShouldReturnDefinedValue()
    {
        (string type, string name, string description) = ObjectDescriptionHelper.Describe(typeof(DisplayNameAttributeTest));
        type.ShouldBe(nameof(DisplayNameAttributeTest));
        name.ShouldBe("Display name attribute example");
        description.ShouldBe("Display name attribute example");
    }

    /// <summary>
    /// Tests that when a type has no descriptive attributes,
    /// the helper falls back to using the type name in a humanized format.
    /// </summary>
    [Fact]
    public void ObjectWithNoAttributesShouldReturnTypeName()
    {
        (string type, string name, string description) = ObjectDescriptionHelper.Describe(typeof(NoAttributesTest));
        type.ShouldBe(nameof(NoAttributesTest));
        name.ShouldBe("No attributes test");
        description.ShouldBe("No attributes test");
    }

    /// <summary>
    /// Test class decorated with Description attribute to verify description extraction.
    /// </summary>
    [Description("This class is used to test a class with a description attribute")]
    public class DescriptionAttributeTest
    {
    }

    /// <summary>
    /// Test class decorated with Display attribute to verify name and description extraction.
    /// </summary>
    [Display(Name = "Display attribute example", Description = "Example of using the display attribute to defined name and description")]
    public class DisplayAttributeTest
    {
    }

    /// <summary>
    /// Test class decorated with DisplayName attribute to verify display name extraction.
    /// </summary>
    [DisplayName("Display name attribute example")]
    public class DisplayNameAttributeTest
    {
    }

    /// <summary>
    /// Test class implementing IMappableType to verify type mapping functionality.
    /// Demonstrates how custom type names can be provided through interface implementation.
    /// </summary>
    public class MappableTypeTestV2 : IMappableType
    {
        /// <summary>
        /// Gets the mapped type name that should be used for this type.
        /// </summary>
        public string TypeMapName => "MappableTest";
    }

    /// <summary>
    /// Test class with no descriptive attributes to verify default behavior.
    /// Used to test the fallback to type name when no attributes are present.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = TestHelper.TestJustification)]
    public class NoAttributesTest
    {
    }
}