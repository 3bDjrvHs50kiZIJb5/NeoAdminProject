# NeoAdmin

基于 **ASP.NET Core Blazor Server** 的现代化后台管理框架，UI 使用 [NeoUI.Blazor](https://neoui.io)，数据访问使用 **FreeSql**，开箱即用 SQLite。`NeoAdmin.Blazor` 提供可复用的管理端核心，`NeoAdmin` 宿主项目演示如何扩展业务模块（博客 CRUD、REST API、定时任务等）。

## 特性

- **Blazor 交互式服务端**：`InteractiveServer` 渲染，组件化开发体验
- **NeoUI 组件库**：侧边栏布局（参照 NeoUI Blocks dashboard-02 / dashboard-11）、主题切换、表单/表格/弹层等完整 UI 能力
- **通用 CRUD**：`CrudTable` 组件 + FreeSql，支持搜索、筛选、分页、批量删除、弹窗编辑
- **RBAC 基础能力**：用户、角色、菜单、组织（树形）、角色-菜单/用户关联
- **系统配置**：数据字典、系统参数、站点设置（标题/Logo 等）
- **安全与运维**：登录鉴权（DataProtection Token）、登录日志、IP 白名单中间件、文件上传管理
- **结构化日志**：Serilog 控制台 + 滚动文件日志，后台「系统日志」页可在线浏览
- **REST API + Swagger**：`AddNeoAdminApi` 注册控制器，开发/生产可按 `IsSwagger` 开关文档 UI
- **定时任务**：集成 [FreeScheduler](https://github.com/2881099/FreeScheduler)，支持 Cron 表达式
- **审批流**：CRUD 页面可挂载提交/一审/拒绝/反审等按钮（博客「文章」已接入示例）
- **雪花 ID**：Yitter.IdGenerator，多实例部署可配置 `WorkId`
- **种子数据**：首次启动自动建表并初始化管理员、菜单、字典、参数等
- **NeoDemo**：内置 NeoUI 组件演示页，便于对照文档与选型
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

## 项目模板（dotnet new）

仓库提供 **NeoAdmin.Templates**，用法与 NovaAdmin 的 `novaadmin` 模板类似：

```bash
# 本地开发：先打包 NeoAdmin.Blazor，再安装模板
dotnet pack NeoAdmin.Blazor/NeoAdmin.Blazor.csproj -c Release -o ./nupkg
dotnet new install ./NeoAdmin.Templates

mkdir MyAdmin && cd MyAdmin
dotnet new neoadmin -n MyAdmin -o .
# 若 NeoAdmin.Blazor 尚未发布到 nuget.org，还原时加上本地包目录：
dotnet restore --source ../nupkg --source https://api.nuget.org/v3/index.json
dotnet watch run
```

详见 [NeoAdmin.Templates/README.md](NeoAdmin.Templates/README.md)。修改 `NeoAdmin/` 后执行 `python3 NeoAdmin.Templates/sync-from-neoadmin.py` 更新模板内容。

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
│   ├── Entities/ / Data/Entities/ # 宿主业务实体
│   ├── SeedData/                  # 博客菜单等宿主种子
│   ├── Jobs/                      # 定时任务（IP 白名单、博客等）
│   ├── Dockerfile                 # 生产镜像
│   ├── docker-compose.yaml        # 默认宿主机端口 5050
│   ├── docker-auto.sh             # 一键构建并启动容器
│   └── dotnet10.sh                # 本地 watch 开发
├── NeoAdmin.Blazor/               # 管理端核心类库（可引用或打包 NuGet）
│   ├── Components/                # 布局、CrudTable、SplitPane、字典/参数组件等
│   ├── Pages/                    # 系统管理页、NeoDemo
│   ├── Entities/ / Data/Entities/ # 系统实体
│   ├── SeedData/                  # 菜单、用户、字典等种子
│   ├── Services/                  # 文件、组织、角色、定时任务、Serilog 日志读取
│   ├── Auth/                     # 登录鉴权
│   ├── Api/                      # 内置 API 基类与 DTO
│   └── Middlewares/              # IP 白名单等
└── README.md
```

## 功能模块

| 模块 | 路由 | 说明 |
|------|------|------|
| 控制台 | `/`、`/Admin` | 管理首页（基础设施监控仪表盘，宿主 `Admin.razor`） |
| 菜单管理 | `/admin/menu` | 动态菜单、权限类型（菜单/按钮/接口/CRUD） |
| 用户管理 | `/admin/user` | 用户 CRUD、启用状态 |
| 角色管理 | `/admin/role` | 角色与菜单、用户绑定 |
| 组织 | `/admin/org` | 树形组织结构 |
| 字典管理 | `/admin/dict` | 字典类型与字典项 |
| 参数配置 | `/admin/param` | 系统键值参数 |
| 站点设置 | `/admin/site-settings` | 站点标题、Logo 等 |
| IP 白名单 | `/admin/ip-whitelist` | 访问 IP 控制 |
| 文件管理 | `/admin/file` | 上传文件记录与下载 |
| 定时任务 | `/admin/task-scheduler` | Cron 任务管理 |
| 系统日志 | `/admin/system-log` | 浏览 Serilog 滚动文件，支持筛选与自动刷新 |
| 登录 | `/login` | 登录页 |
| NeoDemo | `/neo-demo/*` | NeoUI 组件示例（`/neo-demo/ui/*`、`/neo-demo/comp/*`） |
| 博客管理 | `/Blog/*` | 宿主示例：分类、频道、文章（含审批流）、标签、评论等 |
| 宿主占位 | `/Home` | 预留给宿主项目自定义页面 |

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker（可选，用于容器部署）

### 本地运行

```bash
cd NeoAdmin
dotnet watch run
# 或
./dotnet10.sh
```

默认地址：<http://localhost:5038>（见 `Properties/launchSettings.json`）。

### Docker 部署

在 `NeoAdmin/` 目录下：

```bash
./docker-auto.sh
```

- 默认映射宿主机 **5050** → 容器 80（可通过环境变量 `HOST_PORT` 修改）
- 持久化卷：`neoadmin.db`、`Logs/`、`wwwroot/uploads`、`wwwroot/avatars`、`keys/`（DataProtection）
- 访问：<http://localhost:5050>

### 默认账号

首次启动会在 SQLite 中创建 `neoadmin.db` 并写入种子数据：

| 项 | 值 |
|----|-----|
| 用户名 | `admin` |
| 密码 | `admin` |

生产环境请务必修改 `appsettings.json` 中的 `SeedAdminPassword`，并关闭弱口令。

### Swagger

开发环境默认开启；生产可在配置中显式设置 `"IsSwagger": true`。文档 UI 路径：<http://localhost:5038/api>（Docker 部署时将端口改为 5050）。

## 配置说明

`NeoAdmin/appsettings.json` 中的 `NeoAdmin` 节点：

```json
{
  "NeoAdmin": {
    "IsSwagger": true,
    "DataType": "Sqlite",
    "ConnectionString": "Data Source=neoadmin.db",
    "AutoSyncStructure": true,
    "MonitorCommand": false,
    "SeedAdminUserName": "admin",
    "SeedAdminPassword": "admin",
    "WorkId": 1,
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
| `DataType` / `ConnectionString` | FreeSql 数据库类型与连接串 |
| `AutoSyncStructure` | 是否自动同步表结构 |
| `MonitorCommand` | 是否在控制台打印 SQL |
| `WorkId` | 雪花算法机器号（0–63，多实例需不同） |
| `EnableIpWhitelist` | 是否启用 IP 白名单（默认 `true`） |
| `IsSwagger` | 是否启用 Swagger UI；未配置时开发环境开、其它环境关 |
| `LogDirectory` / `LogFilePrefix` | Serilog 文件日志目录与前缀（默认 `Logs/admin-*.log`） |
| `FileUpload` | 上传目录、按日期分目录、大小与扩展名限制 |

## 集成到其他项目

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

- 根路由 `/` 建议由宿主项目自行定义（本仓库示例为控制台仪表盘）
- 业务菜单可通过 `NeoAdmin.Blazor.SeedData.MenuSeedData.EnsureMenus` 追加种子
- 定时任务在 `AddNeoAdmin` 的 `SchedulerAssemblies` 中注册宿主程序集
- `NeoAdmin.Blazor` 可作为类库引用，也可按 NuGet 包发布（`PackageId: NeoAdmin.Blazor`）

## 核心组件

- **`CrudTable<TItem>`**：基于 FreeSql 的增删改查表格，支持列定义、筛选器、搜索、弹窗编辑模板
- **`SplitPane`**：左右分栏布局（如字典管理）
- **`NeoSelectDict` / `NeoParamText`**：字典下拉、参数文本展示
- **`LayoutAdmin`**：带侧边栏、用户菜单的后台主布局

## 发布到 NuGet（GitHub Actions）

仓库已配置 [`.github/workflows/publish-nuget.yml`](.github/workflows/publish-nuget.yml)：推送 **`v*` 标签**（如 `v1.0.1`）时，会自动打包并推送 **NeoAdmin.Blazor** 与 **NeoAdmin.Templates** 到 [nuget.org](https://www.nuget.org)。流水线用**标签号**作为 NuGet 版本（`v1.0.1` → `1.0.1`），但建议仓库内下列位置与标签保持一致，避免本地打包与模板引用错乱。

**发布新版本**

1. 将下表 **4 处** 改为同一版本号（示例：`1.0.2` / `v1.0.2`）。
2. 提交并推送 `main`，再打标签触发 NuGet 发布。

| 位置 | 字段 |
|------|------|
| `NeoAdmin.Blazor/NeoAdmin.Blazor.csproj` | `<Version>`（建议在 `PropertyGroup` 首行） |
| `NeoAdmin.Templates/NeoAdmin.Templates.csproj` | `<Version>` |
| `NeoAdmin.Templates/content/NeoAdminApp/NeoAdminApp.csproj` | `PackageReference` → `NeoAdmin.Blazor` 的 `Version` |
| Git 标签 | `v*`（如 `v1.0.2`，数字与上表一致） |

```bash
git push origin main
git tag v1.0.2
git push origin v1.0.2
```

在 **Actions** 页查看流水线；成功后安装：

```bash
dotnet new install NeoAdmin.Templates
# 或锁定版本：dotnet new install NeoAdmin.Templates --version 1.0.2
mkdir MyApp && cd MyApp
dotnet new neoadmin -n MyApp -o .
cd MyApp && dotnet watch run
```

## 相关链接

- NeoUI 文档与 Blocks：<https://neoui.io>
- FreeSql：<https://freesql.net>
- FreeScheduler：<https://github.com/2881099/FreeScheduler>

## 许可证

请根据仓库实际许可证补充；若尚未声明，使用前请与维护者确认。
