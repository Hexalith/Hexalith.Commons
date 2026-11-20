#!/usr/bin/env python3
"""Build an isolated consumer against local Hexalith.Commons NuGet packages."""

from __future__ import annotations

import argparse
import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from xml.etree import ElementTree
from xml.sax.saxutils import quoteattr


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
        shutil.copyfile(Path(__file__).resolve().parents[1] / "global.json", root / "global.json")
        write_nuget_config(root, args.package_directory)
        write_consumer_project(root, versions)
        write_consumer_source(root)
        assert_package_only(root / "Consumer.csproj")
        isolate_nuget(root)
        project = str(root / "Consumer.csproj")
        restore_config = str(root / "NuGet.Config")
        restore(project, restore_config)
        subprocess.run(["dotnet", "build", project, "--no-restore", "--configuration", "Release",
                        "-noAutoResponse", "-warnaserror",
                        "-p:UseHexalithProjectReferences=false",
                        "-p:RestoreConfigFile=" + restore_config], check=True)

        # The quickstart installs only the core package. Keep the full-family
        # consumer above to catch dependency conflicts across the other packages.
        sample = root / "quickstart"
        sample.mkdir()
        write_consumer_project(sample, {"Hexalith.Commons": versions["Hexalith.Commons"]}, executable=True)
        sample_project = str(sample / "Consumer.csproj")
        restore(sample_project, restore_config)
        repo = Path(__file__).resolve().parents[1]
        for readme, heading in (
            (repo / "README.md", "### String Utilities"),
            (repo / "src/libraries/Hexalith.Commons/README.md", "#### Named Placeholder Formatting"),
        ):
            run_example(sample, readme, heading)

    print("Validated all package references and both documented core examples.")
    return 0


def restore(project: str, config: str) -> None:
    subprocess.run(["dotnet", "restore", project, "-noAutoResponse", "-warnaserror",
                    "-p:UseHexalithProjectReferences=false", "--configfile", config], check=True)


def run_example(directory: Path, readme: Path, heading: str) -> None:
    document = readme.read_text(encoding="utf-8-sig")
    section = re.search(rf"^{re.escape(heading)}$\n(.*?)(?=^#|\Z)", document, re.MULTILINE | re.DOTALL)
    if section is None:
        raise ValueError(f"{readme}: missing {heading}")
    code = re.findall(r"^```csharp\n(.*?)\n```$", section[1], re.MULTILINE | re.DOTALL)
    output = re.findall(r"^```text\n(.*?)\n```$", section[1], re.MULTILINE | re.DOTALL)
    if len(code) != 1 or len(output) != 1:
        raise ValueError(f"{readme}: expected one C# example and one expected output under {heading}")
    (directory / "Program.cs").write_text(code[0] + "\n", encoding="utf-8")
    subprocess.run(["dotnet", "build", "Consumer.csproj", "--no-restore", "-c", "Release"],
                   cwd=directory, check=True)
    result = subprocess.check_output(["dotnet", "bin/Release/net11.0/Consumer.dll"], cwd=directory, text=True)
    if result.splitlines() != output[0].splitlines():
        raise ValueError(f"{readme}: expected {output[0]!r}, got {result!r}")


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
    local_patterns = "\n".join(f'      <package pattern="{name}" />' for name in PACKAGE_IDS)
    root.joinpath("NuGet.Config").write_text(
        f"""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-commons-packages" value={quoteattr(str(package_directory.resolve()))} />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-commons-packages">
{local_patterns}
    </packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
""",
        encoding="utf-8",
    )


def write_consumer_project(root: Path, versions: dict[str, str], *, executable: bool = False) -> None:
    package_references = "\n".join(
        f'    <PackageReference Include="{package_id}" Version="[{version}]" />'
        for package_id, version in versions.items()
    )
    root.joinpath("Consumer.csproj").write_text(
        f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <OutputType>{"Exe" if executable else "Library"}</OutputType>
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


def isolate_nuget(working_directory: Path) -> None:
    os.environ.setdefault("DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER", "1")
    os.environ.setdefault("MSBUILDDISABLENODEREUSE", "1")
    os.environ["NUGET_PACKAGES"] = str(working_directory / ".nuget" / "packages")
    os.environ["NUGET_HTTP_CACHE_PATH"] = str(working_directory / ".nuget" / "http-cache")
    os.environ["NUGET_PLUGINS_CACHE_PATH"] = str(working_directory / ".nuget" / "plugins-cache")
    os.environ["DOTNET_CLI_HOME"] = str(working_directory)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Consumer package validation failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
