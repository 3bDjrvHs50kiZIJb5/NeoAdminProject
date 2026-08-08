using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Services;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Api;

/// <summary>
/// IP 白名单访问拦截页申请（匿名，需验证码）。
/// </summary>
[Microsoft.AspNetCore.Mvc.Route("api/ip-whitelist")]
public sealed class IpWhitelistApplyController : BaseApiController
{
    private readonly IpWhitelistCaptchaService _captchaService;
    private readonly LoginRateLimiter _rateLimiter;
    private readonly SiteSettingsService _siteSettingsService;

    public IpWhitelistApplyController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        IpWhitelistCaptchaService captchaService,
        LoginRateLimiter rateLimiter,
        SiteSettingsService siteSettingsService,
        ILogger<IpWhitelistApplyController> logger)
        : base(freeSql, auth, logger)
    {
        _captchaService = captchaService;
        _rateLimiter = rateLimiter;
        _siteSettingsService = siteSettingsService;
    }

    /// <summary>
    /// 获取申请页验证码。
    /// </summary>
    [HttpGet("captcha")]
    [AllowAnonymous]
    public ApiResult<IpWhitelistCaptchaResponse> Captcha()
    {
        IpWhitelistCaptchaIssue issue = _captchaService.Issue();
        return ApiResult<IpWhitelistCaptchaResponse>.Success(new IpWhitelistCaptchaResponse
        {
            Id = issue.Id,
            Svg = issue.Svg
        });
    }

    /// <summary>
    /// 提交当前 IP 到白名单（未启用，待管理员审核）。
    /// </summary>
    [HttpPost("apply")]
    [AllowAnonymous]
    public async Task<ApiResult> Apply([FromBody] IpWhitelistApplyRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        string clientIp = GetClientIpAddress();
        if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "unknown")
        {
            return ApiResult.Error("无法识别客户端 IP");
        }

        if (_rateLimiter.IsBlocked(clientIp, out string? blockedMessage))
        {
            return ApiResult.Error(blockedMessage!);
        }

        if (!_captchaService.Validate(request.CaptchaId, request.CaptchaCode))
        {
            _rateLimiter.RecordFailure(clientIp);
            return ApiResult.Error("验证码错误或已过期");
        }

        string normalizedIp = IpHelper.NormalizeIp(clientIp);
        bool manualApproval = await IsManualApprovalEnabledAsync();

        bool alreadyEnabled = await FreeSql.Select<SysIpWhitelist>()
            .Where(a => a.IpAddress == normalizedIp && a.IsEnabled)
            .AnyAsync();

        if (alreadyEnabled)
        {
            return ApiResult.Error("该 IP 已在白名单中");
        }

        bool pending = await FreeSql.Select<SysIpWhitelist>()
            .Where(a => a.IpAddress == normalizedIp && !a.IsEnabled)
            .AnyAsync();

        if (pending)
        {
            if (manualApproval)
            {
                return ApiResult.Success("已提交申请，请等待管理员审核");
            }

            await FreeSql.Update<SysIpWhitelist>()
                .Set(a => a.IsEnabled, true)
                .Set(a => a.Description, "访问拦截页申请（自动通过）")
                .Where(a => a.IpAddress == normalizedIp && !a.IsEnabled)
                .ExecuteAffrowsAsync();

            Logger.LogInformation("IP 白名单待审核记录已自动启用，IP={Ip}", normalizedIp);
            return ApiResult.Success("已加入白名单，请刷新页面");
        }

        bool autoEnable = !manualApproval;
        await FreeSql.Insert(new SysIpWhitelist
        {
            UserId = null,
            IpAddress = normalizedIp,
            Description = autoEnable ? "访问拦截页申请（自动通过）" : "访问拦截页申请",
            IsEnabled = autoEnable,
            AccessCount = 0,
            LastAccessTime = null,
            CreatedTime = DateTime.Now
        }).ExecuteAffrowsAsync();

        Logger.LogInformation("IP 白名单申请已提交，IP={Ip}，AutoEnable={AutoEnable}", normalizedIp, autoEnable);
        return autoEnable
            ? ApiResult.Success("已加入白名单，请刷新页面")
            : ApiResult.Success("申请已提交，请等待管理员审核");
    }

    private async Task<bool> IsManualApprovalEnabledAsync()
    {
        SysSiteSettings settings = await _siteSettingsService.GetAsync();
        return settings.IpWhitelistManualApproval;
    }
}

public sealed class IpWhitelistCaptchaResponse
{
    public string Id { get; set; } = string.Empty;

    public string Svg { get; set; } = string.Empty;
}

public sealed class IpWhitelistApplyRequest
{
    [Required(ErrorMessage = "验证码 ID 不能为空")]
    public string CaptchaId { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证码不能为空")]
    public string CaptchaCode { get; set; } = string.Empty;
}
