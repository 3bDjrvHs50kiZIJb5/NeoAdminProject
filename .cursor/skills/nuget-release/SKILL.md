---
name: nuget-release
description: >-
  Bump NeoAdmin NuGet version across csproj files, commit, tag v*, and push to
  trigger GitHub Actions NuGet publish. Use when the user asks to release a
  new version, publish to NuGet, bump NeoAdmin.Blazor or NeoAdmin.Templates
  version, run /nuget-release, or follow README release steps.
---

# NuGet 发布新版本

将 **NeoAdmin.Blazor** 与 **NeoAdmin.Templates** 同步版本并打 `v*` 标签，触发 [`.github/workflows/publish-nuget.yml`](../../.github/workflows/publish-nuget.yml) 发布到 nuget.org。

## 前置确认

1. 工作区在 NeoAdmin 仓库根目录。
2. 目标版本号已明确（用户指定，或在上次 patch 基础上 +1，如 `1.0.5` → `1.0.6`）。
3. 当前分支为 `main`，且本地改动已就绪发布。
4. **仅在用户明确要求时** 才执行 `git commit`、`git push`、`git tag`（遵循仓库 git 安全规则）。

## 必须同步的 4 处

| 位置 | 字段 |
|------|------|
| `NeoAdmin.Blazor/NeoAdmin.Blazor.csproj` | `<Version>`（建议在 `PropertyGroup` 首行） |
| `NeoAdmin.Templates/NeoAdmin.Templates.csproj` | `<Version>` |
| `NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj` | `PackageReference` → `NeoAdmin.Blazor` 的 `Version` |
| Git 标签 | `v*`（如 `v1.0.6`，数字与上表一致） |

版本号格式：`主.次.修订`（如 `1.0.6`）；标签带 `v` 前缀（如 `v1.0.6`）。

## 发布流程

### 1. 读取当前版本

```bash
python3 .cursor/skills/nuget-release/scripts/check-versions.py
```

### 2. 更新 3 个 csproj

将上述 3 个文件中的版本号统一改为目标版本（示例 `1.0.6`）：

- `NeoAdmin.Blazor/NeoAdmin.Blazor.csproj`：`<Version>1.0.6</Version>`
- `NeoAdmin.Templates/NeoAdmin.Templates.csproj`：`<Version>1.0.6</Version>`
- `NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj`：`Version="1.0.6"`（仅 `NeoAdmin.Blazor` 引用）

不要修改其他 `PackageReference` 的版本。

### 3. 校验一致性

```bash
python3 .cursor/skills/nuget-release/scripts/check-versions.py --expect 1.0.6
```

输出 `OK` 后再继续。若失败，修正后重跑。

### 4. 检查标签是否已存在

```bash
git tag -l 'v1.0.6'
```

标签已存在则停止，改用新版本号或请用户确认是否覆盖（**不要** force 推标签）。

### 5. 提交（用户要求时）

并行查看状态：

```bash
git status
git diff
git log -3 --oneline
```

暂存 3 个 csproj，提交信息使用中文，例如：

```bash
git add NeoAdmin.Blazor/NeoAdmin.Blazor.csproj \
  NeoAdmin.Templates/NeoAdmin.Templates.csproj \
  NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj

git commit -m "$(cat <<'EOF'
chore: 发布 v1.0.6

同步 NeoAdmin.Blazor 与 NeoAdmin.Templates 版本号。
EOF
)"
```

### 6. 推送 main 并打标签（用户要求时）

```bash
git push origin main
git tag v1.0.6
git push origin v1.0.6
```

### 7. 发布后验证

1. 在 GitHub **Actions** 查看 `Publish to NuGet` 流水线是否成功。
2. 成功后可在 nuget.org 安装：

```bash
dotnet new install NeoAdmin.Templates
# 或锁定版本：
dotnet new install NeoAdmin.Templates --version 1.0.6
```

## 注意事项

- CI 打包时使用**标签号**作为 NuGet 版本（`v1.0.6` → `1.0.6`），但仓库内 csproj 应与标签保持一致，避免本地打包与模板引用错乱。
- 不要提交 `node_modules` 或其他无关改动；仅包含版本 bump 相关文件。
- 不要修改 git config，不要使用 `--force` 推 main/master 或标签。
- `NeoAdmin/` 宿主项目不在发布包内，无需改其版本。

## 完成清单

```
- [ ] 3 处 csproj 版本已统一
- [ ] check-versions.py --expect 通过
- [ ] 标签 v* 不存在或已确认
- [ ] 已提交（若用户要求）
- [ ] 已 push main + 标签（若用户要求）
- [ ] Actions 流水线成功（发布后）
```
