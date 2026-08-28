using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Api.Dto;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Services;

namespace NeoAdmin.Blazor.Api;

/// <summary>
/// 用户头像 API：自定义上传与 DiceBear 默认头像。
/// </summary>
[Microsoft.AspNetCore.Mvc.Route("api/avatar")]
[Tags("头像接口")]
public sealed class AvatarController : BaseApiController
{
    private readonly AvatarService _avatarService;
    private readonly AvatarStyleRevision _avatarStyleRevision;

    public AvatarController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        AvatarService avatarService,
        AvatarStyleRevision avatarStyleRevision,
        ILogger<AvatarController> logger)
        : base(freeSql, auth, logger)
    {
        _avatarService = avatarService;
        _avatarStyleRevision = avatarStyleRevision;
    }

    /// <summary>
    /// 上传个人头像（Base64 图片），保存后 GET api/avatar/@GetAvatar/{userId} 将优先返回该图。
    /// </summary>
    [HttpPost($"@{nameof(UploadAvatar)}")]
    public async Task<ApiResult> UploadAvatar([FromBody] UploadAvatarRequest request)
    {
        SysUser? user = await GetCurrentUserAsync();
        if (user is null)
        {
            return ApiResult.Error("未登录或登录已过期", 401);
        }

        ApiResult<string> result = await _avatarService.SaveCustomAvatarFromBase64Async(
            user.Id,
            request.Base64 ?? string.Empty);

        return result.Succeeded
            ? ApiResult.Success(result.Message)
            : ApiResult.Error(result.Message, result.Code);
    }

    /// <summary>
    /// 获取用户头像：有自定义头像则 302 跳转至文件 URL，否则返回 DiceBear SVG。
    /// </summary>
    /// <param name="userId">用户 Id（DiceBear 时作为 seed）。</param>
    /// <param name="size">DiceBear 尺寸像素，16–512，默认 64。</param>
    /// <param name="style">可选：临时指定风格 key（预览用）。</param>
    /// <param name="preset">可选：临时指定预设 key（仅 clay 生效）。</param>
    [HttpGet($"@{nameof(GetAvatar)}/{{userId:long}}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvatar(
        long userId,
        [FromQuery] int size = 64,
        [FromQuery] string? style = null,
        [FromQuery] string? preset = null)
    {
        string? customUrl = await _avatarService.GetCustomAvatarUrlAsync(userId, HttpContext.RequestAborted);
        if (!string.IsNullOrWhiteSpace(customUrl))
        {
            Logger.LogInformation("返回自定义头像，UserId={UserId}", userId);
            Response.Headers.CacheControl = "public, max-age=3600";
            return Redirect(customUrl);
        }

        if (style is null || preset is null)
        {
            (string currentStyle, string currentPreset) =
                await _avatarService.GetCurrentSelectionAsync(HttpContext.RequestAborted);
            style ??= currentStyle;
            preset ??= currentPreset;
        }

        string svg = _avatarService.GetSvg(userId.ToString(), style, preset, size);
        Logger.LogInformation(
            "返回 DiceBear 头像，UserId={UserId}，Style={Style}，Preset={Preset}，Revision={Revision}",
            userId,
            style,
            preset,
            _avatarStyleRevision.Current);

        Response.Headers.CacheControl = "public, max-age=300";
        Response.Headers.ETag = $"\"avatar-{_avatarStyleRevision.Current}-{userId}-{style}-{preset}-{size}\"";
        return Content(svg, "image/svg+xml");
    }
}
