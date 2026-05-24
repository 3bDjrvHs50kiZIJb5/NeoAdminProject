using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Services;

public sealed class OrgService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<OrgService> _logger;

    public OrgService(IFreeSql freeSql, ILogger<OrgService> logger)
    {
        _freeSql = freeSql;
        _logger = logger;
    }

    public Task<List<SysOrg>> GetAllAsync() =>
        _freeSql.Select<SysOrg>()
            .OrderBy(a => a.Sort)
            .OrderBy(a => a.Id)
            .ToListAsync();

    public async Task<ApiResult<SysOrg>> SaveAsync(SysOrg model)
    {
        ApiResult? validationError = Validate(model);
        if (validationError is not null)
        {
            _logger.LogWarning("保存组织失败：{Message}，{EntityDesc}", validationError.Message, EntityLogHelper.Describe(model));
            return ApiResult<SysOrg>.Error(validationError.Message, validationError.Code);
        }

        model.Label = model.Label.Trim();
        model.Description = model.Description?.Trim() ?? string.Empty;

        if (model.ParentId > 0 && !await _freeSql.Select<SysOrg>().AnyAsync(a => a.Id == model.ParentId))
        {
            return ApiResult<SysOrg>.Error("父组织不存在");
        }

        if (model.Id > 0 && await IsDescendantAsync(model.ParentId, model.Id))
        {
            return ApiResult<SysOrg>.Error("不能把组织移动到自身或子级下面");
        }

        if (model.Id == 0)
        {
            await _freeSql.Insert(model).ExecuteAffrowsAsync();
        }
        else
        {
            await _freeSql.Update<SysOrg>()
                .SetSource(model)
                .ExecuteAffrowsAsync();
        }

        _logger.LogInformation("保存组织成功：{EntityDesc}", EntityLogHelper.Describe(model));
        return ApiResult<SysOrg>.Success(model, "保存成功");
    }

    public async Task<ApiResult> DeleteAsync(long id)
    {
        if (!await _freeSql.Select<SysOrg>().AnyAsync(a => a.Id == id))
        {
            _logger.LogWarning("删除组织失败：组织不存在，Id={OrgId}", id);
            return ApiResult.Error("组织不存在");
        }

        List<SysOrg> all = await GetAllAsync();
        List<long> ids = CollectDescendantIds(all, id);
        await _freeSql.Delete<SysOrg>().Where(a => ids.Contains(a.Id)).ExecuteAffrowsAsync();
        _logger.LogInformation("删除组织成功：Id={OrgId}，共删除 {Count} 个节点", id, ids.Count);
        return ApiResult.Success("删除成功");
    }

    public async Task<int> GetNextSortAsync(long parentId)
    {
        int? maxSort = await _freeSql.Select<SysOrg>()
            .Where(a => a.ParentId == parentId)
            .MaxAsync(a => (int?)a.Sort);
        return (maxSort ?? 0) + 1;
    }

    public static List<SysOrg> BuildTree(IEnumerable<SysOrg> orgs)
    {
        List<SysOrg> items = orgs
            .OrderBy(a => a.Sort)
            .ThenBy(a => a.Id)
            .Select(CloneFlat)
            .ToList();
        Dictionary<long, SysOrg> map = items.ToDictionary(a => a.Id);
        List<SysOrg> roots = new();

        foreach (SysOrg item in items)
        {
            if (item.ParentId > 0 && map.TryGetValue(item.ParentId, out SysOrg? parent))
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

    private async Task<bool> IsDescendantAsync(long parentId, long id)
    {
        if (parentId == 0)
        {
            return false;
        }

        List<SysOrg> orgs = await GetAllAsync();
        long currentId = parentId;
        while (currentId > 0)
        {
            if (currentId == id)
            {
                return true;
            }

            currentId = orgs.FirstOrDefault(a => a.Id == currentId)?.ParentId ?? 0;
        }

        return false;
    }

    private static List<long> CollectDescendantIds(List<SysOrg> all, long id)
    {
        List<long> ids = [id];
        for (int index = 0; index < ids.Count; index++)
        {
            long parentId = ids[index];
            ids.AddRange(all.Where(a => a.ParentId == parentId).Select(a => a.Id));
        }

        return ids;
    }

    private static SysOrg CloneFlat(SysOrg org) => new()
    {
        Id = org.Id,
        ParentId = org.ParentId,
        Label = org.Label,
        Type = org.Type,
        Sort = org.Sort,
        IsEnabled = org.IsEnabled,
        Description = org.Description
    };

    private static ApiResult? Validate(SysOrg org)
    {
        var validationContext = new ValidationContext(org);
        var validationResults = new List<ValidationResult>();
        if (Validator.TryValidateObject(org, validationContext, validationResults, true))
        {
            return null;
        }

        return ApiResult.Error(string.Join(", ", validationResults.Select(a => a.ErrorMessage)));
    }
}
