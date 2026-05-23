# NovaAdmin

NovaAdmin 是一个基于 `.NET 8` 的后台管理系统项目，采用 **Blazor Server** 构建前端交互，结合 **FreeSql + SQLite** 作为数据访问方案，并集成了登录、菜单、权限、博客示例、文件上传等常见后台能力。

项目代码分成三个主要部分：

- `NovaAdmin`：主站点，负责页面、路由、接口和启动配置。
- `NoAdmin.Blazor`：可复用的组件与后台管理能力封装。
- `NovaAdmin.Tests`：测试项目，用于验证关键逻辑。

## 项目特点

- 基于 `.NET 8` 和 Blazor Server
- 默认使用 SQLite，开箱即用
- 统一封装了后台管理常用组件和页面能力
- 内置博客相关示例页面，方便演示表单、上传、弹窗、表格等功能
- 支持 Swagger 和 API 访问
- 提供 Docker 部署方式

## 如何使用

### 1. 准备环境

请先安装以下环境：

- `.NET 8 SDK`
- SQLite 运行环境通常无需单独安装，项目会直接使用本地数据库文件

### 2. 运行项目

在仓库根目录下执行：

```bash
dotnet restore
dotnet run --project NovaAdmin/NovaAdmin.csproj
```

默认开发地址是：`http://localhost:5038`

如果你使用的是开发环境，启动后会自动打开浏览器。

### 3. 本地数据库

项目默认使用 SQLite 数据库文件：

```text
novaadmin.db
```

数据库连接配置在 `NovaAdmin/Program.cs` 中：

```csharp
.UseConnectionString(DataType.Sqlite, "Data Source=novaadmin.db")
```

首次启动时会自动完成结构同步，并初始化部分示例数据。

### 4. 运行测试

```bash
dotnet test
```

如果只想执行测试项目：

```bash
dotnet test NovaAdmin.Tests/NovaAdmin.Tests.csproj
```

### 5. Docker 方式运行

项目也提供了 `docker-compose.yaml`，可以用容器启动：

```bash
docker compose -f NovaAdmin/docker-compose.yaml up -d --build
```

默认映射端口为 `6038`，如果需要修改宿主机端口，可以设置 `HOST_PORT` 环境变量。

## 目录结构

下面是一个简化后的目录树，重点展示主要模块：

```text
NovaAdmin/
├─ NovaAdmin/                    # 主站点
│  ├─ Api/                       # 接口与登录相关服务
│  ├─ Components/                # Blazor 页面、布局、示例组件
│  ├─ Configs/                   # 配置文件
│  ├─ Entities/                  # 业务实体
│  ├─ SeedData/                  # 初始化种子数据
│  ├─ Properties/                # 启动配置
│  ├─ wwwroot/                   # 静态资源
│  ├─ Program.cs                 # 程序入口
│  ├─ NovaAdmin.csproj           # 主项目文件
│  └─ README.md                  # 本说明文档
├─ NoAdmin.Blazor/             # 可复用组件与后台管理能力封装
│  ├─ AdminBlazor/               # 管理后台页面和基础实现
│  ├─ AdminOmni/                 # API / Swagger 相关扩展
│  ├─ BootstrapBlazor/           # 组件封装
│  ├─ wwwroot/                   # 前端脚本与样式
│  └─ NoAdmin.Blazor.csproj    # 组件库项目文件
├─ NovaAdmin.Tests/              # 测试项目
│  ├─ ApiFlowTests.cs
│  ├─ FileCacheAttributeTests.cs
│  └─ NovaAdmin.Tests.csproj
├─ NovaAdmin.Templates/          # 模板包内容
├─ docs/                         # 文档与教程
├─ AGENTS.md                     # 仓库约定
└─ README.md                     # 仓库级说明
```

## 启动后的常用入口

- 主页：`/`
- 后台页面：`/admin`
- Swagger：由项目中的 API 配置提供，开发环境可直接查看

## 说明

- 项目默认会在启动时检查管理员用户，并初始化部分博客示例数据。
- 如果你想调整数据库、端口、日志或静态资源路径，可以优先查看 `NovaAdmin/Program.cs` 和 `NovaAdmin/appsettings.json`。
- 如果你正在做组件开发，建议同时查看 `NoAdmin.Blazor` 里的 `BootstrapBlazor/Components` 和 `AdminBlazor/Pages`。
