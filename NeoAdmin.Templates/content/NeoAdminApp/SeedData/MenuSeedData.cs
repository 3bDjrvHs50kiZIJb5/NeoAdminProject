using FreeSql;
using NeoAdmin.Blazor.Entities;
using BlazorMenuSeedData = NeoAdmin.Blazor.SeedData.MenuSeedData;

namespace NeoAdminApp.SeedData;

/// <summary>
/// 业务菜单种子数据。
/// </summary>
public static class MenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        BlazorMenuSeedData.EnsureMenus(freeSql, CreateMenus());
        EnsureCustomPageButtons(freeSql);
    }

    private static List<SysMenu> CreateMenus() =>
    [
        BlazorMenuSeedData.Menu("NeoDemo", "flask-conical", string.Empty, 40, SysMenuSidebarStyle.展开,
        [
            BlazorMenuSeedData.Menu("业务组件", "blocks", string.Empty, 400, SysMenuSidebarStyle.收起,
            [
                BlazorMenuSeedData.Page("实体选择", "/neo-demo/comp/select-components", 401, "list-checks"),
                BlazorMenuSeedData.Page("字典和参数", "/neo-demo/comp/dict-param", 402, "sliders-horizontal"),
                BlazorMenuSeedData.Page("权限说明", "/neo-demo/comp/permission-guide", 407, "book-open"),
                BlazorMenuSeedData.Page("上传组件", "/neo-demo/comp/file-upload", 408, "upload"),
                BlazorMenuSeedData.Page("API 上传", "/neo-demo/comp/file-upload-api", 409, "cloud-upload"),
                BlazorMenuSeedData.Page("语音输入", "/neo-demo/comp/voice-input", 410, "mic"),
                BlazorMenuSeedData.Page("更新日志", "/neo-demo/comp/update-log", 411, "scroll-text"),
                BlazorMenuSeedData.Menu("按钮权限", "shield", "/neo-demo/comp/nova-button", 403, children:
                [
                    BlazorMenuSeedData.Button("允许演示", "check", "demo_allow", 301),
                    BlazorMenuSeedData.Button("拦截演示", "ban", "demo_deny", 302)
                ], type: SysMenuType.菜单),
                BlazorMenuSeedData.Page("文件缓存", "/neo-demo/comp/file-cache", 406, "file-archive"),
                BlazorMenuSeedData.Menu("防并发", "timer", "/neo-demo/ui/anti-concurrency", 404, type: SysMenuType.菜单),
                BlazorMenuSeedData.Menu("事务", "database", "/neo-demo/comp/transactional", 405, type: SysMenuType.菜单)
            ]),
            BlazorMenuSeedData.Menu("UI 组件", "layout-grid", string.Empty, 500, SysMenuSidebarStyle.收起,
            [
                BlazorMenuSeedData.Page("表单输入", "/neo-demo/ui/form-inputs", 501, "text-cursor-input"),
                BlazorMenuSeedData.Page("选择与开关", "/neo-demo/ui/form-controls", 502, "toggle-right"),
                BlazorMenuSeedData.Page("增强输入", "/neo-demo/ui/advanced-inputs", 503, "wand-sparkles"),
                BlazorMenuSeedData.Page("日期与时间", "/neo-demo/ui/advanced-datetime", 504, "calendar"),
                BlazorMenuSeedData.Page("编辑器与复杂", "/neo-demo/ui/advanced-complex", 505, "file-code"),
                BlazorMenuSeedData.Page("内容与装饰", "/neo-demo/ui/display-basics", 510, "layout-template"),
                BlazorMenuSeedData.Page("状态与列表", "/neo-demo/ui/display-states", 511, "list"),
                BlazorMenuSeedData.Page("数据展示", "/neo-demo/ui/data-display", 512, "table-2"),
                BlazorMenuSeedData.Page("图表", "/neo-demo/ui/chart", 513, "chart-column"),
                BlazorMenuSeedData.Page("反馈组件", "/neo-demo/ui/feedback", 520, "bell-ring"),
                BlazorMenuSeedData.Page("动画演示", "/neo-demo/comp/animation", 521, "sparkles"),
                BlazorMenuSeedData.Page("移动端演示", "/neo-demo/ui/mobile", 522, "smartphone"),
                BlazorMenuSeedData.Page("导航组件", "/neo-demo/ui/navigation", 530, "compass"),
                BlazorMenuSeedData.Page("按钮与折叠", "/neo-demo/ui/layout-controls", 531, "mouse-pointer-click"),
                BlazorMenuSeedData.Page("布局与主题", "/neo-demo/ui/layout-tools", 532, "columns-3"),
                BlazorMenuSeedData.Page("模态与侧板", "/neo-demo/ui/overlays-modal", 540, "panel-top"),
                BlazorMenuSeedData.Page("浮动与菜单", "/neo-demo/ui/overlays-floating", 541, "layers")
            ])
        ]),

        BlazorMenuSeedData.Menu("博客管理", "newspaper", string.Empty, 45, SysMenuSidebarStyle.展开,
        [
            BlazorMenuSeedData.Page("分类", "/Blog/Classify", 451, "folder", isSystem: false),
            BlazorMenuSeedData.Page("频道", "/Blog/Channel", 452, "rss", isSystem: false),
            BlazorMenuSeedData.PageWithAudit("文章", "/Blog/Article", 453, "file-text", isSystem: false),
            BlazorMenuSeedData.Page("标签", "/Blog/Tag2", 454, "tags", isSystem: false),
            BlazorMenuSeedData.Page("评论", "/Blog/Comment", 455, "message-circle", isSystem: false),
            BlazorMenuSeedData.Page("用户点赞", "/Blog/UserLike", 456, "thumbs-up", isSystem: false),
            BlazorMenuSeedData.Page("收藏", "/Blog/Collection", 457, "bookmark", isSystem: false),
        ],
        isSystem: false),
    ];

    private static void EnsureCustomPageButtons(IFreeSql freeSql)
    {
        EnsurePageButtons(freeSql, "/Blog/Collection",
            ("分配随笔", "file-text", "assign_articles", 304)
        );

        EnsurePageButtons(freeSql, "/Blog/Channel",
            ("分配标签", "tags", "assign_tags", 304)
        );
    }

    private static void EnsurePageButtons(
        IFreeSql freeSql,
        string pagePath,
        params (string Label, string Icon, string Path, int Sort)[] buttons)
    {
        SysMenu? pageMenu = freeSql.Select<SysMenu>()
            .Where(m => m.Path == pagePath && m.Type == SysMenuType.增删改查)
            .First();
        if (pageMenu is null)
        {
            return;
        }

        foreach ((string label, string icon, string path, int sort) in buttons)
        {
            bool exists = freeSql.Select<SysMenu>()
                .Any(m => m.ParentId == pageMenu.Id && m.Path == path && m.Type == SysMenuType.按钮);
            if (exists)
            {
                continue;
            }

            freeSql.Insert(new SysMenu
            {
                ParentId = pageMenu.Id,
                Label = label,
                Icon = icon,
                Path = path,
                Sort = sort,
                Type = SysMenuType.按钮,
                IsSystem = pageMenu.IsSystem,
            }).ExecuteAffrows();
        }
    }
}
