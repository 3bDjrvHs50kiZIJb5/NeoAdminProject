using System.Reflection;
using FreeScheduler;
using FreeSql;
using NeoAdmin.Blazor.Attributes;
using Yitter.IdGenerator;

namespace NeoAdmin.Blazor.Scheduling;

internal static class SchedulerAttributeRegistration
{
    internal sealed record ScheduledMethod(
        MethodInfo Method,
        SchedulerAttribute Attribute,
        bool IsLazyTask,
        TaskInfo TaskInfo);

    public static void Register(
        IFreeSql freeSql,
        Assembly[] assemblies,
        Dictionary<string, Action<IServiceProvider, TaskInfo>> triggers)
    {
        List<ScheduledMethod> methods = Discover(assemblies);
        if (methods.Count == 0)
        {
            return;
        }

        List<TaskInfo> stored = freeSql.Select<TaskInfo>()
            .Where(a => a.Topic.StartsWith(SchedulerAttribute.TopicPrefix))
            .ToList();

        List<TaskInfo> toStore = methods
            .Where(a => !a.IsLazyTask)
            .Select(a => a.TaskInfo)
            .ToList();

        SyncTasksToStorage(freeSql, toStore, stored);

        foreach (ScheduledMethod item in methods)
        {
            string triggerTopic = item.IsLazyTask ? item.Attribute.Name : item.Attribute.StorageTopic;
            triggers[triggerTopic] = (serviceProvider, task) => Invoke(item.Method, serviceProvider, task);
        }
    }

    private static List<ScheduledMethod> Discover(Assembly[] assemblies)
    {
        var result = new List<ScheduledMethod>();
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    SchedulerAttribute? attribute = method.GetCustomAttribute<SchedulerAttribute>();
                    if (attribute is null || string.IsNullOrWhiteSpace(attribute.Name))
                    {
                        continue;
                    }

                    result.Add(new ScheduledMethod(
                        method,
                        attribute,
                        attribute.IsLazyTask,
                        CreateTaskInfo(attribute)));
                }
            }
        }

        return result;
    }

    private static TaskInfo CreateTaskInfo(SchedulerAttribute attribute) => new()
    {
        Topic = attribute.StorageTopic,
        Interval = attribute.Interval,
        IntervalArgument = attribute.Argument ?? string.Empty,
        Round = attribute.Round,
        Status = attribute.Status,
        Body = attribute.Name,
        CreateTime = DateTime.Now,
        CurrentRound = 0,
        ErrorTimes = 0,
        LastRunTime = new DateTime(1970, 1, 1)
    };

    private static void SyncTasksToStorage(IFreeSql freeSql, List<TaskInfo> incoming, List<TaskInfo> existing)
    {
        foreach (TaskInfo task in incoming)
        {
            TaskInfo? found = existing.Find(a => a.Topic == task.Topic);
            if (found is not null)
            {
                task.Id = found.Id;
                task.Body = found.Body;
                task.CreateTime = found.CreateTime;
                task.CurrentRound = found.CurrentRound;
                task.ErrorTimes = found.ErrorTimes;
                task.LastRunTime = found.LastRunTime;
                freeSql.Update<TaskInfo>().SetSource(task).ExecuteAffrows();
            }
            else
            {
                task.Id = $"{DateTime.Now:yyyyMMdd}.{YitIdHelper.NextId()}";
                freeSql.Insert(task).ExecuteAffrows();
            }
        }
    }

    private static void Invoke(MethodInfo method, IServiceProvider serviceProvider, TaskInfo task)
    {
        ParameterInfo[] parameters = method.GetParameters();
        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            Type parameterType = parameters[i].ParameterType;
            if (parameterType == typeof(IServiceProvider))
            {
                args[i] = serviceProvider;
            }
            else if (parameterType == typeof(TaskInfo))
            {
                args[i] = task;
            }
            else
            {
                throw new InvalidOperationException(
                    $"定时任务 {method.DeclaringType?.Name}.{method.Name} 仅支持 IServiceProvider、TaskInfo 参数");
            }
        }

        object? result = method.Invoke(null, args!);
        if (result is Task asyncTask)
        {
            asyncTask.GetAwaiter().GetResult();
        }
    }
}
