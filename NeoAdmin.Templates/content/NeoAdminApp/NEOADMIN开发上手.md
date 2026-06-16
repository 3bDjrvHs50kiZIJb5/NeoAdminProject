# NeoAdmin 开发上手指南

> 面向新开发者的完整参考文档。NeoAdmin 是基于 **ASP.NET Core Blazor Server** 的后台管理框架：底层能力来自 **NeoAdmin.Blazor**（NuGet 类库），业务扩展在宿主项目 **NeoAdminApp**（本模板项目）中完成。UI 组件库为 **NeoUI.Blazor**，数据访问使用 **FreeSql**。

---

## 目录

1. [项目架构与目录结构](#1-项目架构与目录结构)
2. [快速开始](#2-快速开始)
3. [启动与配置](#3-启动与配置)
4. [集成到其他项目](#4-集成到其他项目)
5. [实体与数据访问（FreeSql）](#5-实体与数据访问freesql)
6. [扩展业务模块](#6-扩展业务模块)
7. [页面、路由与菜单注册](#7-页面路由与菜单注册)
8. [权限体系](#8-权限体系)
9. [CrudTable 通用 CRUD 组件](#9-crudtable-通用-crud-组件)
10. [NeoAdmin.Blazor 组件详解](#10-neoadminblazor-组件详解)
11. [NeoUI.Blazor 控件详解](#11-neouiblazor-控件详解)
12. [表单、表格、弹窗与服务](#12-表单表格弹窗与服务)
13. [审批流](#13-审批流)
14. [REST API 与定时任务](#14-rest-api-与定时任务)
15. [典型开发流程（完整示例）](#15-典型开发流程完整示例)
16. [常见坑与最佳实践](#16-常见坑与最佳实践)
17. [参考资源](#17-参考资源)

---

## 1. 项目架构与目录结构

### 1.1 三层关系

```
┌─────────────────────────────────────────────────────────────┐
│  NeoAdmin（宿主 / 业务项目）                                  │
│  · Program.cs 启动入口                                       │
│  · Entities/     业务实体                                    │
│  · Components/   业务 Blazor 页面                            │
│  · SeedData/     菜单种子、表结构同步、演示数据                 │
│  · Api/          REST 控制器（可选）                          │
│  · Jobs/         定时任务（可选）                             │
│  · appsettings.json、Tailwind、Docker                        │
└──────────────────────────┬──────────────────────────────────┘
                           │ ProjectReference / NuGet
┌──────────────────────────▼──────────────────────────────────┐
│  NeoAdmin.Blazor（框架核心）                                  │
│  · CrudTable、LayoutAdmin、NeoSelect* 等组件                  │
│  · Core/（Identity、Authorization、Navigation、Workflow）     │
│  · 系统管理页面 /admin/*、登录、种子数据                       │
└──────────────────────────┬──────────────────────────────────┘
                           │ NuGet 依赖
┌──────────────────────────▼──────────────────────────────────┐
│  NeoUI.Blazor（UI 组件库）                                    │
│  · Button、Input、DataTable、Dialog、Select 等                │
│  · 文档：https://neoui.io                                   │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 仓库目录树

```
NeoAdmin/                          # 仓库根目录
├── NeoAdmin/                      # 宿主 Web 项目（业务扩展示例）
│   ├── Program.cs
│   ├── App.razor / Routes.razor
│   ├── Components/
│   │   ├── Pages/                 # 控制台、NeoDemo 演示
│   │   └── Blog/                  # 博客业务 CRUD
│   ├── Entities/Blog/
│   ├── SeedData/
│   ├── Api/
│   ├── Jobs/
│   └── wwwroot/css/               # Tailwind 编译产物
├── NeoAdmin.Blazor/               # 框架类库
│   ├── Components/                # Crud、Layout、Neo* 组件
│   ├── Pages/                     # 系统管理页
│   ├── Core/                      # 身份、权限、菜单、审批、调度
│   ├── Entities/                  # 系统实体
│   ├── SeedData/
│   ├── Services/
│   └── NeoAdminExtensions.cs
└── NeoAdmin.Templates/            # dotnet new neoadmin 模板
```

### 1.3 宿主 vs 框架职责对照

| 职责 | NeoAdmin.Blazor（框架） | NeoAdmin（宿主业务） |
|------|-------------------------|----------------------|
| 用户/角色/菜单/字典 | ✅ 内置页面与实体 | 可追加业务菜单种子 |
| 通用 CRUD 框架 | ✅ `CrudTable` | 业务页面引用 |
| 登录鉴权 | ✅ | 可自定义 `DefaultLogin` 文案 |
| 业务实体与页面 | ❌ | ✅ `Entities/` + `Components/` |
| 表结构同步（业务表） | ❌ | ✅ `SeedData/DataSetup.cs` |
| Tailwind 样式编译 | 自带部分 CSS | ✅ 宿主 `npm run watch:css` |
| 定时任务实现 | 注册基础设施 | ✅ `Jobs/` + `[Scheduler]` |

### 1.4 与 SmartQC 等旧项目的对应关系

若你熟悉 `SmartQC_Admin` / `SmartQC_APP_103` 等旧项目，对应关系如下：

| 旧项目概念 | NeoAdmin 对应 |
|-----------|---------------|
| AdminBlazor.dll（反编译参考） | `NeoAdmin.Blazor` 源码 |
| Component 目录 | `Components/`（业务页面） |
| Entities 目录 | `Entities/` |
| SeedData 目录 | `SeedData/` |
| NovaSelect* / NovaInput* | `NeoSelect*` / `NeoInput*`（Neo 前缀） |
| FreeSql Repository | 直接 `@inject IFreeSql` 或 `freeSql.Select<T>()` |

**约定：扩展业务时，保持 `Entities/`、`Components/`、`SeedData/` 三处同步维护。**

---

## 2. 快速开始

### 2.1 方式一：项目模板（推荐新建项目）

```bash
mkdir MyAdmin && cd MyAdmin
dotnet new install NeoAdmin.Templates
dotnet new neoadmin -n MyAdmin -o .
dotnet watch run
```

### 2.2 方式二：克隆本仓库

```bash
git clone https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject.git
cd NeoAdminProject/NeoAdmin
dotnet watch run
# 或
./dotnet10.sh
```

默认地址：<http://localhost:5038>

### 2.3 环境要求

| 依赖 | 说明 |
|------|------|
| .NET 10 SDK | `global.json` 指定 `10.0.300` |
| Node.js | 编译宿主 Tailwind CSS（`dotnet build` 自动执行） |
| Docker（可选） | `./docker-auto.sh`，默认端口 5050 |

### 2.4 默认账号

| 项 | 值 |
|----|-----|
| 用户名 | `admin` |
| 密码 | `admin` |

生产环境请修改 `appsettings.json` 中的 `SeedAdminPassword`。

### 2.5 NeoDemo 演示页

启动后访问以下路由对照组件用法：

| 路由前缀 | 内容 |
|---------|------|
| `/neo-demo/ui/*` | NeoUI 基础控件演示（18 个页面） |
| `/neo-demo/comp/*` | NeoAdmin 扩展组件（字典、选择器、上传、权限等） |
| `/neo-demo/comp/file-upload` | `NeoFileUpload` 单文件/多文件上传演示 |
| `/Blog/*` | 博客业务 CRUD 示例 |
| `/admin/*` | 系统管理（框架提供） |

---

## 3. 启动与配置

### 3.1 Program.cs 标准写法

```csharp
using System.Reflection;
using NeoAdmin.Blazor.Components;
using NeoAdmin.Blazor.Extensions;
using NeoAdminApp.SeedData;
using NeoUI.Blazor.Extensions;
using NeoUI.Blazor.Primitives.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddNeoAdminSerilog();

builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddNeoAdmin(builder.Configuration, options =>
{
    // 扫描宿主程序集中带 [Scheduler] 的定时任务
    options.SchedulerAssemblies = [Assembly.GetExecutingAssembly()];
});
builder.Services.AddNeoAdminApi(Assembly.GetExecutingAssembly());

var app = builder.Build();
DataSetup.Initialize(app.Services);  // 宿主：同步业务表结构、菜单种子

app.UseNeoAdminSerilogRequestLogging();
app.UseStaticFiles();
app.MapStaticAssets();
app.UseNeoAdmin();
app.UseAntiforgery();
app.MapRazorComponents<YourApp.App>()
    .AddAdditionalAssemblies(typeof(LayoutAdmin).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();
```

### 3.2 appsettings.json 配置项

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
| `DataType` / `ConnectionString` | FreeSql 数据库类型与连接串（可换 MySQL、PostgreSQL 等） |
| `AutoSyncStructure` | 是否自动同步**框架系统表**结构 |
| `EnableSeedData` | 是否写入业务演示种子（组织、人事、博客等；默认 `true`） |
| `MonitorCommand` | SQL 监控：失败或耗时超过 2 秒的 SQL 以 Error 写入日志（默认 `true`；生产可按需关闭） |
| `WorkId` | 雪花 ID 机器号（0–63，多实例需不同） |
| `EnableIpWhitelist` | IP 白名单（无启用记录时不拦截） |
| `IsSwagger` | Swagger UI 开关 |
| `LogDirectory` / `LogFilePrefix` | Serilog 文件日志目录与前缀（默认 `Logs/admin-*.log`） |
| `FileUpload` | 上传目录、大小、扩展名限制 |

> **注意**：业务表结构需在宿主 `SeedData/DataSetup.SyncStructure` 中显式调用 `freeSql.CodeFirst.SyncStructure<T>()`。

### 3.3 _Imports.razor 全局引用

宿主项目通常包含：

```razor
@using NeoAdmin.Blazor.Components
@using NeoUI.Blazor
@using NeoUI.Blazor.Primitives
@using NeoUI.Blazor.Services
@using NeoUI.Icons.Lucide
```

### 3.4 日志输出

项目已内置 **Serilog**，业务代码直接使用标准 `ILogger<T>` 即可，**无需额外配置**。

#### 3.4.1 已内置的基础设施

`Program.cs` 中调用 `builder.AddNeoAdminSerilog()` 与 `app.UseNeoAdminSerilogRequestLogging()` 后，日志会自动输出到：

| 输出位置 | 内容 |
|---------|------|
| **终端控制台** | 实时日志 |
| **`Logs/admin-YYYYMMDD.log`** | 按天滚动，自动保留最近 30 个文件 |
| **后台 `/admin/system-log`** | 在线浏览、按级别筛选、关键词搜索、自动刷新、清空 INF、删除日志文件 |
| **HTTP 请求** | 自动记录耗时，如 `HTTP GET /api/article responded 200 in 12.3 ms` |
| **SQL 监控** | `MonitorCommand: true` 时，失败或超过 2 秒的 SQL 记为 Error，可在系统日志页筛选 |

#### 3.4.2 各场景写法

**Blazor 页面** — 注入泛型 `ILogger`，类名会自动成为 `SourceContext`：

```razor
@inject ILogger<MyPage> Logger

@code {
    protected override void OnInitialized()
    {
        Logger.LogInformation("进入页面：{Page}", nameof(MyPage));
    }

    private async Task OnSaveAsync()
    {
        try
        {
            // ...
            Logger.LogInformation("保存成功，Id={Id}", id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "保存失败，Id={Id}", id);
        }
    }
}
```

**API 控制器** — 继承 `BaseApiController` 时直接使用 `Logger` 属性：

```csharp
public sealed class MyController : BaseApiController
{
    public MyController(IFreeSql freeSql, NeoAdminAuthService auth, ILogger<MyController> logger)
        : base(freeSql, auth, logger) { }

    [HttpGet]
    public async Task<ApiResult> Get()
    {
        Logger.LogInformation("查询开始");
        // ...
        return ApiResult.Success();
    }
}
```

不继承基类时，构造函数注入 `ILogger<MyController>` 即可（参考 `Api/ArticleController.cs`）。

**Service 类** — 构造函数注入：

```csharp
public sealed class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger) => _logger = logger;

    public async Task ProcessAsync(long orderId)
    {
        _logger.LogInformation("处理订单，OrderId={OrderId}", orderId);
    }
}
```

**定时任务（Jobs）** — 静态方法通过 `ILoggerFactory` 创建：

```csharp
[Scheduler("my.job", "0 0 3 * * *")]
public static async Task Run(IServiceProvider sp, TaskInfo task)
{
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(MyJobs));
    logger.LogInformation("任务开始，TaskId={TaskId}", task.Id);
    // ...
}
```

参考 `Jobs/BlogJobs.cs`。

**Program 启动阶段**：

```csharp
app.Logger.LogInformation("NeoAdmin 启动。Environment={Environment}", app.Environment.EnvironmentName);
```

#### 3.4.3 推荐写法

使用占位符传参，不要用字符串拼接：

```csharp
// 推荐
logger.LogInformation("用户 {UserId} 更新了 {Count} 条记录", userId, count);

// 不推荐
logger.LogInformation($"用户 {userId} 更新了 {count} 条记录");
```

记录异常时带上 `ex` 参数：

```csharp
logger.LogError(ex, "导入失败，文件={FileName}", fileName);
```

常用级别：

| 方法 | 场景 |
|------|------|
| `LogInformation` | 正常业务流程 |
| `LogWarning` | 可恢复异常、数据为空等 |
| `LogError` | 失败、需关注 |
| `LogDebug` | 调试细节（默认不输出，需调低级别） |

#### 3.4.4 查看日志

1. **开发时**：看运行 `dotnet run` / `dotnet watch run` 的终端输出
2. **后台**：登录后打开 **系统日志** → `/admin/system-log`（支持清空 INF 级别、删除当前日志文件）
3. **文件**：应用目录下 `Logs/admin-*.log`

> Serilog 已自动过滤 Blazor Server 页面关闭/刷新时的 Circuit 断开噪声；系统日志页也会隐藏同类无害记录。

可选配置（`appsettings.json`）：

```json
"NeoAdmin": {
  "LogDirectory": "Logs",
  "LogFilePrefix": "admin-"
}
```

> **说明**：`DemoPageLog` 仅供 NeoDemo 演示页使用；业务代码请直接使用 `ILogger<T>`。数据库中的登录日志（`SysUserLoginLog`）不会随文件日志自动清理。

### 3.5 反向代理与 WebSocket（生产部署）

NeoAdmin 为 **Blazor Server**，浏览器通过 SignalR（`/_blazor`）与服务器保持长连接，**必须支持 WebSocket**。

经 Nginx、Caddy、云负载均衡等反向代理对外发布时，请开启 WebSocket 转发。Nginx 示例：

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

| 场景 | 说明 |
|------|------|
| HTTPS 在代理终止 | 设置 `X-Forwarded-Proto https` |
| 多实例 / 多 Pod | 同一用户须固定到同一实例（sticky session），或配置共享 DataProtection |
| 直连 Docker 5050 | 无需额外配置 |

---

## 4. 集成到其他项目

### 4.1 NuGet 安装

```bash
dotnet add package NeoAdmin.Blazor
dotnet new install NeoAdmin.Templates
```

### 4.2 关键集成点

1. `AddNeoUIPrimitives()` + `AddNeoUIComponents()` — 注册 NeoUI
2. `AddNeoAdmin(configuration)` — 注册 FreeSql、鉴权、菜单等服务
3. `AddNeoAdminApi(宿主程序集)` — 注册 REST API
4. `UseNeoAdmin()` — 中间件（IP 白名单等）
5. `AddAdditionalAssemblies(typeof(LayoutAdmin).Assembly)` — 加载框架页面路由
6. 宿主 `DataSetup.Initialize` — 业务表结构与菜单种子

### 4.3 同步到项目模板

修改宿主后执行：

```bash
```

---

## 5. 实体与数据访问（FreeSql）

### 5.1 实体基类继承链

```
Entity（Id 雪花主键）
  └── EntityCreated（CreatedUserId / CreatedUserName / CreatedTime）
        └── EntityModified（ModifiedUserId / ModifiedUserName / ModifiedTime）
              └── EntityAudited（AuditStatus / AuditStep / AuditVersion）
```

| 基类 | 适用场景 |
|------|----------|
| `Entity` | 简单关联表、日志 |
| `EntityCreated` | 只需创建信息 |
| `EntityModified` | 常规业务表（专栏、评论等） |
| `EntityAudited` | 需要审批流的业务（文章等） |

### 5.2 实体定义示例

```csharp
using FreeSql.DataAnnotations;
using NeoAdmin.Blazor.Entities;

namespace NeoAdminApp.Entities.Blog;

[Table(Name = "blog_classify")]
public class Classify : EntityModified
{
    [Column(StringLength = 50)]
    public string ClassifyName { get; set; } = string.Empty;

    public int SortCode { get; set; }
    public int ArticleCount { get; set; }

    [Column(StringLength = 100)]
    public string Thumbnail { get; set; } = string.Empty;
}
```

### 5.3 常用 FreeSql 写法

在 Blazor 页面或服务中注入 `IFreeSql`：

```csharp
@inject IFreeSql FreeSql

// 查询
var list = await FreeSql.Select<BlogClassify>()
    .Where(a => a.ClassifyName.Contains(keyword))
    .OrderByDescending(a => a.Id)
    .ToListAsync();

// 更新
await FreeSql.Update<BlogClassify>()
    .Set(a => a.ArticleCount, count)
    .Where(a => a.Id == id)
    .ExecuteAffrowsAsync();

// 关联查询
var articles = await FreeSql.Select<BlogArticle>()
    .Include(a => a.Classify)
    .Include(a => a.Channel)
    .ToListAsync();
```

### 5.4 CrudTable 默认数据访问

若不指定 `LoadFunc` / `OnSaveAsync` / `OnDeleteAsync`，`CrudTable` 会：

- 通过 `IFreeSql.Select<TItem>()` 自动分页查询
- 在 `OnQuery` 回调中追加 `Where` / `OrderBy`
- 默认执行 Insert / Update / Delete

自定义保存/删除时实现 `OnSaveAsync`、`OnDeleteAsync` 即可覆盖。

### 5.5 框架特性（Attributes）

| 特性 | 用途 |
|------|------|
| `[Snowflake]` | 主键雪花 ID 生成 |
| `[Scheduler("id", "cron")]` | 注册 FreeScheduler 定时任务 |
| `[Transactional]` | 方法级事务（演示见 DemoComp） |
| `[AntiConcurrency]` | 防并发（演示见 DemoComp） |
| `[FileCache]` | 文件缓存（演示见 DemoComp） |
| `[NovaButton]` | 动态按钮注册（演示见 NovaButtonDemo） |

---

## 6. 扩展业务模块

新增业务模块时，**同步维护以下目录**：

| 目录 | 职责 |
|------|------|
| `Entities/` | FreeSql 实体类 |
| `Components/` | Blazor 页面（`@page` + `CrudTable` 等） |
| `SeedData/` | 菜单种子、`SyncStructure`、演示数据 |
| `Api/`（可选） | REST 控制器 |
| `Jobs/`（可选） | `[Scheduler]` 定时任务 |
| `Services/`（可选） | 跨页面复用业务逻辑 |

### 6.1 最小 CRUD 页面模板

```razor
@page "/MyModule/Item"
@layout LayoutAdmin
@using NeoAdmin.Blazor.Components
@inject IFreeSql FreeSql

<PageTitle>示例模块</PageTitle>

<div class="flex flex-col flex-1 min-h-0 overflow-hidden">
    <CrudTable TItem="MyItem" Title="示例" PageSize="20"
               InitQuery="InitQuery" OnQuery="HandleQuery">
        <Columns>
            <DataTableColumn TData="CrudGridRow<MyItem>" TValue="string"
                Property="@(row => row.Item.Name)" Header="名称" Sortable="false" />
        </Columns>
        <EditTemplate Context="item">
            <div class="space-y-2">
                <Label For="item-name">名称</Label>
                <Input id="item-name" Class="w-full" @bind-Value="item.Name" />
            </div>
        </EditTemplate>
    </CrudTable>
</div>

@code {
    private Task InitQuery(CrudQueryInfo e)
    {
        e.Filters = [];
        return Task.CompletedTask;
    }

    private void HandleQuery(CrudQueryEventArgs<MyItem> e)
    {
        e.Select!.WhereIf(!string.IsNullOrWhiteSpace(e.SearchText),
            a => a.Name.Contains(e.SearchText!))
            .OrderByDescending(a => a.Id);
    }
}
```

### 6.2 表结构同步（DataSetup.cs）

```csharp
public static void SyncStructure(IFreeSql freeSql)
{
    freeSql.CodeFirst.SyncStructure<MyItem>();
    // 多对多中间表也需单独 Sync
    freeSql.CodeFirst.SyncStructure<MyItem.MyRelation>();
}
```

### 6.3 菜单种子（MenuSeedData.cs）

```csharp
BlazorMenuSeedData.EnsureMenus(freeSql, CreateMenus());

private static List<SysMenu> CreateMenus() =>
[
    BlazorMenuSeedData.Menu("我的模块", "box", string.Empty, 900, SysMenuSidebarStyle.展开,
    [
        BlazorMenuSeedData.Page("示例", "/MyModule/Item", 901, "file", isSystem: false),
    ], isSystem: false),
];
```

菜单辅助方法：

| 方法 | 说明 |
|------|------|
| `Menu(label, icon, path, sort, ...)` | 目录或普通菜单 |
| `Page(label, path, sort, icon)` | 增删改查页面（自动创建 add/edit/remove 按钮权限） |
| `PageWithAudit(...)` | 增删改查 + 审批按钮 |
| `Button(...)` | 独立按钮权限 |
| `Api(...)` | API 接口权限 |

---

## 7. 页面、路由与菜单注册

### 7.1 Blazor 路由

在 `.razor` 文件顶部声明：

```razor
@page "/Blog/Classify"
@layout LayoutAdmin
```

- 系统管理页路由在 `NeoAdmin.Blazor/Pages/`，如 `/admin/user`
- 业务页放在宿主 `Components/` 下
- `LayoutAdmin`：带侧边栏的后台布局；`LayoutEmpty`：登录页等无侧边栏布局

### 7.2 菜单与路由的对应

菜单 `SysMenu.Path` 必须与 `@page` 路由一致，权限校验才能生效。例如：

- 菜单 Path：`/Blog/Classify`
- 页面：`@page "/Blog/Classify"`

### 7.3 侧边栏渲染

`LayoutAdmin` 自动读取当前用户可见菜单树，通过 `LayoutAdminSidebarMenu` 递归渲染。菜单数据来自 `SysMenu` 表 + 角色-菜单关联。

### 7.4 页面内搜索 Tab（可选）

`PageSearchTabSeedData` 可为复杂页面注册可搜索 Tab（见系统日志等页面）。

---

## 8. 权限体系

### 8.1 菜单类型（SysMenuType）

| 类型 | 用途 |
|------|------|
| 菜单 | 侧边栏导航项 |
| 增删改查 | 页面 + 自动 add/edit/remove 子按钮 |
| 按钮 | 自定义操作按钮 |
| 接口 | REST API 权限 |

### 8.2 代码中校验权限

```csharp
@inject MenuPermissionService MenuPermissionService

// 页面权限
await MenuPermissionService.HasPageAsync("/admin/user");

// 按钮权限（path 为子菜单 Path，如 add / edit / remove）
await MenuPermissionService.HasButtonAsync("/admin/user", "edit");

// API 权限（组名 + 动作名）
await MenuPermissionService.HasApiAsync("login", "ChangePassword");
```

### 8.3 CrudTable 内置权限

`CrudTable` 根据 `MenuPath`（默认当前页面路由）自动控制：

- 工具栏「新增」→ `add` 按钮权限
- 行内「编辑」→ `edit`
- 行内「删除」/ 批量删除 → `remove`

管理员角色（`IsAdministrator`）跳过校验。

### 8.4 用户登录日志

登录成功/失败会写入 `SysUserLoginLog` 表。在 **用户管理** `/admin/user` 行操作中点击「登录日志」可弹窗分页查看该用户记录（与 Serilog 文件日志独立，不会随日志文件清理）。

### 8.5 LayoutAdmin 路由守卫

未授权访问页面时自动跳转 `/forbidden`。

---

## 9. CrudTable 通用 CRUD 组件

`CrudTable<TItem>` 是框架最核心的业务组件，封装了搜索、筛选、分页、多选、弹窗编辑、权限、审批流等能力。

### 9.1 全部 Parameter

`CrudTable` 在保存、删除成功后会自动弹出 Toast；审批中禁止修改/删除时也会 Toast 提示。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Title` | string | `""` | 表格标题 |
| `PageSize` | int | `20` | 每页条数 |
| `PageSizes` | int[] | `[10,20,30,50,100]` | 可选每页条数 |
| `IsSearchText` | bool | `true` | 显示搜索框 |
| `SearchPlaceholder` | string | `"搜索..."` | 搜索占位符 |
| `IsAdd` | bool | `true` | 显示新增按钮 |
| `IsEdit` | bool | `true` | 允许编辑（工具栏级） |
| `IsRowEdit` | bool? | null | 行内编辑，null 时跟随 `IsEdit` |
| `IsRemove` | bool | `true` | 允许删除 |
| `IsRowRemove` | bool? | null | 行内删除 |
| `IsView` | bool | `true` | 允许查看 |
| `IsRowView` | bool | `false` | 行内查看按钮 |
| `IsRefresh` | bool | `true` | 刷新按钮 |
| `IsRowNumber` | bool | `false` | 显示行号列 |
| `IsShowLoading` | bool | `true` | 加载状态 |
| `IsConfirmRemove` | bool | `true` | 删除前确认 |
| `IsMultiSelect` | bool | `true` | 多选复选框 |
| `IsSingleSelect` | bool | `false` | 单选 Radio 模式 |
| `Compact` | bool | `false` | 紧凑模式（隐藏标题，用于嵌入选择器） |
| `FilterExpanded` | bool | `true` | 筛选区默认展开 |
| `IsPickerMode` | bool | `false` | 选择器模式，不触发 OnSelectChanged |
| `KeepSelectionOnPaging` | bool | `false` | 跨页保持勾选 |
| `IsCascadeSelection` | bool | `false` | 树形父子联动勾选 |
| `LevelSelector` | Func<TItem,int>? | null | 树形层级选择器 |
| `SelectionKey` | Func<TItem,object>? | null | 自定义选择主键 |
| `SelectedKeys` | HashSet<object>? | null | 外部绑定已选主键 |
| `NoDataText` | string | `"暂无数据"` | 空数据提示 |
| `AddText` / `SaveText` / `EditText` / `ViewText` / `RemoveText` | string | 中文默认 | 按钮文案 |
| `DialogContentSize` | CrudDialogContentSize | `X2l` | 编辑弹窗宽度 |
| `EditorDialogMaximizable` | bool | `false` | 弹窗可最大化 |
| `EditorDialogOpenMaximized` | bool | `false` | 默认全屏打开 |
| `Columns` | RenderFragment? | null | 列定义（`DataTableColumn`） |
| `EditTemplate` | RenderFragment<TItem>? | null | 编辑表单模板 |
| `ViewTemplate` | RenderFragment<TItem>? | null | 只读查看模板 |
| `RowActionsContent` | RenderFragment<CrudGridRow<TItem>>? | null | 自定义行操作按钮 |
| `ToolbarContent` | RenderFragment? | null | 工具栏左侧扩展 |
| `ToolbarTrailingContent` | RenderFragment? | null | 「新增」按钮右侧扩展 |
| `HeaderContent` / `FooterContent` | RenderFragment? | null | 表格头尾扩展 |
| `EditDialogFooterLeading` | RenderFragment<TItem>? | null | 编辑弹窗底部自定义按钮 |
| `ItemsSource` | List<TItem>? | null | 静态数据源（不走 FreeSql） |
| `LoadFunc` | Func<Task<List<TItem>>>? | null | 自定义加载 |
| `OnSaveAsync` | Func<TItem,bool,Task<bool>>? | null | 自定义保存（第二参数 isNew） |
| `OnDeleteAsync` | Func<TItem,Task<bool>>? | null | 自定义删除 |
| `CanbeSelect` | Func<TItem,bool>? | null | 行是否可选 |
| `IsWorkflowAudit` | bool | `false` | 启用审批流 |
| `WorkflowAuditMenuPath` | string | `""` | 审批按钮权限路径 |
| `WorkflowAuditDialogContentSize` | CrudDialogContentSize | `X2l` | 审批弹窗宽度 |
| `MenuPath` | string | 当前路由 | 权限判断路径 |
| `IsRowHighlighted` | Func<TItem,bool>? | null | 行高亮（如左右分栏选中） |

### 9.2 EventCallback 事件

| 事件 | 说明 |
|------|------|
| `InitQuery` | 初始化 `CrudQueryInfo`（配置筛选器） |
| `OnQuery` | 追加 FreeSql 查询条件 |
| `OnQueried` | 查询完成后回调 |
| `OnEdit` | 打开编辑弹窗前 |
| `OnEditClose` | 关闭编辑弹窗后 |
| `OnSaving` / `OnSaved` | 保存前/后 |
| `OnRemoving` / `OnRemoved` | 单条删除前/后 |
| `OnBatchRemoving` / `OnBatchRemoved` | 批量删除前/后 |
| `OnSelectChanged` | 多选变化 |
| `SelectedKeysChanged` | 已选主键集合变化 |
| `OnRowClick` | 行点击（字典左右分栏常用） |
| `OnWorkflowAuditChanged` | 审批状态变化 |

### 9.3 筛选器（CrudFilterInfo）

在 `InitQuery` 中配置 `e.Filters`：

```csharp
private Task InitQuery(CrudQueryInfo e)
{
    e.Filters =
    [
        // 标签单选：texts 与 values 逗号对应
        new CrudFilterInfo("状态", "status", "全部,启用,禁用", ",-1,1"),
        // 枚举筛选
        new CrudFilterInfo("类型", "type", typeof(MyEnum)),
        // 日期范围
        new CrudFilterInfo("创建时间", "date", CrudFilterType.DateRange, 12),
        // 文本
        new CrudFilterInfo("关键字", "kw", CrudFilterType.Text, 6),
    ];
    return Task.CompletedTask;
}

private void HandleQuery(CrudQueryEventArgs<MyItem> e)
{
    var status = e.Filters.First(f => f.QueryStringName == "status");
    if (status.HasValue)
        e.Select!.Where(a => a.Enabled == status.Value<bool>());

    if (e.Filters.First(f => f.QueryStringName == "date").TryGetDateRange(out var start, out var end))
    {
        if (start.HasValue) e.Select!.Where(a => a.CreatedTime >= start);
        if (end.HasValue) e.Select!.Where(a => a.CreatedTime < end.Value.AddDays(1));
    }
}
```

### 9.4 弹窗宽度 CrudDialogContentSize

| 枚举值 | Tailwind 类 |
|--------|------------|
| Sm ~ X7l | `sm:max-w-sm` ~ `sm:max-w-7xl` |
| FullScreen | 全屏弹窗 |

### 9.5 真实示例：随笔专栏（简单 CRUD）

见 `NeoAdmin/Components/Blog/Classify.razor`：

```razor
<CrudTable @ref="table" TItem="BlogClassify" Title="随笔专栏" PageSize="50"
           IsMultiSelect="false" IsSingleSelect="false"
           InitQuery="InitQuery" OnQuery="HandleQuery"
           DialogContentSize="CrudDialogContentSize.Lg">
    <Columns>...</Columns>
    <EditTemplate Context="item">...</EditTemplate>
</CrudTable>
```

### 9.6 真实示例：左右分栏（字典管理）

见 `NeoAdmin.Blazor/Pages/Dict.razor`，使用 `SplitPane` + 两个 `CrudTable`，左侧 `OnRowClick` 驱动右侧数据。

---

## 10. NeoAdmin.Blazor 组件详解

以下为框架自带的 **22 个** Blazor 组件（含布局与内部壳组件）。

### 10.1 CrudTable\<TItem\>

见 [第 9 章](#9-crudtable-通用-crud-组件)。

### 10.2 CrudSearchFilter

| 参数 | 类型 | 说明 |
|------|------|------|
| `QueryInfo` | CrudQueryInfo | 筛选配置（必填） |
| `FilterExpanded` | bool | 默认 `true` |

内部组件，由 `CrudTable` 自动渲染。

### 10.3 CrudAuditPanel

| 参数 | 类型 | 说明 |
|------|------|------|
| `Item` | EntityAudited | 审批实体 |
| `EntityType` | Type | 实体类型 |
| `MenuPath` | string | 权限路径 |
| `OnChanged` | EventCallback\<EntityAudited\> | 审批后回调 |

### 10.4 CrudTableSingleSelectShell

单选 Radio 包裹壳，一般不由业务直接使用。

### 10.5 NeoSelectDict

按字典分类加载下拉选项。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ParentName` | string | `""` | 字典分类编码（父级 Name） |
| `Value` | string | `""` | 绑定值 |
| `Source` | List\<SysDict\>? | null | 自定义数据源 |
| `DisplayText` | Func\<SysDict,string\>? | null | 显示文本 |
| `Disabled` | bool | false | 禁用 |
| `Placeholder` | string | `"请选择.."` | 占位符 |
| `ValueChanged` / `OnValueChanged` | EventCallback\<string\> | | 值变化 |

```razor
<NeoSelectDict ParentName="Gender" @bind-Value="genderValue" />
```

### 10.6 NeoSelectEnum\<TEnum\>

| 参数 | 类型 | 说明 |
|------|------|------|
| `Value` | TEnum | 枚举值 |
| `Disabled` | bool | 禁用 |
| `ValueChanged` / `OnValueChanged` | EventCallback\<TEnum\> | 值变化 |

```razor
<NeoSelectEnum TEnum="ArticleType" @bind-Value="item.ArticleType" />
```

### 10.7 NeoSelectEntity\<TItem, TKey\>

| 参数 | 类型 | 说明 |
|------|------|------|
| `Value` | TKey | 主键 |
| `DisplayText` | Func\<TItem,string\>? | 显示文本 |
| `Source` | List\<TItem\>? | 静态数据源 |
| `Disabled` | bool | 禁用 |
| `Placeholder` | string | 占位符 |
| `OnQuery` | Action\<ISelect\<TItem\>\>? | 自定义 FreeSql 查询 |
| `ValueChanged` / `OnValueChanged` | EventCallback\<TKey\> | 值变化 |

```razor
<NeoSelectEntity TItem="BlogClassify" TKey="long?" @bind-Value="item.ClassifyId"
    DisplayText="e => e.ClassifyName" />
```

### 10.8 NeoSelectTable\<TItem, TKey\>

页面内嵌表格选择器。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Value` | TKey | | 单选主键 |
| `Items` | List\<TItem\>? | null | 多选列表 |
| `PageSize` | int | 20 | 分页 |
| `IsSearchText` | bool | true | 搜索 |
| `SearchPlaceholder` | string | `"搜索.."` | |
| `KeepSelectionOnPaging` | bool | true | 跨页保持 |
| `RowTemplate` | RenderFragment\<TItem\>? | | 行显示 |
| `ValueChanged` | EventCallback\<TKey\> | | 单选变化 |
| `ItemsChanged` | EventCallback\<List\<TItem\>\> | | 多选变化 |
| `OnQuery` | EventCallback\<CrudQueryEventArgs\<TItem\>\> | | 查询过滤 |

> 绑定了 `ItemsChanged` 时自动进入多选模式；仅 `Value`/`ValueChanged` 时为单选。

### 10.9 NeoInputTable\<TItem, TKey\>

弹窗表格选择器（只读输入框 +「选择」按钮）。

| 参数 | 类型 | 说明 |
|------|------|------|
| `Value` / `ValueChanged` | TKey | 单选主键 |
| `Item` / `ItemChanged` | TItem? | 单选实体 |
| `Items` / `ItemsChanged` | List\<TItem\> | 多选列表 |
| `DisplayText` | Func\<TItem,string\>? | 输入框显示 |
| `ModalTitle` | string | 弹窗标题，默认 `"选择.."` |
| `PageSize` | int | 默认 20 |
| `IsSearchText` | bool | 默认 true |
| `SearchPlaceholder` | string? | |
| `DialogClassName` | string | 弹窗样式类 |
| `IsCustomUI` | bool | 自定义 UI |
| `ChildContent` | RenderFragment? | |
| `TableHeader` / `TableRow` / `TableCell` | RenderFragment | 表格模板 |
| `OnQuery` | EventCallback | 查询过滤 |

依赖全局 `NeoPickerOverlayHost`（已在 `LayoutAdmin` 挂载）。

### 10.10 NeoInputTags\<TItem\>

| 参数 | 类型 | 说明 |
|------|------|------|
| `Value` | ICollection\<TItem\> | 标签集合（必填） |
| `DisplayText` | Func\<TItem,string\> | 显示（必填） |
| `OnCreate` | Func\<string,TItem\> | 创建新标签（必填） |
| `OnSearch` | Func\<string,Task\<IEnumerable\<TItem\>\>\> | 搜索建议（必填） |
| `Placeholder` | string | 默认 `"输入后回车添加.."` |
| `DropdownAsGrid` | bool | 下拉网格布局 |
| `GridMinItemWidth` | int | 网格最小列宽 |
| `ValueChanged` | EventCallback | 集合变化 |

### 10.11 NeoAllocTable\<TItem, TChild, TKey\>

主从分配弹窗（如角色分配用户、角色分配菜单）。

| 参数 | 类型 | 说明 |
|------|------|------|
| `Item` / `ItemChanged` | TItem? | 主实体 |
| `Title` | string | 弹窗标题 |
| `DialogContentSize` | CrudDialogContentSize | 弹窗宽度 |
| `IsSearchText` | bool | 搜索 |
| `SearchPlaceholder` | string | |
| `KeySelector` | Func\<TChild,TKey\> | 子实体主键（必填） |
| `DisplayText` | Func\<TChild,string\> | 显示文本（必填） |
| `LevelSelector` | Func\<TChild,int\>? | 树形层级 |
| `IsCascadeSelection` | bool | 父子联动，默认 true |
| `RowTemplate` | RenderFragment\<TChild\>? | |
| `LoadChoicesFunc` | Func\<string?,Task\<List\<TChild\>\>\>? | 自定义全量加载（树形菜单等，不分页） |
| `LoadChoicesPagedFunc` | Func\<NeoAllocChoicesRequest,Task\<NeoAllocChoicesResult\<TChild\>\>\>? | 分页加载可选项（大数据量推荐，如角色分配用户） |
| `ChoicesPageSize` | int | 500 | 每页可选项数量 |
| `DeferChoicesUntilSearch` | bool? | null | 为 `true` 时须输入关键词后才加载（避免首次打开拉全表） |
| `SearchRequiredHint` | string | `"请输入关键词后搜索"` | 延迟搜索时的提示文案 |
| `LoadSelectedKeysAsync` | Func\<TItem,Task\<IReadOnlyCollection\<TKey\>\>\>? | 加载已选 |
| `OnSaveAsync` | Func\<TItem,IReadOnlyCollection\<TKey\>,Task\<bool\>\>? | 保存关联 |
| `OnQuery` | EventCallback | 查询过滤 |

分页加载示例（角色分配用户，见 `Role.razor`）：

```csharp
private Task<NeoAllocChoicesResult<SysUser>> LoadRoleUserChoicesPagedAsync(NeoAllocChoicesRequest request) =>
    RoleService.GetUsersForAllocPagedAsync(request);
```

```razor
<NeoAllocTable LoadChoicesPagedFunc="LoadRoleUserChoicesPagedAsync"
               LoadSelectedKeysAsync="LoadRoleUserKeysAsync"
               OnSaveAsync="SaveRoleUsersAsync" ... />
```

树形菜单等仍用 `LoadChoicesFunc` 一次加载全量，组件内部使用 `Virtualize` 虚拟滚动。

### 10.12 NeoAllocSelectionBadges

| 参数 | 类型 | 说明 |
|------|------|------|
| `Labels` | IEnumerable\<string\>? | 只读 Badge 列表展示 |

### 10.13 NeoParamText

从系统参数表读取并渲染文本。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `ParamKey` | string | `""` | 参数键 |
| `Field` | string | `Value` | 读取字段 |
| `DefaultText` | string | `""` | 缺省文本 |
| `TagName` | string | `"span"` | HTML 标签 |
| `Class` / `Style` | string? | | 样式 |
| `EnabledOnly` | bool | true | 仅启用参数 |

```razor
<NeoParamText ParamKey="Home_ContactCard" DefaultText="未配置" TagName="div" />
```

### 10.14 NeoFileUpload

封装 NeoUI `FileUpload` 与框架 `FileService`：选文件后自动上传；**图片**显示预览，**其他格式**显示文件名链接；带清除按钮，清除后可重新上传。支持单文件绑定与多文件批量两种模式。

**演示页**：`/neo-demo/comp/file-upload`（标签页：**图片** / **文件**）

#### 单文件模式（默认）

绑定实体字段中的**相对路径**（如 `uploads/20250614/xxx.png`），适用于 CRUD 编辑表单、站点 LOGO 等。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Value` / `ValueChanged` | string? | | 文件相对路径（`LinkUrl`） |
| `DisplayName` | string? | null | 非图片时的展示文件名 |
| `UploadDirectory` | string | `"uploads"` | 上传到 `wwwroot` 下的子目录 |
| `MaxFileSize` | long | 5MB | 单文件大小上限（字节） |
| `Accept` | string | `""` | 浏览器 accept，如 `image/*`、`.pdf`；空表示不限制 |
| `PreviewLabel` | string? | `"当前文件"` | 已上传时的标题 |
| `EmptyHint` | string? | 见组件 | 未上传时的提示 |
| `HelpText` | string? | null | 底部说明 |
| `ClearButtonText` | string | `"清除"` | 清除按钮文字 |
| `PreviewImageClass` | string | 见组件 | 图片预览 CSS 类 |
| `ShowLocalPreview` | bool | true | 选择时 NeoUI 本地预览 |
| `ShowToastOnSuccess` | bool | true | 上传成功 Toast |
| `ToastTitle` | string | `"文件"` | Toast 标题 |
| `Disabled` | bool | false | 禁用 |
| `OnUploaded` | EventCallback\<SysFile\> | | 上传成功（可选，用于同步其他字段或立即落库） |
| `OnCleared` | EventCallback | | 点击清除后（可选，用于同步 `DisplayName` 或立即落库） |

```razor
@* CRUD 编辑：保存表单时一并写入数据库 *@
<NeoFileUpload @bind-Value="item.Thumbnail"
               UploadDirectory="blog/classify"
               Accept="image/*"
               HelpText="仅图片，最大 5MB。" />

@* 文档附件：同步原始文件名 *@
<NeoFileUpload @bind-Value="item.AttachmentUrl"
               DisplayName="@item.AttachmentName"
               Accept=".pdf,.doc,.docx"
               ShowLocalPreview="false"
               OnUploaded="f => item.AttachmentName = f.OriginFileName"
               OnCleared="() => item.AttachmentName = null" />

@* 上传后立即保存（站点设置 LOGO） *@
<NeoFileUpload @bind-Value="model.Logo"
               UploadDirectory="site"
               Accept="image/*"
               PreviewLabel="当前已设置"
               ClearButtonText="清除 LOGO"
               ShowToastOnSuccess="false"
               OnUploaded="HandleLogoUploadedAsync"
               OnCleared="HandleLogoClearedAsync" />
```

> **`OnUploaded` / `OnCleared` 何时需要？**  
> 仅 `@bind-Value` 即可更新 URL 字段；若还有 `DisplayName` 等关联字段要同步，或像站点设置那样**上传/清除后立即写库**，再使用这两个回调。

#### 多文件模式

设置 `Multiple="true"` 时始终显示上传区，不展示单文件预览；适合文件管理页批量上传。

| 参数 | 类型 | 说明 |
|------|------|------|
| `Multiple` | bool | `true` 启用多文件 |
| `ShowProgress` | bool | 是否显示进度条 |
| `OnAllUploaded` | EventCallback\<NeoFileUploadBatchResult\> | 当前选择全部上传完成 |
| `ResetAsync()` | 方法 | 对话框关闭时重置内部状态 |
| `IsUploading` | bool | 是否正在上传 |

```razor
<NeoFileUpload @ref="fileUpload"
               Multiple="true"
               UploadDirectory=""
               MaxFileSize="104857600"
               ShowLocalPreview="false"
               ToastTitle="上传文件"
               OnAllUploaded="HandleAllUploadedAsync" />

@code {
    private NeoFileUpload? fileUpload;

    private async Task HandleAllUploadedAsync(NeoFileUploadBatchResult result)
    {
        await table.ReloadAsync();
        await table.CloseEditorAsync();
    }

    private async Task HandleEditClose(SysFile _)
    {
        if (fileUpload is not null)
            await fileUpload.ResetAsync();
    }
}
```

参考实现：`NeoAdmin.Blazor/Pages/File.razor`（文件管理）、`Pages/SiteSettings.razor`（品牌图片）。

### 10.15 NeoPickerOverlayHost

全局弹窗宿主，无 Parameter。由 `NeoPickerOverlayService` 驱动，已在布局中自动挂载。

### 10.16 SplitPane

左右可拖拽分栏。

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `FirstPaneTemplate` | RenderFragment? | | 左栏内容 |
| `SecondPaneTemplate` | RenderFragment? | | 右栏内容 |
| `Basis` | string | `"22%"` | 左栏默认宽度 |
| `FirstPaneMinimumSize` | string | `"330px"` | 左栏最小宽度 |
| `ShowBarHandle` | bool | true | 显示拖拽条 |
| `Class` | string? | | 容器样式 |

### 10.17 LayoutAdmin / LayoutEmpty

| 布局 | 用途 |
|------|------|
| `LayoutAdmin` | 后台主布局：侧边栏、顶栏、主题切换、Toast/Dialog 宿主、登录校验、侧边栏底部框架版本 Badge |
| `LayoutEmpty` | 无侧边栏（登录页） |

页面声明：`@layout LayoutAdmin`

### 10.18 LayoutAdminSidebarMenu

| 参数 | 类型 | 默认值 |
|------|------|--------|
| `Items` | IReadOnlyList\<SysMenu\> | 空 |
| `Level` | int | 0 |

一般由 `LayoutAdmin` 内部使用。

### 10.19 DefaultLogin

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `UsernameLabel` | string | `"用户名"` | 账号标签 |
| `UsernamePlaceholder` | string | `"请输入用户名"` | |
| `UsernameRequiredMessage` | string | `"请输入用户名"` | 校验提示 |
| `UsernameLengthMessage` | string | `"用户名不能超过 50 个字符"` | |

### 10.20 DemoSection / SearchableTabs

演示页辅助组件：

| 组件 | 主要参数 |
|------|----------|
| `DemoSection` | `Title`, `Description`, `Code`, `ChildContent`, `AiNotes` |
| `SearchableTabs` | `DefaultValue`, `Class`, `ChildContent`（与 URL `?tab=` 同步） |

---

## 11. NeoUI.Blazor 控件详解

NeoUI 是独立 NuGet 包（`NeoUI.Blazor` 4.x），源码不在本仓库。**完整 API 以官方文档为准**：<https://neoui.io>，在线演示：<https://demos.neoui.io>。

本仓库 `NeoAdmin/Components/Pages/DemoUI/` 下有 **18 个演示页**，每个页面配套 `Snippets/*.cs` 源码片段，是学习的最佳入口。

### 11.1 组件总览（约 150+ 个）

以下按功能分类列出 DemoUI 中出现的全部 NeoUI 组件名（共 **约 155 个**标签组件）。

#### 布局与结构（42）

`Accordion`, `AccordionContent`, `AccordionItem`, `AccordionTrigger`, `AspectRatio`, `Button`, `ButtonGroup`, `Card`, `CardContent`, `CardDescription`, `CardFooter`, `CardHeader`, `CardTitle`, `Collapsible`, `CollapsibleContent`, `CollapsibleTrigger`, `ResizableHandle`, `ResizablePanel`, `ResizablePanelGroup`, `ScrollArea`, `Separator`, `Sidebar` 系列（15 个）, `Sortable` 系列（4 个）, `SplitButton` 系列（3 个）, `Tabs`, `TabsContent`, `TabsList`, `TabsTrigger`, `ThemeSwitcher`, `Typography`

#### 表单与输入（48）

`Checkbox`, `ColorPicker`, `Combobox`, `CurrencyInput`, `DateInput`, `DatePicker`, `DateRangePicker`, `DynamicForm`, `Field` 系列（4 个）, `FileUpload`, `FilterBuilder` 系列（3 个）, `Input`, `InputGroup` 系列（4 个）, `InputOtp` 系列（3 个）, `Label`, `MaskedInput`, `MultiSelect`, `NativeSelect`, `NativeSelectOption`, `NumericInput`, `RadioGroup`, `RadioGroupItem`, `RangeSlider`, `Rating`, `RichTextEditor`, `MarkdownEditor`, `Select` 系列（6 个）, `Slider`, `Switch`, `TagInput`, `Textarea`, `TimeInput`, `TimePicker`, `Toggle`, `ToggleGroup`, `ToggleGroupItem`, `Calendar`

#### 数据展示（35）

`Avatar` 系列, `Badge`, `BadgeIcon`, `DataGrid`, `DataGridColumn`, `DataTable`, `DataTableColumn`, `DataView`, `DataViewColumn`, `Empty` 系列（5 个）, `Item` 系列（8 个）, `Kbd`, `Skeleton`, `Timeline` 系列（8 个）, `TreeView`

#### 导航（38）

`Breadcrumb` 系列（6 个）, `Command` 系列（8 个）, `Menubar` 系列（12 个）, `NavigationMenu` 系列（7 个）, `Pagination` 系列（7 个）, `ResponsiveNav` 系列（3 个）

#### 浮层与弹窗（48）

`AlertDialog` 系列（9 个）, `ContextMenu` 系列（5 个）, `Dialog` 系列（8 个）, `Drawer` 系列（8 个）, `DropdownMenu` 系列（9 个）, `HoverCard` 系列（3 个）, `Popover` 系列（3 个）, `Sheet` 系列（8 个）, `Tooltip` 系列（4 个）

#### 反馈（4）

`Alert` 系列, `Progress`, `Spinner`

#### 图表（28）

`Area`, `AreaChart`, `Bar`, `BarChart`, `Candlestick`, `CandlestickChart`, `ChartContainer`, `ChartTooltip`, `ComposedChart`, `Funnel`, `FunnelChart`, `Gauge`, `GaugeChart`, `Grid`, `Heatmap`, `HeatmapChart`, `Legend`, `Line`, `LineChart`, `Pie`, `PieChart`, `PolarGrid`, `Radar`, `RadarChart`, `RadarGrid`, `RadialBar`, `RadialBarChart`, `Scatter`, `ScatterChart`, `XAxis`, `YAxis`

#### 移动端（6）

`AppBar`, `BottomNav`, `BottomNavItem`, `NotificationBadge`, `QuantityStepper`, `SectionHeader`

#### 图标

`LucideIcon` — `Name`（图标名）, `Size`, `Class`

### 11.2 常用控件属性速查

#### Button

| 属性 | 说明 | 示例值 |
|------|------|--------|
| `Variant` | 样式变体 | `Default`, `Outline`, `Ghost`, `Destructive` |
| `Size` | 尺寸 | `Small`, `Default`, `Large` |
| `Disabled` | 禁用 | `true` / `false` |
| `Title` | 提示文字 | `"删除"` |
| `OnClick` | 点击事件 | `HandleClick` |
| `ChildContent` | 按钮内容 | 文字或图标 |

#### Input

| 属性 | 说明 |
|------|------|
| `Type` | `Text`, `Email`, `Password`, `Search`, `Number` 等 |
| `Value` / `@bind-Value` | 文本值 |
| `Placeholder` | 占位符 |
| `MaxLength` | 最大长度 |
| `Id` | 关联 Label 的 for |
| `Class` | Tailwind 样式 |
| `Disabled` | 禁用 |

#### Textarea / RichTextEditor

多行文本推荐使用 NeoUI **`RichTextEditor`** 并关闭工具栏（框架系统页与博客示例已统一采用，中文 IME 输入更稳定）：

```razor
<RichTextEditor Id="description"
                Toolbar="RichTextEditorToolbar.None"
                Class="min-h-24 w-full"
                @bind-Value="item.Description" />
```

`Textarea` 仍可用于简单纯文本场景。

#### NumericInput\<TValue\>

| 属性 | 说明 |
|------|------|
| `TValue` | `int`, `decimal` 等 |
| `Min` / `Max` / `Step` | 数值范围与步进 |
| `@bind-Value` | 绑定数值 |

#### Checkbox

| 属性 | 说明 |
|------|------|
| `@bind-Checked` | 勾选状态 |
| `Checked` + `CheckedChanged` | 手动控制 |
| `Indeterminate` | 半选状态 |
| `Id` | 关联 Label |

#### Switch

| 属性 | 说明 |
|------|------|
| `@bind-Checked` | 开关状态 |
| `Size` | `Small`, `Medium`, `Large` |
| `Id` | 关联 Label |

#### Select / NativeSelect

| 属性 | 说明 |
|------|------|
| `TValue` | 值类型 |
| `@bind-Value` | 选中值 |
| `Placeholder` | 未选择提示 |
| `Class` | 宽度等样式 |
| `Size`（NativeSelect） | `Small`, `Default`, `Large` |

子组件：`SelectTrigger` + `SelectValue` + `SelectContent` + `SelectItem`；或 `NativeSelectOption`。

#### RadioGroup

| 属性 | 说明 |
|------|------|
| `TValue` | 值类型 |
| `@bind-Value` | 选中值 |
| `Class` | 布局样式 |

子组件：`RadioGroupItem` + `Label`。

#### Slider / RangeSlider

| 属性 | 说明 |
|------|------|
| `@bind-Value` | 当前值 |
| `Min` / `Max` / `Step` | 范围 |

#### DataTable / DataTableColumn

CrudTable 内部也使用 DataTable。独立使用时：

| 属性（DataTable） | 说明 |
|-------------------|------|
| `TData` | 行数据类型 |
| `Data` | 数据列表 |
| `ShowToolbar` | 工具栏 |
| `ShowPagination` | 分页 |
| `InitialPageSize` | 初始每页条数 |
| `SelectionMode` | 选择模式 |
| `SelectedItems` / `SelectedItemsChanged` | 多选 |
| `IsLoading` | 加载中 |
| `Dense` | 紧凑行高 |
| `ColumnSizing` | `Fixed` / `Auto` |

| 属性（DataTableColumn） | 说明 |
|-------------------------|------|
| `TData` / `TValue` | 行列类型 |
| `Property` | 字段表达式 |
| `Header` | 列标题 |
| `Sortable` | 可排序 |
| `Width` | 列宽 |
| `Alignment` | `Left` / `Center` / `Right` |
| `CellTemplate` | 自定义单元格 |

#### Dialog / AlertDialog / Drawer / Sheet

| 通用属性 | 说明 |
|----------|------|
| `@bind-Open` | 显示/隐藏 |
| `*Trigger` | 触发器 |
| `*Content` | 内容区 |
| `*Header` / `*Title` / `*Description` | 标题区 |
| `*Footer` | 底部按钮区 |
| `Class`（Content） | 如 `sm:max-w-md` |

Drawer 额外：`Direction` = `Left` / `Right` / `Top` / `Bottom`

#### Alert

| 属性 | 说明 |
|------|------|
| `Variant` | `Default`, `Success`, `Destructive`, `Warning` |
| `AccentBorder` | 强调边框 |
| `Icon` / `ChildContent` | 图标与内容 |

#### Badge

| 属性 | 说明 |
|------|------|
| `Variant` | `Default`, `Secondary`, `Destructive`, `Outline` |
| `Class` | 自定义颜色 |

#### TreeView

| 属性 | 说明 |
|------|------|
| `TItem` | 节点类型 |
| `Items` | 树数据 |
| `ValueField` / `TextField` | 值/文本字段 |
| `ChildrenProperty` | 子节点属性 |
| `SelectionMode` | `Single` / `Multiple` |
| `@bind-SelectedValue` | 选中节点 |
| `DefaultExpandDepth` | 默认展开层级 |
| `ShowLines` | 连接线 |

#### Toast（服务式，非标签）

```csharp
@inject IToastService ToastService

ToastService.Show(new ToastOptions
{
    Title = "已保存",
    Description = "更改已生效",
    Variant = ToastVariant.Success
});
```

#### DialogService（服务式）

```csharp
@inject DialogService DialogService

var result = await DialogService.ConfirmAsync(new DialogOptions
{
    Title = "确认提交",
    Message = "确定要保存吗？",
    Buttons = DialogButtons.OkCancel
});
```

### 11.3 演示页与组件对照表

| 演示页 | 路由 | 主要组件 |
|--------|------|----------|
| FormInputsDemo | `/neo-demo/ui/form-inputs` | Field, Input, Label, Textarea, NumericInput |
| FormControlsDemo | `/neo-demo/ui/form-controls` | Checkbox, Select, RadioGroup, Switch, Slider, Rating, Toggle |
| AdvancedInputsDemo | `/neo-demo/ui/advanced-inputs` | Combobox, MultiSelect, TagInput, ColorPicker, MaskedInput, InputOtp |
| AdvancedDateTimeDemo | `/neo-demo/ui/advanced-datetime` | DatePicker, DateRangePicker, TimePicker, Calendar |
| AdvancedComplexDemo | `/neo-demo/ui/advanced-complex` | DynamicForm, FileUpload, FilterBuilder, RichTextEditor |
| LayoutControlsDemo | `/neo-demo/ui/layout-controls` | Tabs, Accordion, Collapsible, ButtonGroup |
| LayoutToolsDemo | `/neo-demo/ui/layout-tools` | ResizablePanelGroup, Sortable, ThemeSwitcher |
| DisplayBasicsDemo | `/neo-demo/ui/display-basics` | Avatar, Badge, Card, Typography |
| DisplayStatesDemo | `/neo-demo/ui/display-states` | Empty, Skeleton, Item, Kbd |
| DataDisplayDemo | `/neo-demo/ui/data-display` | DataTable, DataGrid, DataView, TreeView, Timeline |
| FeedbackDemo | `/neo-demo/ui/feedback` | Alert, Progress, Spinner, Toast |
| OverlaysModalDemo | `/neo-demo/ui/overlays-modal` | Dialog, AlertDialog, Drawer, Sheet, DialogService |
| OverlaysFloatingDemo | `/neo-demo/ui/overlays-floating` | DropdownMenu, Popover, Tooltip, ContextMenu |
| NavigationDemo | `/neo-demo/ui/navigation` | Breadcrumb, Pagination, Sidebar, Command |
| ChartDemo | `/neo-demo/ui/chart` | 各类 Chart 组件 |
| MobileDemo | `/neo-demo/ui/mobile` | AppBar, BottomNav 等 |
| AntiConcurrencyDemo | `/neo-demo/ui/anti-concurrency` | 防并发特性演示 |
| FileUploadDemo | `/neo-demo/comp/file-upload` | NeoFileUpload 单文件/多文件 |

---

## 12. 表单、表格、弹窗与服务

### 12.1 表单布局推荐写法

使用 `Field` + `Label` + `Input` 组合，保持可访问性：

```razor
<Field Orientation="FieldOrientation.Vertical">
    <FieldLabel For="field-email">邮箱</FieldLabel>
    <FieldContent>
        <Input Id="field-email" Type="InputType.Email" @bind-Value="email" />
        <FieldDescription>我们不会分享你的邮箱。</FieldDescription>
    </FieldContent>
</Field>
```

简单场景可直接：

```razor
<div class="space-y-2">
    <Label For="name">名称</Label>
    <Input id="name" Class="w-full" @bind-Value="item.Name" />
</div>
```

### 12.2 表格选型

| 场景 | 推荐组件 |
|------|----------|
| 标准后台 CRUD | `CrudTable` |
| 自定义列表（无 CRUD） | `DataTable` |
| 高性能大数据表格 | `DataGrid` |
| 卡片/列表切换 | `DataView` |
| 树形数据 | `TreeView` |
| 实体单选/多选 | `NeoInputTable` / `NeoSelectTable` |
| 单文件上传（图片/附件） | `NeoFileUpload` |
| 多文件批量上传 | `NeoFileUpload` + `Multiple="true"` |
| 主从分配 | `NeoAllocTable` |
| 左右联动 | `SplitPane` + 两个 `CrudTable` |

### 12.3 弹窗选型

| 场景 | 推荐 |
|------|------|
| CrudTable 新增/编辑 | 内置 Dialog（`EditTemplate`） |
| 确认删除 | CrudTable 内置 / `AlertDialog` |
| 简单确认 | `DialogService.ConfirmAsync` |
| 右侧详情 | `Drawer` |
| 移动端全屏 | `Sheet` |
| 实体选择 | `NeoInputTable`（NeoPickerOverlayHost） |

### 12.4 常用注入服务

| 服务 | 用途 |
|------|------|
| `IFreeSql` | 数据访问 |
| `IToastService` | 轻提示 |
| `DialogService` | 确认/提示对话框 |
| `MenuPermissionService` | 权限判断 |
| `NeoAdminAuthService` | 当前登录用户 |
| `AuditWorkflowService` | 审批流操作 |
| `FileService` | 文件上传管理 |
| `RoleService` / `OrgService` | 角色/组织业务 |

---

## 13. 审批流

### 13.1 启用条件

1. 实体继承 `EntityAudited`
2. `CrudTable` 设置 `IsWorkflowAudit="true"` 和 `WorkflowAuditMenuPath`
3. 菜单种子使用 `PageWithAudit` 或调用 `AuditMenuSeedData.EnsureButtons`

```razor
<CrudTable TItem="BlogArticle" IsWorkflowAudit="true" WorkflowAuditMenuPath="/Blog/Article" ...>
```

```csharp
AuditMenuSeedData.EnsureButtons(freeSql, "/Blog/Article");
```

### 13.2 审批状态

`SysAuditStatus`：草稿、待审、已通过、已拒绝等。`CrudTable` 自动显示审批状态列；已提交数据默认禁止编辑/删除（除非规则允许）。

### 13.3 编辑页嵌入审批面板

```razor
<CrudAuditPanel Item="item" EntityType="typeof(BlogArticle)"
    MenuPath="/Blog/Article" OnChanged="HandleAuditChanged" />
```

完整示例见 `NeoAdmin/Components/Blog/Article.razor`。

---

## 14. REST API 与定时任务

### 14.1 REST API

路由约定：`[HttpGet($"@{{nameof(MethodName)}}")]`，权限对应菜单 `Api` 类型。

```csharp
[ApiController]
[Route("api/article")]
public sealed class ArticleController : ControllerBase
{
    [HttpGet($"@{nameof(GetAll)}")]
    [AllowAnonymous]
    public async Task<ApiResult<ArticleListResponse>> GetAll() { ... }
}
```

在 `Program.cs` 中 `AddNeoAdminApi(Assembly.GetExecutingAssembly())` 注册。

Swagger：开发环境默认开启，访问 `/api`。

### 14.2 定时任务

```csharp
[Scheduler("blog.sync-article-stats", "0 0 3 * * *")]  // 每天 03:00
public static async Task SyncArticleStats(IServiceProvider sp, TaskInfo task)
{
    var freeSql = sp.GetRequiredService<IFreeSql>();
    // 业务逻辑
}
```

在 `AddNeoAdmin` 中注册宿主程序集：

```csharp
options.SchedulerAssemblies = [Assembly.GetExecutingAssembly()];
```

管理界面：`/admin/task-scheduler`

---

## 15. 典型开发流程（完整示例）

以新增「产品管理」模块为例，从头到尾的步骤：

### 步骤 1：创建实体 `Entities/Product/Product.cs`

```csharp
[Table(Name = "biz_product")]
public class Product : EntityModified
{
    [Column(StringLength = 100)]
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Enabled { get; set; } = true;
}
```

### 步骤 2：创建页面 `Components/Product/Product.razor`

```razor
@page "/Product/Item"
@layout LayoutAdmin
@using NeoAdmin.Blazor.Components
@inject IFreeSql FreeSql

<CrudTable TItem="Product" Title="产品管理" OnQuery="HandleQuery">
    <Columns>
        <DataTableColumn TData="CrudGridRow<Product>" TValue="string"
            Property="@(r => r.Item.Name)" Header="名称" Sortable="false" />
        <DataTableColumn TData="CrudGridRow<Product>" TValue="decimal"
            Property="@(r => r.Item.Price)" Header="价格" Sortable="false" Width="100px" />
    </Columns>
    <EditTemplate Context="item">
        <div class="space-y-2">
            <Label>名称</Label>
            <Input Class="w-full" @bind-Value="item.Name" />
        </div>
        <div class="space-y-2">
            <Label>价格</Label>
            <NumericInput TValue="decimal" Class="w-full" Min="0" Step="0.01m" @bind-Value="item.Price" />
        </div>
        <div class="flex items-center gap-2">
            <Switch @bind-Checked="item.Enabled" Id="product-enabled" />
            <Label For="product-enabled">启用</Label>
        </div>
    </EditTemplate>
</CrudTable>

@code {
    private void HandleQuery(CrudQueryEventArgs<Product> e)
    {
        e.Select!.WhereIf(!string.IsNullOrWhiteSpace(e.SearchText),
            p => p.Name.Contains(e.SearchText!))
            .OrderByDescending(p => p.Id);
    }
}
```

### 步骤 3：注册表结构 `SeedData/DataSetup.cs`

```csharp
freeSql.CodeFirst.SyncStructure<Product>();
```

### 步骤 4：注册菜单 `SeedData/MenuSeedData.cs`

```csharp
BlazorMenuSeedData.Page("产品管理", "/Product/Item", 800, "package", isSystem: false),
```

### 步骤 5：运行验证

```bash
cd NeoAdmin && dotnet watch run
```

登录后侧边栏应出现新菜单，可进行增删改查。

### 步骤 6（可选）：同步模板

```bash
```

---

## 16. 常见坑与最佳实践

### 16.1 常见坑

| 问题 | 原因 | 解决 |
|------|------|------|
| 新页面 404 | 未 `AddAdditionalAssemblies` | 检查 `Program.cs` 是否加载框架程序集 |
| 菜单不显示 | 未写种子或角色未分配 | 检查 `MenuSeedData.Ensure` 和角色-菜单关联 |
| 按钮灰色不可用 | 无按钮权限 | 在角色管理中勾选 add/edit/remove |
| 业务表不存在 | 只开了 `AutoSyncStructure` | 宿主 `DataSetup` 中 `SyncStructure<T>` |
| NeoInputTable 弹窗不出现 | 未使用 LayoutAdmin | 确保页面 `@layout LayoutAdmin`（含 NeoPickerOverlayHost） |
| 样式错乱 | Tailwind 未编译 | 运行 `npm run watch:css` 或 `dotnet build` |
| 雪花 ID 冲突 | 多实例 WorkId 相同 | 各实例配置不同 `WorkId` |
| 上传文件 404 | 仅 MapStaticAssets | 需 `UseStaticFiles()` 提供 uploads 目录 |
| 审批按钮不显示 | 未注册审批菜单 | `PageWithAudit` + `AuditMenuSeedData.EnsureButtons` |
| 代理后页面无交互 / 频繁断开 | 反向代理未开 WebSocket | Nginx 等配置 `Upgrade` + `Connection "upgrade"`，见 §3.5 |

### 16.2 最佳实践

1. **实体基类选对**：需要审批用 `EntityAudited`，否则 `EntityModified` 即可。
2. **CrudTable 优先**：标准列表页不要重复造轮子，用 `OnQuery` 定制查询即可。
3. **权限与菜单 Path 一致**：`@page` 路由 = 菜单 Path = `MenuPath`。
4. **复杂表单用 Tabs 分组**：参考 `Article.razor` 的 `EditTemplate`。
5. **关联选择用 Neo 组件**：`NeoSelectEntity`（简单下拉）、`NeoInputTable`（弹窗搜索选择）。
6. **演示页当文档**：开发前先看 `/neo-demo/ui/*` 和 `/neo-demo/comp/*`。
7. **三目录同步**：`Entities` + `Components` + `SeedData` 同时提交。
8. **生产环境**：改默认密码、评估 IP 白名单、按需关闭 `MonitorCommand`。

### 16.3 命名空间与别名

博客实体在宿主命名空间 `NeoAdmin.Entities.Blog`，页面中常用别名：

```razor
@using BlogArticle = NeoAdminApp.Entities.Blog.Article
```

---

## 17. 参考资源

| 资源 | 链接 |
|------|------|
| NeoAdmin 源码 | <https://github.com/3bDjrvHs50kiZIJb5/NeoAdminProject> |
| NeoAdmin.Blazor NuGet | <https://www.nuget.org/packages/NeoAdmin.Blazor> |
| NeoAdmin.Templates NuGet | <https://www.nuget.org/packages/NeoAdmin.Templates> |
| NeoUI 文档 | <https://neoui.io> |
| NeoUI 在线演示 | <https://demos.neoui.io> |
| FreeSql | <https://freesql.net> |
| FreeScheduler | <https://github.com/2881099/FreeScheduler> |

---

## 附录 A：NeoAdmin.Blazor 组件清单（22 个）

| # | 组件 | 分类 |
|---|------|------|
| 1 | CrudTable\<TItem\> | CRUD |
| 2 | CrudSearchFilter | CRUD |
| 3 | CrudAuditPanel | CRUD / 审批 |
| 4 | CrudTableSingleSelectShell | CRUD 内部 |
| 5 | NeoSelectDict | 选择器 |
| 6 | NeoSelectEnum\<TEnum\> | 选择器 |
| 7 | NeoSelectEntity\<TItem,TKey\> | 选择器 |
| 8 | NeoSelectTable\<TItem,TKey\> | 选择器 |
| 9 | NeoInputTable\<TItem,TKey\> | 选择器 |
| 10 | NeoInputTags\<TItem\> | 选择器 |
| 11 | NeoAllocTable\<TItem,TChild,TKey\> | 分配 |
| 12 | NeoAllocSelectionBadges | 分配 |
| 13 | NeoParamText | 参数 |
| 14 | NeoFileUpload | 上传 |
| 15 | NeoPickerOverlayHost | 弹窗宿主 |
| 16 | SplitPane | 布局 |
| 17 | LayoutAdmin | 布局 |
| 18 | LayoutAdminSidebarMenu | 布局 |
| 19 | LayoutEmpty | 布局 |
| 20 | DefaultLogin | 认证 |
| 21 | DemoSection | 演示 |
| 22 | SearchableTabs | 演示 |

## 附录 B：NeoUI 组件统计

| 分类 | 数量（约） |
|------|-----------|
| 布局与结构 | 42 |
| 表单与输入 | 48 |
| 数据展示 | 35 |
| 导航 | 38 |
| 浮层与弹窗 | 48 |
| 反馈 | 4 |
| 图表 | 28 |
| 移动端 | 6 |
| 图标 | 1 |
| **合计** | **约 155** |

> NeoUI 组件的完整 Parameter 列表不在本仓库内，请以 [neoui.io](https://neoui.io) 官方文档为准；本仓库 DemoUI 演示页 + Snippets 提供了经过验证的用法示例。

---

*文档版本：基于 NeoAdmin 仓库 main 分支（NeoAdmin.Blazor 1.0.30）梳理。如有框架升级，请以源码与 NeoUI 官方文档为准。*
