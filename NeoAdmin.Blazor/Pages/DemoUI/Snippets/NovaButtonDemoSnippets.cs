namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class NovaButtonDemoSnippets
{
    public const string Usage = """
        [NovaButton("demo_allow")]
        private async Task BeginAllowAction()
        {
            allowStatus = "允许动作已进入方法体";
            await Task.Yield();
        }

        [NovaButton("demo_deny")]
        private async Task BeginDenyAction()
        {
            allowStatus = "当前账号拥有 demo_deny 权限";
            await Task.Yield();
        }
        """;

    public const string Behavior = """
        方法执行前会根据 @page 路由解析菜单路径
        再调用 MenuPermissionService 校验页面与按钮权限
        无权限时会弹出 Toast，并阻止进入方法体
        管理员角色默认拥有全部按钮权限
        """;
}
