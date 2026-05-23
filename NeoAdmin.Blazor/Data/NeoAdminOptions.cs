using FreeScheduler;
using FreeSql;

namespace NeoAdmin.Blazor.Data;

public sealed class NeoAdminOptions
{
    public DataType DataType { get; set; } = DataType.Sqlite;

    public string ConnectionString { get; set; } = "Data Source=neoadmin.db";

    public bool AutoSyncStructure { get; set; } = true;

    public bool MonitorCommand { get; set; }

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
    /// 定时任务执行时的回调，可在宿主项目中注册具体业务逻辑。
    /// </summary>
    public Action<IServiceProvider, TaskInfo>? SchedulerExecuting { get; set; }
}
