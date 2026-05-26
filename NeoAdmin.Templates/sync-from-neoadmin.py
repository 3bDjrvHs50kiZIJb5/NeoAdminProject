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
BLAZOR_CSPROJ = ROOT / "NeoAdmin.Blazor" / "NeoAdmin.Blazor.csproj"

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
    if name in KEEP_TEMPLATE_ONLY:
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
    if src.suffix in {".cs", ".razor", ".json", ".sh", ".yaml", ".yml"} or src.name in {
        "Dockerfile",
        "_Imports.razor",
        "Routes.razor",
        "App.razor",
    }:
        raw = src.read_text(encoding="utf-8")
        dst.parent.mkdir(parents=True, exist_ok=True)
        dst.write_text(transform_text(raw), encoding="utf-8")
    else:
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)


def remove_stale(target_rel: Path, source_files: set[Path]) -> None:
    full = TARGET / target_rel
    if not full.exists() or full.is_dir():
        return
    if target_rel.as_posix() not in source_files and full.name not in KEEP_TEMPLATE_ONLY:
        full.unlink()


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
            if rel.name in KEEP_TEMPLATE_ONLY:
                continue
            if rel.as_posix() not in copied:
                dst.unlink()

    version = read_blazor_version()
    patch_csproj_package_version(version)
    print(f"已同步 {len(copied)} 个文件 → {TARGET}")
    print(f"NeoAdmin.Blazor PackageReference 版本: {version}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
