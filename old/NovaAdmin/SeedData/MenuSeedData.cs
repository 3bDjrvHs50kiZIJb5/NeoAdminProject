using BootstrapBlazor.Components;
using FreeSql;

namespace NovaAdmin.SeedData;

/// <summary>
/// 菜单种子数据 - 用于初始化系统菜单
/// </summary>
public static class MenuSeedData
{
    /// <summary>
    /// 初始化当前后台的菜单数据
    /// </summary>
    public static void Initialize(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var repo = fsql.GetAggregateRootRepository<SysMenu>();
        var rootMenus = new[]
        {
            CreateBlogMenu(adminUserId, adminUsername),
            CreateBBDemoMenu(adminUserId, adminUsername),
            CreateNovaDemoMenu(adminUserId, adminUsername),
            CreateApiMenu(adminUserId, adminUsername)
        };

        foreach (var rootMenu in rootMenus)
        {
            EnsureMenuRecursive(fsql, repo, rootMenu, 0);
        }
    }

    /// <summary>
    /// 递归补齐菜单：当前节点不存在则新增，存在则继续检查其子节点
    /// </summary>
    private static void EnsureMenuRecursive(
        FreeSqlCloud fsql,
        IBaseRepository<SysMenu> repo,
        SysMenu targetMenu,
        long parentId)
    {
        var currentMenu = FindMenuByParent(fsql, targetMenu, parentId);

        if (currentMenu == null)
        {
            currentMenu = CreateMenuWithoutChildren(targetMenu, parentId);
            repo.Insert(currentMenu);
        }

        if (targetMenu.Children == null || targetMenu.Children.Count == 0)
        {
            return;
        }

        foreach (var child in targetMenu.Children)
        {
            EnsureMenuRecursive(fsql, repo, child, currentMenu.Id);
        }
    }

    private static SysMenu? FindMenuByParent(FreeSqlCloud fsql, SysMenu menu, long parentId) =>
        fsql.Select<SysMenu>()
            .Where(a => a.ParentId == parentId
                        && a.Label == menu.Label
                        && a.Path == menu.Path
                        && a.Type == menu.Type)
            .First();

    private static SysMenu CreateMenuWithoutChildren(SysMenu source, long parentId) =>
        new()
        {
            ParentId = parentId,
            Label = source.Label,
            Path = source.Path,
            Icon = source.Icon,
            Sort = source.Sort,
            Type = source.Type,
            IsSystem = source.IsSystem,
            IsHidden = source.IsHidden,
            Description = source.Description,
            SidebarStyle = source.SidebarStyle,
            CreatedUserId = source.CreatedUserId,
            CreatedUserName = source.CreatedUserName
        };

    // =====================================================
    // 后台管理菜单（侧边栏展示）
    // =====================================================

    /// <summary>
    /// 博客管理菜单
    /// </summary>
    private static SysMenu CreateBlogMenu(long adminUserId, string adminUsername)
    {
        return CreateRootMenu(
            label: "博客管理",
            icon: "fas fa-blog",
            sort: 45,
            adminUserId: adminUserId,
            adminUsername: adminUsername,
            children:
            [
                CreatePageMenu("分类", "Blog/Classify", 451, "fas fa-folder", adminUserId, adminUsername),
                CreatePageMenu("频道", "Blog/Channel", 452, "fas fa-rss", adminUserId, adminUsername),
                CreatePageMenu("文章", "Blog/Article", 453, "fas fa-file-alt", adminUserId, adminUsername),
                CreatePageMenu("标签", "Blog/Tag2", 454, "fas fa-tags", adminUserId, adminUsername),
                CreatePageMenu("评论", "Blog/Comment", 455, "fas fa-comment", adminUserId, adminUsername),
                CreatePageMenu("用户点赞", "Blog/UserLike", 456, "fas fa-thumbs-up", adminUserId, adminUsername),
                CreatePageMenu("收藏", "Blog/Collection", 457, "fas fa-bookmark", adminUserId, adminUsername)
            ]);
    }

    /// <summary>
    /// BBDemo 菜单
    /// </summary>
    private static SysMenu CreateBBDemoMenu(long adminUserId, string adminUsername)
    {
        return CreateRootMenu(
            label: "BBDemo",
            icon: "fas fa-puzzle-piece",
            sort: 46,
            adminUserId: adminUserId,
            adminUsername: adminUsername,
            children:
            [
                CreatePlainPageMenu("浏览器指纹", "BBDemo/BrowserFingerprintDemo", 464, "fas fa-fingerprint", adminUserId, adminUsername),
                CreatePlainPageMenu("屏幕键盘", "BBDemo/OnScreenKeyboardDemo", 465, "fas fa-keyboard", adminUserId, adminUsername),
                CreatePlainPageMenu("拖拽上传", "BBDemo/UploadDropDemo", 466, "fas fa-file-upload", adminUserId, adminUsername),
                CreatePlainPageMenu("Markdown 演示", "BBDemo/MarkdownDemo", 467, "fas fa-file-alt", adminUserId, adminUsername),
                CreatePlainPageMenu("Tab 演示", "BBDemo/TabDemo", 469, "fas fa-columns", adminUserId, adminUsername),
                CreatePlainPageMenu("图标演示", "BBDemo/FaIconDemo", 471, "fa-icons", adminUserId, adminUsername),
                CreatePlainPageMenu("日期时间选择器", "BBDemo/DateTimePickerDemo", 472, "fas fa-calendar-alt", adminUserId, adminUsername),
                CreatePlainPageMenu("评分组件", "BBDemo/RateDemo", 473, "fas fa-star", adminUserId, adminUsername),
                CreatePlainPageMenu("省市区选择", "BBDemo/SelectRegionDemo", 474, "fas fa-map-marked-alt", adminUserId, adminUsername),
                CreatePlainPageMenu("Popover 演示", "BBDemo/PopoverDemo", 475, "fas fa-comment-dots", adminUserId, adminUsername),
                CreatePlainPageMenu("文件选择", "NovaDemo/NovaInputFileDemo", 468, "fas fa-folder-open", adminUserId, adminUsername)
            ]);
    }

    /// <summary>
    /// NovaDemo 菜单
    /// </summary>
    private static SysMenu CreateNovaDemoMenu(long adminUserId, string adminUsername)
    {
        return CreateRootMenu(
            label: "NovaDemo",
            icon: "fas fa-shapes",
            sort: 47,
            adminUserId: adminUserId,
            adminUsername: adminUsername,
            children:
            [
                CreatePageMenu("字典和参数配置", "NovaDemo/DictParamDemo", 458, "fas fa-sliders-h", adminUserId, adminUsername),
                CreatePageMenu("NovaAdminTable", "Blog/NovaAdminTableDemo", 459, "fas fa-table", adminUserId, adminUsername),
                CreatePageMenu(
                    "NovaButton",
                    "NovaDemo/NovaButtonDemo",
                    470,
                    "fas fa-shield-alt",
                    adminUserId,
                    adminUsername,
                    CreateDemoButtonMenu("demo_allow", 301, "fas fa-check", adminUserId, adminUsername),
                    CreateDemoButtonMenu("demo_deny", 302, "fas fa-ban", adminUserId, adminUsername)),
                CreatePageMenu("NovaModal", "NovaDemo/NovaModalDemo", 461, "fas fa-window-restore", adminUserId, adminUsername),
                CreatePageMenu("提示类", "NovaDemo/PromptDemo", 462, "fas fa-bell", adminUserId, adminUsername),
                CreatePageMenu("文件缓存", "NovaDemo/FileCacheDemo", 463, "fas fa-file-archive", adminUserId, adminUsername)
            ]);
    }

    private static SysMenu CreatePlainPageMenu(
        string label,
        string path,
        int sort,
        string icon,
        long adminUserId,
        string adminUsername) =>
        new()
        {
            Label = label,
            Path = path,
            Icon = icon,
            Sort = sort,
            Type = SysMenuType.菜单,
            IsSystem = true,
            IsHidden = false,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername,
            Children = new List<SysMenu>()
        };

    // =====================================================
    // Api 隐藏菜单（权限控制用）
    // =====================================================

    /// <summary>
    /// 创建 API 隐藏菜单，用于接口权限控制
    /// </summary>
    private static SysMenu CreateApiMenu(long adminUserId, string adminUsername)
    {
        return new SysMenu
        {
            Label = "Api",
            Path = string.Empty,
            Sort = 0,
            Type = SysMenuType.菜单,
            Icon = "fas fa-code",
            IsSystem = true,
            IsHidden = true,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername,
            Children =
            [
                CreateApiGroupMenu(
                    "Login",
                    "login",
                    100,
                    "fas fa-sign-in-alt",
                    adminUserId,
                    adminUsername,
                    CreateApiActionMenu("Register", 101, "fas fa-user-plus", adminUserId, adminUsername),
                    CreateApiActionMenu("GetWhoIsUsingList", 102, "fas fa-users", adminUserId, adminUsername),
                    CreateApiActionMenu("Login", 103, "fas fa-sign-in-alt", adminUserId, adminUsername),
                    CreateApiActionMenu("Logout", 104, "fas fa-sign-out-alt", adminUserId, adminUsername),
                    CreateApiActionMenu("Check", 105, "fas fa-check-circle", adminUserId, adminUsername),
                    CreateApiActionMenu("UpdateMemberInfo", 106, "fas fa-user-edit", adminUserId, adminUsername),
                    CreateApiActionMenu("ChangePassword", 107, "fas fa-key", adminUserId, adminUsername),
                    CreateApiActionMenu("DeleteAccount", 108, "fas fa-user-times", adminUserId, adminUsername),
                    CreateApiActionMenu("UploadAvatar", 109, "fas fa-image", adminUserId, adminUsername),
                    CreateApiActionMenu("UploadBadgePhoto", 110, "fas fa-id-badge", adminUserId, adminUsername),
                    CreateApiActionMenu("SendResetPasswordCode", 111, "fas fa-envelope", adminUserId, adminUsername),
                    CreateApiActionMenu("ResetPassword", 112, "fas fa-unlock-alt", adminUserId, adminUsername),
                    CreateApiActionMenu("SetAIAlarmLevel", 113, "fas fa-robot", adminUserId, adminUsername)),
                CreateApiGroupMenu(
                    "Article",
                    "article",
                    200,
                    "fas fa-newspaper",
                    adminUserId,
                    adminUsername,
                    CreateApiActionMenu("GetAll", 201, "fas fa-list", adminUserId, adminUsername))
            ]
        };
    }

    private static SysMenu CreateRootMenu(
        string label,
        string icon,
        int sort,
        long adminUserId,
        string adminUsername,
        List<SysMenu> children) =>
        new()
        {
            Label = label,
            Path = "",
            Icon = icon,
            Sort = sort,
            Type = SysMenuType.菜单,
            IsSystem = true,
            IsHidden = false,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername,
            Children = children
        };

    private static SysMenu CreatePageMenu(
        string label,
        string path,
        int sort,
        string icon,
        long adminUserId,
        string adminUsername,
        params SysMenu[] children) =>
        new()
        {
            Label = label,
            Path = path,
            Icon = icon,
            Sort = sort,
            Type = SysMenuType.菜单,
            IsSystem = true,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername,
            Children = children != null && children.Length > 0 ? children.ToList() : GetCrudButtons(adminUserId, adminUsername)
        };

    private static SysMenu CreateDemoButtonMenu(
        string label,
        int sort,
        string icon,
        long adminUserId,
        string adminUsername) =>
        new()
        {
            Label = label,
            Path = label,
            Sort = sort,
            Type = SysMenuType.按钮,
            Icon = icon,
            IsSystem = true,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername
        };

    private static SysMenu CreateApiGroupMenu(
        string label,
        string path,
        int sort,
        string icon,
        long adminUserId,
        string adminUsername,
        params SysMenu[] actions) =>
        new()
        {
            Label = label,
            Path = path,
            Icon = icon,
            Sort = sort,
            Type = SysMenuType.菜单,
            IsSystem = true,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername,
            Children = actions.ToList()
        };

    private static SysMenu CreateApiActionMenu(
        string label,
        int sort,
        string icon,
        long adminUserId,
        string adminUsername) =>
        new()
        {
            Label = label,
            Path = label,
            Sort = sort,
            Type = SysMenuType.按钮,
            Icon = icon,
            IsSystem = true,
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername
        };

    private static List<SysMenu> GetCrudButtons(long adminUserId, string adminUsername) =>
        new()
        {
            new SysMenu
            {
                Label = "添加",
                Path = "add",
                Sort = 301,
                Type = SysMenuType.按钮,
                IsSystem = true,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            },
            new SysMenu
            {
                Label = "编辑",
                Path = "edit",
                Sort = 302,
                Type = SysMenuType.按钮,
                IsSystem = true,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            },
            new SysMenu
            {
                Label = "删除",
                Path = "remove",
                Sort = 303,
                Type = SysMenuType.按钮,
                IsSystem = true,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            }
        };
}
