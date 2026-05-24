using Microsoft.Extensions.Logging;

namespace NeoAdmin.Blazor.Pages.Demo;

/// <summary>
/// NeoDemo 演示页统一日志辅助。
/// </summary>
public static class DemoPageLog
{
    public static void Enter(ILogger logger, string pageName) =>
        logger.LogInformation("NeoDemo 进入页面：{PageName}", pageName);

    public static void Action(ILogger logger, string pageName, string action) =>
        logger.LogInformation("NeoDemo 操作：{PageName} — {Action}", pageName, action);

    public static void Action<T0>(ILogger logger, string pageName, string action, T0 arg0) =>
        logger.LogInformation("NeoDemo 操作：{PageName} — {Action} {Arg0}", pageName, action, arg0);

    public static void Action<T0, T1>(ILogger logger, string pageName, string action, T0 arg0, T1 arg1) =>
        logger.LogInformation("NeoDemo 操作：{PageName} — {Action} {Arg0} {Arg1}", pageName, action, arg0, arg1);
}
