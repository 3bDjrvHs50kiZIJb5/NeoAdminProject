using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Core.Workflow;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Core.Navigation;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 菜单路径与按钮权限（对齐旧版 AuthPath / AuthButton）。
/// </summary>
public sealed class MenuPermissionService
{
  private readonly IFreeSql _freeSql;
  private readonly NeoAdminAuthService _authService;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IJSRuntime _jsRuntime;

  private UserPermissionSnapshot? _snapshot;

  public MenuPermissionService(
    IFreeSql freeSql,
    NeoAdminAuthService authService,
    IHttpContextAccessor httpContextAccessor,
    IJSRuntime jsRuntime)
  {
    _freeSql = freeSql;
    _authService = authService;
    _httpContextAccessor = httpContextAccessor;
    _jsRuntime = jsRuntime;
  }

  public void Invalidate() => _snapshot = null;

  public async Task<bool> HasPageAsync(string menuPath)
  {
    UserPermissionSnapshot snapshot = await GetSnapshotAsync();
    if (!snapshot.UserId.HasValue)
    {
      return false;
    }

    if (MenuPathHelper.IsExemptPage(menuPath))
    {
      return true;
    }

    SysMenu? pageMenu = FindMenuByPath(snapshot.AllMenus, menuPath);
    if (pageMenu is null && MenuPathHelper.PathsEqual(menuPath, "/"))
    {
      pageMenu = FindMenuByPath(snapshot.AllMenus, "/admin");
    }

    if (pageMenu is null || !IsPageMenuType(pageMenu.Type))
    {
      return false;
    }

    return snapshot.AllowedMenuIds.Contains(pageMenu.Id);
  }

  public async Task<bool> HasButtonAsync(string menuPath, string buttonPath)
  {
    UserPermissionSnapshot snapshot = await GetSnapshotAsync();
    if (!snapshot.UserId.HasValue)
    {
      return false;
    }

    SysMenu? pageMenu = FindMenuByPath(snapshot.AllMenus, menuPath);
    if (pageMenu is null)
    {
      return false;
    }

    if (!snapshot.AllowedMenuIds.Contains(pageMenu.Id))
    {
      return false;
    }

    SysMenu? buttonMenu = snapshot.AllMenus.FirstOrDefault(menu =>
      menu.ParentId == pageMenu.Id
      && string.Equals(menu.Path, buttonPath, StringComparison.OrdinalIgnoreCase));

    return buttonMenu is not null && snapshot.AllowedMenuIds.Contains(buttonMenu.Id);
  }

  public async Task<bool> HasApiAsync(string apiGroupPath, string? actionName = null)
  {
    UserPermissionSnapshot snapshot = await GetSnapshotAsync();
    if (!snapshot.UserId.HasValue)
    {
      return false;
    }

    SysMenu? groupMenu = FindMenuByPath(snapshot.AllMenus, apiGroupPath);
    if (groupMenu is null)
    {
      return false;
    }

    if (string.IsNullOrWhiteSpace(actionName))
    {
      return snapshot.AllowedMenuIds.Contains(groupMenu.Id);
    }

    SysMenu? actionMenu = snapshot.AllMenus.FirstOrDefault(menu =>
      menu.ParentId == groupMenu.Id
      && string.Equals(menu.Path, actionName, StringComparison.OrdinalIgnoreCase));

    return actionMenu is not null && snapshot.AllowedMenuIds.Contains(actionMenu.Id);
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

  /// <summary>
  /// 当前用户可访问的导航菜单（含父级目录）。
  /// </summary>
  public async Task<List<SysMenu>> GetNavigationMenusAsync()
  {
    UserPermissionSnapshot snapshot = await GetSnapshotAsync();
    if (!snapshot.UserId.HasValue)
    {
      return [];
    }

    Dictionary<long, SysMenu> menuMap = snapshot.AllMenus.ToDictionary(menu => menu.Id);
    List<SysMenu> all = snapshot.AllMenus
      .Where(menu => !menu.Type.IsPermissionNode())
      .Where(menu => !menu.IsHidden)
      .Where(menu => !MenuService.HasHiddenAncestor(menu, menuMap))
      .OrderBy(menu => menu.Sort)
      .ThenBy(menu => menu.Id)
      .ToList();

    List<SysMenu> allowed = all
      .Where(menu => snapshot.AllowedMenuIds.Contains(menu.Id))
      .ToList();

    return MenuService.BuildTree(allowed);
  }

  public async Task<SysMenu?> TryGetPageMenuAsync(string menuPath)
  {
    UserPermissionSnapshot snapshot = await GetSnapshotAsync();
    SysMenu? menu = FindMenuByPath(snapshot.AllMenus, menuPath);
    if (menu is null || !IsPageMenuType(menu.Type))
    {
      return null;
    }

    if (!snapshot.AllowedMenuIds.Contains(menu.Id))
    {
      return null;
    }

    menu.Parent = snapshot.AllMenus.FirstOrDefault(parent => parent.Id == menu.ParentId);
    return menu;
  }

  private async Task<UserPermissionSnapshot> GetSnapshotAsync()
  {
    if (_snapshot is not null)
    {
      return _snapshot;
    }

    long? userId = await GetCurrentUserIdAsync();
    if (!userId.HasValue)
    {
      _snapshot = new UserPermissionSnapshot(null, false, [], []);
      return _snapshot;
    }

    List<SysMenu> allMenus = await _freeSql.Select<SysMenu>()
      .OrderBy(menu => menu.Sort)
      .OrderBy(menu => menu.Id)
      .ToListAsync();

    List<long> roleIds = await _freeSql.Select<SysRoleUser>()
      .Where(link => link.UserId == userId.Value)
      .ToListAsync(link => link.RoleId);

    bool isAdministrator = roleIds.Count > 0
      && await _freeSql.Select<SysRole>()
        .Where(role => roleIds.Contains(role.Id) && role.IsAdministrator)
        .AnyAsync();

    HashSet<long> allowedMenuIds;
    if (isAdministrator)
    {
      allowedMenuIds = allMenus.Select(menu => menu.Id).ToHashSet();
    }
    else if (roleIds.Count == 0)
    {
      allowedMenuIds = [];
    }
    else
    {
      List<long> grantedMenuIds = await _freeSql.Select<SysRoleMenu>()
        .Where(link => roleIds.Contains(link.RoleId))
        .ToListAsync(link => link.MenuId);

      allowedMenuIds = ExpandWithAncestors(allMenus, grantedMenuIds.ToHashSet());
    }

    _snapshot = new UserPermissionSnapshot(userId, isAdministrator, allowedMenuIds, allMenus);
    return _snapshot;
  }

  private static HashSet<long> ExpandWithAncestors(List<SysMenu> allMenus, HashSet<long> menuIds)
  {
    Dictionary<long, SysMenu> map = allMenus.ToDictionary(menu => menu.Id);
    HashSet<long> result = new(menuIds);

    foreach (long menuId in menuIds.ToList())
    {
      long parentId = map.TryGetValue(menuId, out SysMenu? menu) ? menu.ParentId : 0;
      while (parentId > 0)
      {
        result.Add(parentId);
        parentId = map.TryGetValue(parentId, out SysMenu? parent) ? parent.ParentId : 0;
      }
    }

    return result;
  }

  private static SysMenu? FindMenuByPath(IReadOnlyList<SysMenu> menus, string path)
  {
    string normalized = MenuPathHelper.NormalizePagePath(path);
    SysMenu? exact = menus.FirstOrDefault(menu =>
      string.Equals(MenuPathHelper.NormalizePagePath(menu.Path), normalized, StringComparison.Ordinal));
    if (exact is not null)
    {
      return exact;
    }

    return menus.FirstOrDefault(menu => MenuPathHelper.PathsEqual(menu.Path, path));
  }

  private static bool IsPageMenuType(SysMenuType type) =>
    type is SysMenuType.菜单 or SysMenuType.增删改查 or SysMenuType.外部连接;

  private async Task<long?> GetCurrentUserIdAsync()
  {
    string? token = GetBearerTokenFromHttpContext();
    if (string.IsNullOrWhiteSpace(token))
    {
      try
      {
        token = await _jsRuntime.InvokeAsync<string?>("neoAdminAuth.getToken");
      }
      catch (InvalidOperationException)
      {
        // 非 Blazor 交互上下文（如纯 API 请求）时忽略。
      }
      catch (JSException)
      {
        // JS 未就绪时忽略。
      }
    }

    if (string.IsNullOrWhiteSpace(token))
    {
      return null;
    }

    ApiResult<UserSummaryResponse> result = await _authService.CheckAsync(token);
    return result.Succeeded ? result.Data!.Id : null;
  }

  private string? GetBearerTokenFromHttpContext()
  {
    HttpContext? httpContext = _httpContextAccessor.HttpContext;
    if (httpContext is null)
    {
      return null;
    }

    string? authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authorization))
    {
      return null;
    }

    return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
      ? authorization[7..].Trim()
      : authorization.Trim();
  }

  private sealed record UserPermissionSnapshot(
    long? UserId,
    bool IsAdministrator,
    HashSet<long> AllowedMenuIds,
    List<SysMenu> AllMenus);
}
