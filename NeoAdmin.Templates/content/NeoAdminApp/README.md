# NeoAdmin 宿主项目

本目录是 **NeoAdmin 的宿主 Web 应用**（启动入口），演示如何在 `NeoAdmin.Blazor` 平台之上扩展业务模块。系统管理、登录、CRUD 框架等核心能力来自类库 `../NeoAdmin.Blazor`；本仓库负责博客示例、REST API、定时任务、NeoUI 演示页及 Docker 部署。

完整仓库说明见 [../README.md](../README.md)。

## 目录树

```
NeoAdmin/
├── Program.cs                 # 应用入口：NeoUI、NeoAdmin、Serilog、Blazor、API
├── App.razor                  # 根组件
├── Routes.razor               # 路由容器
├── _Imports.razor             # 全局 using
├── appsettings.json           # NeoAdmin 配置（数据库、种子账号、上传、日志等）
├── appsettings.Production.json
│
├── Api/                       # 宿主 REST API
│   ├── LoginController.cs     # 登录相关接口
│   ├── ArticleController.cs   # 博客文章接口示例
│   └── Dto/                   # API 请求/响应 DTO
│
├── Components/
│   ├── Pages/
│   │   ├── Home.razor         # 路由 /（可自定义宿主首页）
│   │   ├── Admin.razor        # 路由 /Admin，控制台/基础设施监控仪表盘
│   │   ├── Error.razor        # 错误页
│   │   ├── DemoUI/            # NeoUI 组件演示（/neo-demo/ui/*）
│   │   └── DemoComp/          # NeoAdmin 扩展组件演示（/neo-demo/comp/*）
│   └── Blog/                  # 博客业务 CRUD 页面（/Blog/*）
│
├── Entities/
│   └── Blog/                  # 博客实体（FreeSql 映射）
│
├── SeedData/
│   ├── DataSetup.cs           # 启动时：同步表结构、菜单、审批按钮、演示数据
│   ├── MenuSeedData.cs        # 博客与演示菜单种子
│   ├── DemoMenuSeedData.cs    # NeoDemo 菜单
│   ├── PageSearchTabSeedData.cs
│   └── SeedData.cs            # 博客演示数据
│
├── Services/
│   └── BlogRelationService.cs # 博客关联业务逻辑
│
├── Jobs/
│   ├── IpWhitelistJobs.cs     # IP 白名单相关定时任务
│   └── BlogJobs.cs            # 博客相关定时任务
│
├── Properties/
│   └── launchSettings.json    # 本地开发端口（默认 5038）
│
├── wwwroot/
│   ├── css/                   # Tailwind 输入/编译产物（app-input.css → tailwind.css）
│   ├── images/                # 静态图片
│   └── uploads/               # 运行时上传目录（持久化，勿提交大文件）
│
├── package.json               # Tailwind v4；dotnet build 时自动 npm install + 编译 CSS
├── tailwind.config.js
├── Dockerfile                 # 生产镜像
├── docker-compose.yaml        # 容器编排（默认宿主机 5050）
├── docker-auto.sh             # 一键构建并启动 Docker
├── dotnet10.sh                # 本地 dotnet watch 开发
└── kill-port.sh               # 释放占用端口
```

运行时生成（通常不纳入版本控制）：

- `neoadmin.db` — SQLite 数据库
- `Logs/` — Serilog 滚动日志
- `keys/` — DataProtection 密钥（Docker 卷挂载）
- `node_modules/` — npm 依赖

## 功能说明

### 启动与配置

| 文件 | 功能 |
|------|------|
| `Program.cs` | 注册 NeoUI、NeoAdmin、Serilog；映射 Blazor 与 API；启动时调用 `DataSetup.Initialize` |
| `appsettings.json` | 连接串、种子管理员、雪花 WorkId、IP 白名单、Swagger、文件上传、日志目录等 |

### 页面路由（本宿主）

| 模块 | 路由 | 说明 |
|------|------|------|
| 首页 | `/` | `Home.razor`，预留给宿主自定义 |
| 控制台 | `/Admin` | 基础设施监控仪表盘 |
| 博客 | `/Blog/*` | 分类、频道、文章、标签、评论、点赞、收藏 |
| NeoDemo UI | `/neo-demo/ui/*` | NeoUI 表单、布局、弹层、图表等演示 |
| NeoDemo Comp | `/neo-demo/comp/*` | 字典/参数、事务、权限、NeoSelect、文件缓存等演示 |

### NeoDemo 演示页文件

每个 `*.razor` 演示页在同级 `Snippets/` 下有对应的 `*Snippets.cs` 代码片段类。

#### `Components/Pages/DemoUI/`（NeoUI，路由 `/neo-demo/ui/*`）

| 页面 | 说明 |
|------|------|
| `FormInputsDemo.razor` | 基础表单输入 |
| `FormControlsDemo.razor` | 表单控件 |
| `AdvancedInputsDemo.razor` | 高级输入 |
| `AdvancedDateTimeDemo.razor` | 日期时间 |
| `AdvancedComplexDemo.razor` | 复杂高级组件 |
| `LayoutControlsDemo.razor` | 布局控件 |
| `LayoutToolsDemo.razor` | 布局工具 |
| `DisplayBasicsDemo.razor` | 基础展示 |
| `DisplayStatesDemo.razor` | 状态展示 |
| `DataDisplayDemo.razor` | 数据展示 |
| `FeedbackDemo.razor` | 反馈（消息、加载等） |
| `OverlaysModalDemo.razor` | 模态弹层 |
| `OverlaysFloatingDemo.razor` | 浮层 |
| `NavigationDemo.razor` | 导航 |
| `ChartDemo.razor` | 图表 |
| `AntiConcurrencyDemo.razor` | 防并发 |
| `MobileDemo.razor` | 移动端 |
| `MobileDevicePreview.razor` | 移动设备预览 |

`Snippets/`：`FormInputsDemoSnippets.cs`、`FormControlsDemoSnippets.cs`、`AdvancedInputsDemoSnippets.cs`、`AdvancedDateTimeDemoSnippets.cs`、`AdvancedComplexDemoSnippets.cs`、`LayoutControlsDemoSnippets.cs`、`LayoutToolsDemoSnippets.cs`、`DisplayBasicsDemoSnippets.cs`、`DisplayStatesDemoSnippets.cs`、`DataDisplayDemoSnippets.cs`、`FeedbackDemoSnippets.cs`、`OverlaysModalDemoSnippets.cs`、`OverlaysFloatingDemoSnippets.cs`、`NavigationDemoSnippets.cs`、`ChartDemoSnippets.cs`、`AntiConcurrencyDemoSnippets.cs`、`MobileDemoSnippets.cs`

#### `Components/Pages/DemoComp/`（NeoAdmin 扩展组件，路由 `/neo-demo/comp/*`）

| 页面 | 说明 |
|------|------|
| `AnimationDemo.razor` | 动画 |
| `DictParamDemo.razor` | 字典 / 参数 |
| `FileCacheDemo.razor` | 文件缓存 |
| `FileUploadDemo.razor` | NeoFileUpload 单文件/多文件上传 |
| `NeoSelectComponentsDemo.razor` | NeoSelect 组件 |
| `NovaButtonDemo.razor` | Nova 按钮 |
| `PermissionGuideDemo.razor` | 权限引导 |
| `TransactionalDemo.razor` | 事务 |

`Snippets/`：`AnimationDemoSnippets.cs`、`DictParamDemoSnippets.cs`、`FileCacheDemoSnippets.cs`、`FileUploadDemoSnippets.cs`、`NeoSelectComponentsDemoSnippets.cs`、`NovaButtonDemoSnippets.cs`、`PermissionGuideDemoSnippets.cs`、`TransactionalDemoSnippets.cs`

系统管理页面（用户、角色、菜单、字典、定时任务等）在 **`NeoAdmin.Blazor/Pages/`**，路由形如 `/admin/user`，由类库提供。

### 各目录职责

| 目录 | 功能 |
|------|------|
| **Api/** | 对外 REST 接口；与 Blazor 共用 FreeSql 与鉴权体系 |
| **Components/Pages/** | 宿主专属页面：控制台、演示、占位首页 |
| **Components/Blog/** | 博客 CRUD；`Article.razor` 含审批流示例 |
| **Entities/Blog/** | 博客表实体；继承框架基类（如 `EntityAudited`） |
| **SeedData/** | 首次/启动时建表、写菜单、审批按钮、演示数据 |
| **Services/** | 跨页面复用的业务服务（如博客关联维护） |
| **Jobs/** | 带 `[Scheduler]` 的定时任务，在 `Program.cs` 中通过 `SchedulerAssemblies` 注册 |
| **wwwroot/** | 静态资源与 Tailwind 编译 CSS |

### 博客示例模块

| 实体/页面 | 说明 |
|-----------|------|
| Classify | 随笔专栏 |
| Channel | 技术频道 |
| Article | 文章；启用 `IsWorkflowAudit` 演示提交/一审/拒绝/反审 |
| Tag2 | 文章标签 |
| Comment | 评论 |
| UserLike | 点赞 |
| Collection | 收藏 |

### 与 NeoAdmin.Blazor 的分工

```
┌─────────────────────────────────────────────────────────┐
│  NeoAdmin（本目录）                                      │
│  · 启动入口 Program.cs                                   │
│  · 业务实体 / 页面 / API / Jobs / 宿主 SeedData          │
│  · Tailwind、Docker、appsettings                         │
└──────────────────────────┬──────────────────────────────┘
                           │ ProjectReference
┌──────────────────────────▼──────────────────────────────┐
│  NeoAdmin.Blazor                                        │
│  · 系统管理页、CrudTable、LayoutAdmin、登录鉴权           │
│  · Core（Identity / Authorization / Navigation / Workflow）│
│  · 系统实体、菜单/用户/字典种子、IP 白名单、Swagger      │
└─────────────────────────────────────────────────────────┘
```

扩展新业务时，通常同步维护：`Entities/`、`Components/`、`SeedData/`，可选 `Api/`、`Jobs/`。修改后若需更新 `dotnet new neoadmin` 模板，在仓库根目录执行：

```bash
python3 NeoAdmin.Templates/sync-from-neoadmin.py
```

## 本地运行

```bash
cd NeoAdmin
dotnet watch run
# 或
./dotnet10.sh
```

默认：<http://localhost:5038>。首次启动创建 `neoadmin.db`，默认账号 `admin` / `admin`。

样式开发（可选）：

```bash
npm run watch:css
```

## Docker

```bash
./docker-auto.sh
```

默认访问 <http://localhost:5050>。

经 Nginx 等反向代理对外暴露时，须开启 **WebSocket** 转发（Blazor Server / SignalR）。示例见 [NeoAdmin.Blazor/README.md](../NeoAdmin.Blazor/README.md#部署注意反向代理--websocket)。
