# NeoAdmin

基于 **ASP.NET Core Blazor Server** 的现代化后台管理框架，UI 使用 [NeoUI.Blazor](https://neoui.io)，数据访问使用 **FreeSql**，开箱即用 SQLite，适合作为业务系统的管理端骨架或二次开发起点。

## 特性

- **Blazor 交互式服务端**：`InteractiveServer` 渲染，组件化开发体验
- **NeoUI 组件库**：侧边栏布局（参照 NeoUI Blocks dashboard-02）、主题切换、表单/表格/弹层等完整 UI 能力
- **通用 CRUD**：`CrudTable` 组件 + FreeSql，支持搜索、筛选、分页、批量删除、弹窗编辑
- **RBAC 基础能力**：用户、角色、菜单、组织（树形）、角色-菜单/用户关联
- **系统配置**：数据字典、系统参数、站点设置（标题/Logo 等）
- **安全与运维**：登录鉴权（DataProtection Token）、登录日志、IP 白名单中间件、文件上传管理
- **定时任务**：集成 [FreeScheduler](https://github.com/2881099/FreeScheduler)，支持 Cron 表达式
- **雪花 ID**：Yitter.IdGenerator，多实例部署可配置 `WorkId`
- **种子数据**：首次启动自动建表并初始化管理员、菜单、字典、参数等
- **NeoDemo**：内置 NeoUI 组件演示页，便于对照文档与选型

## 技术栈

| 类别 | 技术 |
|------|------|
| 运行时 | .NET 10 |
| 前端框架 | Blazor Server（Razor Components） |
| UI | NeoUI.Blazor 4.x |
| ORM | FreeSql 3.x + Sqlite（可换其他 FreeSql Provider） |
| 调度 | FreeScheduler + NCrontab |
| ID | Yitter.IdGenerator（雪花） |

## 项目结构

```
NeoAdmin/
├── NeoAdmin/                 # 宿主 Web 项目（启动入口）
│   ├── Program.cs            # 注册 NeoUI、NeoAdmin、Blazor 路由
│   └── appsettings.json      # NeoAdmin 配置节
├── NeoAdmin.Blazor/          # 管理端核心类库（可引用或打包）
│   ├── Components/           # 布局、CrudTable、SplitPane、字典/参数组件等
│   ├── Pages/                # 业务页面与 NeoDemo
│   ├── Data/Entities/        # 实体定义
│   ├── SeedData/             # 菜单、用户、字典等种子数据
│   ├── Services/             # 文件、组织、角色、定时任务等业务服务
│   ├── Auth/                 # 登录鉴权
│   └── Middlewares/          # IP 白名单等
└── old/                      # 历史版本（NovaAdmin 等），仅供参考
```

## 功能模块

| 模块 | 路由 | 说明 |
|------|------|------|
| 控制台 | `/admin` | 管理首页 |
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
| 登录 | `/login` | 登录页 |
| NeoDemo | `/neo-demo/*` | NeoUI 组件示例 |

菜单种子中还预留了「博客管理」等占位路由，页面需自行实现。

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### 运行

```bash
cd NeoAdmin
dotnet watch run
```

默认地址：<http://localhost:5038>（见 `Properties/launchSettings.json`）。

### 默认账号

首次启动会在 SQLite 中创建 `neoadmin.db` 并写入种子数据：

| 项 | 值 |
|----|-----|
| 用户名 | `admin` |
| 密码 | `admin` |

生产环境请务必修改 `appsettings.json` 中的 `SeedAdminPassword`，并关闭弱口令。

## 配置说明

`NeoAdmin/appsettings.json` 中的 `NeoAdmin` 节点：

```json
{
  "NeoAdmin": {
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
| `EnableIpWhitelist` | 是否启用 IP 白名单（代码默认 `true`，可在 `NeoAdminOptions` 中调整） |
| `FileUpload` | 上传目录、按日期分目录、大小与扩展名限制 |

## 集成到其他项目

在宿主 `Program.cs` 中：

```csharp
builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddNeoAdmin(builder.Configuration);

// ...

app.UseNeoAdmin();
app.MapRazorComponents<YourApp>()
    .AddAdditionalAssemblies(typeof(NeoAdmin.Blazor.Components.LayoutAdmin).Assembly)
    .AddInteractiveServerRenderMode();
```

`NeoAdmin.Blazor` 可作为类库引用，也可按 NuGet 包方式发布（`PackageId: NeoAdmin.Blazor`）。

## 核心组件

- **`CrudTable<TItem>`**：基于 FreeSql 的增删改查表格，支持列定义、筛选器、搜索、弹窗编辑模板
- **`SplitPane`**：左右分栏布局（如字典管理）
- **`NeoSelectDict` / `NeoParamText`**：字典下拉、参数文本展示
- **`LayoutAdmin`**：带侧边栏、用户菜单的后台主布局

## 相关链接

- NeoUI 文档与 Blocks：<https://neoui.io>
- FreeSql：<https://freesql.net>
- FreeScheduler：<https://github.com/2881099/FreeScheduler>

## 许可证

请根据仓库实际许可证补充；若尚未声明，使用前请与维护者确认。
