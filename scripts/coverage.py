#!/usr/bin/env python3
"""Render the committed coverage inventory and optionally compare it to Close's public reference."""

import argparse
import csv
import json
import re
import subprocess
from pathlib import Path
from urllib.request import urlopen

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
HTTP_METHODS = {"get", "post", "put", "patch", "delete", "head", "options", "trace"}
SPEC_URL = "https://api.close.com/api/openapi.json"
INDEX_URL = "https://developer.close.com/llms.txt"


def read_index():
    # curl uses the same public index response as a browser.
    index = subprocess.check_output(["curl", "--fail", "--silent", "--show-error", "--max-time", "30", INDEX_URL], text=True)
    return dict(re.findall(r"^- \[(.+?)\]\(([^)]+)\): API Endpoints >", index, re.MULTILINE))


def check_upstream(rows):
    with urlopen(SPEC_URL, timeout=30) as source:
        paths = json.load(source)["paths"]
    reference = read_index()
    upstream = {}
    titles = set()
    for route, operations in paths.items():
        for method, operation in operations.items():
            if method not in HTTP_METHODS:
                continue
            title = operation["summary"]
            titles.add(title)
            upstream[(method.upper(), route, operation["operationId"])] = reference.get(title, "").removesuffix(".md")

    recorded = {(r["method"], r["route"], r["operation_id"]): r["reference"] for r in rows}
    if upstream != recorded or set(reference) != titles:
        for label, changes in [
            ("New or changed entries", upstream.items() - recorded.items()),
            ("Removed or changed entries", recorded.items() - upstream.items()),
            ("Reference-only titles", set(reference) - titles),
            ("Spec-only titles", titles - set(reference)),
        ]:
            for change in sorted(changes):
                print(f"{label}: {change}")
        raise SystemExit("Close's public reference and the coverage inventory differ.")


def render(rows):
    named = sum(bool(row["sdk_method"]) for row in rows)
    lines = [
        "# Endpoint coverage",
        "",
        f"{named} of {len(rows)} documented REST operations have named methods. "
        "Use `CloseClient.SendAsync` for rows marked **Raw JSON**.",
        "",
        "Routes are relative to `https://api.close.com/api/v1`. OAuth and webhook signature helpers are documented separately.",
        "",
        "| Resource | Method | Route | .NET method |",
        "| --- | --- | --- | --- |",
    ]
    for row in rows:
        method = f"`{row['sdk_method']}`" if row["sdk_method"] else "**Raw JSON**"
        lines.append(f"| {row['resource']} | {row['method']} | [`{row['route']}`]({row['reference']}) | {method} |")
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--write", action="store_true", help="Regenerate the Markdown table.")
    parser.add_argument("--stable", action="store_true", help="Require named methods for every operation.")
    parser.add_argument("--upstream", action="store_true", help="Compare the inventory with Close's current reference and OpenAPI feed.")
    options = parser.parse_args()

    with (DOCS / "endpoint-coverage.csv").open(encoding="utf-8", newline="") as source:
        rows = list(csv.DictReader(source))
    if len(rows) != len({(r["method"], r["route"]) for r in rows}) or not rows:
        raise SystemExit("Coverage inventory is empty or contains duplicate routes.")
    if options.upstream:
        check_upstream(rows)
    if options.stable and any(not row["sdk_method"] for row in rows):
        raise SystemExit("A stable release requires a named method for every documented operation.")
    output = DOCS / "coverage.md"
    expected = render(rows)
    if options.write:
        output.write_text(expected, encoding="utf-8")
    elif not output.exists() or output.read_text(encoding="utf-8") != expected:
        raise SystemExit("Run python3 scripts/coverage.py --write to refresh the coverage table.")


if __name__ == "__main__":
    main()
