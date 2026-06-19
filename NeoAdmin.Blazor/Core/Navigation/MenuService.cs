using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Core.Workflow;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Core.Navigation;

public sealed class MenuService
{
    private readonly IFreeSql freeSql;
    private readonly ILogger<MenuService> logger;

    public MenuService(IFreeSql freeSql, ILogger<MenuService> logger)
    {
        this.freeSql = freeSql;
        this.logger = logger;
    }

    public Task<List<SysMenu>> GetAllAsync() =>
        freeSql.Select<SysMenu>()
            .OrderBy(a => a.Sort)
            .OrderBy(a => a.Id)
            .ToListAsync();

    public async Task<List<SysMenu>> GetNavigationAsync()
    {
        List<SysMenu> menus = await GetAllAsync();
        Dictionary<long, SysMenu> map = menus.ToDictionary(a => a.Id);
        return menus
            .Where(a => !a.Type.IsPermissionNode())
            .Where(a => !a.IsHidden)
            .Where(a => !HasHiddenAncestor(a, map))
            .ToList();
    }

    public async Task<ApiResult<SysMenu>> SaveAsync(MenuEditModel model)
    {
        ApiResult? validationError = ValidateRequest(model);
        if (validationError is not null)
        {
            logger.LogWarning("保存菜单失败：{Message}，Label={Label}", validationError.Message, model.Label);
            return ApiResult<SysMenu>.Error(validationError.Message, validationError.Code);
        }

        Normalize(model);
        if (model.ParentId > 0 && !await freeSql.Select<SysMenu>().AnyAsync(a => a.Id == model.ParentId))
        {
            return ApiResult<SysMenu>.Error("父级菜单不存在");
        }

        if (model.Id > 0 && await IsDescendantAsync(model.ParentId, model.Id))
        {
            return ApiResult<SysMenu>.Error("不能把菜单移动到自身或子级下面");
        }

        SysMenu menu;
        if (model.Id == 0)
        {
            menu = new SysMenu();
            Apply(menu, model);
            await freeSql.Insert(menu).ExecuteAffrowsAsync();
        }
        else
        {
            menu = await freeSql.Select<SysMenu>().Where(a => a.Id == model.Id).FirstAsync()
                ?? throw new InvalidOperationException("菜单不存在");
            Apply(menu, model);
            await freeSql.Update<SysMenu>()
                .SetSource(menu)
                .IgnoreColumns(a => new { a.IsSystem, a.CreatedTime })
                .ExecuteAffrowsAsync();
        }

        if (model.Type == SysMenuType.增删改查)
        {
            SyncCrudButtons(freeSql, menu.Id, model.GenerateCrudButtons);
        }

        SyncAuditButtons(freeSql, menu.Id, model.AuditButtonPaths);

        logger.LogInformation("保存菜单成功：{EntityDesc}", EntityLogHelper.Describe(menu));
        return ApiResult<SysMenu>.Success(menu, "保存成功");
    }

    public async Task<ApiResult> DeleteAsync(long id)
    {
        SysMenu? menu = await freeSql.Select<SysMenu>().Where(a => a.Id == id).FirstAsync();
        if (menu is null)
        {
            logger.LogWarning("删除菜单失败：菜单不存在，Id={MenuId}", id);
            return ApiResult.Error("菜单不存在");
        }

        if (menu.IsSystem)
        {
            logger.LogWarning("删除菜单失败：不能删除系统菜单，{EntityDesc}", EntityLogHelper.Describe(menu));
            return ApiResult.Error("不能删除系统菜单");
        }

        List<SysMenu> all = await GetAllAsync();
        List<long> ids = CollectDescendantIds(all, id);
        if (all.Any(a => ids.Contains(a.Id) && a.IsSystem))
        {
            return ApiResult.Error("不能删除包含系统菜单的分支");
        }

        await freeSql.Delete<SysMenu>().Where(a => ids.Contains(a.Id)).ExecuteAffrowsAsync();
        logger.LogInformation("删除菜单成功：{EntityDesc}，共删除 {Count} 个节点", EntityLogHelper.Describe(menu), ids.Count);
        return ApiResult.Success("删除成功");
    }

    public async Task<int> GetNextSortAsync(long parentId)
    {
        int? maxSort = await freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == parentId)
            .MaxAsync(a => (int?)a.Sort);
        return (maxSort ?? 0) + 10;
    }

    public static List<SysMenu> BuildTree(IEnumerable<SysMenu> menus)
    {
        List<SysMenu> items = menus
            .OrderBy(a => a.Sort)
            .ThenBy(a => a.Id)
            .Select(CloneFlat)
            .ToList();
        Dictionary<long, SysMenu> map = items.ToDictionary(a => a.Id);
        List<SysMenu> roots = new();

        foreach (SysMenu item in items)
        {
            if (item.ParentId > 0 && map.TryGetValue(item.ParentId, out SysMenu? parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                roots.Add(item);
            }
        }

        return roots;
    }

    public static string NormalizePath(string path)
    {
        path = path.Trim();
        if (string.IsNullOrWhiteSpace(path) || path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return path.StartsWith('/') ? path : "/" + path;
    }

    public static void SyncCrudButtons(IFreeSql freeSql, long parentId, bool generate)
    {
        if (generate)
        {
            bool isSystem = freeSql.Select<SysMenu>().Where(a => a.Id == parentId).First(a => a.IsSystem);
            foreach (SysMenu template in CreateCrudButtonTemplates(parentId, isSystem))
            {
                EnsureCrudButton(freeSql, parentId, template, isSystem);
            }

            return;
        }

        List<long> removeIds = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == parentId && CrudButtonPaths.Contains(menu.Path))
            .ToList(menu => menu.Id);

        if (removeIds.Count == 0)
        {
            return;
        }

        freeSql.Delete<SysRoleMenu>().Where(link => removeIds.Contains(link.MenuId)).ExecuteAffrows();
        freeSql.Delete<SysMenu>().Where(menu => removeIds.Contains(menu.Id)).ExecuteAffrows();
    }

    private static readonly string[] CrudButtonPaths = ["add", "edit", "remove"];

    private static SysMenu[] CreateCrudButtonTemplates(long parentId, bool isSystem) =>
    [
        NewButton(parentId, "添加", "add", 301, isSystem),
        NewButton(parentId, "编辑", "edit", 302, isSystem),
        NewButton(parentId, "删除", "remove", 303, isSystem)
    ];

    private static void EnsureCrudButton(IFreeSql freeSql, long parentId, SysMenu template, bool isSystem)
    {
        SysMenu? existing = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == parentId && menu.Path == template.Path)
            .First();
        if (existing is null)
        {
            freeSql.Insert(template).ExecuteAffrows();
        }
        else if (existing.IsSystem != isSystem)
        {
            freeSql.Update<SysMenu>()
                .Where(menu => menu.Id == existing.Id)
                .Set(menu => menu.IsSystem, isSystem)
                .ExecuteAffrows();
        }
    }

    public static void EnsureAuditButtons(IFreeSql freeSql, long parentId) =>
        EnsureAuditButtons(
            freeSql,
            parentId,
            AuditMenuDefinitions.CreateAuditButtons().Select(a => a.Path));

    public static void EnsureAuditButtons(IFreeSql freeSql, long parentId, IEnumerable<string> paths)
    {
        HashSet<string> pathSet = paths.ToHashSet(StringComparer.Ordinal);
        bool isSystem = freeSql.Select<SysMenu>().Where(a => a.Id == parentId).First(a => a.IsSystem);
        foreach (SysMenu template in AuditMenuDefinitions.CreateAuditButtons().Where(a => pathSet.Contains(a.Path)))
        {
            EnsureAuditButton(freeSql, parentId, template, isSystem);
        }
    }

    /// <summary>按编辑页勾选结果创建/删除审批流按钮权限点。</summary>
    public static void SyncAuditButtons(IFreeSql freeSql, long parentId, IEnumerable<string> selectedPaths)
    {
        HashSet<string> pathSet = selectedPaths.ToHashSet(StringComparer.Ordinal);
        bool isSystem = freeSql.Select<SysMenu>().Where(a => a.Id == parentId).First(a => a.IsSystem);

        foreach (SysMenu template in AuditMenuDefinitions.CreateAuditButtons().Where(a => pathSet.Contains(a.Path)))
        {
            EnsureAuditButton(freeSql, parentId, template, isSystem);
        }

        List<long> removeIds = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == parentId
                && AuditMenuDefinitions.AllButtonPaths.Contains(menu.Path)
                && !pathSet.Contains(menu.Path))
            .ToList(menu => menu.Id);

        if (removeIds.Count == 0)
        {
            return;
        }

        freeSql.Delete<SysRoleMenu>().Where(link => removeIds.Contains(link.MenuId)).ExecuteAffrows();
        freeSql.Delete<SysMenu>().Where(menu => removeIds.Contains(menu.Id)).ExecuteAffrows();
    }

    private static void EnsureAuditButton(IFreeSql freeSql, long parentId, SysMenu template, bool isSystem)
    {
        SysMenu? existing = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == parentId && menu.Path == template.Path)
            .First();
        if (existing is null)
        {
            SysMenu button = NewButton(parentId, template.Label, template.Path, template.Sort, isSystem);
            freeSql.Insert(button).ExecuteAffrows();
        }
        else if (existing.IsSystem != isSystem)
        {
            freeSql.Update<SysMenu>()
                .Where(menu => menu.Id == existing.Id)
                .Set(menu => menu.IsSystem, isSystem)
                .ExecuteAffrows();
        }
    }

    private async Task<bool> IsDescendantAsync(long parentId, long id)
    {
        if (parentId == 0)
        {
            return false;
        }

        List<SysMenu> menus = await GetAllAsync();
        long currentId = parentId;
        while (currentId > 0)
        {
            if (currentId == id)
            {
                return true;
            }

            currentId = menus.FirstOrDefault(a => a.Id == currentId)?.ParentId ?? 0;
        }

        return false;
    }

    private static List<long> CollectDescendantIds(List<SysMenu> all, long id)
    {
        List<long> ids = [id];
        for (int index = 0; index < ids.Count; index++)
        {
            long parentId = ids[index];
            ids.AddRange(all.Where(a => a.ParentId == parentId).Select(a => a.Id));
        }

        return ids;
    }

    public static bool HasHiddenAncestor(SysMenu menu, IReadOnlyDictionary<long, SysMenu> map)
    {
        long parentId = menu.ParentId;
        while (parentId > 0)
        {
            if (!map.TryGetValue(parentId, out SysMenu? parent))
            {
                return false;
            }

            if (parent.IsHidden)
            {
                return true;
            }

            parentId = parent.ParentId;
        }

        return false;
    }

    private static void Normalize(MenuEditModel model)
    {
        model.Label = model.Label.Trim();
        model.Icon = string.IsNullOrWhiteSpace(model.Icon) ? "circle" : model.Icon.Trim();
        model.Path = NormalizePath(model.Path);
        model.Description = model.Description.Trim();

        if (model.Type.IsPermissionNode())
        {
            model.IsHidden = false;
            model.SidebarStyle = SysMenuSidebarStyle.收起;
        }
    }

    private static void Apply(SysMenu menu, MenuEditModel model)
    {
        menu.ParentId = model.ParentId;
        menu.Label = model.Label;
        menu.Icon = model.Icon;
        menu.Path = model.Path;
        menu.Sort = model.Sort;
        menu.Type = model.Type;
        menu.Description = model.Description;
        menu.SidebarStyle = model.SidebarStyle;
        menu.IsHidden = model.IsHidden;
    }

    private static SysMenu NewButton(long parentId, string label, string path, int sort, bool isSystem = true) => new()
    {
        ParentId = parentId,
        Label = label,
        Path = path,
        Icon = "dot",
        Sort = sort,
        Type = SysMenuType.按钮,
        SidebarStyle = SysMenuSidebarStyle.收起,
        IsSystem = isSystem
    };

    private static SysMenu CloneFlat(SysMenu menu) => new()
    {
        Id = menu.Id,
        ParentId = menu.ParentId,
        Label = menu.Label,
        Icon = menu.Icon,
        Path = menu.Path,
        Sort = menu.Sort,
        Type = menu.Type,
        Description = menu.Description,
        SidebarStyle = menu.SidebarStyle,
        IsSystem = menu.IsSystem,
        IsHidden = menu.IsHidden,
        CreatedTime = menu.CreatedTime
    };

    private static ApiResult? ValidateRequest(object request)
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            return null;
        }

        return ApiResult.Error(string.Join(", ", validationResults.Select(a => a.ErrorMessage)));
    }
}
