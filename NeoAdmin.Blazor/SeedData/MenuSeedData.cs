using FreeSql;
using NeoAdmin.Blazor.Core.Workflow;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class MenuSeedData
{
    /// <summary>
    /// 仅在系统菜单尚未初始化时写入框架 /admin 菜单种子，不受 <see cref="Data.NeoAdminOptions.EnableSeedData"/> 影响。
    /// </summary>
    public static void Ensure(IFreeSql freeSql)
    {
        if (!NeedsInitialSystemMenus(freeSql))
        {
            return;
        }

        EnsureMenus(freeSql, CreateMenus());
    }

    /// <summary>库中尚无 /admin/menu 时表示系统菜单未初始化。</summary>
    private static bool NeedsInitialSystemMenus(IFreeSql freeSql) =>
        !freeSql.Select<SysMenu>().Any(a => a.Path == "/admin/menu");

    public static void EnsureMenus(IFreeSql freeSql, IEnumerable<SysMenu> menus)
    {
        foreach (SysMenu menu in menus)
        {
            EnsureRecursive(freeSql, menu, 0);
        }
    }

    /// <summary>
    /// 在指定父节点下增量补齐菜单（用于旧库升级时新增 Api 分组等）。
    /// </summary>
    public static void EnsureMenuUnderParent(IFreeSql freeSql, SysMenu target, long parentId) =>
        EnsureRecursive(freeSql, target, parentId);

    /// <summary>
    /// 按 <c>ParentId + Label + Path + Type</c> 判重：不存在则插入，已存在则永不更新，并继续递归子节点以补齐缺失项。
    /// </summary>
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
        IsSystem = source.IsSystem,
        IsHidden = source.IsHidden
    };

    private static List<SysMenu> CreateMenus() =>
    [
        Menu("控制台", "layout-dashboard", "/admin", 10, isHidden: true),
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
            Menu("定时任务", "clock", "/admin/task-scheduler", 955, type: SysMenuType.增删改查),
            Menu("系统日志", "scroll-text", "/admin/system-log", 958)
        ])
    ];

    public static SysMenu Page(string label, string path, int sort, string icon, bool isSystem = true) =>
        Menu(label, icon, path, sort, type: SysMenuType.增删改查, isSystem: isSystem);

    /// <summary>增删改查 + 审批流按钮（提交/一审～四审/拒绝/反审/历史版本）。</summary>
    public static SysMenu PageWithAudit(string label, string path, int sort, string icon, bool isSystem = true) =>
        Menu(label, icon, path, sort, type: SysMenuType.增删改查, children: CreateCrudAndAuditButtons(isSystem), isSystem: isSystem);

    public static SysMenu Button(string label, string icon, string path, int sort, bool isSystem = true) =>
        Menu(label, icon, path, sort, type: SysMenuType.按钮, isSystem: isSystem);

    public static SysMenu Api(string label, string icon, string path, int sort, bool isSystem = true) =>
        Menu(label, icon, path, sort, type: SysMenuType.接口, isSystem: isSystem);

    public static SysMenu Menu(
        string label,
        string icon,
        string path,
        int sort,
        SysMenuSidebarStyle sidebarStyle = SysMenuSidebarStyle.收起,
        List<SysMenu>? children = null,
        SysMenuType type = SysMenuType.菜单,
        bool isHidden = false,
        bool isSystem = true)
    {
        SysMenu menu = new()
        {
            Label = label,
            Icon = icon,
            Path = path,
            Sort = sort,
            Type = type,
            SidebarStyle = sidebarStyle,
            IsSystem = isSystem,
            IsHidden = isHidden
        };

        if (type == SysMenuType.增删改查 && (children is null || children.Count == 0))
        {
            children =
            [
                Button("添加", "plus", "add", 301, isSystem),
                Button("编辑", "pencil", "edit", 302, isSystem),
                Button("删除", "trash-2", "remove", 303, isSystem)
            ];
        }

        if (children is not null)
        {
            menu.Children = children;
        }

        return menu;
    }

    private static List<SysMenu> CreateCrudAndAuditButtons(bool isSystem = true)
    {
        List<SysMenu> buttons =
        [
            Button("添加", "plus", "add", 301, isSystem),
            Button("编辑", "pencil", "edit", 302, isSystem),
            Button("删除", "trash-2", "remove", 303, isSystem)
        ];
        foreach (SysMenu auditButton in AuditMenuDefinitions.CreateAuditButtons())
        {
            auditButton.IsSystem = isSystem;
            buttons.Add(auditButton);
        }

        return buttons;
    }
}
