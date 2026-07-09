#!/usr/bin/env python3
"""Build an isolated consumer against local Hexalith.Commons NuGet packages."""

from __future__ import annotations

import argparse
import os
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from xml.etree import ElementTree


PACKAGE_IDS = [
    "Hexalith.Commons",
    "Hexalith.Commons.Aspire",
    "Hexalith.Commons.Configurations",
    "Hexalith.Commons.Diagnostics",
    "Hexalith.Commons.Http",
    "Hexalith.Commons.Metadatas",
    "Hexalith.Commons.Publication",
    "Hexalith.Commons.Serialization",
    "Hexalith.Commons.ServiceDefaults",
    "Hexalith.Commons.StringEncoders",
    "Hexalith.Commons.TenantAccess",
    "Hexalith.Commons.UniqueIds",
]


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate package-only Commons consumption.")
    parser.add_argument("package_directory", type=Path, help="Directory containing local packages.")
    args = parser.parse_args()

    versions = package_versions(args.package_directory)
    with tempfile.TemporaryDirectory(prefix="hexalith-commons-consumer-") as temp:
        root = Path(temp)
        write_nuget_config(root, args.package_directory)
        write_consumer_project(root, versions)
        write_consumer_source(root)
        assert_package_only(root / "Consumer.csproj")
        run_dotnet(["restore", "Consumer.csproj"], root)
        run_dotnet(["build", "Consumer.csproj", "--no-restore", "--configuration", "Release"], root)

    print("Validated isolated package-only consumer build.")
    return 0


def package_versions(package_directory: Path) -> dict[str, str]:
    versions: dict[str, str] = {}
    for package_path in sorted(package_directory.glob("*.nupkg")):
        if package_path.name.endswith(".symbols.nupkg"):
            continue

        with zipfile.ZipFile(package_path) as package:
            nuspec_names = [name for name in package.namelist() if name.endswith(".nuspec")]
            if len(nuspec_names) != 1:
                raise ValueError(f"{package_path.name}: expected exactly one .nuspec file")

            root = ElementTree.fromstring(package.read(nuspec_names[0]))
            ns = {"n": root.tag.split("}")[0].strip("{")} if root.tag.startswith("{") else {}
            metadata = root.find(".//n:metadata", ns) if ns else root.find(".//metadata")
            if metadata is None:
                raise ValueError(f"{package_path.name}: missing nuspec metadata")
            package_id = text(metadata, "id", ns, package_path)
            version = text(metadata, "version", ns, package_path)
            versions[package_id] = version

    missing = sorted(set(PACKAGE_IDS) - set(versions))
    if missing:
        raise ValueError(f"Missing local packages required for consumer validation: {missing}")

    distinct_versions = {versions[package_id] for package_id in PACKAGE_IDS}
    if len(distinct_versions) != 1:
        raise ValueError(f"Expected Commons packages to share one version, found {sorted(distinct_versions)}")
    return versions


def text(metadata: ElementTree.Element, name: str, ns: dict[str, str], package_path: Path) -> str:
    element = metadata.find(f"n:{name}", ns) if ns else metadata.find(name)
    if element is None or element.text is None or not element.text.strip():
        raise ValueError(f"{package_path.name}: missing {name} metadata")
    return element.text.strip()


def write_nuget_config(root: Path, package_directory: Path) -> None:
    root.joinpath("NuGet.Config").write_text(
        f"""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-commons-packages" value="{package_directory.resolve()}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
""",
        encoding="utf-8",
    )


def write_consumer_project(root: Path, versions: dict[str, str]) -> None:
    package_references = "\n".join(
        f'    <PackageReference Include="{package_id}" Version="{versions[package_id]}" />'
        for package_id in PACKAGE_IDS
    )
    root.joinpath("Consumer.csproj").write_text(
        f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
{package_references}
  </ItemGroup>
</Project>
""",
        encoding="utf-8",
    )


def write_consumer_source(root: Path) -> None:
    root.joinpath("Consumer.cs").write_text(
        """namespace Hexalith.Commons.ConsumerValidation;

public static class Consumer
{
    public static string Name => "Hexalith.Commons";
}
""",
        encoding="utf-8",
    )


def assert_package_only(project_file: Path) -> None:
    project_text = project_file.read_text(encoding="utf-8")
    if "ProjectReference" in project_text:
        raise ValueError(f"{project_file}: consumer project must not use ProjectReference")

    for package_id in PACKAGE_IDS:
        if f'PackageReference Include="{package_id}"' not in project_text:
            raise ValueError(f"{project_file}: missing PackageReference for {package_id}")


def run_dotnet(args: list[str], working_directory: Path) -> None:
    env = os.environ.copy()
    env.setdefault("DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER", "1")
    env.setdefault("MSBUILDDISABLENODEREUSE", "1")
    env["NUGET_PACKAGES"] = str(working_directory.parent / ".nuget" / "packages")
    subprocess.run(["dotnet", *args], cwd=working_directory, check=True, env=env)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Consumer package validation failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
