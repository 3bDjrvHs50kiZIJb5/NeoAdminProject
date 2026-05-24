namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class AntiConcurrencyDemoSnippets
{
    public const string Usage = """
        [AntiConcurrency(500)]
        private async Task ExecuteSlowActionAsync()
        {
            int current = Interlocked.Increment(ref executeCount);
            await Task.Delay(800);
            lastExecutedAt = DateTime.Now.ToString("HH:mm:ss.fff");
            lastExecuteCount = current;
        }
        """;

    public const string Behavior = """
        第一次点击会进入方法体
        方法未结束前的重复点击会被直接拦截
        方法结束后延迟 500ms 才允许再次触发
        适合保存、提交、删除等按钮防连点
        """;
}
