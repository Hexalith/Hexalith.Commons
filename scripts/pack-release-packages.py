#!/usr/bin/env python3
"""Pack the exact Hexalith.Commons NuGet packages published by release."""

from __future__ import annotations

import argparse
import os
import subprocess
import sys
from pathlib import Path


PACKAGE_PROJECTS = [
    "src/libraries/Hexalith.Commons/Hexalith.Commons.csproj",
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
]


def main() -> int:
    parser = argparse.ArgumentParser(description="Pack Hexalith.Commons release packages.")
    parser.add_argument("output_directory", type=Path, help="Directory where packages are written.")
    parser.add_argument("version", help="Package version to apply.")
    args = parser.parse_args()

    output_directory = args.output_directory
    output_directory.mkdir(parents=True, exist_ok=True)
    for package in [*output_directory.glob("*.nupkg"), *output_directory.glob("*.snupkg")]:
        package.unlink()

    env = os.environ.copy()
    env.setdefault("DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER", "1")
    env.setdefault("MSBUILDDISABLENODEREUSE", "1")

    for project in PACKAGE_PROJECTS:
        subprocess.run(
            [
                "dotnet",
                "pack",
                project,
                "--no-build",
                "--configuration",
                "Release",
                "--output",
                str(output_directory),
                f"-p:Version={args.version}",
                f"-p:FileVersion={args.version}",
                "-p:GeneratePackageOnBuild=false",
                "-p:IDEBuild=false",
                "/m:1",
                "/nr:false",
            ],
            check=True,
            env=env,
        )

    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except subprocess.CalledProcessError as exc:
        print(f"Package packing failed with exit code {exc.returncode}.", file=sys.stderr)
        raise SystemExit(exc.returncode)
