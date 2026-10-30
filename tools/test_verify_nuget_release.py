"""Exercise incomplete feeds, delayed indexing, and incorrect published content."""

import io
import unittest
import urllib.error
from unittest.mock import patch
import zipfile

from verify_nuget_release import PACKAGES, verify_release


def package_response(package_id, version="2.30.5", commit="source-sha"):
    stream = io.BytesIO()
    with zipfile.ZipFile(stream, "w") as archive:
        archive.writestr(
            f"{package_id}.nuspec",
            '<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">'
            f"<metadata><id>{package_id}</id><version>{version}</version>"
            f'<repository commit="{commit}" /></metadata></package>',
        )
    return io.BytesIO(stream.getvalue())


class VerifyReleaseTests(unittest.TestCase):
    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_invalid_retry_budget_fails_before_requesting_packages(self, request):
        with self.assertRaisesRegex(ValueError, "At least one"):
            verify_release("2.30.6", attempts=0)
        request.assert_not_called()

    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_all_manifest_packages_must_be_downloadable_from_the_expected_source(self, request):
        request.side_effect = [package_response(name) for name in sorted(PACKAGES)]
        verify_release("2.30.5", "source-sha", attempts=1)
        self.assertEqual(request.call_count, len(PACKAGES))

    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_partial_publication_fails(self, request):
        request.side_effect = [
            package_response(sorted(PACKAGES)[0]),
            urllib.error.HTTPError("url", 404, "Not Found", None, None),
            *[package_response(name) for name in sorted(PACKAGES)[2:]],
        ]
        with self.assertRaisesRegex(RuntimeError, "publication incomplete"):
            verify_release("2.30.5", attempts=1)

    @patch("verify_nuget_release.time.sleep")
    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_indexing_delay_retries_only_the_missing_package(self, request, sleep):
        request.side_effect = [
            package_response(sorted(PACKAGES)[0]),
            urllib.error.HTTPError("url", 404, "Not Found", None, None),
            *[package_response(name) for name in sorted(PACKAGES)[2:]],
            package_response(sorted(PACKAGES)[1]),
        ]
        verify_release("2.30.5", attempts=2, interval=1)
        sleep.assert_called_once_with(1)
        self.assertEqual(request.call_count, len(PACKAGES) + 1)

    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_wrong_version_fails(self, request):
        request.return_value = package_response(sorted(PACKAGES)[0], version="2.30.3")
        with self.assertRaisesRegex(RuntimeError, "identity does not match"):
            verify_release("2.30.5", attempts=1)

    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_wrong_commit_fails(self, request):
        request.return_value = package_response(sorted(PACKAGES)[0], commit="another-sha")
        with self.assertRaisesRegex(RuntimeError, "unexpected source commit"):
            verify_release("2.30.5", "source-sha", attempts=1)

    @patch("verify_nuget_release.urllib.request.urlopen")
    def test_authorization_failure_is_not_retried(self, request):
        request.side_effect = urllib.error.HTTPError("url", 403, "Forbidden", None, None)
        with self.assertRaises(urllib.error.HTTPError):
            verify_release("2.30.5", attempts=2)
        request.assert_called_once()
