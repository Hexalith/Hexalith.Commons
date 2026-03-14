// <copyright file="UniqueIdHelper.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.UniqueIds;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

using BaUlid = ByteAether.Ulid.Ulid;

/// <summary>
/// Provides helper methods for generating unique IDs.
/// </summary>
public static partial class UniqueIdHelper
{
    private static readonly Lock _dateTimeLock = new();

    /// <summary>
    /// Generation options for ULID with monotonic increment to ensure within-millisecond ordering.
    /// </summary>
    private static readonly BaUlid.GenerationOptions _ulidOptions = new()
    {
        Monotonicity = BaUlid.GenerationOptions.MonotonicityOptions.MonotonicIncrement,
    };

    private static DateTime _previous = DateTime.MinValue;

    /// <summary>
    /// Extracts the creation timestamp from a ULID string.
    /// </summary>
    /// <param name="ulid">A 26-character ULID string in Crockford Base32 format.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the ULID's creation time in UTC.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ulid"/> is null, empty, or whitespace.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="ulid"/> is not a valid ULID string.</exception>
    public static DateTimeOffset ExtractTimestamp(string ulid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ulid);
        if (!CrockfordBase32Pattern().IsMatch(ulid))
        {
            throw new FormatException($"The value '{ulid}' is not a valid ULID string.");
        }

        return BaUlid.Parse(ulid, CultureInfo.InvariantCulture).Time;
    }

    /// <summary>
    /// Generates a new unique ID based on the current UTC date/time with the format "yyyyMMddHHmmssfff".
    /// It ensures uniqueness by incrementing the time if multiple calls occur within the same millisecond.
    /// </summary>
    /// <returns>A unique 17-character ID string derived from the current date/time.</returns>
    public static string GenerateDateTimeId()
    {
        using (_dateTimeLock.EnterScope())
        {
            DateTime now = DateTime.UtcNow;
            DateTime currentDateTime = new(
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                now.Second,
                now.Millisecond,
                DateTimeKind.Utc
            );
            while (currentDateTime <= _previous)
            {
                currentDateTime = currentDateTime.AddMilliseconds(1);
            }

            _previous = currentDateTime;
            return currentDateTime.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Generates a sortable unique 26-character ID string based on the ULID specification.
    /// ULIDs are chronologically sortable and distributed-safe, making them ideal for
    /// event sourcing, aggregate identifiers, and any use case requiring natural ordering.
    /// </summary>
    /// <returns>A 26-character Crockford Base32 encoded ULID string.</returns>
    public static string GenerateSortableUniqueStringId()
        => BaUlid.New(_ulidOptions).ToString();

    /// <summary>
    /// Generates a unique 22-character ID string derived from a GUID encoded in Base64 URL-safe format.
    /// </summary>
    /// <returns>A 22-character unique ID string.</returns>
    public static string GenerateUniqueStringId() =>
        Convert
            .ToBase64String(Guid.NewGuid().ToByteArray())[..22]
            .Replace("/", "_", StringComparison.InvariantCulture)
            .Replace("+", "-", StringComparison.InvariantCulture);

    /// <summary>
    /// Converts a ULID string to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="ulid">A 26-character Crockford Base32 ULID string.</param>
    /// <returns>A <see cref="Guid"/> preserving the ULID's 128-bit identity.</returns>
    /// <remarks>
    /// The resulting Guid preserves identity but NOT lexicographic sort order.
    /// Two ULIDs that sort correctly as strings may not sort the same way as Guids
    /// due to Guid byte-order differences.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="ulid"/> is null, empty, or whitespace.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="ulid"/> is not a valid ULID string.</exception>
    public static Guid ToGuid(string ulid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ulid);
        if (!CrockfordBase32Pattern().IsMatch(ulid))
        {
            throw new FormatException($"The value '{ulid}' is not a valid ULID string.");
        }

        return BaUlid.Parse(ulid, CultureInfo.InvariantCulture).ToGuid();
    }

    /// <summary>
    /// Converts a <see cref="Guid"/> to a 26-character ULID string.
    /// </summary>
    /// <param name="value">The Guid to convert.</param>
    /// <returns>A 26-character Crockford Base32 ULID string.</returns>
    /// <remarks>
    /// When called with a Guid not originally derived from a ULID (e.g., <see cref="Guid.NewGuid()"/>),
    /// the result is a valid ULID string but its embedded timestamp is meaningless.
    /// </remarks>
    public static string ToSortableUniqueId(Guid value)
        => BaUlid.New(value).ToString();

    [GeneratedRegex("^[0-9A-HJKMNP-TV-Z]{26}$")]
    private static partial Regex CrockfordBase32Pattern();
}