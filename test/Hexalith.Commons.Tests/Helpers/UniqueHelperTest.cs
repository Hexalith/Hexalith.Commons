// <copyright file="UniqueHelperTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Helpers;

using Hexalith.Commons.Objects;

using Shouldly;

public class UniqueHelperTest
{
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

    [Fact]
    public void GetDateTimeIdStringReturns17Chars()
    {
        string id = UniqueIdHelper.GenerateDateTimeId();
        id.Length.ShouldBe(17, "the id is a millisecond precision date time string");
    }

    [Fact]
    public void GetUniqueIdStringReturns22Chars()
    {
        string id = UniqueIdHelper.GenerateUniqueStringId();
        id.Length.ShouldBe(22, "the id is a base64 string of 16 bytes");
    }
}