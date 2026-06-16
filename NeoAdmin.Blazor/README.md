# NeoAdmin.Blazor

基于 **ASP.NET Core Blazor Server** 的后台管理框架核心库：NeoUI 组件、FreeSql CRUD、RBAC、字典/参数、定时任务、审批流、REST API 等。

## 安装

```bash
dotnet add package NeoAdmin.Blazor
```

## 快速集成

在宿主 `Program.cs` 中：

```csharp
builder.AddNeoAdminSerilog();

builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddNeoAdmin(builder.Configuration);
builder.Services.AddNeoAdminApi(Assembly.GetExecutingAssembly());

// ...

app.UseNeoAdminSerilogRequestLogging();
app.UseNeoAdmin();
app.MapRazorComponents<YourApp>()
    .AddAdditionalAssemblies(typeof(NeoAdmin.Blazor.Components.LayoutAdmin).Assembly)
    .AddInteractiveServerRenderMode();
```

## 项目模板

推荐使用 [NeoAdmin.Templates](https://www.nuget.org/packages/NeoAdmin.Templates) 通过 `dotnet new neoadmin` 生成完整宿主项目。

## 部署注意（反向代理 / WebSocket）

NeoAdmin 使用 **Blazor Server**（`InteractiveServer`），浏览器与服务器通过 **SignalR** 长连接通信，底层依赖 **WebSocket**（路径通常为 `/_blazor`）。

若前面有 **Nginx、Caddy、Traefik、云负载均衡** 等反向代理，必须开启 WebSocket 转发，否则会出现页面空白、按钮无响应、频繁「连接已断开」等问题。

**Nginx 示例**（将 `5050` 换成实际 upstream 端口）：

```nginx
location / {
    proxy_pass http://127.0.0.1:5050;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 3600s;
}
```

要点：

- `Upgrade` / `Connection "upgrade"` 缺一不可
- HTTPS 终止在代理层时，建议设置 `X-Forwarded-Proto`，以便应用识别真实协议
- 多实例部署时，SignalR 连接须**会话保持**（sticky session），或另行配置共享 DataProtection / SignalR Backplane

更多部署说明见仓库根目录 [README.md](../README.md#docker-部署) 与宿主 [NEOADMIN开发上手.md](../NeoAdmin/NEOADMIN开发上手.md)。

## 核心模块目录（`Core/`）

按功能划分的平台能力，避免根目录散落多个小文件夹：

| 路径 | 命名空间 | 职责 |
|------|----------|------|
| `Core/Identity/` | `NeoAdmin.Blazor.Core.Identity` | 登录、Token、API 统一返回体 |
| `Core/Authorization/` | `NeoAdmin.Blazor.Core.Authorization` | REST API 路径权限过滤器 |
| `Core/Navigation/` | `NeoAdmin.Blazor.Core.Navigation` | 菜单树、路径解析、菜单 CRUD |
| `Core/Workflow/` | `NeoAdmin.Blazor.Core.Workflow` | 审批流规则与审批按钮定义 |
| `Core/Scheduling/` | `NeoAdmin.Blazor.Core.Scheduling` | FreeScheduler 注册与任务同步 |

## 文档与源码

- 仓库：<https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject>
- NeoUI：<https://neoui.io>
- FreeSql：<https://freesql.net>
