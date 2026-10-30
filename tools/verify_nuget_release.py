"""Download all manifest release packages and verify their identities and source commit."""

import argparse
import io
import json
from pathlib import Path
import re
import time
import urllib.error
import urllib.request
import xml.etree.ElementTree as ET
import zipfile


PACKAGES = tuple(
    package["id"]
    for package in json.loads(
        (Path(__file__).parent / "release-packages.json").read_text(encoding="utf-8")
    )["packages"]
)


def verify_release(version, commit=None, attempts=180, interval=10):
    """Wait for NuGet indexing; fail if any archive is missing or incorrect."""
    if not re.fullmatch(r"\d+\.\d+\.\d+", version):
        raise ValueError("A stable release version such as 2.30.5 is required.")
    if attempts < 1:
        raise ValueError("At least one download attempt is required.")
    pending = set(PACKAGES)
    for attempt in range(attempts):
        for package_id in sorted(pending):
            name = package_id.lower()
            url = f"https://api.nuget.org/v3-flatcontainer/{name}/{version}/{name}.{version}.nupkg"
            try:
                with urllib.request.urlopen(url, timeout=30) as response:
                    content = response.read()
            except urllib.error.HTTPError as error:
                if error.code not in (404, 408, 429) and error.code < 500:
                    raise
                print(f"Waiting for {package_id} {version}: HTTP {error.code}", flush=True)
                continue
            except (urllib.error.URLError, TimeoutError) as error:
                print(f"Waiting for {package_id} {version}: {error}", flush=True)
                continue
            with zipfile.ZipFile(io.BytesIO(content)) as archive:
                nuspecs = [path for path in archive.namelist() if path.endswith(".nuspec")]
                if len(nuspecs) != 1:
                    raise RuntimeError(f"Expected one nuspec in {package_id} {version}.")
                root = ET.fromstring(archive.read(nuspecs[0]))
            metadata = root.find("{*}metadata")
            if metadata is None or (
                metadata.findtext("{*}id") != package_id
                or metadata.findtext("{*}version") != version
            ):
                raise RuntimeError(f"Downloaded package identity does not match {package_id} {version}.")
            repository = metadata.find("{*}repository")
            if commit and (repository is None or repository.get("commit") != commit):
                raise RuntimeError(f"Downloaded {package_id} {version} has an unexpected source commit.")
            pending.remove(package_id)
            print(f"Verified {package_id} {version}: {url}", flush=True)
        if not pending:
            return
        if attempt + 1 < attempts:
            time.sleep(interval)
    raise RuntimeError(f"NuGet publication incomplete for {version}: {', '.join(sorted(pending))}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("version")
    parser.add_argument("--commit", help="Expected repository commit recorded in each nuspec.")
    parser.add_argument("--attempts", type=int, default=180, help="Download attempts, ten seconds apart (default: 180).")
    arguments = parser.parse_args()
    verify_release(arguments.version, arguments.commit, attempts=arguments.attempts)
