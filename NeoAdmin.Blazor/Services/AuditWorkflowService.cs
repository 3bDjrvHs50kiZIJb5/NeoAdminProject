using System.Text.Json;
using FreeSql;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Audit;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Menus;

namespace NeoAdmin.Blazor.Services;

public sealed class AuditWorkflowService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IFreeSql _freeSql;
    private readonly MenuPermissionService _permissionService;
    private readonly NeoAdminAuthService _authService;
    private readonly IJSRuntime _jsRuntime;

    public AuditWorkflowService(
        IFreeSql freeSql,
        MenuPermissionService permissionService,
        NeoAdminAuthService authService,
        IJSRuntime jsRuntime)
    {
        _freeSql = freeSql;
        _permissionService = permissionService;
        _authService = authService;
        _jsRuntime = jsRuntime;
    }

    public string GetTableName(Type entityType) =>
        _freeSql.CodeFirst.GetTableByEntity(entityType).DbName;

    public async Task<AuditMenuContext> LoadMenuContextAsync(string menuPath)
    {
        string normalized = MenuService.NormalizePath(menuPath);
        SysMenu? pageMenu = await _freeSql.Select<SysMenu>()
            .Where(a => a.Path == normalized)
            .FirstAsync();
        if (pageMenu is null)
        {
            return new AuditMenuContext();
        }

        List<SysMenu> allButtons = await _freeSql.Select<SysMenu>()
            .Where(a => a.ParentId == pageMenu.Id && a.Type == SysMenuType.按钮 && a.IsSystem)
            .OrderBy(a => a.Sort)
            .ToListAsync();

        List<SysMenu> auditAll = allButtons
            .Where(a => a.Path.StartsWith("audit_", StringComparison.Ordinal))
            .ToList();

        List<SysMenu> steps = auditAll
            .Where(a => a.Path is not ("audit_00" or "audit_98" or "audit_99"))
            .ToList();

        return new AuditMenuContext
        {
            PageMenuId = pageMenu.Id,
            MenuPath = normalized,
            AuditAllButtons = auditAll,
            StepButtons = steps,
            HasEditVersion = allButtons.Any(a => a.Path == "edit_version")
        };
    }

    public async Task<List<SysAuditLog>> GetWorkflowLogsAsync(string tableName, long tableId) =>
        await _freeSql.Select<SysAuditLog>()
            .Where(a => a.TableName == tableName && a.TableId == tableId)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync();

    public async Task<List<SysAuditEntityLog>> GetEntityLogsAsync(string tableName, long tableId) =>
        await _freeSql.Select<SysAuditEntityLog>()
            .Where(a => a.TableName == tableName && a.TableId == tableId)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync();

    public async Task<ApiResult<EntityAudited>> SubmitAsync(
        EntityAudited item,
        Type entityType,
        string menuPath,
        AuditMenuContext menu,
        string opinion)
    {
        string tableName = _freeSql.CodeFirst.GetTableByEntity(entityType).DbName;
        SysAuditLog? auditLog = null;
        string actionName = "提交";

        if (item.AuditStatus is SysAuditStatus.待提交 or SysAuditStatus.退回)
        {
            if (!await _permissionService.HasButtonAsync(menuPath, "audit_00"))
            {
                return ApiResult<EntityAudited>.Error("您没有权限进行此操作");
            }

            auditLog = new SysAuditLog
            {
                TableName = tableName,
                TableId = item.Id,
                Step = string.IsNullOrWhiteSpace(item.AuditStep) ? "audit_00" : item.AuditStep,
                Opinion = opinion,
                Result = SysAuditStatus.通过,
                NextStep = menu.StepButtons.FirstOrDefault()?.Path ?? string.Empty
            };
        }
        else if (item.AuditStatus == SysAuditStatus.审核中)
        {
            if (!await _permissionService.HasButtonAsync(menuPath, item.AuditStep))
            {
                return ApiResult<EntityAudited>.Error("您没有权限进行此操作");
            }

            actionName = "审核";
            int index = menu.StepButtons.FindIndex(a => a.Path == item.AuditStep);
            auditLog = new SysAuditLog
            {
                TableName = tableName,
                TableId = item.Id,
                Step = item.AuditStep,
                Opinion = opinion,
                Result = SysAuditStatus.通过,
                NextStep = index >= 0 && index + 1 < menu.StepButtons.Count
                    ? menu.StepButtons[index + 1].Path
                    : string.Empty
            };
        }

        if (auditLog is null)
        {
            return ApiResult<EntityAudited>.Error("当前状态无法提交或审核");
        }

        return await ApplyWorkflowChangeAsync(
            item,
            entityType,
            tableName,
            auditLog,
            string.IsNullOrEmpty(auditLog.NextStep) ? SysAuditStatus.通过 : SysAuditStatus.审核中,
            auditLog.NextStep,
            actionName);
    }

    public async Task<ApiResult<EntityAudited>> BackAsync(
        EntityAudited item,
        Type entityType,
        string menuPath,
        AuditMenuContext menu,
        string opinion)
    {
        if (string.IsNullOrWhiteSpace(opinion))
        {
            return ApiResult<EntityAudited>.Error("处理意见不能为空");
        }

        if (!await _permissionService.HasButtonAsync(menuPath, "audit_99"))
        {
            return ApiResult<EntityAudited>.Error("您没有权限进行此操作");
        }

        string tableName = _freeSql.CodeFirst.GetTableByEntity(entityType).DbName;
        SysAuditLog? auditLog = null;
        SysAuditStatus backStatus = SysAuditStatus.退回;
        string actionName = "退回";

        if (item.AuditStatus == SysAuditStatus.审核中)
        {
            int index = menu.StepButtons.FindIndex(a => a.Path == item.AuditStep);
            if (index >= 1)
            {
                backStatus = SysAuditStatus.审核中;
            }

            auditLog = new SysAuditLog
            {
                TableName = tableName,
                TableId = item.Id,
                Step = item.AuditStep,
                Opinion = opinion,
                Result = SysAuditStatus.退回,
                NextStep = index - 1 >= 0 ? menu.StepButtons[index - 1].Path : "audit_00"
            };
        }
        else if (item.AuditStatus is SysAuditStatus.通过 or SysAuditStatus.拒绝)
        {
            actionName = "反审";
            auditLog = new SysAuditLog
            {
                TableName = tableName,
                TableId = item.Id,
                Step = item.AuditStatus == SysAuditStatus.通过 ? string.Empty : item.AuditStep,
                Opinion = opinion,
                Result = SysAuditStatus.退回,
                NextStep = "audit_00"
            };
        }

        if (auditLog is null)
        {
            return ApiResult<EntityAudited>.Error("当前状态无法退回");
        }

        return await ApplyWorkflowChangeAsync(item, entityType, tableName, auditLog, backStatus, auditLog.NextStep, actionName);
    }

    public async Task<ApiResult<EntityAudited>> RefuseAsync(
        EntityAudited item,
        Type entityType,
        string menuPath,
        string opinion)
    {
        if (string.IsNullOrWhiteSpace(opinion))
        {
            return ApiResult<EntityAudited>.Error("处理意见不能为空");
        }

        if (!await _permissionService.HasButtonAsync(menuPath, "audit_98"))
        {
            return ApiResult<EntityAudited>.Error("您没有权限进行此操作");
        }

        if (item.AuditStatus != SysAuditStatus.审核中)
        {
            return ApiResult<EntityAudited>.Error("当前状态无法拒绝");
        }

        string tableName = _freeSql.CodeFirst.GetTableByEntity(entityType).DbName;
        SysAuditLog auditLog = new()
        {
            TableName = tableName,
            TableId = item.Id,
            Step = item.AuditStep,
            Opinion = opinion,
            Result = SysAuditStatus.拒绝,
            NextStep = "audit_98"
        };

        return await ApplyWorkflowChangeAsync(
            item,
            entityType,
            tableName,
            auditLog,
            SysAuditStatus.拒绝,
            auditLog.NextStep,
            "拒绝");
    }

    public async Task WriteEntityLogAsync<T>(
        T item,
        string logType,
        T? oldItem = null,
        CancellationToken cancellationToken = default)
        where T : EntityAudited
    {
        string tableName = _freeSql.CodeFirst.GetTableByEntity(typeof(T)).DbName;
        SysAuditEntityLog log = new()
        {
            TableName = tableName,
            TableId = item.Id,
            LogType = logType,
            Data = JsonSerializer.Serialize(item, JsonOptions)
        };

        if (oldItem is not null)
        {
            log.DataOld = JsonSerializer.Serialize(oldItem, JsonOptions);
        }

        long? userId = await GetCurrentUserIdAsync();
        string? userName = await GetCurrentUserNameAsync();
        if (userId.HasValue)
        {
            log.CreatedUserId = userId;
            log.CreatedUserName = userName ?? string.Empty;
            log.CreatedTime = DateTime.Now;
        }

        await _freeSql.Insert(log).ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task<bool> TryBumpVersionForEditAsync<T>(T item, CancellationToken cancellationToken = default)
        where T : EntityAudited
    {
        string tableName = _freeSql.CodeFirst.GetTableByEntity(typeof(T)).DbName;
        int rows = await _freeSql.Update<EntityAudited>()
            .AsTable(tableName)
            .Where(a => a.Id == item.Id && a.AuditVersion == item.AuditVersion)
            .Set(a => a.AuditVersion + 1)
            .ExecuteAffrowsAsync(cancellationToken);

        if (rows == 1)
        {
            item.AuditVersion++;
            return true;
        }

        return false;
    }

    private async Task<ApiResult<EntityAudited>> ApplyWorkflowChangeAsync(
        EntityAudited item,
        Type entityType,
        string tableName,
        SysAuditLog auditLog,
        SysAuditStatus nextStatus,
        string nextStep,
        string actionName)
    {
        await FillAuditLogUserAsync(auditLog);

        using IUnitOfWork uow = _freeSql.CreateUnitOfWork();
        try
        {
            int rows = await uow.Orm.Update<EntityAudited>()
                .AsTable(tableName)
                .Where(a => a.Id == item.Id && a.AuditVersion == item.AuditVersion)
                .Set(a => a.AuditStatus, nextStatus)
                .Set(a => a.AuditStep, nextStep)
                .Set(a => a.AuditVersion + 1)
                .ExecuteAffrowsAsync();

            if (rows != 1)
            {
                uow.Rollback();
                return ApiResult<EntityAudited>.Error($"{actionName}失败，数据已被他人修改，请刷新后重试");
            }

            await uow.Orm.Insert(auditLog).ExecuteAffrowsAsync();
            uow.Commit();

            item.AuditStatus = nextStatus;
            item.AuditStep = nextStep;
            item.AuditVersion++;

            return ApiResult<EntityAudited>.Success(item, $"{actionName}成功");
        }
        catch
        {
            uow.Rollback();
            throw;
        }
    }

    private async Task FillAuditLogUserAsync(SysAuditLog auditLog)
    {
        long? userId = await GetCurrentUserIdAsync();
        string? userName = await GetCurrentUserNameAsync();
        auditLog.CreatedUserId = userId;
        auditLog.CreatedUserName = userName ?? string.Empty;
        auditLog.CreatedTime = DateTime.Now;
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

    private async Task<string?> GetCurrentUserNameAsync()
    {
        string? token = await _jsRuntime.InvokeAsync<string?>("neoAdminAuth.getToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        ApiResult<UserSummaryResponse> result = await _authService.CheckAsync(token);
        return result.Succeeded ? result.Data!.Username : null;
    }
}

public sealed class AuditMenuContext
{
    public long PageMenuId { get; init; }
    public string MenuPath { get; init; } = string.Empty;
    public List<SysMenu> AuditAllButtons { get; init; } = [];
    public List<SysMenu> StepButtons { get; init; } = [];
    public bool HasEditVersion { get; init; }

    public string? GetStepLabel(string stepPath)
    {
        if (string.IsNullOrEmpty(stepPath))
        {
            return "结束";
        }

        if (stepPath == "audit_00")
        {
            return "提交";
        }

        if (stepPath == "audit_98")
        {
            return "拒绝";
        }

        return StepButtons.Find(a => a.Path == stepPath)?.Label ?? stepPath;
    }
}
