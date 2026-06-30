using System.Reflection;
using FreeScheduler;
using FreeSql;

namespace NeoAdmin.Blazor.Data;

public sealed class NeoAdminOptions
{
    public DataType DataType { get; set; } = DataType.Sqlite;

    public string ConnectionString { get; set; } = "Data Source=neoadmin.db";

    public bool AutoSyncStructure { get; set; } = true;

    /// <summary>
    /// 启动时是否同步宿主种子数据：业务/Demo 菜单增量插入、演示组织/用户、博客演示数据等。
    /// 开发环境建议 <c>true</c>；生产环境建议 <c>false</c>，避免每次启动写库。
    /// 升级业务菜单或演示数据后，可临时设为 <c>true</c> 并重启以补齐缺失项。
    /// 管理员账号、角色、字典等基础数据不受此开关影响；框架 /admin 系统菜单仅在库中尚无时会初始化一次。
    /// </summary>
    public bool EnableSeedData { get; set; } = true;

    /// <summary>
    /// 登录后默认跳转路径，以及侧栏 Logo 点击目标（如业务工作台）。
    /// </summary>
    public string HomePath { get; set; } = "/Admin";

    public bool MonitorCommand { get; set; } = true;

    public string SeedAdminUserName { get; set; } = "admin";

    public string SeedAdminPassword { get; set; } = "admin";

    /// <summary>
    /// 雪花算法机器号（WorkerId），多实例部署时每台应不同，范围 0–63。
    /// </summary>
    public ushort WorkId { get; set; } = 1;

    /// <summary>
    /// 是否启用 IP 白名单中间件。无任何启用记录时不拦截。
    /// </summary>
    public bool EnableIpWhitelist { get; set; } = true;

    /// <summary>
    /// 文件上传配置。
    /// </summary>
    public FileUploadOptions FileUpload { get; set; } = new();

    /// <summary>
    /// 扫描带 <see cref="Attributes.SchedulerAttribute"/> 的程序集。
    /// </summary>
    public Assembly[] SchedulerAssemblies { get; set; } = [];

    /// <summary>
    /// <c>AddNeoAdminApi</c> 注册的 API 程序集，用于反射生成接口权限菜单。
    /// </summary>
    public Assembly[] ApiAssemblies { get; set; } = [];

    /// <summary>
    /// 定时任务执行时的回调（未匹配 SchedulerAttribute 时触发）。
    /// </summary>
    public Action<IServiceProvider, TaskInfo>? SchedulerExecuting { get; set; }

    /// <summary>
    /// 是否启用 Swagger UI。在 appsettings.json 的 NeoAdmin 节点配置，例如 "IsSwagger": true。
    /// 未配置时：开发环境默认开启，其它环境默认关闭。
    /// </summary>
    public bool? IsSwagger { get; set; }

    /// <summary>
    /// Swagger 文档中隐藏的路径片段（不区分大小写）。
    /// </summary>
    public string[] SwaggerHides { get; set; } = [];

    /// <summary>
    /// Serilog 文件日志目录（相对路径基于应用根目录）。
    /// </summary>
    public string LogDirectory { get; set; } = "Logs";

    /// <summary>
    /// Serilog 滚动日志文件名前缀，例如 admin- 对应 admin-20260524.log。
    /// </summary>
    public string LogFilePrefix { get; set; } = "admin-";
}
