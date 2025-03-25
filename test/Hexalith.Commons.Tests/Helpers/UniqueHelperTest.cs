// <copyright file="UniqueHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Helpers;

using Hexalith.Commons.Objects;

using Shouldly;

/// <summary>
/// Test class for UniqueIdHelper functionality.
/// Contains unit tests to verify the generation of unique identifiers,
/// including concurrent generation, format validation, and uniqueness checks
/// for both datetime-based and random-based identifiers.
/// </summary>
public class UniqueHelperTest
{
    /// <summary>
    /// Tests that concurrent generation of 100 datetime-based IDs
    /// produces unique values without any duplicates, verifying
    /// thread-safety of the generation process.
    /// </summary>
    [Fact]
    public async Task GetAHundredConcurrentDateTimeIdStringWithoutAnyDuplicatesAsync()
    {
        List<Task<string>> ids = [];
        for (int i = 0; i < 100; i++)
        {
            ids.Add(Task.Run(UniqueIdHelper.GenerateDateTimeId));
        }

        string[] result = await Task.WhenAll(ids);
        result.Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
    }

    /// <summary>
    /// Tests that sequential generation of 100 datetime-based IDs
    /// produces unique values without any duplicates, verifying
    /// the uniqueness of generated IDs in a single thread.
    /// </summary>
    [Fact]
    public void GetAHundredDateTimeIdStringWithoutAnyDuplicates()
    {
        List<string> ids = [];
        for (int i = 0; i < 100; i++)
        {
            ids.Add(UniqueIdHelper.GenerateDateTimeId());
        }

        ids.Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
    }

    /// <summary>
    /// Tests that sequential generation of 1000 random-based unique IDs
    /// produces unique values without any duplicates, verifying the
    /// collision resistance of the generation algorithm.
    /// </summary>
    [Fact]
    public void GetAThousandUniqueIdStringWithoutAnyDuplicates()
    {
        List<string> ids = [];
        for (int i = 0; i < 1000; i++)
        {
            ids.Add(UniqueIdHelper.GenerateUniqueStringId());
        }

        ids.Distinct(StringComparer.Ordinal).Count().ShouldBe(1000);
    }

    /// <summary>
    /// Tests that generated datetime-based IDs have exactly 17 characters,
    /// which corresponds to the millisecond precision datetime string format.
    /// </summary>
    [Fact]
    public void GetDateTimeIdStringReturns17Chars()
    {
        string id = UniqueIdHelper.GenerateDateTimeId();
        id.Length.ShouldBe(17, "the id is a millisecond precision date time string");
    }

    /// <summary>
    /// Tests that generated random-based unique IDs have exactly 22 characters,
    /// which corresponds to the base64 encoding of 16 random bytes.
    /// </summary>
    [Fact]
    public void GetUniqueIdStringReturns22Chars()
    {
        string id = UniqueIdHelper.GenerateUniqueStringId();
        id.Length.ShouldBe(22, "the id is a base64 string of 16 bytes");
    }
}