using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Api;

/// <summary>
/// API 控制器基类，提供 FreeSql、鉴权与常用 HTTP 辅助方法。
/// </summary>
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IFreeSql FreeSql { get; }

    protected NeoAdminAuthService Auth { get; }

    protected ILogger Logger { get; }

    protected BaseApiController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        ILogger logger)
    {
        FreeSql = freeSql;
        Auth = auth;
        Logger = logger;
    }

    /// <summary>
    /// 获取客户端 IP。
    /// </summary>
    protected string GetClientIpAddress()
    {
        string? forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
    }

    /// <summary>
    /// 从 Authorization 头提取 Bearer Token。
    /// </summary>
    protected string? GetBearerToken(string? token = null)
    {
        token ??= HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? token[7..].Trim()
            : token.Trim();
    }

    /// <summary>
    /// 校验当前登录态。
    /// </summary>
    protected Task<ApiResult<UserSummaryResponse>> CheckCurrentUserAsync() =>
        Auth.CheckAsync(GetBearerToken());

    /// <summary>
    /// 获取当前登录用户实体。
    /// </summary>
    protected async Task<SysUser?> GetCurrentUserAsync()
    {
        ApiResult<UserSummaryResponse> check = await CheckCurrentUserAsync();
        if (!check.Succeeded || check.Data is null)
        {
            return null;
        }

        return await FreeSql.Select<SysUser>()
            .Where(a => a.Id == check.Data.Id)
            .FirstAsync();
    }

    /// <summary>
    /// 写入登录日志。
    /// </summary>
    protected Task WriteLoginLogAsync(
        string username,
        SysUserLoginLog.LogType type,
        string extra = "") =>
        FreeSql.Insert(new SysUserLoginLog
        {
            Username = username,
            Type = type,
            Extra = extra,
            Ip = GetClientIpAddress(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        }).ExecuteAffrowsAsync();

    /// <summary>
    /// 获取用户显示名。
    /// </summary>
    protected static string DisplayName(SysUser user) =>
        !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;

    /// <summary>
    /// 校验请求 DTO。
    /// </summary>
    protected static ApiResult? ValidateRequest(object request)
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
