#!/usr/bin/env bash
# NeoAdmin Playwright E2E：直接运行 ./test.sh 即无头模式跑完全部用例
# 额外参数会透传给 playwright，例如: ./test.sh --grep "登录"

set -euo pipefail

NEOADMIN_ROOT="$(cd "$(dirname "$0")" && pwd)"
PLAYRIGHT_DIR="${NEOADMIN_ROOT}/Playright"

say() {
  echo "$1"
}

require_command() {
  local name="$1"
  if ! command -v "${name}" >/dev/null 2>&1; then
    say "错误：未找到 ${name}，请先安装。"
    exit 1
  fi
}

if [[ ! -d "${PLAYRIGHT_DIR}" ]]; then
  say "错误：未找到 Playright 目录: ${PLAYRIGHT_DIR}"
  exit 1
fi

require_command node
require_command npm
require_command dotnet

if [[ ! -d "${PLAYRIGHT_DIR}/node_modules" ]]; then
  say "首次运行：正在安装 npm 依赖…"
  (cd "${PLAYRIGHT_DIR}" && npm install)
  say "正在安装 Chromium…"
  (cd "${PLAYRIGHT_DIR}" && npx playwright install chromium)
fi

cd "${PLAYRIGHT_DIR}"
exec npm run test -- "$@"
