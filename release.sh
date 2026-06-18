#!/usr/bin/env bash
# NeoAdmin 发布工作流（本地门禁 + 云端 NuGet / Docker）
#
#   ./release.sh                  自动 patch +1（无标签则从 1.0.0 开始）
#   ./release.sh 1.0.35           手动指定版本
#   ./release.sh --bump minor       自动 minor +1
#
# 流程:
#   1. 检查工作区与分支
#   2. 同步模板并更新 csproj 版本号
#   3. 本地 E2E：NeoAdmin/test.sh
#   4. Release 预构建（pack NuGet + publish 宿主，与 CI Docker 一致）
#   5. 推送 main → git tag v* 并 push → 触发 GitHub Actions
#
# 推送 v* 标签后触发:
#   - .github/workflows/publish-nuget.yml  → nuget.org
#   - .github/workflows/publish-docker.yml → ghcr.io
#
# 选项:
#   --bump <patch|minor|major>  自动累加版本（默认 patch）
#   --dry-run                   只打印将执行的步骤，不实际测试/提交/打标签/推送
#   --skip-test                 跳过 E2E（仅紧急情况，默认不推荐）

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"
DRY_RUN=false
SKIP_TEST=false
AUTO_VERSION=false
BUMP_MODE="patch"
VERSION=""

BLAZOR_CSPROJ="${REPO_ROOT}/NeoAdmin.Blazor/NeoAdmin.Blazor.csproj"
TEMPLATES_CSPROJ="${REPO_ROOT}/NeoAdmin.Templates/NeoAdmin.Templates.csproj"
SYNC_SCRIPT="${REPO_ROOT}/NeoAdmin.Templates/sync-from-neoadmin.py"
TEST_SCRIPT="${REPO_ROOT}/NeoAdmin/test.sh"
HOST_CSPROJ="${REPO_ROOT}/NeoAdmin/NeoAdmin.csproj"
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"

say() {
  echo "$1"
}

die() {
  say "错误：$1" >&2
  exit 1
}

run() {
  if [[ "${DRY_RUN}" == true ]]; then
    say "[dry-run] $*"
  else
    say "→ $*"
    "$@"
  fi
}

usage() {
  cat <<'EOF'
用法: ./release.sh [选项] [版本号]

版本号:
  省略版本号        按最新 v* 标签自动累加（默认 patch +1）
  1.0.0 / v1.0.0    手动指定版本（最终标签统一为 v 前缀）

选项:
  --bump <级别>     自动累加：patch（默认）、minor、major
  --dry-run         预览发布步骤，不执行测试与 git 操作
  --skip-test       跳过本地 E2E（不推荐）
  -h, --help        显示此帮助

自动累加示例（假设最新标签为 v1.0.34）:
  ./release.sh              → v1.0.35
  ./release.sh --bump minor → v1.1.0
  ./release.sh --bump major → v2.0.0
  尚无 v* 标签时            → v1.0.0

发布前建议:
  - 业务代码已提交（工作区干净）
  - 在 main 分支发布
  - 仓库 Settings → Secrets 已配置 NUGET_API_KEY（NuGet 发布）

推送 v* 标签后，GitHub Actions 会发布 NuGet 包与 Docker 镜像。
EOF
}

normalize_version() {
  local raw="$1"
  if [[ ! "${raw}" =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
    die "版本号格式无效: ${raw}（示例: 1.0.0 或 v1.2.3）"
  fi
  if [[ "${raw}" == v* ]]; then
    echo "${raw}"
  else
    echo "v${raw}"
  fi
}

version_without_v() {
  local tag="$1"
  echo "${tag#v}"
}

latest_semver_tag() {
  # grep 无匹配时在 pipefail 下会返回 1，需用 {} 包住 grep || true，避免 || 抢占管道优先级
  git -C "${REPO_ROOT}" tag -l 'v*' --sort=-v:refname \
    | { grep -E '^v[0-9]+\.[0-9]+\.[0-9]+$' || true; } \
    | head -n 1
}

resolve_auto_version() {
  local latest_tag major minor patch

  latest_tag="$(latest_semver_tag)"
  if [[ -z "${latest_tag}" ]]; then
    echo "1.0.0"
    return
  fi

  local ver="${latest_tag#v}"
  IFS='.' read -r major minor patch <<< "${ver}"

  case "${BUMP_MODE}" in
    major)
      echo "$((major + 1)).0.0"
      ;;
    minor)
      echo "${major}.$((minor + 1)).0"
      ;;
    patch)
      echo "${major}.${minor}.$((patch + 1))"
      ;;
    *)
      die "未知的 --bump 级别: ${BUMP_MODE}（可选: patch、minor、major）"
      ;;
  esac
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --dry-run)
        DRY_RUN=true
        shift
        ;;
      --skip-test)
        SKIP_TEST=true
        shift
        ;;
      --bump)
        [[ $# -ge 2 ]] || die "--bump 需要指定 patch、minor 或 major"
        BUMP_MODE="$2"
        AUTO_VERSION=true
        shift 2
        ;;
      -h|--help)
        usage
        exit 0
        ;;
      -*)
        die "未知选项: $1"
        ;;
      *)
        if [[ -n "${VERSION}" ]]; then
          die "只能指定一个版本号"
        fi
        VERSION="$1"
        shift
        ;;
    esac
  done

  if [[ -z "${VERSION}" ]]; then
    AUTO_VERSION=true
    VERSION="$(resolve_auto_version)"
  fi
}

require_command() {
  local name="$1"
  command -v "${name}" >/dev/null 2>&1 || die "未找到 ${name}"
}

check_git_state() {
  require_command git

  if ! git -C "${REPO_ROOT}" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    die "当前目录不是 git 仓库"
  fi

  local branch
  branch="$(git -C "${REPO_ROOT}" branch --show-current)"
  if [[ -z "${branch}" ]]; then
    die "当前处于 detached HEAD，请先切换到分支（建议 main）"
  fi

  if [[ "${branch}" != "main" && "${branch}" != "master" ]]; then
    say "警告：当前分支为 ${branch}，通常应在 main/master 上打发布标签。"
    if [[ "${DRY_RUN}" == false ]]; then
      read -r -p "是否继续? [y/N] " confirm
      [[ "${confirm}" == [yY] ]] || die "已取消"
    fi
  fi

  if [[ -n "$(git -C "${REPO_ROOT}" status --porcelain --untracked-files=no)" ]]; then
    say "错误：以下已跟踪文件有未提交改动，请先 commit 后再发布：" >&2
    git -C "${REPO_ROOT}" status --short --untracked-files=no >&2
    exit 1
  fi

  local untracked
  untracked="$(git -C "${REPO_ROOT}" status --porcelain --untracked-files=normal | grep '^??' || true)"
  if [[ -n "${untracked}" ]]; then
    say "提示：工作区有未跟踪文件（不影响发布）："
    say "${untracked}"
  fi

  local tag="${1}"
  if git -C "${REPO_ROOT}" rev-parse "${tag}" >/dev/null 2>&1; then
    die "标签 ${tag} 已存在"
  fi
}

bump_csproj_version() {
  local file="$1"
  local ver="$2"
  [[ -f "${file}" ]] || die "未找到项目文件: ${file}"

  python3 - "${file}" "${ver}" <<'PY'
import re
import sys
from pathlib import Path

path = Path(sys.argv[1])
version = sys.argv[2]
text = path.read_text(encoding="utf-8")
new_text, count = re.subn(
    r"(<Version>)[^<]+(</Version>)",
    rf"\g<1>{version}\g<2>",
    text,
    count=1,
)
if count != 1:
    raise SystemExit(f"未在 {path} 中找到 <Version>")
path.write_text(new_text, encoding="utf-8")
PY
}

sync_and_bump_versions() {
  local ver="$1"

  say ""
  say "=== 1/4 同步模板并更新版本号 ==="
  require_command python3

  [[ -f "${SYNC_SCRIPT}" ]] || die "未找到同步脚本: ${SYNC_SCRIPT}"

  if [[ "${DRY_RUN}" == true ]]; then
    say "[dry-run] 将更新 NeoAdmin.Blazor / NeoAdmin.Templates 版本为 ${ver}"
  else
    bump_csproj_version "${BLAZOR_CSPROJ}" "${ver}"
    bump_csproj_version "${TEMPLATES_CSPROJ}" "${ver}"
    say "已更新 NeoAdmin.Blazor / NeoAdmin.Templates 版本为 ${ver}"
  fi

  run python3 "${SYNC_SCRIPT}"
}

commit_release_changes() {
  local tag="$1"
  local ver
  ver="$(version_without_v "${tag}")"

  if [[ -z "$(git -C "${REPO_ROOT}" status --porcelain)" ]]; then
    say "版本号与模板已是最新，无需额外提交"
    return
  fi

  say "提交发布版本变更…"
  run git -C "${REPO_ROOT}" add \
    "NeoAdmin.Blazor/NeoAdmin.Blazor.csproj" \
    "NeoAdmin.Templates/NeoAdmin.Templates.csproj" \
    "NeoAdmin.Templates/content"
  run git -C "${REPO_ROOT}" commit -m "release: v${ver}"
}

run_local_tests() {
  if [[ "${SKIP_TEST}" == true ]]; then
    say "已跳过本地 E2E（--skip-test）"
    return
  fi

  say ""
  say "=== 2/4 本地 E2E 测试 ==="
  [[ -x "${TEST_SCRIPT}" ]] || die "未找到可执行测试脚本: ${TEST_SCRIPT}"
  run "${TEST_SCRIPT}"
}

run_release_build() {
  local ver="$1"

  say ""
  say "=== 3/4 Release 预构建（NuGet + 宿主 publish，与 CI 一致）==="
  require_command dotnet

  if [[ "${DRY_RUN}" == false ]]; then
    rm -rf "${ARTIFACTS_DIR}"
    mkdir -p "${ARTIFACTS_DIR}"
  fi

  run dotnet pack "${BLAZOR_CSPROJ}" -c Release -o "${ARTIFACTS_DIR}" "/p:Version=${ver}"
  run dotnet pack "${TEMPLATES_CSPROJ}" -c Release -o "${ARTIFACTS_DIR}" "/p:Version=${ver}"
  run dotnet publish "${HOST_CSPROJ}" -c Release -o "${ARTIFACTS_DIR}/publish"
}

push_main_and_tag() {
  local tag="$1"

  say ""
  say "=== 4/4 推送 main 并打标签 ==="
  say "将推送 main 并创建标签: ${tag}"
  say "推送后将触发 GitHub Actions：NuGet（nuget.org）+ Docker（ghcr.io）"

  if [[ "${DRY_RUN}" == false ]]; then
    read -r -p "确认发布 ${tag}? [y/N] " confirm
    [[ "${confirm}" == [yY] ]] || die "已取消"
  fi

  run git -C "${REPO_ROOT}" push origin HEAD
  run git -C "${REPO_ROOT}" tag -a "${tag}" -m "Release ${tag}"
  run git -C "${REPO_ROOT}" push origin "${tag}"
}

main() {
  parse_args "$@"
  local tag ver
  tag="$(normalize_version "${VERSION}")"
  ver="$(version_without_v "${tag}")"

  say "NeoAdmin 发布: ${tag}"
  if [[ "${AUTO_VERSION}" == true ]]; then
    local latest
    latest="$(latest_semver_tag)"
    if [[ -n "${latest}" ]]; then
      say "（基于 ${latest} 自动 ${BUMP_MODE} +1）"
    else
      say "（尚无 v* 标签，使用首个版本 ${tag}）"
    fi
  fi
  [[ "${DRY_RUN}" == true ]] && say "（dry-run 模式）"

  check_git_state "${tag}"
  sync_and_bump_versions "${ver}"
  commit_release_changes "${tag}"
  run_local_tests
  run_release_build "${ver}"
  push_main_and_tag "${tag}"

  say ""
  if [[ "${DRY_RUN}" == true ]]; then
    say "dry-run 完成。去掉 --dry-run 即可正式发布。"
  else
    say "发布已触发：main 与标签 ${tag} 已推送，请在 GitHub Actions 查看 NuGet / Docker 构建进度。"
  fi
}

main "$@"
