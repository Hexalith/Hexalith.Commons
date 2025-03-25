// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Helpers;

using System.Diagnostics.CodeAnalysis;

using Hexalith.Commons.Common;
using Hexalith.Commons.Objects;

using Shouldly;

/// <summary>
/// Test class for ExampleHelper functionality.
/// Contains unit tests to verify the behavior of example object creation
/// with various property types, attributes, and inheritance scenarios.
/// Tests the ExampleValue attribute and default value handling.
/// </summary>
public class ExampleTest
{
    /// <summary>
    /// Tests that properties in a base class decorated with ExampleValue
    /// attribute are correctly initialized when creating derived class instances.
    /// </summary>
    [Fact]
    public void BasePropertyWithAttributeShouldHaveValue()
    {
        BasePropertyExample example = ExampleHelper.CreateExample<BasePropertyExample>();
        example.Value.ShouldBe("Hello");
    }

    /// <summary>
    /// Tests that both readonly properties and properties with ExampleValue
    /// attributes are correctly handled in example object creation.
    /// </summary>
    [Fact]
    public void BaseReadOnlyPropertyAndPropertyWithAttributeShouldHaveValue()
    {
        BaseReadOnlyPropertyExample example = ExampleHelper.CreateExample<BaseReadOnlyPropertyExample>();
        BaseReadOnlyProperty.ReadOnlyValue.ShouldBe("Read");
        example.Value.ShouldBe("Hello");
    }

    /// <summary>
    /// Tests that example objects are created with correct values
    /// for properties with ExampleValue attributes and default values.
    /// </summary>
    [Fact]
    public void ExampleCreatedIsValid()
    {
        TestExample example = ExampleHelper.CreateExample<TestExample>();
        example.StringValue.ShouldBe("Hello");
        example.StringDefault.ShouldBe("string");
        example.IntValue.ShouldBe(10);
    }

    /// <summary>
    /// Tests that example object creation does not throw any exceptions,
    /// verifying the robustness of the creation process.
    /// </summary>
    [Fact]
    public void ExampleCreationShouldNotThrowExceptions() => Should.NotThrow(ExampleHelper.CreateExample<TestExample>);

    /// <summary>
    /// Tests that integer properties decorated with ExampleValue
    /// attribute are correctly initialized with the specified value.
    /// </summary>
    [Fact]
    public void IntegerWithAttributeShouldHaveValue()
    {
        IntegerExample example = ExampleHelper.CreateExample<IntegerExample>();
        example.Value.ShouldBe(129);
    }

    /// <summary>
    /// Tests that integer properties without ExampleValue attribute
    /// retain their default values during example object creation.
    /// </summary>
    [Fact]
    public void IntegerWithNoAttributeShouldHaveValue()
    {
        IntegerDefaultExample example = ExampleHelper.CreateExample<IntegerDefaultExample>();
        example.Value.ShouldBe(101);
    }

    /// <summary>
    /// Tests that string properties decorated with ExampleValue
    /// attribute are correctly initialized with the specified value.
    /// </summary>
    [Fact]
    public void StringWithAttributeShouldHaveValue()
    {
        StringExample example = ExampleHelper.CreateExample<StringExample>();
        example.Value.ShouldBe("Hello");
    }

    /// <summary>
    /// Tests that string properties without ExampleValue attribute
    /// retain their default values during example object creation.
    /// </summary>
    [Fact]
    public void StringWithNoAttributeShouldHaveValue()
    {
        StringDefaultExample example = ExampleHelper.CreateExample<StringDefaultExample>();
        example.Value.ShouldBe("string");
    }

    /// <summary>
    /// Static class containing readonly test values.
    /// Used to verify handling of readonly properties in example creation.
    /// </summary>
    private static class BaseReadOnlyProperty
    {
        /// <summary>
        /// A constant string value used for testing readonly property behavior.
        /// </summary>
        public const string ReadOnlyValue = "Read";
    }

    /// <summary>
    /// Base class with an ExampleValue-decorated property.
    /// Used to test inheritance scenarios in example creation.
    /// </summary>
    private class BaseProperty
    {
        /// <summary>
        /// Gets or sets a string value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue("Hello")]
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Empty derived class used to test inheritance of ExampleValue attributes.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = TestHelper.TestJustification)]
    private class BasePropertyExample : BaseProperty
    {
    }

    /// <summary>
    /// Test class combining readonly and ExampleValue-decorated properties.
    /// Used to verify handling of mixed property types.
    /// </summary>
    private class BaseReadOnlyPropertyExample
    {
        /// <summary>
        /// A constant string value used for testing readonly property behavior.
        /// </summary>
        [SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = TestHelper.TestJustification)]
        public const string ReadOnlyValue = "Read";

        /// <summary>
        /// Gets or sets a string value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue("Hello")]
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with an integer property having a default value.
    /// Used to verify default value retention during example creation.
    /// </summary>
    private class IntegerDefaultExample
    {
        /// <summary>
        /// Gets or sets an integer value with a default initialization.
        /// </summary>
        public int Value { get; set; } = 101;
    }

    /// <summary>
    /// Test class with an ExampleValue-decorated integer property.
    /// Used to verify attribute-based initialization of numeric types.
    /// </summary>
    private class IntegerExample
    {
        /// <summary>
        /// Gets or sets an integer value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue(129)]
        public int Value { get; set; }
    }

    /// <summary>
    /// Test class with a string property having a default value.
    /// Used to verify default value retention during example creation.
    /// </summary>
    private class StringDefaultExample
    {
        /// <summary>
        /// Gets or sets a string value with a default initialization.
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test class with an ExampleValue-decorated string property.
    /// Used to verify attribute-based initialization of string types.
    /// </summary>
    private class StringExample
    {
        /// <summary>
        /// Gets or sets a string value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue("Hello")]
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comprehensive test class with multiple properties of different types.
    /// Used to verify correct handling of mixed property scenarios.
    /// </summary>
    private class TestExample
    {
        /// <summary>
        /// Gets or sets an integer value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue(10)]
        public int IntValue { get; set; }

        /// <summary>
        /// Gets or sets a string value with default initialization.
        /// </summary>
        public string StringDefault { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a string value initialized through ExampleValue attribute.
        /// </summary>
        [ExampleValue("Hello")]
        public string StringValue { get; set; } = string.Empty;
    }
}