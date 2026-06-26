using FreeSql;
using NeoAdmin.Blazor.Core.Workflow;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Core.Navigation;

namespace NeoAdmin.Blazor.SeedData;

/// <summary>
/// 为指定页面菜单补齐审批流按钮权限点。
/// 仅在旧库升级、且该页尚无审批按钮时手动调用一次；
/// 不要放在 <c>DataSetup.Initialize</c> 里每次启动执行，否则会覆盖用户在菜单管理里取消勾选的审批按钮。
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

    /// <summary>移除已废弃的五审按钮权限点。</summary>
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
