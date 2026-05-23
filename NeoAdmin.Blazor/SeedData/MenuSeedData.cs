using FreeSql;
using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class MenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        RemoveObsolete(freeSql);
        EnsureMenus(freeSql, CreateMenus());
    }

    public static void EnsureMenus(IFreeSql freeSql, IEnumerable<SysMenu> menus)
    {
        foreach (SysMenu menu in menus)
        {
            EnsureRecursive(freeSql, menu, 0);
        }
    }

    private static void RemoveObsolete(IFreeSql freeSql)
    {
        freeSql.Delete<SysMenu>()
            .Where(a => a.IsSystem
                        && a.Path.StartsWith("/neo-demo/")
                        && !a.Path.StartsWith("/neo-demo/comp/")
                        && !a.Path.StartsWith("/neo-demo/ui/"))
            .ExecuteAffrows();

        long? neoDemoId = freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == 0 && a.IsSystem && a.Label == "NeoDemo")
            .First(a => (long?)a.Id);

        if (neoDemoId is not null)
        {
            freeSql.Delete<SysMenu>()
                .Where(a => a.ParentId == neoDemoId && a.IsSystem && a.Path.StartsWith("/neo-demo/"))
                .ExecuteAffrows();
        }

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

    private static void EnsureRecursive(IFreeSql freeSql, SysMenu target, long parentId)
    {
        SysMenu? current = freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == parentId
                        && a.Label == target.Label
                        && a.Path == target.Path
                        && a.Type == target.Type)
            .First();

        if (current is null)
        {
            current = Copy(target, parentId);
            freeSql.Insert(current).ExecuteAffrows();
        }
        else
        {
            freeSql.Update<SysMenu>()
                .Where(a => a.Id == current.Id)
                .Set(a => a.Icon, target.Icon)
                .Set(a => a.Sort, target.Sort)
                .Set(a => a.Type, target.Type)
                .Set(a => a.SidebarStyle, target.SidebarStyle)
                .Set(a => a.IsHidden, target.IsHidden)
                .Set(a => a.IsSystem, true)
                .ExecuteAffrows();
        }

        foreach (SysMenu child in target.Children)
        {
            EnsureRecursive(freeSql, child, current.Id);
        }
    }

    private static SysMenu Copy(SysMenu source, long parentId) => new()
    {
        ParentId = parentId,
        Label = source.Label,
        Icon = source.Icon,
        Path = source.Path,
        Sort = source.Sort,
        Type = source.Type,
        Description = source.Description,
        SidebarStyle = source.SidebarStyle,
        IsSystem = true,
        IsHidden = source.IsHidden
    };

    private static List<SysMenu> CreateMenus() =>
    [
        Menu("控制台", "layout-dashboard", "/admin", 10),
        Menu("系统管理", "settings", string.Empty, 900, SysMenuSidebarStyle.展开,
        [
            Menu("菜单管理", "list-tree", "/admin/menu", 910, type: SysMenuType.增删改查),
            Menu("用户管理", "users", "/admin/user", 920, type: SysMenuType.增删改查),
            Menu("角色管理", "shield", "/admin/role", 922, type: SysMenuType.增删改查),
            Menu("组织", "network", "/admin/org", 925, type: SysMenuType.增删改查),
            Menu("字典管理", "book-open", "/admin/dict", 930, type: SysMenuType.增删改查),
            Menu("参数配置", "sliders-horizontal", "/admin/param", 940, type: SysMenuType.增删改查),
            Menu("站点设置", "globe", "/admin/site-settings", 942),
            Menu("IP 白名单", "shield-check", "/admin/ip-whitelist", 945, type: SysMenuType.增删改查),
            Menu("文件管理", "folder-open", "/admin/file", 950, type: SysMenuType.增删改查),
            Menu("定时任务", "clock", "/admin/task-scheduler", 955, type: SysMenuType.增删改查)
        ]),
        Menu("NeoDemo", "flask-conical", string.Empty, 40, SysMenuSidebarStyle.展开,
        [
            Menu("业务组件", "blocks", string.Empty, 400, SysMenuSidebarStyle.收起,
            [
                Page("实体选择组件", "/neo-demo/comp/select-components", 401, "list-checks"),
                Page("字典和参数配置", "/neo-demo/comp/dict-param", 402, "sliders-horizontal")
            ]),
            Menu("UI 组件", "layout-grid", string.Empty, 500, SysMenuSidebarStyle.收起,
            [
                Page("表单输入", "/neo-demo/ui/form-inputs", 501, "text-cursor-input"),
                Page("选择与开关", "/neo-demo/ui/form-controls", 502, "toggle-right"),
                Page("增强输入", "/neo-demo/ui/advanced-inputs", 503, "wand-sparkles"),
                Page("日期与时间", "/neo-demo/ui/advanced-datetime", 504, "calendar"),
                Page("编辑器与复杂", "/neo-demo/ui/advanced-complex", 505, "file-code"),
                Page("内容与装饰", "/neo-demo/ui/display-basics", 510, "layout-template"),
                Page("状态与列表", "/neo-demo/ui/display-states", 511, "list"),
                Page("数据展示", "/neo-demo/ui/data-display", 512, "table-2"),
                Page("图表", "/neo-demo/ui/chart", 513, "chart-column"),
                Page("反馈组件", "/neo-demo/ui/feedback", 520, "bell-ring"),
                Page("动画演示", "/neo-demo/ui/animation", 521, "sparkles"),
                Page("移动端演示", "/neo-demo/ui/mobile", 522, "smartphone"),
                Page("导航组件", "/neo-demo/ui/navigation", 530, "compass"),
                Page("按钮与折叠", "/neo-demo/ui/layout-controls", 531, "mouse-pointer-click"),
                Page("布局与主题", "/neo-demo/ui/layout-tools", 532, "columns-3"),
                Page("模态与侧板", "/neo-demo/ui/overlays-modal", 540, "panel-top"),
                Page("浮动与菜单", "/neo-demo/ui/overlays-floating", 541, "layers")
            ])
        ]),
        Menu("Api", "code", string.Empty, 0, SysMenuSidebarStyle.收起,
        [
            Menu("Login", "log-in", "login", 100, children:
            [
                Api("Register", "user-plus", "Register", 101),
                Api("GetWhoIsUsingList", "users", "GetWhoIsUsingList", 102),
                Api("Login", "log-in", "Login", 103),
                Api("Logout", "log-out", "Logout", 104),
                Api("Check", "circle-check", "Check", 105),
                Api("UpdateMemberInfo", "user-pen", "UpdateMemberInfo", 106),
                Api("ChangePassword", "key-round", "ChangePassword", 107),
                Api("DeleteAccount", "user-x", "DeleteAccount", 108),
                Api("UploadAvatar", "image", "UploadAvatar", 109),
                Api("UploadBadgePhoto", "badge", "UploadBadgePhoto", 110),
                Api("SendResetPasswordCode", "mail", "SendResetPasswordCode", 111),
                Api("ResetPassword", "unlock-keyhole", "ResetPassword", 112),
                Api("SetAIAlarmLevel", "bot", "SetAIAlarmLevel", 113)
            ]),
            Menu("Article", "newspaper", "article", 200, children:
            [
                Api("GetAll", "list", "GetAll", 201)
            ])
        ], isHidden: true)
    ];

    public static SysMenu Page(string label, string path, int sort, string icon) =>
        Menu(label, icon, path, sort, type: SysMenuType.增删改查);

    private static SysMenu Button(string label, string icon, string path, int sort) =>
        Menu(label, icon, path, sort, type: SysMenuType.按钮);

    private static SysMenu Api(string label, string icon, string path, int sort) =>
        Menu(label, icon, path, sort, type: SysMenuType.接口);

    public static SysMenu Menu(
        string label,
        string icon,
        string path,
        int sort,
        SysMenuSidebarStyle sidebarStyle = SysMenuSidebarStyle.收起,
        List<SysMenu>? children = null,
        SysMenuType type = SysMenuType.菜单,
        bool isHidden = false)
    {
        SysMenu menu = new()
        {
            Label = label,
            Icon = icon,
            Path = path,
            Sort = sort,
            Type = type,
            SidebarStyle = sidebarStyle,
            IsSystem = true,
            IsHidden = isHidden
        };

        if (type == SysMenuType.增删改查 && (children is null || children.Count == 0))
        {
            children =
            [
                Button("添加", "plus", "add", 301),
                Button("编辑", "pencil", "edit", 302),
                Button("删除", "trash-2", "remove", 303)
            ];
        }

        if (children is not null)
        {
            menu.Children = children;
        }

        return menu;
    }
}
