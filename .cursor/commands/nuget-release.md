---
description: 同步 NeoAdmin NuGet 版本号并发布（NeoAdmin.Blazor + NeoAdmin.Templates）
---

# NuGet 发布新版本

按项目发布流程执行（完整步骤见 `.cursor/skills/nuget-release/SKILL.md`）：

1. 确认目标版本号（如 `1.0.6`），将以下 3 处改为同一版本：
   - `NeoAdmin.Blazor/NeoAdmin.Blazor.csproj` → `<Version>`
   - `NeoAdmin.Templates/NeoAdmin.Templates.csproj` → `<Version>`
   - `NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj` → `NeoAdmin.Blazor` 的 `Version`
2. 运行校验：`python3 .cursor/skills/nuget-release/scripts/check-versions.py --expect <版本>`
3. 仅在用户明确要求时：提交（中文 commit）、`git push origin main`、`git tag v<版本>`、`git push origin v<版本>`
4. 提醒在 GitHub Actions 查看 NuGet 发布结果

若用户未指定版本，先读取当前版本并建议 patch +1。
