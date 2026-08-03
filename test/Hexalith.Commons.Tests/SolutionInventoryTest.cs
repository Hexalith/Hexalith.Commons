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
    private static readonly string[] OwnedProjectRoots = ["src", "test"];

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

        projectPaths.ShouldBe(EnumerateOwnedProjectFiles(repositoryRoot));
        solution.Descendants()
            .Where(element => element.Name.LocalName is "Project" or "File")
            .Select(element => NormalizePath((string?)element.Attribute("Path")))
            .ShouldAllBe(path => !path.StartsWith("references/", StringComparison.Ordinal));
    }

    private static string[] EnumerateOwnedProjectFiles(string repositoryRoot) =>
        [
            .. OwnedProjectRoots
                .SelectMany(root => Directory.EnumerateFiles(
                    Path.Combine(repositoryRoot, root),
                    "*.csproj",
                    SearchOption.AllDirectories))
                .Where(path => !NormalizePath(Path.GetRelativePath(repositoryRoot, path))
                    .Split('/')
                    .Any(segment => segment is "bin" or "obj"))
                .Select(path => NormalizePath(Path.GetRelativePath(repositoryRoot, path)))
                .Order(StringComparer.Ordinal),
        ];

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
