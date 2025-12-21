// <copyright file="EquatableHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Objects;

using System;
using System.Collections;

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
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (DictionaryEntry entry in a)
        {
            if (!b.Contains(entry.Key))
            {
                return false;
            }

            if (!entry.Value.AreSame(b[entry.Key]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Ares the same.
    /// </summary>
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool AreSameEnumeration(this IEnumerable? a, IEnumerable? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        IEnumerator aEnumerator = a.GetEnumerator();
        IEnumerator bEnumerator = b.GetEnumerator();
        try
        {
            while (true)
            {
                bool aHasNext = aEnumerator.MoveNext();
                bool bHasNext = bEnumerator.MoveNext();

                if (aHasNext != bHasNext)
                {
                    return false;
                }

                if (!aHasNext)
                {
                    return true;
                }

                if (!aEnumerator.Current.AreSame(bEnumerator.Current))
                {
                    return false;
                }
            }
        }
        finally
        {
            (aEnumerator as IDisposable)?.Dispose();
            (bEnumerator as IDisposable)?.Dispose();
        }
    }
}