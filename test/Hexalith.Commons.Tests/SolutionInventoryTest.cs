// <copyright file="SolutionInventoryTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests;

using System.Xml.Linq;

using Shouldly;

/// <summary>
/// Verifies the governance-only standalone solution inventory.
/// </summary>
public class SolutionInventoryTest
{
    private static readonly string[] ExpectedProjects =
    [
        "src/libraries/Hexalith.Commons.Aspire/Hexalith.Commons.Aspire.csproj",
        "src/libraries/Hexalith.Commons.Configurations/Hexalith.Commons.Configurations.csproj",
        "src/libraries/Hexalith.Commons.Diagnostics/Hexalith.Commons.Diagnostics.csproj",
        "src/libraries/Hexalith.Commons.Http/Hexalith.Commons.Http.csproj",
        "src/libraries/Hexalith.Commons.Metadatas/Hexalith.Commons.Metadatas.csproj",
        "src/libraries/Hexalith.Commons.Publication/Hexalith.Commons.Publication.csproj",
        "src/libraries/Hexalith.Commons.Serialization/Hexalith.Commons.Serialization.csproj",
        "src/libraries/Hexalith.Commons.ServiceDefaults/Hexalith.Commons.ServiceDefaults.csproj",
        "src/libraries/Hexalith.Commons.StringEncoders/Hexalith.Commons.StringEncoders.csproj",
        "src/libraries/Hexalith.Commons.TenantAccess/Hexalith.Commons.TenantAccess.csproj",
        "src/libraries/Hexalith.Commons.UniqueIds/Hexalith.Commons.UniqueIds.csproj",
        "src/libraries/Hexalith.Commons/Hexalith.Commons.csproj",
        "test/Hexalith.Commons.Aspire.Tests/Hexalith.Commons.Aspire.Tests.csproj",
        "test/Hexalith.Commons.Diagnostics.Tests/Hexalith.Commons.Diagnostics.Tests.csproj",
        "test/Hexalith.Commons.Http.Tests/Hexalith.Commons.Http.Tests.csproj",
        "test/Hexalith.Commons.Publication.Tests/Hexalith.Commons.Publication.Tests.csproj",
        "test/Hexalith.Commons.Serialization.Tests/Hexalith.Commons.Serialization.Tests.csproj",
        "test/Hexalith.Commons.ServiceDefaults.Tests/Hexalith.Commons.ServiceDefaults.Tests.csproj",
        "test/Hexalith.Commons.TenantAccess.Tests/Hexalith.Commons.TenantAccess.Tests.csproj",
        "test/Hexalith.Commons.Tests/Hexalith.Commons.Tests.csproj",
    ];

    /// <summary>
    /// Verifies that the standalone solution contains every owned project and no dependency paths.
    /// </summary>
    [Fact]
    public void StandaloneSolutionShouldContainExactlyOwnedProjects()
    {
        string repositoryRoot = FindRepositoryRoot();
        var solution = XDocument.Load(Path.Combine(repositoryRoot, "Hexalith.Commons.Standalone.slnx"));
        string[] projectPaths =
        [
            .. solution.Descendants("Project")
            .Select(element => NormalizePath((string?)element.Attribute("Path")))
            .Order(StringComparer.Ordinal),
        ];

        projectPaths.ShouldBe(ExpectedProjects.Order(StringComparer.Ordinal));
        solution.Descendants()
            .Where(element => element.Name.LocalName is "Project" or "File")
            .Select(element => NormalizePath((string?)element.Attribute("Path")))
            .ShouldAllBe(path => !path.StartsWith("references/", StringComparison.Ordinal));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Hexalith.Commons.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the Hexalith.Commons repository root.");
    }

    private static string NormalizePath(string? path) =>
        (path ?? throw new InvalidDataException("A solution entry is missing its Path attribute."))
            .Replace('\\', '/');
}
