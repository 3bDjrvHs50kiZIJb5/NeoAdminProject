using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Services;

namespace NeoAdmin.Blazor.Api;

/// <summary>
/// 用户头像：优先返回自定义上传图，否则服务端生成 DiceBear SVG。
/// </summary>
[Microsoft.AspNetCore.Mvc.Route("api/avatar")]
public sealed class AvatarController : BaseApiController
{
    private readonly AvatarService _avatarService;

    public AvatarController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        AvatarService avatarService,
        ILogger<AvatarController> logger)
        : base(freeSql, auth, logger)
    {
        _avatarService = avatarService;
    }

    /// <summary>
    /// 获取用户头像：有自定义头像则 302 跳转至文件 URL，否则返回 DiceBear SVG。
    /// </summary>
    /// <param name="userId">用户 Id（DiceBear 时作为 seed）。</param>
    /// <param name="size">DiceBear 尺寸像素，16–512，默认 64。</param>
    /// <param name="style">可选：临时指定风格 key（预览用）。</param>
    /// <param name="preset">可选：临时指定预设 key（仅 clay 生效）。</param>
    [HttpGet("{userId:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
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
            "返回 DiceBear 头像，UserId={UserId}，Style={Style}，Preset={Preset}",
            userId,
            style,
            preset);

        Response.Headers.CacheControl = "public, max-age=3600";
        return Content(svg, "image/svg+xml");
    }
}
