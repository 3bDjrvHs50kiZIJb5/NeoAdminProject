using System.Security.Claims;
using NoAdmin.Blazor.Infrastructure.Encrypt;
using BootstrapBlazor.Components;
using FreeScheduler;
using FreeSql;
using Microsoft.AspNetCore.Mvc;

namespace NovaAdmin.API;

[ApiController]
[Consumes("application/json")]
public abstract class BaseService : ControllerBase
{
    protected readonly FreeSqlCloud _freeSqlCloud;
    protected readonly Scheduler _scheduler;
    protected readonly ILogger _logger;
    protected readonly NovaAdminContext _adminContext;
    protected readonly IConfiguration _configuration;
    protected readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// 初始化基类
    /// </summary>
    protected BaseService(
        FreeSqlCloud freeSqlCloud,
        Scheduler scheduler,
        ILogger logger,
        NovaAdminContext adminContext,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        _freeSqlCloud = freeSqlCloud;
        _scheduler = scheduler;
        _logger = logger;
        _adminContext = adminContext;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    protected IFreeSql CurrentOrm => _adminContext.Orm ?? _freeSqlCloud.Use("main");

    protected bool IsDevelopment => _webHostEnvironment.IsDevelopment();

    /// <summary>
    /// 获取客户端IP
    /// </summary>
    protected string GetClientIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
    }

    /// <summary>
    /// 提取访问令牌
    /// </summary>
    protected string? GetBearerToken(string? token = null)
    {
        token ??= HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? token[7..].Trim()
            : token.Trim();
    }

    /// <summary>
    /// 获取当前用户
    /// </summary>
    protected async Task<long?> GetCurrentUserIdAsync(string? token = null)
    {
        token = GetBearerToken(token);

        if (string.IsNullOrWhiteSpace(token))
        {
            return IsDevelopment
                ? await CurrentOrm.Select<SysUser>().OrderBy(a => a.Id).FirstAsync<long?>(a => a.Id)
                : null;
        }

        try
        {
            var decrypted = DesEncrypt.Decrypt(token);
            var parts = decrypted.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1 || !long.TryParse(parts[0], out var userId))
            {
                return null;
            }

            var exists = await CurrentOrm.Select<SysUser>().AnyAsync(a => a.Id == userId);
            return exists ? userId : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse auth token.");
            return null;
        }
    }

    /// <summary>
    /// 查询当前账号
    /// </summary>
    protected async Task<SysUser?> GetCurrentUserAsync(string? token = null)
    {
        var userId = await GetCurrentUserIdAsync(token);
        if (!userId.HasValue)
        {
            return null;
        }

        return await CurrentOrm.Select<SysUser>()
            .IncludeMany(a => a.Roles)
            .Where(a => a.Id == userId.Value)
            .FirstAsync();
    }

    /// <summary>
    /// 写入登录日志
    /// </summary>
    protected async Task WriteLoginLogAsync(
        string username,
        SysUserLoginLog.LogType type,
        string? extra = null,
        string? browser = null)
    {
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        await CurrentOrm.Insert(new SysUserLoginLog
        {
            Username = username,
            Type = type,
            Extra = extra ?? string.Empty,
            Ip = GetClientIpAddress(),
            City = string.Empty,
            Browser = browser ?? "API",
            OS = string.Empty,
            Device = default,
            Language = HttpContext.Request.Headers.AcceptLanguage.ToString(),
            UserAgent = userAgent,
            Engine = string.Empty
        }).ExecuteAffrowsAsync();
    }

    /// <summary>
    /// 生成登录令牌
    /// </summary>
    protected string BuildToken(SysUser user)
    {
        var fingerprint = HttpContext.Request.Headers["Fingerprint"].FirstOrDefault() ?? string.Empty;
        return DesEncrypt.Encrypt($"{user.Id}|{user.LoginTime:yyyy-MM-dd HH:mm:ss}|{fingerprint}");
    }

    /// <summary>
    /// 获取显示名
    /// </summary>
    protected static string DisplayName(SysUser user) =>
        !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;
}
