using FreeScheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Attributes;

namespace NeoAdmin.Jobs;

/// <summary>
/// 本地开发环境验证定时任务。
/// </summary>
public static class DevVerifyJobs
{
    /// <summary>
    /// 每分钟执行一次，用于验证 SchedulerAutoLoad 是否生效。
    /// </summary>
    [Scheduler("dev.verify-every-minute")]
    public static Task VerifyEveryMinute(IServiceProvider serviceProvider, TaskInfo task)
    {
        ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DevVerifyJobs));
        DateTime now = DateTime.Now;

        logger.LogInformation("本地定时任务验证执行成功：TaskId={TaskId}，Time={Time}", task.Id, now);
        task.Remark($"本地验证执行成功：{now:yyyy-MM-dd HH:mm:ss}");

        return Task.CompletedTask;
    }
}
