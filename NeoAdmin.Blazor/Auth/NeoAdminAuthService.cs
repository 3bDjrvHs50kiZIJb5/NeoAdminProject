using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Auth;

public sealed class NeoAdminAuthService
{
    private readonly IFreeSql freeSql;
    private readonly IDataProtector tokenProtector;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<NeoAdminAuthService> logger;

    public NeoAdminAuthService(
        IFreeSql freeSql,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<NeoAdminAuthService> logger)
    {
        this.freeSql = freeSql;
        tokenProtector = dataProtectionProvider.CreateProtector("NeoAdmin.Auth.Token.v1");
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            logger.LogWarning("登录失败：{Message}", validationError.Message);
            return ApiResult<LoginResponse>.Error(validationError.Message, validationError.Code);
        }

        string username = request.Username.Trim();
        SysUser? user = await freeSql.Select<SysUser>()
            .Where(a => a.Username == username)
            .FirstAsync();

        if (user is null || !string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            await WriteLoginLogAsync(username, SysUserLoginLog.LogType.登陆失败, "用户名或密码错误");
            logger.LogWarning("登录失败：用户名或密码错误，Username={Username}", username);
            return ApiResult<LoginResponse>.Error("用户名或密码错误");
        }

        if (!user.IsEnabled)
        {
            await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆失败, "账户已禁用");
            logger.LogWarning("登录失败：账户已禁用，Username={Username}", user.Username);
            return ApiResult<LoginResponse>.Error("账户已被禁用");
        }

        user.LoginTime = DateTime.Now;
        await freeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.LoginTime, user.LoginTime)
            .ExecuteAffrowsAsync();

        await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆成功);
        logger.LogInformation("登录成功：UserId={UserId}，Username={Username}", user.Id, user.Username);

        return ApiResult<LoginResponse>.Success(new LoginResponse
        {
            Token = BuildToken(user),
            User = ToSummary(user)
        }, "登录成功");
    }

    public async Task<ApiResult<UserSummaryResponse>> CheckAsync(string? token)
    {
        long? userId = TryReadUserId(token);
        if (!userId.HasValue)
        {
            return ApiResult<UserSummaryResponse>.Error("未登录或登录已过期", 401);
        }

        SysUser? user = await freeSql.Select<SysUser>()
            .Where(a => a.Id == userId.Value)
            .FirstAsync();

        if (user is null || !user.IsEnabled)
        {
            return ApiResult<UserSummaryResponse>.Error("未登录或登录已过期", 401);
        }

        return ApiResult<UserSummaryResponse>.Success(ToSummary(user));
    }

    /// <summary>
    /// 为已登录用户签发 API Token。
    /// </summary>
    public string CreateToken(SysUser user) => BuildToken(user);

    private string BuildToken(SysUser user) =>
        tokenProtector.Protect($"{user.Id}|{user.LoginTime:O}");

    private long? TryReadUserId(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            string value = tokenProtector.Unprotect(token);
            string[] parts = value.Split('|', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 && long.TryParse(parts[0], out long userId) ? userId : null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse NeoAdmin auth token.");
            return null;
        }
    }

    private async Task WriteLoginLogAsync(
        string username,
        SysUserLoginLog.LogType type,
        string extra = "")
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        await freeSql.Insert(new SysUserLoginLog
        {
            Username = username,
            Type = type,
            Extra = extra,
            Ip = GetClientIpAddress(httpContext),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty
        }).ExecuteAffrowsAsync();
    }

    private static string GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return string.Empty;
        }

        string? forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
    }

    private static UserSummaryResponse ToSummary(SysUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Nickname = user.Nickname,
        IsEnabled = user.IsEnabled,
        LoginTime = user.LoginTime
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
