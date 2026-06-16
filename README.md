# NeoAdmin

[![NuGet NeoAdmin.Blazor](https://img.shields.io/nuget/v/NeoAdmin.Blazor.svg?label=NeoAdmin.Blazor)](https://www.nuget.org/packages/NeoAdmin.Blazor)
[![NuGet NeoAdmin.Templates](https://img.shields.io/nuget/v/NeoAdmin.Templates.svg?label=NeoAdmin.Templates)](https://www.nuget.org/packages/NeoAdmin.Templates)
[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4)](https://dotnet.microsoft.com/download)

基于 **ASP.NET Core Blazor Server** 的现代化后台管理框架。UI 使用 [NeoUI.Blazor](https://neoui.io)，数据访问使用 **FreeSql**，开箱即用 SQLite。

- **`NeoAdmin.Blazor`**：可复用的管理端核心（NuGet 包，当前 `1.0.30`）
- **`NeoAdmin`**：宿主 Web 项目，演示如何扩展业务模块（博客 CRUD、REST API、定时任务、审批流等）
- **`NeoAdmin.Templates`**：`dotnet new neoadmin` 项目模板

源码仓库：[NeoAdminProject](https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject)

## 目录

- [特性](#特性)
- [技术栈](#技术栈)
- [快速开始](#快速开始)
- [功能模块](#功能模块)
- [配置说明](#配置说明)
- [集成到其他项目](#集成到其他项目)
- [扩展业务模块](#扩展业务模块)
- [核心组件与架构](#核心组件与架构)
- [宿主 Tailwind CSS](#宿主-tailwind-cssneoadmin--模板项目)
- [项目结构](#项目结构)
- [发布到 NuGet](#发布到-nugetgithub-actions)
- [相关链接](#相关链接)

## 特性

- **Blazor 交互式服务端**：`InteractiveServer` 渲染，组件化开发体验
- **NeoUI 组件库**：侧边栏布局（参照 NeoUI Blocks dashboard-02 / dashboard-11）、主题切换、表单/表格/弹层等完整 UI 能力
- **通用 CRUD**：`CrudTable` 组件 + FreeSql，支持搜索、筛选、分页、批量删除、弹窗编辑；保存/删除成功自动 Toast 反馈
- **RBAC 基础能力**：用户、角色、菜单、组织（树形）、角色-菜单/用户关联；用户管理可查看登录日志
- **系统配置**：数据字典、系统参数、站点设置（标题/Logo 等）；多行备注使用 NeoUI `RichTextEditor`（无工具栏模式）
- **文件上传**：`NeoFileUpload` 封装单文件/多文件上传，系统「文件管理」与站点 LOGO 等已接入
- **安全与运维**：登录鉴权（DataProtection Token）、登录日志（数据库 + 用户页弹窗）、IP 白名单中间件
- **结构化日志**：Serilog 控制台 + 滚动文件日志；后台「系统日志」支持筛选、自动刷新、清空 INF、删除文件；自动过滤 Blazor Circuit 断开噪声
- **SQL 监控**：`MonitorCommand: true` 时记录失败或超过 2 秒的慢 SQL（写入日志，可在系统日志页筛选 ERR）
- **REST API + Swagger**：`AddNeoAdminApi` 注册控制器，开发/生产可按 `IsSwagger` 开关文档 UI
- **定时任务**：集成 [FreeScheduler](https://github.com/2881099/FreeScheduler)，支持 Cron 表达式
- **审批流**：CRUD 页面可挂载提交/一审/拒绝/反审等按钮（博客「文章」已接入示例）
- **雪花 ID**：Yitter.IdGenerator，多实例部署可配置 `WorkId`
- **种子数据**：首次启动自动建表并初始化管理员、菜单、字典、参数等
- **NeoDemo**：内置 NeoUI 组件演示页，便于对照文档与选型（含 `NeoFileUpload` 演示 `/neo-demo/comp/file-upload`）
- **侧边栏版本**：后台侧边栏底部 Badge 显示当前 `NeoAdmin.Blazor` 框架版本
- **Docker 部署**：多阶段镜像 + compose 卷挂载（数据库、日志、上传目录、DataProtection keys）

## 技术栈

| 类别 | 技术 |
|------|------|
| 运行时 | .NET 10 |
| 前端框架 | Blazor Server（Razor Components） |
| UI | NeoUI.Blazor 4.x |
| ORM | FreeSql 3.x + Sqlite（可换其他 FreeSql Provider） |
| 日志 | Serilog（Console + 按日滚动文件） |
| 调度 | FreeScheduler + NCrontab |
| API 文档 | Swashbuckle + Swagger UI |
| ID | Yitter.IdGenerator（雪花） |
| 样式 | Tailwind CSS v4（宿主项目编译） |

## 快速开始

### 方式一：项目模板（推荐新建项目）

```bash
mkdir MyAdmin && cd MyAdmin
dotnet new install NeoAdmin.Templates
dotnet new neoadmin -n MyAdmin -o .
dotnet watch run
```

首次 `dotnet build` 会执行 `npm install` 并编译 `wwwroot/css/tailwind.css`，需本机安装 **Node.js**。样式开发可另开终端运行 `npm run watch:css`。

详见 [NeoAdmin.Templates/README.md](NeoAdmin.Templates/README.md)。

### 方式二：克隆仓库本地开发

```bash
git clone https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject.git
cd NeoAdminProject/NeoAdmin
dotnet watch run
# 或
./dotnet10.sh
```

默认地址：<http://localhost:5038>（见 `Properties/launchSettings.json`）。

### 环境要求

| 依赖 | 说明 |
|------|------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | 仓库 `global.json` 指定 `10.0.300` |
| [Node.js](https://nodejs.org/) | 编译宿主 Tailwind CSS（`dotnet build` 时自动执行） |
| Docker（可选） | 容器化部署 |

### 默认账号

首次启动会在 SQLite 中创建 `neoadmin.db` 并写入种子数据：

| 项 | 值 |
|----|-----|
| 用户名 | `admin` |
| 密码 | `admin` |

生产环境请务必修改 `appsettings.json` 中的 `SeedAdminPassword`，并关闭弱口令。

### Swagger

开发环境默认开启；生产可在配置中显式设置 `"IsSwagger": true`。文档 UI 路径：<http://localhost:5038/api>（Docker 部署时将端口改为 5050）。

### Docker 部署

在 `NeoAdmin/` 目录下：

```bash
./docker-auto.sh
```

- 默认映射宿主机 **5050** → 容器 80（可通过环境变量 `HOST_PORT` 修改）
- 持久化卷：`neoadmin.db`、`Logs/`、`wwwroot/uploads`、`wwwroot/avatars`、`keys/`（DataProtection）
- 访问：<http://localhost:5050>

### 反向代理与 WebSocket

Blazor Server 依赖 **WebSocket**（SignalR，路径 `/_blazor`）。经 Nginx 等反向代理对外暴露时，须开启 WebSocket 转发：

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

未配置时常见现象：页面能打开但交互失效、频繁提示连接断开。多实例部署还需会话保持（sticky session）。详见 [NeoAdmin.Blazor/README.md](NeoAdmin.Blazor/README.md#部署注意反向代理--websocket)。

### 本地验证模板（维护者）

修改 `NeoAdmin/` 宿主后，先同步到模板再打包验证：

```bash
python3 NeoAdmin.Templates/sync-from-neoadmin.py
dotnet pack NeoAdmin.Blazor/NeoAdmin.Blazor.csproj -c Release -o ./nupkg
dotnet new install ./NeoAdmin.Templates --force
```

## 功能模块

### 系统管理

| 模块 | 路由 | 说明 |
|------|------|------|
| 控制台 | `/`、`/Admin` | 管理首页（基础设施监控仪表盘，宿主 `Admin.razor`） |
| 菜单管理 | `/admin/menu` | 动态菜单、权限类型（菜单/按钮/接口/CRUD） |
| 用户管理 | `/admin/user` | 用户 CRUD、启用状态、登录日志弹窗 |
| 角色管理 | `/admin/role` | 角色与菜单、用户绑定（分配用户支持分页加载） |
| 组织 | `/admin/org` | 树形组织结构 |
| 字典管理 | `/admin/dict` | 字典类型与字典项 |
| 参数配置 | `/admin/param` | 系统键值参数 |
| 站点设置 | `/admin/site-settings` | 站点标题、Logo 等 |
| IP 白名单 | `/admin/ip-whitelist` | 访问 IP 控制 |
| 文件管理 | `/admin/file` | 上传文件记录、批量上传、下载 |
| 定时任务 | `/admin/task-scheduler` | Cron 任务管理 |
| 系统日志 | `/admin/system-log` | 浏览 Serilog 滚动文件，支持级别/关键词筛选、自动刷新、清空 INF、删除日志文件 |
| 登录 | `/login` | 登录页 |
| NeoDemo | `/neo-demo/*` | NeoUI 组件示例（`/neo-demo/ui/*`、`/neo-demo/comp/*`） |
| 宿主占位 | `/Home` | 预留给宿主项目自定义页面 |

### 博客示例（宿主业务）

| 模块 | 路由 | 说明 |
|------|------|------|
| 分类 | `/Blog/Classify` | 随笔专栏 |
| 频道 | `/Blog/Channel` | 技术频道 |
| 文章 | `/Blog/Article` | 随笔文章，含审批流示例 |
| 标签 | `/Blog/Tag2` | 文章标签 |
| 评论 | `/Blog/Comment` | 文章评论 |
| 用户点赞 | `/Blog/UserLike` | 点赞记录 |
| 收藏 | `/Blog/Collection` | 收藏记录 |

## 配置说明

`NeoAdmin/appsettings.json` 中的 `NeoAdmin` 节点：

```json
{
  "NeoAdmin": {
    "IsSwagger": true,
    "DataType": "Sqlite",
    "ConnectionString": "Data Source=neoadmin.db",
    "AutoSyncStructure": true,
    "MonitorCommand": true,
    "EnableSeedData": true,
    "SeedAdminUserName": "admin",
    "SeedAdminPassword": "admin",
    "WorkId": 1,
    "EnableIpWhitelist": true,
    "LogDirectory": "Logs",
    "LogFilePrefix": "admin-",
    "FileUpload": {
      "Directory": "uploads",
      "DateTimeDirectory": "yyyyMMdd",
      "Md5": false,
      "MaxSize": 104857600,
      "IncludeExtension": [],
      "ExcludeExtension": [ ".exe", ".dll", ".jar" ]
    }
  }
}
```

| 配置项 | 说明 |
|--------|------|
| `DataType` / `ConnectionString` | FreeSql 数据库类型与连接串（可换 MySQL、PostgreSQL 等 Provider） |
| `AutoSyncStructure` | 是否自动同步表结构 |
| `MonitorCommand` | 是否启用 SQL 监控：失败或耗时超过 2 秒的 SQL 以 Error 级别写入日志（默认 `true`） |
| `EnableSeedData` | 是否写入业务演示种子（组织、人事、博客等；默认 `true`） |
| `WorkId` | 雪花算法机器号（0–63，多实例需不同） |
| `EnableIpWhitelist` | 是否启用 IP 白名单（默认 `true`；无任何启用记录时不拦截） |
| `IsSwagger` | 是否启用 Swagger UI；未配置时开发环境开、其它环境关 |
| `SwaggerHides` | Swagger 文档中隐藏的路径片段 |
| `LogDirectory` / `LogFilePrefix` | Serilog 文件日志目录与前缀（默认 `Logs/admin-*.log`） |
| `FileUpload` | 上传目录、按日期分目录、大小与扩展名限制 |

## 集成到其他项目

在宿主 `Program.cs` 中：

```csharp
builder.AddNeoAdminSerilog();

builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddNeoAdmin(builder.Configuration, options =>
{
    // 扫描宿主程序集中带 [Scheduler] 的定时任务
    options.SchedulerAssemblies = [Assembly.GetExecutingAssembly()];
});
builder.Services.AddNeoAdminApi(Assembly.GetExecutingAssembly());

// ...

app.UseNeoAdminSerilogRequestLogging();
app.UseNeoAdmin();
app.MapRazorComponents<YourApp>()
    .AddAdditionalAssemblies(typeof(NeoAdmin.Blazor.Components.LayoutAdmin).Assembly)
    .AddInteractiveServerRenderMode();
```

要点：

- 根路由 `/` 建议由宿主项目自行定义（本仓库示例为控制台仪表盘）
- 业务菜单可通过 `NeoAdmin.Blazor.SeedData.MenuSeedData.EnsureMenus` 追加种子
- 定时任务在 `AddNeoAdmin` 的 `SchedulerAssemblies` 中注册宿主程序集
- `NeoAdmin.Blazor` 可作为类库引用（`ProjectReference`），也可通过 NuGet 安装

NuGet 安装：

```bash
dotnet add package NeoAdmin.Blazor
dotnet new install NeoAdmin.Templates
```

## 扩展业务模块

本仓库采用 **宿主扩展** 模式：`NeoAdmin.Blazor` 提供平台能力，`NeoAdmin` 宿主添加业务实体、页面、API 与种子数据。新增业务模块时，通常需同步维护以下目录：

| 目录 | 职责 |
|------|------|
| `Entities/` | 业务实体（FreeSql 映射） |
| `Components/` | Blazor 页面（`@page` 路由 + `CrudTable` 等） |
| `SeedData/` | 菜单种子、表结构同步、演示数据 |
| `Api/` | REST 控制器（可选） |
| `Jobs/` | 定时任务（可选，带 `[Scheduler]` 特性） |

### 1. 定义实体

在 `Entities/` 下创建 FreeSql 实体类，继承框架基类（如 `EntityCreated`、`EntityAudited`）。

### 2. 创建 CRUD 页面

在 `Components/` 下新建 `.razor` 页面，使用 `LayoutAdmin` 布局与 `CrudTable<TItem>`：

```razor
@page "/MyModule/Item"
@layout LayoutAdmin

<CrudTable TItem="MyItem" Title="示例" PageSize="20" />
```

### 3. 注册菜单与表结构

在 `SeedData/DataSetup.cs` 中同步表结构，在 `SeedData/MenuSeedData.cs` 中追加菜单：

```csharp
// DataSetup.Initialize 中
MenuSeedData.Ensure(freeSql);
freeSql.CodeFirst.SyncStructure<MyItem>();

// MenuSeedData.CreateMenus 中
BlazorMenuSeedData.Page("示例", "/MyModule/Item", 900, "box", isSystem: false),
```

### 4. 审批流（可选）

对需要审核的业务，在 `CrudTable` 上启用审批并注册按钮菜单：

```razor
<CrudTable IsWorkflowAudit="true" WorkflowAuditMenuPath="/MyModule/Item" ... />
```

```csharp
AuditMenuSeedData.EnsureButtons(freeSql, "/MyModule/Item");
```

完整示例见 `NeoAdmin/Components/Blog/Article.razor` 与 `NeoAdmin/SeedData/`。

### 5. 定时任务（可选）

在 `Jobs/` 中定义静态方法并标注 Cron：

```csharp
[Scheduler("my-job.sync", "0 */5 * * * *")]
public static async Task SyncData(IServiceProvider sp, TaskInfo task) { ... }
```

在 `Program.cs` 的 `AddNeoAdmin` 回调里将宿主程序集加入 `SchedulerAssemblies`。

### 6. 同步到项目模板

修改宿主后执行：

```bash
python3 NeoAdmin.Templates/sync-from-neoadmin.py
```

脚本会将 `NeoAdmin/` 复制到 `NeoAdmin.Templates/content/NeoAdminApp/`，并完成命名空间替换（`NeoAdmin` → `NeoAdminApp`）。

## 核心组件与架构

### UI 组件

- **`CrudTable<TItem>`**：基于 FreeSql 的增删改查表格，支持列定义、筛选器、搜索、弹窗编辑模板；内置保存/删除 Toast
- **`NeoFileUpload`**：单文件/多文件上传，绑定相对路径，图片预览与非图片文件名展示
- **`NeoAllocTable`**：主从分配弹窗，大数据量可选项支持分页与虚拟滚动
- **`SplitPane`**：左右分栏布局（如字典管理）
- **`NeoSelectDict` / `NeoParamText`**：字典下拉、参数文本展示
- **`LayoutAdmin`**：带侧边栏、用户菜单、框架版本 Badge 的后台主布局

### Core 模块（`NeoAdmin.Blazor/Core/`）

| 路径 | 命名空间 | 职责 |
|------|----------|------|
| `Core/Identity/` | `NeoAdmin.Blazor.Core.Identity` | 登录、Token、API 统一返回体 |
| `Core/Authorization/` | `NeoAdmin.Blazor.Core.Authorization` | REST API 路径权限过滤器 |
| `Core/Navigation/` | `NeoAdmin.Blazor.Core.Navigation` | 菜单树、路径解析、菜单 CRUD |
| `Core/Workflow/` | `NeoAdmin.Blazor.Core.Workflow` | 审批流规则与审批按钮定义 |
| `Core/Scheduling/` | `NeoAdmin.Blazor.Core.Scheduling` | FreeScheduler 注册与任务同步 |

## 宿主 Tailwind CSS（NeoAdmin / 模板项目）

`NeoAdmin` 与 `dotnet new neoadmin` 生成的项目自带 **Tailwind v4** 流水线（见 [NeoUI Theming](https://neoui.io/docs/theming)）：

- `dotnet build` / `dotnet publish` 会自动 `npm install` 并编译 `wwwroot/css/tailwind.css`
- 开发时可另开终端：`cd NeoAdmin && npm run watch:css`
- **需安装 Node.js**（Docker 镜像构建阶段已安装）
- `NeoAdmin.Blazor`（NuGet）内页面样式仍走 `_content/NeoAdmin.Blazor/*.css`；宿主 Tailwind 主要扫描本工程 `.razor`（开发时 `ProjectReference` 会额外扫描 `NeoAdmin.Blazor` 源码）

## 项目结构

```
NeoAdmin/
├── NeoAdmin.Templates/            # dotnet new 项目模板（由 NeoAdmin 宿主同步，见 sync-from-neoadmin.py）
├── NeoAdmin/                      # 宿主 Web 项目（启动入口、业务扩展示例）
│   ├── Program.cs                 # NeoUI、NeoAdmin、Serilog、Blazor 路由、API
│   ├── Components/
│   │   ├── Pages/                 # 控制台（/、/Admin）、/Home 占位页
│   │   └── Blog/                  # 博客业务页面（分类、文章、评论等）
│   ├── Api/                       # 宿主 REST 控制器（Login、Article 等）
│   ├── Entities/                  # 宿主业务实体
│   ├── SeedData/                  # 博客菜单、表结构同步、演示数据
│   ├── Jobs/                      # 定时任务（IP 白名单、博客等）
│   ├── Dockerfile                 # 生产镜像
│   ├── docker-compose.yaml        # 默认宿主机端口 5050
│   ├── docker-auto.sh             # 一键构建并启动容器
│   ├── dotnet10.sh                # 本地 watch 开发
│   ├── package.json               # Tailwind v4（build 时编译 wwwroot/css/tailwind.css）
│   └── wwwroot/css/               # app-input.css → tailwind.css
├── NeoAdmin.Blazor/               # 管理端核心类库（可引用或打包 NuGet）
│   ├── Core/                      # Identity、Authorization、Navigation、Workflow、Scheduling
│   ├── Components/                # 布局、CrudTable、SplitPane、字典/参数组件等
│   ├── Pages/                     # 系统管理页、NeoDemo
│   ├── Entities/                  # 系统实体
│   ├── SeedData/                  # 菜单、用户、字典等种子
│   ├── Services/                  # 文件、组织、角色、定时任务、Serilog 日志读取
│   ├── Auth/                      # 登录鉴权
│   ├── Api/                       # 内置 API 基类与 DTO
│   └── Middlewares/               # IP 白名单等
└── README.md
```

## 发布到 NuGet（GitHub Actions）

仓库已配置 [`.github/workflows/publish-nuget.yml`](.github/workflows/publish-nuget.yml)：推送 **`v*` 标签**（如 `v1.0.30`）时，会自动打包并推送 **NeoAdmin.Blazor** 与 **NeoAdmin.Templates** 到 [nuget.org](https://www.nuget.org)。流水线用**标签号**作为 NuGet 版本（`v1.0.30` → `1.0.30`），但建议仓库内下列位置与标签保持一致，避免本地打包与模板引用错乱。

**发布新版本**

1. 执行 `python3 NeoAdmin.Templates/sync-from-neoadmin.py` 同步宿主到模板。
2. 将下表 **4 处** 改为同一版本号（示例：`1.0.31` / `v1.0.31`）。
3. 提交并推送 `main`，再打标签触发 NuGet 发布。

| 位置 | 字段 |
|------|------|
| `NeoAdmin.Blazor/NeoAdmin.Blazor.csproj` | `<Version>`（建议在 `PropertyGroup` 首行） |
| `NeoAdmin.Templates/NeoAdmin.Templates.csproj` | `<Version>` |
| `NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj` | `PackageReference` → `NeoAdmin.Blazor` 的 `Version` |
| Git 标签 | `v*`（如 `v1.0.31`，数字与上表一致） |

```bash
git push origin main
git tag v1.0.31
git push origin v1.0.31
```

在 **Actions** 页查看流水线；成功后安装：

```bash
dotnet new install NeoAdmin.Templates
# 或锁定版本：dotnet new install NeoAdmin.Templates --version 1.0.30
mkdir MyApp && cd MyApp
dotnet new neoadmin -n MyApp -o .
cd MyApp && dotnet watch run
```

## 相关链接

| 资源 | 链接 |
|------|------|
| 源码仓库 | <https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject> |
| NeoAdmin.Blazor（NuGet） | <https://www.nuget.org/packages/NeoAdmin.Blazor> |
| NeoAdmin.Templates（NuGet） | <https://www.nuget.org/packages/NeoAdmin.Templates> |
| NeoUI 文档与 Blocks | <https://neoui.io> |
| FreeSql | <https://freesql.net> |
| FreeScheduler | <https://github.com/2881099/FreeScheduler> |

## 许可证

请根据仓库实际许可证补充；若尚未声明，使用前请与维护者确认。
