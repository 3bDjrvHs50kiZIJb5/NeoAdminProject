using FreeSql;
using NeoAdmin.Blazor.Entities;
using BlazorMenuSeedData = NeoAdmin.Blazor.SeedData.MenuSeedData;

namespace NeoAdmin.SeedData;

/// <summary>
/// 博客管理菜单种子数据（宿主项目专用）。
/// </summary>
public static class MenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        BlazorMenuSeedData.EnsureMenus(freeSql, CreateMenus());
    }

    private static List<SysMenu> CreateMenus() =>
    [
        BlazorMenuSeedData.Menu("博客管理", "newspaper", string.Empty, 45, SysMenuSidebarStyle.展开,
        [
            BlazorMenuSeedData.Page("分类", "/Blog/Classify", 451, "folder"),
            BlazorMenuSeedData.Page("频道", "/Blog/Channel", 452, "rss"),
            BlazorMenuSeedData.PageWithAudit("文章", "/Blog/Article", 453, "file-text"),
            BlazorMenuSeedData.Page("标签", "/Blog/Tag2", 454, "tags"),
            BlazorMenuSeedData.Page("评论", "/Blog/Comment", 455, "message-circle"),
            BlazorMenuSeedData.Page("用户点赞", "/Blog/UserLike", 456, "thumbs-up"),
            BlazorMenuSeedData.Page("收藏", "/Blog/Collection", 457, "bookmark")
        ])
    ];
}
