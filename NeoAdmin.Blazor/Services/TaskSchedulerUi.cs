using FreeScheduler;

namespace NeoAdmin.Blazor.Services;

public static class TaskSchedulerUi
{
    public static string FormatInterval(TaskInfo task)
    {
        if ((int)task.Interval == 21)
        {
            return task.IntervalArgument;
        }

        return $"{task.Interval} {task.IntervalArgument}";
    }

    public static string FormatIntervalArgumentLabel(TaskInfo task)
    {
        return (int)task.Interval switch
        {
            1 => $"{task.IntervalArgument} 秒",
            11 => $"每天 {task.IntervalArgument}",
            12 => FormatWeeklyArgument(task.IntervalArgument),
            13 => FormatMonthlyArgument(task.IntervalArgument),
            21 => "Cron 表达式",
            _ => "参数"
        };
    }

    public static string FormatStatus(TaskInfo task) => (int)task.Status switch
    {
        1 => "已暂停",
        0 => "运行中",
        2 => "已结束",
        _ => task.Status.ToString()
    };

    public static void ApplyIntervalDefaults(TaskInfo task, TaskInterval interval)
    {
        task.Interval = interval;
        switch ((int)interval)
        {
            case 1:
                task.IntervalArgument = "60";
                break;
            case 11:
                task.IntervalArgument = "22:00:00";
                break;
            case 12:
                task.IntervalArgument = "5:22:00:00";
                break;
            case 13:
                task.IntervalArgument = "1:22:00:00";
                break;
            case 21:
                task.IntervalArgument = "* * * * * *";
                break;
        }
    }

    public static void ApplyTemplate(TaskInfo task, string template)
    {
        switch (template)
        {
            case "1":
                task.Topic = "[系统预留]Http请求";
                task.Body =
                    """
                    {
                        "method": "get",
                        "url": "https://freesql.net/guide/freescheduler.html",
                        "timeout": "30",
                        "header": {
                            "Content-Type": "application/json"
                        },
                        "body": "{}"
                    }
                    """;
                break;
            case "2":
                task.Topic = "[系统预留]清理任务数据";
                task.Body = "86400";
                break;
            default:
                task.Topic = string.Empty;
                task.Body = string.Empty;
                break;
        }
    }

    private static string FormatWeeklyArgument(string argument)
    {
        string[] parts = argument.Split(':');
        if (parts.Length != 4)
        {
            return argument;
        }

        string[] weekNames = ["日", "一", "二", "三", "四", "五", "六"];
        int dayIndex = int.TryParse(parts[0], out int index) ? index : 0;
        if (dayIndex < 0 || dayIndex >= weekNames.Length)
        {
            dayIndex = 0;
        }

        return $"每周{weekNames[dayIndex]} {string.Join(':', parts.Skip(1))}";
    }

    private static string FormatMonthlyArgument(string argument)
    {
        string[] parts = argument.Split(':');
        if (parts.Length != 4 || !int.TryParse(parts[0], out int day))
        {
            return argument;
        }

        string dayText = day > 0 ? $"第{day}天" : $"最后第{Math.Abs(day)}天";
        return $"每月{dayText} {string.Join(':', parts.Skip(1))}";
    }
}
