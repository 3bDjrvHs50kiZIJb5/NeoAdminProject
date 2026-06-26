using FreeSql;
using NeoAdmin.Blazor.Entities;
using BlazorMenuSeedData = NeoAdmin.Blazor.SeedData.MenuSeedData;

namespace NeoAdminApp.SeedData;

/// <summary>
/// NeoDemo 演示菜单种子（宿主专用）：迁移旧菜单并增量写入菜单树。
/// </summary>
public static class DemoMenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        MigrateLegacy(freeSql);
        BlazorMenuSeedData.EnsureMenus(freeSql, CreateMenus());
    }

    private static void MigrateLegacy(IFreeSql freeSql)
    {
        RemoveLegacyDemoRoots(freeSql);
        RemoveMovedBusinessDemoMenus(freeSql);

        freeSql.Delete<SysMenu>()
            .Where(a => a.Path == "/neo-demo/comp/image-upload")
            .ExecuteAffrows();

        freeSql.Delete<SysMenu>()
            .Where(a => a.IsSystem
                        && a.Path.StartsWith("/neo-demo/")
                        && !a.Path.StartsWith("/neo-demo/comp/")
                        && !a.Path.StartsWith("/neo-demo/ui/"))
            .ExecuteAffrows();
    }

    private static void RemoveLegacyDemoRoots(IFreeSql freeSql)
    {
        List<long> rootIds = freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == 0
                        && a.IsSystem
                        && (a.Label == "BBDemo" || a.Label == "NovaDemo"))
            .ToList(a => a.Id);

        if (rootIds.Count == 0)
        {
            return;
        }

        List<SysMenu> all = freeSql.Select<SysMenu>().ToList();
        List<long> deleteIds = new(rootIds);
        for (int index = 0; index < deleteIds.Count; index++)
        {
            long parentId = deleteIds[index];
            deleteIds.AddRange(all.Where(a => a.ParentId == parentId && a.IsSystem).Select(a => a.Id));
        }

        freeSql.Delete<SysMenu>()
            .Where(a => deleteIds.Contains(a.Id) && a.IsSystem)
            .ExecuteAffrows();
    }

    /// <summary>移除已迁移/改名的演示菜单，避免侧边栏重复。</summary>
    private static void RemoveMovedBusinessDemoMenus(IFreeSql freeSql)
    {
        string[] movedPaths =
        [
            "/neo-demo/ui/nova-button",
            "/neo-demo/ui/file-cache",
            "/neo-demo/ui/anti-concurrency",
            "/neo-demo/ui/transactional",
            "/neo-demo/ui/animation"
        ];

        string[] obsoleteLabels = ["NovaButton", "AntiConcurrency", "Transactional"];

        List<long> rootIds = new();

        long? uiCompId = freeSql.Select<SysMenu>()
            .Where(a => a.IsSystem && a.Label == "UI 组件")
            .First(a => (long?)a.Id);

        if (uiCompId is not null)
        {
            rootIds.AddRange(freeSql.Select<SysMenu>()
                .Where(a => a.ParentId == uiCompId && a.IsSystem && movedPaths.Contains(a.Path))
                .ToList(a => a.Id));
        }

        rootIds.AddRange(freeSql.Select<SysMenu>()
            .Where(a => a.IsSystem && movedPaths.Contains(a.Path) && obsoleteLabels.Contains(a.Label))
            .ToList(a => a.Id));

        rootIds = rootIds.Distinct().ToList();
        if (rootIds.Count == 0)
        {
            return;
        }

        List<SysMenu> all = freeSql.Select<SysMenu>().ToList();
        List<long> deleteIds = new(rootIds);
        for (int index = 0; index < deleteIds.Count; index++)
        {
            long parentId = deleteIds[index];
            deleteIds.AddRange(all.Where(a => a.ParentId == parentId && a.IsSystem).Select(a => a.Id));
        }

        freeSql.Delete<SysMenu>()
            .Where(a => deleteIds.Contains(a.Id))
            .ExecuteAffrows();
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
                BlazorMenuSeedData.Page("语音输入", "/neo-demo/comp/voice-input", 409, "mic"),
                BlazorMenuSeedData.Page("更新日志", "/neo-demo/comp/update-log", 410, "scroll-text"),
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
        ])
    ];
}
