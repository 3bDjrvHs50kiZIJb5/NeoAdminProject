#!/usr/bin/env bash
# 释放 NovaAdmin 开发常用端口 5038。
# 这个脚本不只杀“占端口的子进程”，还会把同项目的 dotnet 宿主一起结束，
# 避免刚杀掉又被 `dotnet run` / `dotnet watch run` 重新拉起来。

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
PORT="${1:-5038}"
PROJECT_FILE="${ROOT}/NovaAdmin.csproj"
APP_NAME="$(basename "${ROOT}")"
APP_BIN_PATTERN="${ROOT}/bin/.*/${APP_NAME}"
PROJECT_RUN_PATTERN="dotnet run.*${PROJECT_FILE}"
WATCH_PATTERN="DOTNET_WATCH=1"
SELF_PID="$$"
PARENT_PID="${PPID:-}"

say() {
  echo "$1"
}

pid_exists() {
  local pid="$1"
  kill -0 "${pid}" 2>/dev/null
}

should_skip_pid() {
  local pid="$1"
  [[ -z "${pid}" ]] && return 0
  [[ "${pid}" == "${SELF_PID}" ]] && return 0
  [[ -n "${PARENT_PID}" ]] && [[ "${pid}" == "${PARENT_PID}" ]] && return 0
  return 1
}

pid_stat() {
  local pid="$1"
  ps -p "${pid}" -o stat= 2>/dev/null | tr -d ' ' || true
}

pid_args() {
  local pid="$1"
  ps -p "${pid}" -o args= 2>/dev/null || true
}

pid_ppid() {
  local pid="$1"
  ps -p "${pid}" -o ppid= 2>/dev/null | tr -d ' ' || true
}

pid_pgid() {
  local pid="$1"
  ps -p "${pid}" -o pgid= 2>/dev/null | tr -d ' ' || true
}

pid_cwd() {
  local pid="$1"
  local cwd_line
  cwd_line="$(lsof -a -p "${pid}" -d cwd 2>/dev/null | tail -1 || true)"
  echo "${cwd_line##* }"
}

kill_one() {
  local pid="$1"
  local reason="$2"
  local stat

  [[ -z "${pid}" ]] && return 0
  should_skip_pid "${pid}" && return 0
  pid_exists "${pid}" || return 0

  say "结束 PID ${pid}：${reason}"
  kill -TERM "${pid}" 2>/dev/null || true
  sleep 0.2

  if pid_exists "${pid}"; then
    kill -KILL "${pid}" 2>/dev/null || true
    sleep 0.3
  fi

  if pid_exists "${pid}"; then
    stat="$(pid_stat "${pid}")"
    say "PID ${pid} 还活着（状态: ${stat:-unknown}）"
  fi
}

kill_group() {
  local pid="$1"
  local pgid

  [[ -z "${pid}" ]] && return 0
  should_skip_pid "${pid}" && return 0
  pgid="$(pid_pgid "${pid}")"
  [[ -z "${pgid}" ]] && return 0
  [[ "${pgid}" == "0" ]] && return 0
  [[ "${pgid}" == "${pid}" ]] || true

  if ps -g "${pgid}" -o pid= 2>/dev/null | grep -q '[0-9]'; then
    say "结束进程组 ${pgid}（来源 PID ${pid}）"
    kill -TERM -"${pgid}" 2>/dev/null || true
    sleep 0.2
    kill -KILL -"${pgid}" 2>/dev/null || true
    sleep 0.3
  fi
}

kill_ancestor_chain() {
  local pid="$1"
  local depth=0
  local parent
  local args

  while [[ -n "${pid}" ]] && [[ "${pid}" != "0" ]] && [[ "${pid}" != "1" ]] && [[ "${depth}" -lt 12 ]]; do
    args="$(pid_args "${pid}")"
    if [[ "${args}" == *"${PROJECT_FILE}"* ]] || [[ "${args}" == *"${ROOT}/bin/"*"${APP_NAME}"* ]] || [[ "${args}" == *"dotnet run"* ]] || [[ "${args}" == *"${WATCH_PATTERN}"* ]]; then
      kill_one "${pid}" "项目相关宿主"
    fi
    parent="$(pid_ppid "${pid}")"
    [[ "${parent}" == "${pid}" ]] && break
    pid="${parent}"
    depth=$((depth + 1))
  done
}

kill_project_matches() {
  local pattern="$1"
  local reason="$2"
  local pid

  while IFS= read -r pid; do
    [[ -z "${pid}" ]] && continue
    should_skip_pid "${pid}" && continue
    kill_group "${pid}"
    kill_one "${pid}" "${reason}"
  done < <(pgrep -f "${pattern}" 2>/dev/null || true)
}

kill_project_cwd_processes() {
  local pid
  local cwd
  local args

  while IFS= read -r pid; do
    [[ -z "${pid}" ]] && continue
    should_skip_pid "${pid}" && continue
    cwd="$(pid_cwd "${pid}")"
    [[ "${cwd}" != "${ROOT}" ]] && continue
    args="$(pid_args "${pid}")"
    if [[ "${args}" == *"dotnet"* ]] || [[ "${args}" == *"${APP_NAME}"* ]]; then
      kill_group "${pid}"
      kill_one "${pid}" "当前项目目录下的相关进程"
    fi
  done < <(pgrep -f "dotnet|${APP_NAME}" 2>/dev/null || true)
}

listeners() {
  lsof -tiTCP:"${PORT}" -sTCP:LISTEN 2>/dev/null | sort -u
}

wait_for_release() {
  local i
  for i in 1 2 3 4 5 6 7 8 9 10; do
    if ! lsof -iTCP:"${PORT}" -sTCP:LISTEN >/dev/null 2>&1; then
      return 0
    fi
    sleep 0.3
  done
  return 1
}

print_still_busy_hint() {
  local pid
  say "端口 ${PORT} 还在被占用。当前监听进程："
  lsof -nP -iTCP:"${PORT}" -sTCP:LISTEN || true

  while IFS= read -r pid; do
    [[ -z "${pid}" ]] && continue
    say "PID ${pid} 状态：$(pid_stat "${pid}")"
    say "PID ${pid} 命令：$(pid_args "${pid}")"
  done < <(listeners)

  say "如果状态里有 U，说明这是系统层面卡住的进程，普通 kill 也不一定能立刻清掉。"
  say "这时最稳的办法是先保存手头工作，再重启电脑后重新启动后端。"
}

say "检查端口 ${PORT} …"

if lsof -iTCP:"${PORT}" -sTCP:LISTEN >/dev/null 2>&1; then
  while IFS= read -r pid; do
    [[ -z "${pid}" ]] && continue
    kill_group "${pid}"
    kill_ancestor_chain "${pid}"
    kill_one "${pid}" "直接监听 ${PORT} 的进程"
  done < <(listeners)
else
  say "当前无进程在 ${PORT} 上 LISTEN。"
fi

kill_project_matches "${PROJECT_RUN_PATTERN}" "dotnet run 宿主"
kill_project_matches "${WATCH_PATTERN}.*${PROJECT_FILE}" "dotnet watch 宿主"
kill_project_matches "${APP_BIN_PATTERN}" "项目编译产物"
kill_project_cwd_processes

if wait_for_release; then
  say "端口 ${PORT} 已释放，可以重新启动后端。"
  exit 0
fi

print_still_busy_hint
exit 1
