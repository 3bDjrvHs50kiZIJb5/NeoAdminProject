# NoAdmin.Blazor（源自 AdminBlazor 2.5.7 反编译）

此目录由 AdminBlazor NuGet 包反编译并迁移命名而来；原始基线仍为 `AdminBlazor.dll`。

## 编译

```bash
dotnet build NoAdmin.Blazor/NoAdmin.Blazor.csproj
```

当前在 .NET 8 SDK 下可编译（存在既有警告）。

## 布局

- `NoAdmin.Blazor.csproj`：Razor 类库项目文件。
- `wwwroot/`：静态资源（原包 `staticwebassets`）。
- 第三方的部分 OSS 实现已从反编译源码中移除，改为 NuGet 包引用。

## 反编译修复说明（与原版一致）

- 使用 Rougamo/Fody 处理过的 DLL，部分 AOP 包装在反编译后已简化为调用恢复的 `_0024Rougamo_*` 主体。
- 补全包引用、移除无用文件、目标框架 `net8.0`、静态资源放入 `wwwroot`、修复歧义类型与 `init` 属性等。
- Fody 提示程序集已处理过属预期，不影响编译。
