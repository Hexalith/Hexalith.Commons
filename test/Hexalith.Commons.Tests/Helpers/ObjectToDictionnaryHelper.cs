// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Hexalith.Commons.Tests.Helpers;

using System.Collections.Generic;

using Shouldly;

public class ObjectToDictionaryHelper
{
    [Fact]
    public void Test()
    {
        // convert an object to a dictionary
        var obj = new { Name = "John", Age = 42 };
        IDictionary<string, object?> dictionary = ToDictionary(obj);

        _ = dictionary.ShouldNotBeNull();
        dictionary.Count.ShouldBe(2);
        dictionary.ShouldContainKey("Name");
        dictionary.ShouldContainKey("Age");
        dictionary["Name"].ShouldBe("John");
        dictionary["Age"].ShouldBe(42);
    }

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