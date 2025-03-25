// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Helpers;

using System.Collections.Generic;

using Shouldly;

/// <summary>
/// Helper class for converting objects to dictionaries.
/// Provides functionality to transform any object's properties
/// into a key-value dictionary representation.
/// </summary>
public class ObjectToDictionaryHelper
{
    /// <summary>
    /// Tests the object to dictionary conversion functionality.
    /// Verifies that an anonymous object's properties are correctly
    /// extracted and stored in a dictionary with proper key-value pairs.
    /// </summary>
    [Fact]
    public void Test()
    {
        // Create a test object with sample properties
        var obj = new { Name = "John", Age = 42 };
        IDictionary<string, object?> dictionary = ToDictionary(obj);

        // Verify the dictionary was created correctly
        _ = dictionary.ShouldNotBeNull();
        dictionary.Count.ShouldBe(2);
        dictionary.ShouldContainKey("Name");
        dictionary.ShouldContainKey("Age");
        dictionary["Name"].ShouldBe("John");
        dictionary["Age"].ShouldBe(42);
    }

    /// <summary>
    /// Converts an object to a dictionary by extracting its properties.
    /// Uses reflection to get all public properties of the object and
    /// creates a dictionary with property names as keys and their values.
    /// </summary>
    /// <param name="obj">The object to convert to a dictionary.</param>
    /// <returns>A dictionary containing the object's property names and values.</returns>
    private static IDictionary<string, object?> ToDictionary(object obj)
    {
        Dictionary<string, object?> dictionary = new(StringComparer.Ordinal);
        foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
        {
            dictionary.Add(prop.Name, prop.GetValue(obj));
        }

        return dictionary;
    }
}