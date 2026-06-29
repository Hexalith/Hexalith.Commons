// <copyright file="PagedResultTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using Shouldly;

/// <summary>
/// Unit tests for the generic HTTP paging contract.
/// </summary>
public sealed class PagedResultTest
{
    /// <summary>
    /// Stores all paging fields.
    /// </summary>
    [Fact]
    public void PagedResultShouldStorePagingFields()
    {
        PagedResult<string> result = new()
        {
            Items = ["one"],
            Page = 2,
            PageSize = 10,
            TotalCount = 11,
            TotalPages = 2,
        };

        result.Items.ShouldBe(["one"]);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalCount.ShouldBe(11);
        result.TotalPages.ShouldBe(2);
    }
}
