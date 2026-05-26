# NeoAdmin.Templates

NeoAdmin 的 `dotnet new` 项目模板，用于快速创建 NeoAdmin 项目。

模板内容来自仓库根目录的 **`NeoAdmin`** 宿主项目（命名空间替换为 `NeoAdminApp`，并引用 `NeoAdmin.Blazor` NuGet 包）。修改宿主项目后请执行同步：

```bash
python3 NeoAdmin.Templates/sync-from-neoadmin.py
```

## 创建项目

推荐先建**外层目录**（仓库/解决方案根），再在目录内生成项目（内层为实际 `.csproj`）：

```bash
mkdir MyProject && cd MyProject
dotnet new neoadmin -n MyAdmin -o .
cd MyAdmin
dotnet watch run
```

## 本地验证

见 [Test.md](Test.md)（仅本地，已加入 `.gitignore`）。
