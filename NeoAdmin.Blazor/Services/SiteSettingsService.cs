using FreeSql;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.SeedData;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Services;

public sealed class SiteSettingsService
{
    private readonly IFreeSql _freeSql;
    private readonly ILogger<SiteSettingsService> _logger;
    private SysSiteSettings? _cached;

    public event Action? SettingsChanged;

    public SiteSettingsService(IFreeSql freeSql, ILogger<SiteSettingsService> logger)
    {
        _freeSql = freeSql;
        _logger = logger;
    }

    public async Task<SysSiteSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        SysSiteSettings? settings = await _freeSql.Select<SysSiteSettings>()
            .OrderBy(a => a.Id)
            .FirstAsync(cancellationToken);

        if (settings is null)
        {
            SiteSettingsSeedData.Ensure(_freeSql);
            settings = await _freeSql.Select<SysSiteSettings>()
                .OrderBy(a => a.Id)
                .FirstAsync(cancellationToken);
        }

        _cached = settings ?? new SysSiteSettings
        {
            Title = "NeoAdmin",
            IsEnabled = true
        };

        return _cached;
    }

    public async Task<ApiResult<SysSiteSettings>> SaveAsync(
        SysSiteSettings settings,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.Title))
        {
            _logger.LogWarning("保存站点设置失败：站点标题为空");
            return ApiResult<SysSiteSettings>.Error("请填写站点标题");
        }

        SysSiteSettings? existing = await _freeSql.Select<SysSiteSettings>()
            .Where(a => a.Id == settings.Id)
            .FirstAsync(cancellationToken);

        if (existing is null)
        {
            await _freeSql.Insert(settings).ExecuteAffrowsAsync(cancellationToken);
        }
        else
        {
            await _freeSql.Update<SysSiteSettings>()
                .Where(a => a.Id == settings.Id)
                .Set(a => a.Title, settings.Title)
                .Set(a => a.Host, settings.Host)
                .Set(a => a.Host2, settings.Host2)
                .Set(a => a.Host3, settings.Host3)
                .Set(a => a.Description, settings.Description)
                .Set(a => a.Logo, settings.Logo)
                .Set(a => a.LoginImage, settings.LoginImage)
                .Set(a => a.IsEnabled, settings.IsEnabled)
                .ExecuteAffrowsAsync(cancellationToken);
        }

        _cached = settings;
        SettingsChanged?.Invoke();
        _logger.LogInformation("保存站点设置成功：{EntityDesc}", EntityLogHelper.Describe(settings));
        return ApiResult<SysSiteSettings>.Success(settings, "保存成功");
    }

    public async Task<ApiResult> ChangeAdministratorPasswordAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("修改管理员密码失败：账号或密码为空");
            return ApiResult.Error("账号或密码不能为空");
        }

        SysUser? user = await FindAdministratorByUsernameAsync(username.Trim(), cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("修改管理员密码失败：未找到管理员账号，Username={Username}", username);
            return ApiResult.Error("未找到该管理员账号");
        }

        await _freeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.Password, password)
            .Set(a => a.LoginTime, DateTime.Now)
            .ExecuteAffrowsAsync(cancellationToken);

        _logger.LogInformation("修改管理员密码成功：Username={Username}", user.Username);
        return ApiResult.Success("密码修改成功");
    }

    public async Task<string> GetAdministratorUsernameAsync(CancellationToken cancellationToken = default)
    {
        SysUser? user = await FindFirstAdministratorAsync(cancellationToken);
        return user?.Username ?? string.Empty;
    }

    private async Task<SysUser?> FindAdministratorByUsernameAsync(
        string username,
        CancellationToken cancellationToken)
    {
        long? adminRoleId = await _freeSql.Select<SysRole>()
            .Where(a => a.IsAdministrator)
            .FirstAsync(a => (long?)a.Id, cancellationToken);

        if (!adminRoleId.HasValue)
        {
            return null;
        }

        List<long> adminUserIds = await _freeSql.Select<SysRoleUser>()
            .Where(a => a.RoleId == adminRoleId.Value)
            .ToListAsync(a => a.UserId, cancellationToken);

        if (adminUserIds.Count == 0)
        {
            return null;
        }

        return await _freeSql.Select<SysUser>()
            .Where(a => adminUserIds.Contains(a.Id) && a.Username == username)
            .FirstAsync(cancellationToken);
    }

    private async Task<SysUser?> FindFirstAdministratorAsync(CancellationToken cancellationToken)
    {
        long? adminRoleId = await _freeSql.Select<SysRole>()
            .Where(a => a.IsAdministrator)
            .FirstAsync(a => (long?)a.Id, cancellationToken);

        if (!adminRoleId.HasValue)
        {
            return null;
        }

        long? userId = await _freeSql.Select<SysRoleUser>()
            .Where(a => a.RoleId == adminRoleId.Value)
            .OrderBy(a => a.UserId)
            .FirstAsync(a => (long?)a.UserId, cancellationToken);

        if (!userId.HasValue)
        {
            return null;
        }

        return await _freeSql.Select<SysUser>()
            .Where(a => a.Id == userId.Value)
            .FirstAsync(cancellationToken);
    }

    public void InvalidateCache()
    {
        _cached = null;
    }

    public static string JoinHosts(SysSiteSettings settings) =>
        string.Join(", ", new[] { settings.Host, settings.Host2, settings.Host3 }
            .Where(a => !string.IsNullOrWhiteSpace(a)));

    public static void ApplyHosts(SysSiteSettings settings, string hostText)
    {
        string[] hosts = hostText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToArray();

        settings.Host = hosts.Length > 0 ? hosts[0] : string.Empty;
        settings.Host2 = hosts.Length > 1 ? hosts[1] : string.Empty;
        settings.Host3 = hosts.Length > 2 ? hosts[2] : string.Empty;
    }
}
