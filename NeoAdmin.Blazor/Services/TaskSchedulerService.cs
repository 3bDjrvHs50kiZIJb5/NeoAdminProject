using FreeScheduler;
using FreeTaskStatus = FreeScheduler.TaskStatus;
using ResultGetClusterLogs = FreeScheduler.Datafeed.ResultGetClusterLogs;
using ResultGetLogs = FreeScheduler.Datafeed.ResultGetLogs;
using ResultGetPage = FreeScheduler.Datafeed.ResultGetPage;

namespace NeoAdmin.Blazor.Services;

public sealed class TaskSchedulerService
{
    private readonly global::FreeScheduler.Scheduler _scheduler;

    public TaskSchedulerService(global::FreeScheduler.Scheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public ResultGetPage GetPage(
        string? clusterId,
        string? searchText,
        FreeTaskStatus? status,
        int pageSize,
        int pageNumber) =>
        Datafeed.GetPage(
            _scheduler,
            clusterId,
            searchText,
            status,
            null,
            null,
            pageSize,
            pageNumber);

    public ResultGetLogs GetLogs(string taskId, int pageSize, int pageNumber) =>
        Datafeed.GetLogs(_scheduler, taskId, pageSize, pageNumber);

    public ResultGetClusterLogs GetClusterLogs(int pageSize, int pageNumber) =>
        Datafeed.GetClusterLogs(_scheduler, pageSize, pageNumber);

    public void AddTask(
        string topic,
        string body,
        int round,
        TaskInterval interval,
        string intervalArgument) =>
        Datafeed.AddTask(_scheduler, topic, body, round, interval, intervalArgument);

    public void RemoveTask(string taskId) => _scheduler.RemoveTask(taskId);

    public void ResumeTask(string taskId) => _scheduler.ResumeTask(taskId);

    public void PauseTask(string taskId) => _scheduler.PauseTask(taskId);

    public void RunNowTask(string taskId) => _scheduler.RunNowTask(taskId);
}
