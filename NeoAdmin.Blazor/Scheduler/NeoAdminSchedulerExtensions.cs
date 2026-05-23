using FreeScheduler;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCrontab;
using NeoAdmin.Blazor.Data;

namespace NeoAdmin.Blazor.Scheduling;

public static class NeoAdminSchedulerExtensions
{
    public static IServiceCollection AddNeoAdminScheduler(this IServiceCollection services)
    {
        services.AddSingleton<global::FreeScheduler.Scheduler>(serviceProvider =>
        {
            IFreeSql freeSql = serviceProvider.GetRequiredService<IFreeSql>();
            IHostEnvironment environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            NeoAdminOptions? options = serviceProvider.GetService<IOptions<NeoAdminOptions>>()?.Value;

            FreeSqlSchedulerSetup.ConfigureEntities(freeSql);

            return new FreeSchedulerBuilder()
                .OnExecuting(task =>
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    options?.SchedulerExecuting?.Invoke(scope.ServiceProvider, task);
                })
                .UseTimeZone(TimeSpan.FromHours(8))
                .UseStorage(freeSql, !environment.IsDevelopment(), null)
                .UseCustomInterval(task =>
                {
                    if ((int)task.Interval != 21)
                    {
                        return null;
                    }

                    DateTime utcNow = DateTime.UtcNow;
                    DateTime nextOccurrence = CrontabSchedule.Parse(
                            task.IntervalArgument,
                            new CrontabSchedule.ParseOptions { IncludingSeconds = true })
                        .GetNextOccurrence(utcNow);

                    return nextOccurrence < utcNow
                        ? TimeSpan.FromSeconds(5)
                        : nextOccurrence.Subtract(utcNow);
                })
                .Build();
        });

        return services;
    }
}
