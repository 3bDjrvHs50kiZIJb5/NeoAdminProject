#!/usr/bin/env python3
"""从 Git 提交记录生成 Data/git-changelog.json，供发布后环境读取。"""

from __future__ import annotations

import json
import subprocess
import sys
from datetime import datetime
from pathlib import Path


def find_git_root(start: Path) -> Path | None:
    current = start.resolve()
    while True:
        if (current / ".git").exists():
            return current
        if current.parent == current:
            return None
        current = current.parent


def parse_git_date(value: str) -> datetime:
    text = value.strip()
    if len(text) > 6 and text[-6] == " " and text[-5] in "+-":
        text = text[:-6] + text[-5:]
    return datetime.fromisoformat(text)


def read_git_entries(repo_root: Path, max_count: int) -> list[dict[str, str]]:
    field_sep = "\x1e"
    record_sep = "\x1d"
    fmt = f"%H{field_sep}%ai{field_sep}%s{field_sep}%b{record_sep}"
    result = subprocess.run(
        ["git", "-C", str(repo_root), "log", f"-{max_count}", f"--format={fmt}"],
        capture_output=True,
        text=True,
        check=False,
    )
    if result.returncode != 0:
        if result.stderr.strip():
            print(result.stderr.strip(), file=sys.stderr)
        return []

    entries: list[dict[str, str]] = []
    records = [record for record in result.stdout.split(record_sep) if record]
    for record in records:
        parts = record.split(field_sep)
        if len(parts) < 4:
            continue

        commit_hash = parts[0].strip()
        if not commit_hash:
            continue

        try:
            committed_at = parse_git_date(parts[1])
        except ValueError:
            continue

        subject = parts[2].strip()
        body = parts[3].strip()
        if not subject:
            continue

        entries.append(
            {
                "shortHash": commit_hash[:7],
                "committedAt": committed_at.isoformat(),
                "subject": subject,
                "body": body,
            }
        )

    return entries


def load_existing_entries(output: Path) -> list[dict[str, str]]:
    if not output.is_file():
        return []

    try:
        data = json.loads(output.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return []

    return data if isinstance(data, list) and len(data) > 0 else []


def main() -> int:
    project_root = Path(__file__).resolve().parent.parent
    output = Path(sys.argv[1]) if len(sys.argv) > 1 else project_root / "Data/git-changelog.json"
    max_count = int(sys.argv[2]) if len(sys.argv) > 2 else 50

    repo_root = find_git_root(project_root)
    entries = read_git_entries(repo_root, max_count) if repo_root is not None else []

    if not entries:
        existing = load_existing_entries(output)
        if existing:
            print(
                f"未找到 Git 历史，保留已有更新日志（{len(existing)} 条）：{output}",
                file=sys.stderr,
            )
            return 0

        if repo_root is None:
            print("未找到 Git 仓库且无已有更新日志文件。", file=sys.stderr)

    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(entries, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
