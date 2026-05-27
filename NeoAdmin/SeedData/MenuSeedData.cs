using FreeSql;
using NeoAdmin.Blazor.Entities;
using BlazorMenuSeedData = NeoAdmin.Blazor.SeedData.MenuSeedData;

namespace NeoAdmin.SeedData;

/// <summary>
/// 业务菜单种子数据（宿主项目专用）。
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
            BlazorMenuSeedData.Page("分类", "/Blog/Classify", 451, "folder", isSystem: false),
            BlazorMenuSeedData.Page("频道", "/Blog/Channel", 452, "rss", isSystem: false),
            BlazorMenuSeedData.PageWithAudit("文章", "/Blog/Article", 453, "file-text", isSystem: false),
            BlazorMenuSeedData.Page("标签", "/Blog/Tag2", 454, "tags", isSystem: false),
            BlazorMenuSeedData.Page("评论", "/Blog/Comment", 455, "message-circle", isSystem: false),
            BlazorMenuSeedData.Page("用户点赞", "/Blog/UserLike", 456, "thumbs-up", isSystem: false),
            BlazorMenuSeedData.Page("收藏", "/Blog/Collection", 457, "bookmark", isSystem: false)
        ], isSystem: false),
        
        BlazorMenuSeedData.Menu("Api", "code", string.Empty, 0, SysMenuSidebarStyle.收起,
        [
            BlazorMenuSeedData.Menu("Login", "log-in", "login", 100, children:
            [
                BlazorMenuSeedData.Api("Register", "user-plus", "Register", 101, isSystem: false),
                BlazorMenuSeedData.Api("GetWhoIsUsingList", "users", "GetWhoIsUsingList", 102, isSystem: false),
                BlazorMenuSeedData.Api("Login", "log-in", "Login", 103, isSystem: false),
                BlazorMenuSeedData.Api("Logout", "log-out", "Logout", 104, isSystem: false),
                BlazorMenuSeedData.Api("Check", "circle-check", "Check", 105, isSystem: false),
                BlazorMenuSeedData.Api("UpdateMemberInfo", "user-pen", "UpdateMemberInfo", 106, isSystem: false),
                BlazorMenuSeedData.Api("ChangePassword", "key-round", "ChangePassword", 107, isSystem: false),
                BlazorMenuSeedData.Api("DeleteAccount", "user-x", "DeleteAccount", 108, isSystem: false),
                BlazorMenuSeedData.Api("UploadAvatar", "image", "UploadAvatar", 109, isSystem: false),
                BlazorMenuSeedData.Api("UploadBadgePhoto", "badge", "UploadBadgePhoto", 110, isSystem: false),
                BlazorMenuSeedData.Api("SendResetPasswordCode", "mail", "SendResetPasswordCode", 111, isSystem: false),
                BlazorMenuSeedData.Api("ResetPassword", "unlock-keyhole", "ResetPassword", 112, isSystem: false),
                BlazorMenuSeedData.Api("SetAIAlarmLevel", "bot", "SetAIAlarmLevel", 113, isSystem: false)
            ], type: SysMenuType.接口, isSystem: false),
            BlazorMenuSeedData.Menu("Article", "newspaper", "article", 200, children:
            [
                BlazorMenuSeedData.Api("GetAll", "list", "GetAll", 201, isSystem: false)
            ], type: SysMenuType.接口, isSystem: false)
        ], type: SysMenuType.接口, isHidden: true, isSystem: false)
    ];
}
