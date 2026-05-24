using FreeSql;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Audit;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Menus;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 按菜单路径校验当前用户是否拥有按钮权限。
/// </summary>
public sealed class MenuPermissionService
{
    private readonly IFreeSql _freeSql;
    private readonly NeoAdminAuthService _authService;
    private readonly IJSRuntime _jsRuntime;

    public MenuPermissionService(
        IFreeSql freeSql,
        NeoAdminAuthService authService,
        IJSRuntime jsRuntime)
    {
        _freeSql = freeSql;
        _authService = authService;
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> HasPageAsync(string menuPath)
    {
        long? userId = await GetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            return false;
        }

        if (await IsAdministratorAsync(userId.Value))
        {
            return true;
        }

        string normalizedMenuPath = MenuService.NormalizePath(menuPath);
        SysMenu? pageMenu = await _freeSql.Select<SysMenu>()
            .Where(a => a.Path == normalizedMenuPath)
            .FirstAsync();
        if (pageMenu is null)
        {
            return false;
        }

        List<long> roleIds = await _freeSql.Select<SysRoleUser>()
            .Where(a => a.UserId == userId.Value)
            .ToListAsync(a => a.RoleId);

        if (roleIds.Count == 0)
        {
            return false;
        }

        return await _freeSql.Select<SysRoleMenu>()
            .Where(a => roleIds.Contains(a.RoleId) && a.MenuId == pageMenu.Id)
            .AnyAsync();
    }

    public async Task<bool> HasButtonAsync(string menuPath, string buttonPath)
    {
        long? userId = await GetCurrentUserIdAsync();
        if (!userId.HasValue)
        {
            return false;
        }

        if (await IsAdministratorAsync(userId.Value))
        {
            return true;
        }

        string normalizedMenuPath = MenuService.NormalizePath(menuPath);
        SysMenu? pageMenu = await _freeSql.Select<SysMenu>()
            .Where(a => a.Path == normalizedMenuPath)
            .FirstAsync();
        if (pageMenu is null)
        {
            return false;
        }

        SysMenu? buttonMenu = await _freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == pageMenu.Id && a.Path == buttonPath)
            .FirstAsync();
        if (buttonMenu is null)
        {
            return false;
        }

        List<long> roleIds = await _freeSql.Select<SysRoleUser>()
            .Where(a => a.UserId == userId.Value)
            .ToListAsync(a => a.RoleId);

        if (roleIds.Count == 0)
        {
            return false;
        }

        return await _freeSql.Select<SysRoleMenu>()
            .Where(a => roleIds.Contains(a.RoleId) && a.MenuId == buttonMenu.Id)
            .AnyAsync();
    }

    public async Task<bool> HasAnyAuditButtonAsync(string menuPath)
    {
        foreach (string path in AuditMenuDefinitions.AllButtonPaths)
        {
            if (await HasButtonAsync(menuPath, path))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> IsAdministratorAsync(long userId)
    {
        List<long> roleIds = await _freeSql.Select<SysRoleUser>()
            .Where(a => a.UserId == userId)
            .ToListAsync(a => a.RoleId);
        if (roleIds.Count == 0)
        {
            return false;
        }

        return await _freeSql.Select<SysRole>()
            .Where(a => roleIds.Contains(a.Id) && a.IsAdministrator)
            .AnyAsync();
    }

    private async Task<long?> GetCurrentUserIdAsync()
    {
        string? token = await _jsRuntime.InvokeAsync<string?>("neoAdminAuth.getToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        ApiResult<UserSummaryResponse> result = await _authService.CheckAsync(token);
        return result.Succeeded ? result.Data!.Id : null;
    }
}
