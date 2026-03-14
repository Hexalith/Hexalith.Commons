// <copyright file="UniqueHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.UniqueIds;

using System.Globalization;
using System.Text.RegularExpressions;

using Hexalith.Commons.UniqueIds;

using Shouldly;

/// <summary>
/// Test class for UniqueIdHelper functionality.
/// Contains unit tests to verify the generation of unique identifiers,
/// including concurrent generation, format validation, and uniqueness checks
/// for both datetime-based and random-based identifiers.
/// </summary>
public partial class UniqueHelperTest
{
    /// <summary>
    /// Tests that 100 sequential datetime-based IDs are monotonically increasing,
    /// verifying the increment logic when calls occur within the same millisecond.
    /// </summary>
    [Fact]
    public void GenerateDateTimeIdProducesMonotonicallyIncreasingIds()
    {
        string previous = UniqueIdHelper.GenerateDateTimeId();
        previous.Length.ShouldBe(17);
        DateTime.TryParseExact(
            previous,
            "yyyyMMddHHmmssfff",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out _).ShouldBeTrue($"ID '{previous}' should match format yyyyMMddHHmmssfff");

        for (int i = 0; i < 99; i++)
        {
            string current = UniqueIdHelper.GenerateDateTimeId();
            current.Length.ShouldBe(17);
            DateTime.TryParseExact(
                current,
                "yyyyMMddHHmmssfff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out _).ShouldBeTrue($"ID '{current}' should match format yyyyMMddHHmmssfff");
            StringComparer.Ordinal.Compare(current, previous).ShouldBeGreaterThan(0);
            previous = current;
        }
    }

    /// <summary>
    /// Tests that 10,000 generated unique string IDs are exactly 22 characters,
    /// contain only Base64URL-safe characters (A-Za-z0-9_-), and are all unique.
    /// </summary>
    [Fact]
    public void GenerateUniqueStringIdProducesOnly22CharBase64UrlStringsAcrossTenThousandIds()
    {
        HashSet<string> ids = [];
        Regex base64UrlPattern = Base64UrlPattern();
        for (int i = 0; i < 10_000; i++)
        {
            string id = UniqueIdHelper.GenerateUniqueStringId();
            id.Length.ShouldBe(22);
            base64UrlPattern.IsMatch(id).ShouldBeTrue($"ID '{id}' contains invalid Base64URL characters");
            _ = ids.Add(id);
        }

        ids.Count.ShouldBe(10_000);
    }

    /// <summary>
    /// Tests that concurrent generation of 100 datetime-based IDs
    /// produces unique values without any duplicates, verifying
    /// thread-safety of the generation process.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of generated IDs.</returns>
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

    /// <summary>
    /// Tests that all three ID strategies (DateTime, Base64URL, ULID) coexist independently
    /// without interfering with each other, verifying FR14 incremental adoption guarantee.
    /// </summary>
    [Fact]
    public void AllThreeIdStrategiesCoexistIndependently()
    {
        string dateTimeId = UniqueIdHelper.GenerateDateTimeId();
        string uniqueId = UniqueIdHelper.GenerateUniqueStringId();
        string sortableId = UniqueIdHelper.GenerateSortableUniqueStringId();

        dateTimeId.Length.ShouldBe(17);
        uniqueId.Length.ShouldBe(22);
        sortableId.Length.ShouldBe(26);
    }

    /// <summary>
    /// Tests that a generated sortable unique string ID is exactly 26 characters
    /// and contains only valid Crockford Base32 characters conforming to the ULID specification.
    /// </summary>
    [Fact]
    public void GenerateSortableUniqueStringIdProduces26CharCrockfordBase32String()
    {
        HashSet<string> ids = [];
        Regex pattern = CrockfordBase32Pattern();
        for (int i = 0; i < 1_000; i++)
        {
            string id = UniqueIdHelper.GenerateSortableUniqueStringId();
            id.Length.ShouldBe(26);
            pattern.IsMatch(id).ShouldBeTrue($"ID '{id}' contains invalid Crockford Base32 characters");
            _ = ids.Add(id);
        }

        ids.Count.ShouldBe(1_000);
    }

    /// <summary>
    /// Tests that 100 sequential sortable unique IDs are monotonically increasing,
    /// verifying the monotonic increment behavior within the same millisecond window.
    /// </summary>
    [Fact]
    public void GenerateSortableUniqueStringIdProducesMonotonicallyIncreasingIds()
    {
        string previous = UniqueIdHelper.GenerateSortableUniqueStringId();
        for (int i = 0; i < 99; i++)
        {
            string current = UniqueIdHelper.GenerateSortableUniqueStringId();
            StringComparer.Ordinal.Compare(current, previous).ShouldBeGreaterThan(0);
            previous = current;
        }
    }

    /// <summary>
    /// Tests that concurrent generation of 100 sortable unique IDs
    /// produces unique values without any duplicates, verifying
    /// thread-safety of the ULID generation process.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GetAHundredConcurrentSortableUniqueIdStringWithoutAnyDuplicatesAsync()
    {
        List<Task<string>> tasks = [];
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(UniqueIdHelper.GenerateSortableUniqueStringId));
        }

        string[] result = await Task.WhenAll(tasks);
        result.Distinct(StringComparer.Ordinal).Count().ShouldBe(100);
    }

    /// <summary>
    /// Tests that 1,000 sequentially generated sortable unique IDs maintain chronological
    /// order when sorted lexicographically, verifying the ULID specification's sortability guarantee.
    /// </summary>
    [Fact]
    public void GetAThousandSortableUniqueIdStringInChronologicalOrder()
    {
        List<string> ids = [];
        for (int i = 0; i < 1_000; i++)
        {
            ids.Add(UniqueIdHelper.GenerateSortableUniqueStringId());
        }

        List<string> sorted = [.. ids.OrderBy(id => id, StringComparer.Ordinal)];
        ids.ShouldBe(sorted);
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{22}$")]
    private static partial Regex Base64UrlPattern();

    [GeneratedRegex("^[0-9A-HJKMNP-TV-Z]{26}$")]
    private static partial Regex CrockfordBase32Pattern();
}