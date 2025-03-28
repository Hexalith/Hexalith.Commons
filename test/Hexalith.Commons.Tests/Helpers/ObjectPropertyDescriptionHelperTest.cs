// <copyright file="ObjectPropertyDescriptionHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Helpers;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Hexalith.Commons.Objects;

using Shouldly;

/// <summary>
/// Test class for ObjectDescriptionHelper's property description functionality.
/// Contains unit tests to verify the behavior of property description extraction
/// from various attribute decorations including DefaultValue, Description, Display,
/// DisplayName, and Required attributes.
/// </summary>
public class ObjectPropertyDescriptionHelperTest
{
    /// <summary>
    /// Tests that when a property is decorated with DefaultValue attribute,
    /// the helper correctly extracts the default value while maintaining
    /// appropriate display name and description.
    /// </summary>
    [Fact]
    public void ObjectPropertyWithDefaultValueAttributeShouldReturnDefinedValue()
    {
        IDictionary<string, (string DisplayName, string Description, object? DefaultValue, bool IsRequired)> props = ObjectDescriptionHelper.DescribeProperties(typeof(DefaultValuePropertyAttributeTest));
        (string displayName, string description, object? defaultValue, bool isRequired) = props[nameof(DefaultValuePropertyAttributeTest.MyValue)];
        description.ShouldBe("My value");
        displayName.ShouldBe("My value");
        defaultValue.ShouldBe("this value is for me");
        isRequired.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that when a property is decorated with Description attribute,
    /// the helper correctly extracts the description while maintaining
    /// appropriate display name and null default value.
    /// </summary>
    [Fact]
    public void ObjectPropertyWithDescriptionAttributeShouldReturnDefinedValue()
    {
        IDictionary<string, (string DisplayName, string Description, object? DefaultValue, bool IsRequired)> props = ObjectDescriptionHelper.DescribeProperties(typeof(DescriptionPropertyAttributeTest));
        (string displayName, string description, object? defaultValue, bool isRequired) = props[nameof(DescriptionPropertyAttributeTest.MyValue)];
        description.ShouldBe("This class is used to test a class property with a description attribute");
        displayName.ShouldBe("My value");
        defaultValue.ShouldBeNull();
        isRequired.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that when a property is decorated with Display attribute,
    /// the helper correctly extracts both the name and description values
    /// while maintaining appropriate null default value.
    /// </summary>
    [Fact]
    public void ObjectPropertyWithDisplayAttributeShouldReturnDefinedValue()
    {
        IDictionary<string, (string DisplayName, string Description, object? DefaultValue, bool IsRequired)> props = ObjectDescriptionHelper.DescribeProperties(typeof(DisplayPropertyAttributeTest));
        (string displayName, string description, object? defaultValue, bool isRequired) = props[nameof(DisplayPropertyAttributeTest.MyValue)];
        description.ShouldBe("This class is used to test a class property with a description attribute");
        displayName.ShouldBe("My property value");
        defaultValue.ShouldBeNull();
        isRequired.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that when a property is decorated with DisplayName attribute,
    /// the helper correctly extracts the display name and uses it for both
    /// name and description while maintaining appropriate null default value.
    /// </summary>
    [Fact]
    public void ObjectPropertyWithDisplayNameAttributeShouldReturnDefinedValue()
    {
        IDictionary<string, (string DisplayName, string Description, object? DefaultValue, bool IsRequired)> props = ObjectDescriptionHelper.DescribeProperties(typeof(DisplayNamePropertyAttributeTest));
        (string displayName, string description, object? defaultValue, bool isRequired) = props[nameof(DisplayNamePropertyAttributeTest.MyValue)];
        description.ShouldBe("My property value");
        displayName.ShouldBe("My property value");
        defaultValue.ShouldBeNull();
        isRequired.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that when a property is decorated with Required attribute,
    /// the helper correctly sets the IsRequired flag while maintaining
    /// appropriate display name and description.
    /// </summary>
    [Fact]
    public void ObjectPropertyWithRequiredValueAttributeShouldReturnDefinedValue()
    {
        IDictionary<string, (string DisplayName, string Description, object? DefaultValue, bool IsRequired)> props = ObjectDescriptionHelper.DescribeProperties(typeof(RequiredValuePropertyAttributeTest));
        (string displayName, string description, object? defaultValue, bool isRequired) = props[nameof(RequiredValuePropertyAttributeTest.MyValue)];
        description.ShouldBe("My value");
        displayName.ShouldBe("My value");
        defaultValue.ShouldBeNull();
        isRequired.ShouldBeTrue();
    }

    /// <summary>
    /// Test class with a property decorated with DefaultValue attribute
    /// to verify default value extraction.
    /// </summary>
    public class DefaultValuePropertyAttributeTest
    {
        /// <summary>
        /// Gets or sets a value with a default value of "this value is for me".
        /// </summary>
        [DefaultValue("this value is for me")]
        public string MyValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with a property decorated with Description attribute
    /// to verify description extraction.
    /// </summary>
    public class DescriptionPropertyAttributeTest
    {
        /// <summary>
        /// Gets or sets a value with a custom description.
        /// </summary>
        [Description("This class is used to test a class property with a description attribute")]
        public string MyValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with a property decorated with DisplayName attribute
    /// to verify display name extraction.
    /// </summary>
    public class DisplayNamePropertyAttributeTest
    {
        /// <summary>
        /// Gets or sets a value with a custom display name.
        /// </summary>
        [DisplayName("My property value")]
        public string MyValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with a property decorated with Display attribute
    /// to verify name and description extraction.
    /// </summary>
    public class DisplayPropertyAttributeTest
    {
        /// <summary>
        /// Gets or sets a value with custom display name and description.
        /// </summary>
        [Display(Name = "My property value", Description = "This class is used to test a class property with a description attribute")]
        public string MyValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with a property decorated with Required attribute
    /// to verify required flag extraction.
    /// </summary>
    public class RequiredValuePropertyAttributeTest
    {
        /// <summary>
        /// Gets or sets a required value.
        /// </summary>
        [Required]
        public string MyValue { get; set; } = string.Empty;
    }
}