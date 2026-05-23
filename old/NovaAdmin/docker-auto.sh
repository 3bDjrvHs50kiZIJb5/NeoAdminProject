#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# 已加入 docker 组但当前终端会话未刷新附加组时（常见于 IDE 内终端），用 sg docker 重新执行本脚本
if ! docker info &>/dev/null; then
  if getent group docker | grep -qw "$(id -un)" && ! id -nG | grep -qw docker; then
    echo "当前 shell 未加载 docker 组，正在用 sg docker 重新执行..."
    _sh="${BASH:-/bin/bash}"
    exec sg docker -c "$(printf '%q ' "$_sh" "$0" "$@")"
  fi
fi

# 与 docker-compose.yaml 宿主机端口一致；默认部署端口为 6038
HOST_PORT="${HOST_PORT:-6038}"

mkdir -p Logs keys wwwroot/uploads wwwroot/avatars wwwroot/badge-photos
touch novaadmin.db

for pid in $(lsof -ti:"$HOST_PORT" 2>/dev/null); do
  kill -9 "$pid" 2>/dev/null || true
done

docker compose down --remove-orphans
docker compose build
docker compose up -d
