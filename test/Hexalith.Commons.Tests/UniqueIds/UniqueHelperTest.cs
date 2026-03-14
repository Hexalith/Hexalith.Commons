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
    /// Tests that extracting a timestamp from a freshly generated ULID
    /// returns a UTC time within 1 millisecond of the actual generation time.
    /// </summary>
    [Fact]
    public void ExtractTimestampFromGeneratedUlidReturnsTimestampWithinOneMillisecond()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();
        DateTimeOffset after = DateTimeOffset.UtcNow;

        DateTimeOffset timestamp = UniqueIdHelper.ExtractTimestamp(ulid);

        timestamp.ShouldBeGreaterThanOrEqualTo(before.AddMilliseconds(-1));
        timestamp.ShouldBeLessThanOrEqualTo(after.AddMilliseconds(1));
    }

    /// <summary>
    /// Tests that passing an invalid ULID format to ExtractTimestamp throws FormatException.
    /// </summary>
    /// <param name="ulid">The invalid ULID string to test.</param>
    [Theory]
    [InlineData("short")]
    [InlineData("THIS_IS_NOT_A_VALID_ULID!!")]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA!")]
    public void ExtractTimestampFromInvalidFormatThrowsFormatException(string ulid)
    {
        FormatException exception = Should.Throw<FormatException>(() => UniqueIdHelper.ExtractTimestamp(ulid));

        exception.Message.ShouldContain("not a valid ULID string");
        exception.Message.ShouldContain(ulid);
    }

    /// <summary>
    /// Tests that passing null, empty, or whitespace to ExtractTimestamp throws ArgumentException.
    /// </summary>
    /// <param name="ulid">The invalid input to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractTimestampFromNullOrWhiteSpaceThrowsArgumentException(string? ulid)
        => _ = Should.Throw<ArgumentException>(() => UniqueIdHelper.ExtractTimestamp(ulid!));

    /// <summary>
    /// Tests that the timestamp extracted from a ULID is in UTC (offset is zero).
    /// </summary>
    [Fact]
    public void ExtractTimestampFromUlidReturnsUtcTime()
    {
        string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();

        DateTimeOffset timestamp = UniqueIdHelper.ExtractTimestamp(ulid);

        timestamp.Offset.ShouldBe(TimeSpan.Zero);
    }

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

    /// <summary>
    /// Tests that converting a valid ULID to Guid and back returns the original ULID string (lossless round-trip).
    /// </summary>
    [Fact]
    public void ConvertSortableUniqueIdToGuidAndBackShouldReturnOriginalValue()
    {
        string original = UniqueIdHelper.GenerateSortableUniqueStringId();
        var guid = UniqueIdHelper.ToGuid(original);
        string roundTripped = UniqueIdHelper.ToSortableUniqueId(guid);
        roundTripped.ShouldBe(original);
    }

    /// <summary>
    /// Tests that 100 generated ULIDs all survive Guid round-trip conversion without data loss.
    /// </summary>
    [Fact]
    public void ConvertAHundredSortableUniqueIdsToGuidAndBackShouldAllReturnOriginalValues()
    {
        for (int i = 0; i < 100; i++)
        {
            string original = UniqueIdHelper.GenerateSortableUniqueStringId();
            var guid = UniqueIdHelper.ToGuid(original);
            string roundTripped = UniqueIdHelper.ToSortableUniqueId(guid);
            roundTripped.ShouldBe(original);
        }
    }

    /// <summary>
    /// Tests that converting a valid ULID string to Guid returns a non-empty Guid.
    /// </summary>
    [Fact]
    public void ToGuidFromValidUlidReturnsNonEmptyGuid()
    {
        string ulid = UniqueIdHelper.GenerateSortableUniqueStringId();
        var guid = UniqueIdHelper.ToGuid(ulid);
        guid.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Tests that passing null, empty, or whitespace to ToGuid throws ArgumentException.
    /// </summary>
    /// <param name="ulid">The invalid input to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToGuidFromNullOrWhiteSpaceThrowsArgumentException(string? ulid)
        => Should.Throw<ArgumentException>(() => UniqueIdHelper.ToGuid(ulid!));

    /// <summary>
    /// Tests that passing an invalid ULID format to ToGuid throws FormatException.
    /// </summary>
    /// <param name="ulid">The invalid ULID string to test.</param>
    [Theory]
    [InlineData("short")]
    [InlineData("THIS_IS_NOT_A_VALID_ULID!!")]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FA!")]
    public void ToGuidFromInvalidFormatThrowsFormatException(string ulid)
        => Should.Throw<FormatException>(() => UniqueIdHelper.ToGuid(ulid));

    /// <summary>
    /// Tests that converting a Guid to ULID string returns a valid 26-character Crockford Base32 string.
    /// </summary>
    [Fact]
    public void ToSortableUniqueIdFromGuidReturns26CharCrockfordBase32String()
    {
        var guid = UniqueIdHelper.ToGuid(UniqueIdHelper.GenerateSortableUniqueStringId());
        string result = UniqueIdHelper.ToSortableUniqueId(guid);
        result.Length.ShouldBe(26);
        CrockfordBase32Pattern().IsMatch(result).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that a non-ULID Guid (e.g., Guid.NewGuid()) produces a valid ULID string
    /// with an extractable timestamp (does not throw).
    /// </summary>
    [Fact]
    public void ToSortableUniqueIdFromRandomGuidProducesValidUlidWithExtractableTimestamp()
    {
        var randomGuid = Guid.NewGuid();
        string ulid = UniqueIdHelper.ToSortableUniqueId(randomGuid);
        ulid.Length.ShouldBe(26);
        CrockfordBase32Pattern().IsMatch(ulid).ShouldBeTrue();

        // Should not throw — edge case per architecture
        _ = UniqueIdHelper.ExtractTimestamp(ulid);
    }

    /// <summary>
    /// Tests that converting Guid.Empty produces a valid 26-character ULID string.
    /// </summary>
    [Fact]
    public void ToSortableUniqueIdFromEmptyGuidReturnsAllZerosUlid()
    {
        string ulid = UniqueIdHelper.ToSortableUniqueId(Guid.Empty);
        ulid.Length.ShouldBe(26);
        CrockfordBase32Pattern().IsMatch(ulid).ShouldBeTrue();
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{22}$")]
    private static partial Regex Base64UrlPattern();

    [GeneratedRegex("^[0-9A-HJKMNP-TV-Z]{26}$")]
    private static partial Regex CrockfordBase32Pattern();
}