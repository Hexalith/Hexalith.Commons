namespace Hexalith.Commons.Tests.Common;

using Hexalith.Commons.Objects;

using Shouldly;

/// <summary>
/// Tests for the AreSame and AreSameDictionary extension methods.
/// </summary>
public class AreSameEnumerationTest
{
    /// <summary>
    /// Tests that AreSameDictionary and AreSame return false when dictionaries have different values.
    /// </summary>
    [Fact]
    public void EquatableDictionaryAreSameShouldReturnFalse()
    {
        Dictionary<int, DummyEquatable> a = new()
        {
            [100] = new DummyEquatable(),
            [101] = new DummyEquatable { Property3 = 10 },
        };
        Dictionary<int, DummyEquatable> b = new()
        {
            [100] = new DummyEquatable(),
            [101] = new DummyEquatable { Property3 = 11 },
        };
        a.AreSameDictionary(b).ShouldBeFalse();
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSameDictionary and AreSame return true when dictionaries have the same values.
    /// </summary>
    [Fact]
    public void EquatableDictionaryAreSameShouldReturnTrue()
    {
        Dictionary<int, DummyEquatable> a = new()
        {
            [100] = new DummyEquatable(),
            [101] = new DummyEquatable { Property3 = 10 },
        };
        Dictionary<int, DummyEquatable> b = new()
        {
            [100] = new DummyEquatable(),
            [101] = new DummyEquatable { Property3 = 10 },
        };
        a.AreSameDictionary(b).ShouldBeTrue();
        a.AreSame(b).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AreSameEnumeration and AreSame return false when lists have different values.
    /// </summary>
    [Fact]
    public void EquatableListAreSameShouldReturnFalse()
    {
        List<DummyEquatable> a = [new DummyEquatable(), new DummyEquatable { Property3 = 10 }];
        List<DummyEquatable> b = [new DummyEquatable(), new DummyEquatable { Property3 = 11 }];
        a.AreSameEnumeration(b).ShouldBeFalse();
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSameEnumeration and AreSame return true when lists have the same values.
    /// </summary>
    [Fact]
    public void EquatableListAreSameShouldReturnTrue()
    {
        List<DummyEquatable> a = [new DummyEquatable(), new DummyEquatable { Property3 = 10 }];
        List<DummyEquatable> b = [new DummyEquatable { Property3 = 10 }];
        a.AreSameEnumeration(b).ShouldBeTrue();
        a.AreSame(b).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AreSameDictionary and AreSame return false when simple dictionaries have different values.
    /// </summary>
    [Fact]
    public void SimpleDictionaryAreSameShouldReturnFalse()
    {
        Dictionary<int, string> a = new() { [10] = "Hello", [11] = "world" };
        Dictionary<int, string> b = new() { [10] = "Hello", [11] = "world*" };
        a.AreSameDictionary(b).ShouldBeFalse();
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSameDictionary and AreSame return true when simple dictionaries have the same values.
    /// </summary>
    [Fact]
    public void SimpleDictionaryAreSameShouldReturnTrue()
    {
        Dictionary<int, string> a = new() { [10] = "Hello", [11] = "world" };
        Dictionary<int, string> b = new() { [10] = "Hello", [11] = "world" };
        a.AreSameDictionary(b).ShouldBeTrue();
        a.AreSame(b).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AreSameEnumeration and AreSame return false when simple lists have different values.
    /// </summary>
    [Fact]
    public void SimpleListAreSameShouldReturnFalse()
    {
        List<string> a = ["Hello", "World"];
        List<string> b = ["Hello", "World*"];
        a.AreSameEnumeration(b).ShouldBeFalse();
        a.AreSame(b).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that AreSameEnumeration and AreSame return true when simple lists have the same values.
    /// </summary>
    [Fact]
    public void SimpleListAreSameShouldReturnTrue()
    {
        List<string> a = ["Hello", "World"];
        List<string> b = ["Hello", "World"];
        a.AreSameEnumeration(b).ShouldBeTrue();
        a.AreSame(b).ShouldBeTrue();
    }
}