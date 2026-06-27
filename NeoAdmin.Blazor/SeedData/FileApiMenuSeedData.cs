using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

/// <summary>
/// 框架 <c>/api/file</c> 接口权限菜单（挂在宿主 Api 根节点下）。
/// 旧库已有 Api 时 <see cref="MenuSeedData.EnsureRecursive"/> 不会自动补接口子节点，故每次启动增量执行。
/// </summary>
public static class FileApiMenuSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        SysMenu? apiRoot = freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == 0 && a.Label == "Api" && a.Type == SysMenuType.接口)
            .First();
        if (apiRoot is null)
        {
            return;
        }

        SysMenu fileApiGroup = MenuSeedData.Menu("File", "folder-open", "file", 300, children:
        [
            MenuSeedData.Api("Upload", "upload", "Upload", 301, isSystem: false),
            MenuSeedData.Api("UploadMultiple", "files", "UploadMultiple", 302, isSystem: false),
            MenuSeedData.Api("Delete", "trash-2", "Delete", 303, isSystem: false)
        ], type: SysMenuType.接口, isSystem: false);

        MenuSeedData.EnsureMenuUnderParent(freeSql, fileApiGroup, apiRoot.Id);
    }
}
