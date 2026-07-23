using FreeScheduler;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using NeoAdmin.Blazor.Data;

namespace NeoAdmin.Blazor.Core.Scheduling;

public static class NeoAdminSchedulerExtensions
{
    public static IServiceCollection AddNeoAdminScheduler(this IServiceCollection services)
    {
        services.AddSingleton<global::FreeScheduler.Scheduler>(serviceProvider =>
        {
            IFreeSql freeSql = serviceProvider.GetRequiredService<IFreeSql>();
            IHostEnvironment environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(NeoAdminSchedulerExtensions));
            NeoAdminOptions? options = serviceProvider.GetService<IOptions<NeoAdminOptions>>()?.Value;
            var attributeTriggers = new Dictionary<string, Action<IServiceProvider, TaskInfo>>();
            bool schedulerAutoLoad = options?.SchedulerAutoLoad ?? !environment.IsDevelopment();

            FreeSqlSchedulerSetup.ConfigureEntities(freeSql);

            if (options?.SchedulerAssemblies is { Length: > 0 } assemblies)
            {
                SchedulerAttributeRegistration.Register(freeSql, assemblies, attributeTriggers);
            }

            return new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    if (attributeTriggers.TryGetValue(task.Topic, out Action<IServiceProvider, TaskInfo>? action))
                    {
                        action(scope.ServiceProvider, task);
                    }
                    else
                    {
                        options?.SchedulerExecuting?.Invoke(scope.ServiceProvider, task);
                    }
                })
                .UseTimeZone(TimeSpan.FromHours(8))
                .UseStorage(freeSql, schedulerAutoLoad, null)
                .UseCustomInterval(task => GetCronNextDelay(task, logger))
                .Build();
        });

        return services;
    }

    private static TimeSpan? GetCronNextDelay(TaskInfo task, ILogger logger)
    {
        if ((int)task.Interval != 21)
        {
            return null;
        }

        string cron = task.IntervalArgument?.Trim() ?? string.Empty;
        string[] parts = cron.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is not (5 or 6))
        {
            logger.LogWarning(
                "定时任务 Cron 表达式无效：TaskId={TaskId}，Topic={Topic}，Cron={Cron}",
                task.Id,
                task.Topic,
                cron);
            return null;
        }

        try
        {
            var parseOptions = new CrontabSchedule.ParseOptions { IncludingSeconds = parts.Length == 6 };
            CrontabSchedule? schedule = CrontabSchedule.TryParse(cron, parseOptions);
            if (schedule is null)
            {
                logger.LogWarning(
                    "定时任务 Cron 表达式无效：TaskId={TaskId}，Topic={Topic}，Cron={Cron}",
                    task.Id,
                    task.Topic,
                    cron);
                return null;
            }

            DateTime now = DateTime.UtcNow.AddHours(8);
            DateTime nextOccurrence = schedule.GetNextOccurrence(now);
            return nextOccurrence <= now
                ? TimeSpan.FromSeconds(5)
                : nextOccurrence.Subtract(now);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "计算定时任务下次运行时间失败：TaskId={TaskId}，Topic={Topic}，Cron={Cron}",
                task.Id,
                task.Topic,
                cron);
            return null;
        }
    }
}
