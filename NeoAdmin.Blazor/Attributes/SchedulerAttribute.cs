using FreeScheduler;

namespace NeoAdmin.Blazor.Attributes;

/// <summary>
/// 标记静态方法为 FreeScheduler 定时任务（参照 NoAdmin SchedulerAttribute）。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SchedulerAttribute : Attribute
{
    public const string TopicPrefix = "[SchedulerAttribute]";

    public string Name { get; }

    public TaskInterval Interval { get; set; }

    public string? Argument { get; set; }

    public int Round { get; set; } = -1;

    public global::FreeScheduler.TaskStatus Status { get; set; }

    /// <summary>
    /// 懒注册：仅绑定执行方法，调度规则在后台「定时任务」里配置（Topic 填 Name，不要带前缀）。
    /// </summary>
    public SchedulerAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Cron 表达式（6 段，含秒），自动写入任务表。
    /// </summary>
    public SchedulerAttribute(string name, string cron)
    {
        Name = name;
        Interval = (TaskInterval)21;
        Argument = cron;
    }

    public string StorageTopic => TopicPrefix + Name;

    public bool IsLazyTask => string.IsNullOrWhiteSpace(Argument);
}
