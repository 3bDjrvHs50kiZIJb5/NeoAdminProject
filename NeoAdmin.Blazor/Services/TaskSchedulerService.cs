using FreeScheduler;
using Microsoft.Extensions.Logging;
using FreeTaskStatus = FreeScheduler.TaskStatus;
using ResultGetClusterLogs = FreeScheduler.Datafeed.ResultGetClusterLogs;
using ResultGetLogs = FreeScheduler.Datafeed.ResultGetLogs;
using ResultGetPage = FreeScheduler.Datafeed.ResultGetPage;

namespace NeoAdmin.Blazor.Services;

public sealed class TaskSchedulerService
{
    private readonly global::FreeScheduler.Scheduler _scheduler;
    private readonly ILogger<TaskSchedulerService> _logger;

    public TaskSchedulerService(global::FreeScheduler.Scheduler scheduler, ILogger<TaskSchedulerService> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public ResultGetPage GetPage(
        string? clusterId,
        string? searchText,
        FreeTaskStatus? status,
        int pageSize,
        int pageNumber)
    {
        ResultGetPage result = Datafeed.GetPage(
            _scheduler,
            clusterId,
            searchText,
            status,
            null,
            null,
            pageSize,
            pageNumber);

        _logger.LogInformation(
            "查询定时任务：第 {PageNumber} 页，每页 {PageSize} 条，共 {Total} 条，搜索={SearchText}",
            pageNumber,
            pageSize,
            result.Total,
            searchText ?? string.Empty);

        return result;
    }

    public ResultGetLogs GetLogs(string taskId, int pageSize, int pageNumber) =>
        Datafeed.GetLogs(_scheduler, taskId, pageSize, pageNumber);

    public ResultGetClusterLogs GetClusterLogs(int pageSize, int pageNumber) =>
        Datafeed.GetClusterLogs(_scheduler, pageSize, pageNumber);

    public void AddTask(
        string topic,
        string body,
        int round,
        TaskInterval interval,
        string intervalArgument)
    {
        Datafeed.AddTask(_scheduler, topic, body, round, interval, intervalArgument);
        _logger.LogInformation(
            "添加定时任务：Topic={Topic}，Interval={Interval}，Round={Round}",
            topic,
            interval,
            round);
    }

    public void RemoveTask(string taskId)
    {
        _scheduler.RemoveTask(taskId);
        _logger.LogInformation("删除定时任务：TaskId={TaskId}", taskId);
    }

    public void ResumeTask(string taskId)
    {
        _scheduler.ResumeTask(taskId);
        _logger.LogInformation("恢复定时任务：TaskId={TaskId}", taskId);
    }

    public void PauseTask(string taskId)
    {
        _scheduler.PauseTask(taskId);
        _logger.LogInformation("暂停定时任务：TaskId={TaskId}", taskId);
    }

    public void RunNowTask(string taskId)
    {
        _scheduler.RunNowTask(taskId);
        _logger.LogInformation("立即执行定时任务：TaskId={TaskId}", taskId);
    }
}
