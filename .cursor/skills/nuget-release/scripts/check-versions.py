#!/usr/bin/env python3
"""校验 NeoAdmin 发布版本在 csproj 中是否一致。"""

import argparse
import re
import sys
from pathlib import Path


def repo_root() -> Path:
    # scripts -> nuget-release -> skills -> .cursor -> repo
    return Path(__file__).resolve().parents[4]


def read_version(path: Path, pattern: re.Pattern[str], label: str) -> str:
    text = path.read_text(encoding="utf-8")
    match = pattern.search(text)
    if not match:
        print(f"错误: 未在 {label} 中找到版本号", file=sys.stderr)
        sys.exit(1)
    return match.group(1)


def main() -> None:
    parser = argparse.ArgumentParser(description="校验 NeoAdmin 发布版本一致性")
    parser.add_argument(
        "--expect",
        help="期望的统一版本号（如 1.0.6）；提供则校验是否全部匹配",
    )
    args = parser.parse_args()

    root = repo_root()
    locations = {
        "NeoAdmin.Blazor": (
            root / "NeoAdmin.Blazor/NeoAdmin.Blazor.csproj",
            re.compile(r"<Version>([^<]+)</Version>"),
        ),
        "NeoAdmin.Templates": (
            root / "NeoAdmin.Templates/NeoAdmin.Templates.csproj",
            re.compile(r"<Version>([^<]+)</Version>"),
        ),
        "NeoAdminApp PackageReference": (
            root / "NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj",
            re.compile(
                r'PackageReference\s+Include="NeoAdmin\.Blazor"\s+Version="([^"]+)"'
            ),
        ),
    }

    versions: dict[str, str] = {}
    for label, (path, pattern) in locations.items():
        if not path.is_file():
            print(f"错误: 文件不存在 {path}", file=sys.stderr)
            sys.exit(1)
        versions[label] = read_version(path, pattern, str(path))

    unique = set(versions.values())
    print("当前版本:")
    for label, version in versions.items():
        print(f"  {label}: {version}")

    if len(unique) != 1:
        print("\n不一致，请统一上述版本号后再发布。", file=sys.stderr)
        sys.exit(1)

    current = next(iter(unique))
    if args.expect and current != args.expect:
        print(
            f"\n期望 {args.expect}，实际为 {current}。",
            file=sys.stderr,
        )
        sys.exit(1)

    print(f"\nOK: 全部为 {current}")
    if args.expect:
        print(f"标签应为: v{args.expect}")


if __name__ == "__main__":
    main()
