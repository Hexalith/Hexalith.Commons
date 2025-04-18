// <copyright file="EquatableHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Objects;

using System.Collections;
using System.Linq;

/// <summary>
/// Class EquatableHelper.
/// </summary>
public static class EquatableHelper
{
    /// <summary>
    /// Ares the same.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool AreSame(this object? a, object? b)
    {
        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a.Equals(b))
        {
            return true;
        }

        if (a.GetType() != b.GetType())
        {
            return false;
        }

        if (a is IDictionary aDictionary)
        {
            return aDictionary.AreSameDictionary((IDictionary)b);
        }

        if (a is IEnumerable aEnumerable)
        {
            return aEnumerable.AreSameEnumeration((IEnumerable)b);
        }

        if (a is IEquatableObject aEquatable)
        {
            var bEquatable = (IEquatableObject)b;
            return aEquatable
                .GetEqualityComponents()
                .AreSameEnumeration(bEquatable.GetEqualityComponents());
        }

        return false;
    }

    /// <summary>
    /// Ares the same dictionary.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool AreSameDictionary(this IDictionary? a, IDictionary? b)
    {
        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        object?[] aKeys = [.. a.Keys.Cast<object?>()];
        object?[] aValues = [.. a.Values.Cast<object?>()];
        object?[] bKeys = [.. b.Keys.Cast<object?>()];
        object?[] bValues = [.. b.Values.Cast<object?>()];

        return aKeys.AreSameEnumeration(bKeys) && aValues.AreSameEnumeration(bValues);
    }

    /// <summary>
    /// Ares the same.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool AreSameEnumeration(this IEnumerable? a, IEnumerable? b)
    {
        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        object?[]? aArray = [.. a.Cast<object?>()];
        object?[]? bArray = [.. b.Cast<object?>()];
        if (aArray is null)
        {
            return bArray is null;
        }

        if (bArray is null)
        {
            return false;
        }

        if (aArray.Length != bArray.Length)
        {
            return false;
        }

        for (int i = 0; i < aArray.Length; i++)
        {
            if (!aArray[i].AreSame(bArray[i]))
            {
                return false;
            }
        }

        return true;
    }
}
