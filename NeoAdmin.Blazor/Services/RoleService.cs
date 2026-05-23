using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FreeSql;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Data.Entities;
using NeoAdmin.Blazor.Menus;

namespace NeoAdmin.Blazor.Services;

public sealed class RoleService
{
    private static readonly Regex IpWhiteListSeparatorRegex = new(
        @"\b(\r\n|\n|;)\b",
        RegexOptions.Compiled);

    private readonly IFreeSql _freeSql;

    public RoleService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public Task<List<SysRole>> GetAllAsync() =>
        _freeSql.Select<SysRole>()
            .OrderBy(a => a.Id)
            .ToListAsync();

    public async Task<ApiResult<SysRole>> SaveAsync(SysRole model)
    {
        ApiResult? validationError = Validate(model);
        if (validationError is not null)
        {
            return ApiResult<SysRole>.Error(validationError.Message, validationError.Code);
        }

        model.Name = model.Name.Trim();
        model.Description = model.Description?.Trim() ?? string.Empty;
        model.IpWhiteList = NormalizeIpWhiteList(model.IpWhiteList);

        if (model.Id == 0)
        {
            await _freeSql.Insert(model).ExecuteAffrowsAsync();
        }
        else
        {
            SysRole? existing = await _freeSql.Select<SysRole>()
                .Where(a => a.Id == model.Id)
                .FirstAsync();
            if (existing is null)
            {
                return ApiResult<SysRole>.Error("角色不存在");
            }

            model.IsAdministrator = existing.IsAdministrator;
            await _freeSql.Update<SysRole>()
                .SetSource(model)
                .ExecuteAffrowsAsync();
        }

        return ApiResult<SysRole>.Success(model, "保存成功");
    }

    public async Task<ApiResult> DeleteAsync(long id)
    {
        SysRole? role = await _freeSql.Select<SysRole>().Where(a => a.Id == id).FirstAsync();
        if (role is null)
        {
            return ApiResult.Error("角色不存在");
        }

        if (role.IsAdministrator)
        {
            return ApiResult.Error("不能删除系统角色");
        }

        await _freeSql.Delete<SysRoleUser>().Where(a => a.RoleId == id).ExecuteAffrowsAsync();
        await _freeSql.Delete<SysRoleMenu>().Where(a => a.RoleId == id).ExecuteAffrowsAsync();
        await _freeSql.Delete<SysRole>().Where(a => a.Id == id).ExecuteAffrowsAsync();
        return ApiResult.Success("删除成功");
    }

    public Task<List<long>> GetUserIdsAsync(long roleId) =>
        _freeSql.Select<SysRoleUser>()
            .Where(a => a.RoleId == roleId)
            .ToListAsync(a => a.UserId);

    public Task<List<long>> GetMenuIdsAsync(long roleId) =>
        _freeSql.Select<SysRoleMenu>()
            .Where(a => a.RoleId == roleId)
            .ToListAsync(a => a.MenuId);

    public async Task<ApiResult> SetUsersAsync(long roleId, IReadOnlyCollection<long> userIds)
    {
        if (!await _freeSql.Select<SysRole>().AnyAsync(a => a.Id == roleId))
        {
            return ApiResult.Error("角色不存在");
        }

        await _freeSql.Delete<SysRoleUser>().Where(a => a.RoleId == roleId).ExecuteAffrowsAsync();
        if (userIds.Count > 0)
        {
            List<SysRoleUser> rows = userIds
                .Distinct()
                .Select(userId => new SysRoleUser { RoleId = roleId, UserId = userId })
                .ToList();
            await _freeSql.Insert(rows).ExecuteAffrowsAsync();
        }

        return ApiResult.Success("用户分配已保存");
    }

    public async Task<ApiResult> SetMenusAsync(long roleId, IReadOnlyCollection<long> menuIds)
    {
        SysRole? role = await _freeSql.Select<SysRole>().Where(a => a.Id == roleId).FirstAsync();
        if (role is null)
        {
            return ApiResult.Error("角色不存在");
        }

        if (role.IsAdministrator)
        {
            return ApiResult.Error("管理员角色拥有全部菜单，无需分配");
        }

        await _freeSql.Delete<SysRoleMenu>().Where(a => a.RoleId == roleId).ExecuteAffrowsAsync();
        if (menuIds.Count > 0)
        {
            List<SysRoleMenu> rows = menuIds
                .Distinct()
                .Select(menuId => new SysRoleMenu { RoleId = roleId, MenuId = menuId })
                .ToList();
            await _freeSql.Insert(rows).ExecuteAffrowsAsync();
        }

        return ApiResult.Success("菜单分配已保存");
    }

    public Task<List<SysUser>> GetUsersForAllocAsync(string? searchText) =>
        _freeSql.Select<SysUser>()
            .WhereIf(!string.IsNullOrWhiteSpace(searchText),
                a => a.Username.Contains(searchText!) || a.Nickname.Contains(searchText!))
            .OrderBy(a => a.Username)
            .ToListAsync();

    public async Task<List<SysMenu>> GetMenusForAllocAsync()
    {
        List<SysMenu> menus = await _freeSql.Select<SysMenu>()
            .OrderBy(a => a.Sort)
            .OrderBy(a => a.Id)
            .ToListAsync();
        return FlattenMenus(MenuService.BuildTree(menus));
    }

    public static string NormalizeIpWhiteList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return IpWhiteListSeparatorRegex.Replace(value.Trim(), "; ");
    }

    private static List<SysMenu> FlattenMenus(IEnumerable<SysMenu> menus)
    {
        List<SysMenu> rows = new();
        void Walk(IEnumerable<SysMenu> items)
        {
            foreach (SysMenu item in items)
            {
                rows.Add(item);
                Walk(item.Children);
            }
        }

        Walk(menus);
        return rows;
    }

    private static ApiResult? Validate(SysRole role)
    {
        var validationContext = new ValidationContext(role);
        var validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(role, validationContext, validationResults, true))
        {
            return null;
        }

        return ApiResult.Error(string.Join(", ", validationResults.Select(a => a.ErrorMessage)));
    }
}
