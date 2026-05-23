using FreeSql;
using NeoAdmin.Blazor.Data.Entities;
using NeoAdmin.Blazor.SeedData;

namespace NeoAdmin.SeedData;

/// <summary>
/// 博客管理菜单种子数据（宿主项目专用）。
/// </summary>
public static class BlogMenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        MenuSeedData.EnsureMenus(freeSql, CreateMenus());
    }

    private static List<SysMenu> CreateMenus() =>
    [
        MenuSeedData.Menu("博客管理", "newspaper", string.Empty, 45, SysMenuSidebarStyle.展开,
        [
            MenuSeedData.Page("分类", "/Blog/Classify", 451, "folder"),
            MenuSeedData.Page("频道", "/Blog/Channel", 452, "rss"),
            MenuSeedData.Page("文章", "/Blog/Article", 453, "file-text"),
            MenuSeedData.Page("标签", "/Blog/Tag2", 454, "tags"),
            MenuSeedData.Page("评论", "/Blog/Comment", 455, "message-circle"),
            MenuSeedData.Page("用户点赞", "/Blog/UserLike", 456, "thumbs-up"),
            MenuSeedData.Page("收藏", "/Blog/Collection", 457, "bookmark")
        ])
    ];
}
