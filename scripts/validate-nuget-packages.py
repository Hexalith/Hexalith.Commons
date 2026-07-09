#!/usr/bin/env python3
"""Validate Hexalith.Commons NuGet packages before publishing."""

from __future__ import annotations

import argparse
import sys
import zipfile
from dataclasses import dataclass
from pathlib import Path
from xml.etree import ElementTree


EXPECTED_PACKAGE_IDS = frozenset(
    {
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
    }
)


@dataclass(frozen=True)
class PackageMetadata:
    package_id: str
    version: str
    has_net10_assets: bool


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Hexalith.Commons package outputs.")
    parser.add_argument("package_directory", type=Path, help="Directory containing .nupkg files.")
    args = parser.parse_args()

    packages = load_packages(args.package_directory)
    package_ids = set(packages)
    missing = sorted(EXPECTED_PACKAGE_IDS - package_ids)
    extra = sorted(package_ids - EXPECTED_PACKAGE_IDS)
    if missing:
        raise ValueError(f"Missing packages: {missing}")
    if extra:
        raise ValueError(f"Unexpected packages: {extra}")

    versions = {package.version for package in packages.values()}
    if len(versions) != 1:
        raise ValueError(f"Expected one shared package version, found {sorted(versions)}")

    missing_assets = sorted(
        package.package_id for package in packages.values() if not package.has_net10_assets
    )
    if missing_assets:
        raise ValueError(f"Packages missing lib/net10.0 assets: {missing_assets}")

    symbol_ids = package_ids_from_symbols(args.package_directory)
    missing_symbols = sorted(EXPECTED_PACKAGE_IDS - symbol_ids)
    if missing_symbols:
        raise ValueError(f"Packages missing .snupkg symbol packages: {missing_symbols}")

    print(f"Validated {len(packages)} Hexalith.Commons packages at version {versions.pop()}.")
    return 0


def load_packages(package_directory: Path) -> dict[str, PackageMetadata]:
    packages: dict[str, PackageMetadata] = {}
    for package_path in sorted(package_directory.glob("*.nupkg")):
        if package_path.name.endswith(".symbols.nupkg"):
            continue
        package = read_package(package_path)
        packages[package.package_id] = package

    if not packages:
        raise ValueError(f"No .nupkg files found in {package_directory}")
    return packages


def read_package(package_path: Path) -> PackageMetadata:
    with zipfile.ZipFile(package_path) as package:
        nuspec_names = [name for name in package.namelist() if name.endswith(".nuspec")]
        if len(nuspec_names) != 1:
            raise ValueError(f"{package_path.name}: expected exactly one .nuspec file")

        root = ElementTree.fromstring(package.read(nuspec_names[0]))
        ns = {"n": root.tag.split("}")[0].strip("{")} if root.tag.startswith("{") else {}
        metadata_path = ".//n:metadata" if ns else ".//metadata"
        metadata = root.find(metadata_path, ns)
        if metadata is None:
            raise ValueError(f"{package_path.name}: missing nuspec metadata")

        package_id = text(metadata, "id", ns, package_path)
        version = text(metadata, "version", ns, package_path)
        has_net10_assets = any(name.startswith("lib/net10.0/") for name in package.namelist())
        return PackageMetadata(package_id, version, has_net10_assets)


def text(metadata: ElementTree.Element, name: str, ns: dict[str, str], package_path: Path) -> str:
    element = metadata.find(f"n:{name}", ns) if ns else metadata.find(name)
    if element is None or element.text is None or not element.text.strip():
        raise ValueError(f"{package_path.name}: missing {name} metadata")
    return element.text.strip()


def package_ids_from_symbols(package_directory: Path) -> set[str]:
    ids: set[str] = set()
    for package_path in sorted(package_directory.glob("*.snupkg")):
        with zipfile.ZipFile(package_path) as package:
            nuspec_names = [name for name in package.namelist() if name.endswith(".nuspec")]
            if len(nuspec_names) != 1:
                raise ValueError(f"{package_path.name}: expected exactly one .nuspec file")
            root = ElementTree.fromstring(package.read(nuspec_names[0]))
            ns = {"n": root.tag.split("}")[0].strip("{")} if root.tag.startswith("{") else {}
            metadata = root.find(".//n:metadata", ns) if ns else root.find(".//metadata")
            if metadata is None:
                raise ValueError(f"{package_path.name}: missing nuspec metadata")
            ids.add(text(metadata, "id", ns, package_path))
    return ids


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Package validation failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
