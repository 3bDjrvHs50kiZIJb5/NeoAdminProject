#!/usr/bin/env python3
"""将 NeoAdmin 宿主项目同步到 dotnet new 模板 content/NeoAdminApp。"""

from __future__ import annotations

import re
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "NeoAdmin"
TARGET = Path(__file__).resolve().parent / "content" / "NeoAdminApp"
CURSOR_RULES = ROOT / ".cursor" / "rules"
# monorepo 根规则中不同步到 dotnet new 模板的条目（其他项目专用）
SKIP_CURSOR_RULES = {"novolab-seed-core.mdc"}
BLAZOR_CSPROJ = ROOT / "NeoAdmin.Blazor" / "NeoAdmin.Blazor.csproj"

# 从 NeoAdmin 同步到模板时跳过的文件名（模板保留自己的版本）
SKIP_SYNC_FILENAMES = {
    "tailwind.config.js",  # 模板用 NuGet，不扫描 ../NeoAdmin.Blazor 源码
}

# 不同步（模板专用：NuGet 引用、单项目 Docker、Fody）
KEEP_TEMPLATE_ONLY = {
    "NeoAdminApp.csproj",
    # 宿主工程文件不应复制到模板，模板始终使用 NeoAdminApp.csproj
    "NeoAdmin.csproj",
    "Dockerfile",
    "docker-compose.yaml",
    "FodyWeavers.xml",
    "FodyWeavers.xsd",
    "appsettings.Development.json",
}

SKIP_DIRS = {"bin", "obj", "Logs"}
SKIP_SUFFIXES = (".db", ".db-shm", ".db-wal")

PROTECTED = (
    ("NeoAdmin.Blazor", "\0BLAZOR\0"),
    ("NeoAdmin.Templates", "\0TEMPLATES\0"),
    ("NeoAdmin.Auth", "\0AUTH\0"),
)


def should_skip(path: Path) -> bool:
    if any(part in SKIP_DIRS for part in path.parts):
        return True
    name = path.name
    if name in KEEP_TEMPLATE_ONLY or name in SKIP_SYNC_FILENAMES:
        return True
    return any(name.endswith(s) for s in SKIP_SUFFIXES)


def transform_text(text: str) -> str:
    for original, token in PROTECTED:
        text = text.replace(original, token)

    text = text.replace("NeoAdmin.csproj", "NeoAdminApp.csproj")
    text = text.replace("NeoAdmin.dll", "NeoAdminApp.dll")

    text = re.sub(r"\bnamespace NeoAdmin\.", "namespace NeoAdminApp.", text)
    text = re.sub(r"\busing NeoAdmin\.", "using NeoAdminApp.", text)
    text = re.sub(r"@namespace NeoAdmin\.", "@namespace NeoAdminApp.", text)
    text = re.sub(r"@using NeoAdmin\.", "@using NeoAdminApp.", text)
    text = re.sub(
        r"= NeoAdmin\.(Entities|Services|Api|Jobs|SeedData|Components)",
        r"= NeoAdminApp.\1",
        text,
    )
    text = text.replace(
        "MapRazorComponents<NeoAdmin.App>()",
        "MapRazorComponents<NeoAdminApp.App>()",
    )
    text = text.replace("<NeoAdmin.Routes", "<NeoAdminApp.Routes")

    for original, token in PROTECTED:
        text = text.replace(token, original)

    return text


def transform_dev_guide(text: str) -> str:
    """宿主 NEOADMIN开发上手.md → 模板项目（NeoAdminApp、NuGet 消费者）。"""
    text = transform_text(text)
    text = text.replace(
        "业务扩展在宿主项目 **NeoAdmin**（本仓库示例）中完成",
        "业务扩展在宿主项目 **NeoAdminApp**（本模板项目）中完成",
    )
    text = re.sub(
        r"\npython3 NeoAdmin\.Templates/sync-from-neoadmin\.py\n",
        "\n",
        text,
    )
    return text


def transform_cursor_rule(text: str) -> str:
    """monorepo 根 .cursor/rules → 模板项目（NeoAdminApp、NuGet 消费者）。"""
    text = transform_text(text)
    text = text.replace(
        "（monorepo 为 `../NeoAdmin.Blazor/`，模板项目为 NuGet）",
        "（NuGet）",
    )
    text = text.replace(
        "- monorepo 维护宿主后同步模板：`python3 NeoAdmin.Templates/sync-from-neoadmin.py`\n",
        "",
    )
    text = text.replace(
        "- monorepo 维护宿主后：`python3 NeoAdmin.Templates/sync-from-neoadmin.py`\n",
        "",
    )
    text = text.replace(
        "完整文档见 monorepo 根目录或 `NeoAdmin/NEOADMIN开发上手.md`；宿主项目内见根目录 `NEOADMIN开发上手.md`。",
        "完整文档见本项目根目录 `NEOADMIN开发上手.md` 或 `README.md`。",
    )
    text = text.replace(
        "完整文档见本项目根目录 `NEOADMIN开发上手.md` 或 `README.md`。",
        "完整文档见本项目根目录 `NEOADMIN开发上手.md` 或 `README.md`。",
    )
    text = text.replace(
        "完整文档见 monorepo 根目录 `NEOADMIN开发上手.md` 或本项目 `README.md`。",
        "完整文档见本项目根目录 `NEOADMIN开发上手.md` 或 `README.md`。",
    )
    return text


def read_blazor_version() -> str:
    content = BLAZOR_CSPROJ.read_text(encoding="utf-8")
    m = re.search(r"<Version>([^<]+)</Version>", content)
    if not m:
        raise SystemExit(f"未在 {BLAZOR_CSPROJ} 中找到 <Version>")
    return m.group(1).strip()


def patch_csproj_package_version(version: str) -> None:
    csproj = TARGET / "NeoAdminApp.csproj"
    text = csproj.read_text(encoding="utf-8")
    text, n = re.subn(
        r'(<PackageReference Include="NeoAdmin\.Blazor" Version=")[^"]+(")',
        rf"\g<1>{version}\2",
        text,
        count=1,
    )
    if n != 1:
        raise SystemExit("NeoAdminApp.csproj 中未找到 NeoAdmin.Blazor PackageReference")
    csproj.write_text(text, encoding="utf-8")


def sync_file(src: Path, dst: Path) -> None:
    text_suffixes = {".cs", ".razor", ".json", ".sh", ".yaml", ".yml", ".mdc"}
    if src.suffix in text_suffixes or src.name in {
        "Dockerfile",
        "_Imports.razor",
        "Routes.razor",
        "App.razor",
        "NEOADMIN开发上手.md",
    }:
        raw = src.read_text(encoding="utf-8")
        rel = src.relative_to(SOURCE).as_posix()
        if rel.startswith(".cursor/rules/"):
            raw = transform_cursor_rule(raw)
        elif src.name == "NEOADMIN开发上手.md":
            raw = transform_dev_guide(raw)
        else:
            raw = transform_text(raw)
        dst.parent.mkdir(parents=True, exist_ok=True)
        dst.write_text(raw, encoding="utf-8")
    else:
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)


def remove_stale(target_rel: Path, source_files: set[Path]) -> None:
    full = TARGET / target_rel
    if not full.exists() or full.is_dir():
        return
    if target_rel.as_posix() not in source_files and full.name not in KEEP_TEMPLATE_ONLY:
        full.unlink()


def sync_cursor_rules() -> int:
    """将 monorepo 根 .cursor/rules 同步到模板（排除 NovoLab 等专用规则）。"""
    if not CURSOR_RULES.is_dir():
        return 0

    dst_dir = TARGET / ".cursor" / "rules"
    dst_dir.mkdir(parents=True, exist_ok=True)
    synced: set[str] = set()
    for src in sorted(CURSOR_RULES.glob("*.mdc")):
        if src.name in SKIP_CURSOR_RULES:
            continue
        raw = transform_cursor_rule(src.read_text(encoding="utf-8"))
        (dst_dir / src.name).write_text(raw, encoding="utf-8")
        synced.add(src.name)

    for dst in dst_dir.glob("*.mdc"):
        if dst.name not in synced:
            dst.unlink()

    return len(synced)


def main() -> int:
    if not SOURCE.is_dir():
        print(f"源目录不存在: {SOURCE}", file=sys.stderr)
        return 1

    copied: set[str] = set()
    for src in sorted(SOURCE.rglob("*")):
        if not src.is_file() or should_skip(src):
            continue
        rel = src.relative_to(SOURCE)
        if rel.name in KEEP_TEMPLATE_ONLY:
            continue
        dst = TARGET / rel
        sync_file(src, dst)
        copied.add(rel.as_posix())

    # 删除源中已不存在的文件（保留模板专用文件）
    if TARGET.is_dir():
        for dst in TARGET.rglob("*"):
            if not dst.is_file():
                continue
            rel = dst.relative_to(TARGET)
            if rel.name in KEEP_TEMPLATE_ONLY or rel.name in SKIP_SYNC_FILENAMES:
                continue
            if rel.as_posix() not in copied:
                dst.unlink()

    rule_count = sync_cursor_rules()

    version = read_blazor_version()
    patch_csproj_package_version(version)
    print(f"已同步 {len(copied)} 个文件 → {TARGET}")
    if rule_count:
        print(f"已同步 {rule_count} 条 Cursor 规则 → {TARGET / '.cursor' / 'rules'}")
    print(f"NeoAdmin.Blazor PackageReference 版本: {version}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
