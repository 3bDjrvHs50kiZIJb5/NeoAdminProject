namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class FeedbackDemoSnippets
{
    public const string Alert = """
        <Alert Variant="AlertVariant.Default" AccentBorder="true">
            <Icon><LucideIcon Name="info" Class="h-4 w-4" /></Icon>
            <ChildContent>
                <AlertTitle>提示</AlertTitle>
                <AlertDescription>这是一条默认样式的提示信息。</AlertDescription>
            </ChildContent>
        </Alert>

        <Alert Variant="AlertVariant.Success" AccentBorder="true">
            <Icon><LucideIcon Name="circle-check" Class="h-4 w-4" /></Icon>
            <ChildContent>
                <AlertTitle>成功</AlertTitle>
                <AlertDescription>操作已成功完成。</AlertDescription>
            </ChildContent>
        </Alert>

        <Alert Variant="AlertVariant.Destructive" AccentBorder="true">
            <Icon><LucideIcon Name="circle-x" Class="h-4 w-4" /></Icon>
            <ChildContent>
                <AlertTitle>错误</AlertTitle>
                <AlertDescription>提交失败，请检查后重试。</AlertDescription>
            </ChildContent>
        </Alert>
        """;

    public const string Progress = """
        <Progress Value="33" Max="100" Class="w-full max-w-md" />
        <p class="text-sm text-muted-foreground">33%</p>

        <Progress Value="@progressValue" Max="100" Class="w-full max-w-md" />
        <Button OnClick="SimulateProgressAsync">模拟上传</Button>
        """;

    public const string Spinner = """
        <Spinner Size="SpinnerSize.Small" />
        <Spinner Size="SpinnerSize.Medium" />
        <Spinner Size="SpinnerSize.Large" />

        <Button Disabled="true">
            <Spinner Size="SpinnerSize.Small" Class="mr-2" />
            加载中…
        </Button>
        """;

    public const string Toast = """
        @inject IToastService ToastService

        <Button OnClick="ShowSuccessToast">成功</Button>
        <Button OnClick="ShowErrorToast">错误</Button>
        <Button OnClick="ShowWarningToast">警告</Button>
        <Button OnClick="ShowInfoToast">信息</Button>

        ToastService.Show(new ToastOptions
        {
            Title = "已删除",
            Description = "记录已从列表中移除。",
            Variant = ToastVariant.Default,
            ActionLabel = "撤销",
            OnAction = () => ToastService.Info("撤销", "操作已恢复。")
        });
        """;
}
