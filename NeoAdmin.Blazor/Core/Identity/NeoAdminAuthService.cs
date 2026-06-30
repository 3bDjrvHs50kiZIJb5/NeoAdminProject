using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FreeSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Core.Identity;

public sealed class NeoAdminAuthService
{
    private readonly IFreeSql freeSql;
    private readonly IDataProtector tokenProtector;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly LoginRateLimiter loginRateLimiter;
    private readonly ILogger<NeoAdminAuthService> logger;
    private readonly IDisabledAccountLoginMessageResolver? disabledAccountLoginMessageResolver;

    public NeoAdminAuthService(
        IFreeSql freeSql,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        LoginRateLimiter loginRateLimiter,
        ILogger<NeoAdminAuthService> logger,
        IDisabledAccountLoginMessageResolver? disabledAccountLoginMessageResolver = null)
    {
        this.freeSql = freeSql;
        tokenProtector = dataProtectionProvider.CreateProtector("NeoAdmin.Auth.Token.v1");
        this.httpContextAccessor = httpContextAccessor;
        this.loginRateLimiter = loginRateLimiter;
        this.logger = logger;
        this.disabledAccountLoginMessageResolver = disabledAccountLoginMessageResolver;
    }

    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            logger.LogWarning("登录失败：{Message}", validationError.Message);
            return ApiResult<LoginResponse>.Error(validationError.Message, validationError.Code);
        }

        string clientIp = IpHelper.GetClientIpAddress(httpContextAccessor.HttpContext, logger);
        if (loginRateLimiter.IsBlocked(clientIp, out string? blockedMessage))
        {
            logger.LogWarning("登录失败：IP 限流，ClientIp={ClientIp}", clientIp);
            return ApiResult<LoginResponse>.Error(blockedMessage!);
        }

        string username = request.Username.Trim();
        SysUser? user = await freeSql.Select<SysUser>()
            .Where(a => a.Username == username)
            .FirstAsync();

        if (user is null || !string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            int count = loginRateLimiter.RecordFailure(clientIp);
            await WriteLoginLogAsync(username, SysUserLoginLog.LogType.登陆失败, $"failed:{count}");
            logger.LogWarning("登录失败：用户名或密码错误，Username={Username}，ClientIp={ClientIp}，Count={Count}",
                username, clientIp, count);
            return ApiResult<LoginResponse>.Error($"用户名或密码错误，当前限制次数：{count}");
        }

        if (!user.IsEnabled)
        {
            string message = await ResolveDisabledAccountLoginMessageAsync(user);
            await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆失败, "账户未开通");
            logger.LogWarning("登录失败：账户未开通，Username={Username}", user.Username);
            return ApiResult<LoginResponse>.Error(message);
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
        AuthTokenPayload? payload = TryParseToken(token);
        if (payload is null)
        {
            return ApiResult<UserSummaryResponse>.Error("未登录或登录已过期", 401);
        }

        SysUser? user = await freeSql.Select<SysUser>()
            .Where(a => a.Id == payload.UserId)
            .FirstAsync();

        if (user is null || !user.IsEnabled)
        {
            return ApiResult<UserSummaryResponse>.Error("未登录或登录已过期", 401);
        }

        if (payload.LoginTime is null || !LoginTimesMatch(payload.LoginTime.Value, user.LoginTime))
        {
            logger.LogInformation(
                "登录失效：Token 与当前会话不一致（可能已在其他地方登录），UserId={UserId}",
                user.Id);
            return ApiResult<UserSummaryResponse>.Error("账号已在其他地方登录，请重新登录", 401);
        }

        return ApiResult<UserSummaryResponse>.Success(ToSummary(user));
    }

    /// <summary>
    /// 为已登录用户签发 API Token。
    /// </summary>
    public string CreateToken(SysUser user) => BuildToken(user);

    private string BuildToken(SysUser user) =>
        tokenProtector.Protect($"{user.Id}|{user.LoginTime:O}");

    private AuthTokenPayload? TryParseToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            string value = tokenProtector.Unprotect(token);
            string[] parts = value.Split('|', 2, StringSplitOptions.None);
            if (parts.Length == 0 || !long.TryParse(parts[0], out long userId))
            {
                return null;
            }

            if (parts.Length < 2
                || !DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime loginTime))
            {
                return null;
            }

            return new AuthTokenPayload(userId, loginTime);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse NeoAdmin auth token.");
            return null;
        }
    }

    /// <summary>
    /// 比对 Token 内嵌的登录时间与数据库记录（允许毫秒级往返误差）。
    /// </summary>
    private static bool LoginTimesMatch(DateTime tokenLoginTime, DateTime dbLoginTime)
    {
        if (dbLoginTime == default)
        {
            return false;
        }

        return Math.Abs((tokenLoginTime - dbLoginTime).TotalSeconds) < 1;
    }

    private sealed record AuthTokenPayload(long UserId, DateTime? LoginTime);

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
            Ip = IpHelper.GetClientIpAddress(httpContext, logger),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty
        }).ExecuteAffrowsAsync();
    }

    private async Task<string> ResolveDisabledAccountLoginMessageAsync(SysUser user)
    {
        if (disabledAccountLoginMessageResolver is not null)
        {
            string? customMessage = await disabledAccountLoginMessageResolver.TryResolveAsync(user);
            if (!string.IsNullOrWhiteSpace(customMessage))
            {
                return customMessage;
            }
        }

        return "账户已被禁用";
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
