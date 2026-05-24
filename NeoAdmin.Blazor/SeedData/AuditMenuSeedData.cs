using FreeSql;
using NeoAdmin.Blazor.Audit;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Menus;

namespace NeoAdmin.Blazor.SeedData;

/// <summary>
/// 为指定页面菜单补齐审批流按钮权限点。
/// </summary>
public static class AuditMenuSeedData
{
    public static void EnsureButtons(IFreeSql freeSql, params string[] menuPaths)
    {
        RemoveObsoleteStepButtons(freeSql);

        foreach (string menuPath in menuPaths)
        {
            EnsureButtonsForPath(freeSql, menuPath);
        }
    }

    /// <summary>移除已废弃的二审～五审按钮权限点。</summary>
    public static void RemoveObsoleteStepButtons(IFreeSql freeSql)
    {
        List<long> menuIds = freeSql.Select<SysMenu>()
            .Where(a => a.IsSystem && AuditMenuDefinitions.ObsoleteStepButtonPaths.Contains(a.Path))
            .ToList(a => a.Id);
        if (menuIds.Count == 0)
        {
            return;
        }

        freeSql.Delete<SysRoleMenu>().Where(a => menuIds.Contains(a.MenuId)).ExecuteAffrows();
        freeSql.Delete<SysMenu>().Where(a => menuIds.Contains(a.Id)).ExecuteAffrows();
    }

    public static void EnsureButtonsForPath(IFreeSql freeSql, string menuPath)
    {
        string normalized = MenuService.NormalizePath(menuPath);
        SysMenu? page = freeSql.Select<SysMenu>()
            .Where(a => a.Path == normalized)
            .First();
        if (page is null)
        {
            return;
        }

        MenuService.EnsureAuditButtons(freeSql, page.Id);
    }
}
