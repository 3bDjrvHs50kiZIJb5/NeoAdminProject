using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using DiceBear;
using FreeSql;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// DiceBear 头像服务：按 seed 服务端生成 SVG，不依赖外网 API。
/// 风格与预设由站点设置 <see cref="Entities.SysSiteSettings.AvatarStyle"/> /
/// <see cref="Entities.SysSiteSettings.AvatarPreset"/> 控制；预设仅对 clay 风格生效。
/// </summary>
public sealed class AvatarService
{
    /// <summary>风格定义：kebab-case key（如 pixel-art-neutral）→ 显示名（如 Pixel Art Neutral）。</summary>
    public sealed record StyleInfo(string Key, string Label);

    /// <summary>Clay 预设定义：key → 中文标签 + DiceBear 渲染 options（JSON）。</summary>
    public sealed record PresetInfo(string Key, string Label, string OptionsJson);

    public const string DefaultStyleKey = "clay";

    /// <summary>
    /// DiceBear.Styles 包内全部风格（反射枚举，按显示名排序）。
    /// </summary>
    public static readonly IReadOnlyList<StyleInfo> AvailableStyles = BuildStyleList();

    /// <summary>
    /// Clay 官方 12 个预设（https://www.dicebear.com/styles/clay/presets/index.md）。
    /// 首项 default 表示不套预设、全部随 seed 变化（旧数据的空字符串同样按默认处理）。
    /// </summary>
    public static readonly IReadOnlyList<PresetInfo> Presets =
    [
        new("default", "默认（随机）", "{}"),
        new("bare", "极简 Bare", """{"topProbability":0,"patternProbability":0}"""),
        new("sepia", "复古 Sepia", """{"backgroundColor":["ede0c9"],"bodyColor":["b39572","9c7d5c","c7ab8a","8a6a4d"],"accentColor":["7a5c3f","a8895f"],"inkColor":["3b2f21"],"mouthVariant":["teeth","smile","o","frown","line","wavy","toothy","pout","grin","smirk","zigzag","dot","cat","smileBig","uu","openSmall"]}"""),
        new("greyscale", "灰阶 Greyscale", """{"backgroundColor":["ececee"],"bodyColor":["c1c1c7","a1a1aa","d4d4d8","8a8a92"],"accentColor":["71717a","9c9ca4"],"inkColor":["27272a"],"mouthVariant":["teeth","smile","o","frown","line","wavy","toothy","pout","grin","smirk","zigzag","dot","cat","smileBig","uu","openSmall"]}"""),
        new("duotone", "双色 Duotone", """{"backgroundColor":["e2ecf4"],"bodyColor":["6f8fb0"],"accentColor":["44607a"],"inkColor":["23323f"],"mouthVariant":["teeth","smile","o","frown","line","wavy","toothy","pout","grin","smirk","zigzag","dot","cat","smileBig","uu","openSmall"]}"""),
        new("cool", "冷色 Cool", """{"mouthVariant":["teeth","smile","o","frown","line","wavy","toothy","pout","grin","smirk","zigzag","dot","cat","smileBig","uu","openSmall"],"bodyColor":["83b0a4","86a5c3","9793bd","8ba06b","6f9fb8"],"accentColor":["5d8b80","63819e","70689c","6b7d4f"]}"""),
        new("warm", "暖色 Warm", """{"bodyColor":["c4795c","d99277","e0bd6a","cf9f52","cd8ea6"],"accentColor":["9c5940","b0714f","b3903f","a86b7f"]}"""),
        new("electric", "霓虹 Electric", """{"backgroundColor":["f4f4f5"],"bodyColor":["ff2e88","00e5ff","7cff00","ffe600","ff6a00","b400ff"]}"""),
        new("bold-pop", "撞色 Bold Pop", """{"backgroundColor":["ff2e63","00c2a8","ffb300","3d5afe","8e24aa","00e676"]}"""),
        new("night-shift", "暗夜 Night Shift", """{"backgroundColor":["18181b"]}"""),
        new("sunrise", "日出 Sunrise", """{"backgroundColor":["ffd5a8","ffb0c4"],"backgroundColorFill":"linear","backgroundColorAngle":45}"""),
        new("close-up", "特写 Close Up", """{"scale":1.2}"""),
        new("animated", "动画 Animated", """{"tags":["animation"]}""")
    ];

    /// <summary>风格 key → Styles 属性 getter（定义 JSON 较大，延迟读取）。</summary>
    private static readonly Dictionary<string, Func<string>> StyleDefinitions = BuildStyleDefinitions();

    /// <summary>已解析的 Style 实例缓存（解析一次即可复用）。</summary>
    private static readonly ConcurrentDictionary<string, Style> ParsedStyles = new();

    private const int MaxCustomAvatarBytes = 2 * 1024 * 1024;

    private readonly IFreeSql _freeSql;
    private readonly SiteSettingsService _siteSettingsService;
    private readonly FileService _fileService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AvatarService> _logger;

    public AvatarService(
        IFreeSql freeSql,
        SiteSettingsService siteSettingsService,
        FileService fileService,
        IMemoryCache cache,
        ILogger<AvatarService> logger)
    {
        _freeSql = freeSql;
        _siteSettingsService = siteSettingsService;
        _fileService = fileService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>读取用户自定义头像 URL；未设置时返回 null。</summary>
    public async Task<string?> GetCustomAvatarUrlAsync(long userId, CancellationToken cancellationToken = default)
    {
        string? avatar = await _freeSql.Select<SysUser>()
            .Where(u => u.Id == userId)
            .FirstAsync(u => u.Avatar, cancellationToken);

        return string.IsNullOrWhiteSpace(avatar) ? null : avatar.Trim();
    }

    /// <summary>保存用户自定义头像（Base64 图片），返回可访问 URL。</summary>
    public async Task<ApiResult<string>> SaveCustomAvatarFromBase64Async(
        long userId,
        string base64,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            _logger.LogWarning("上传自定义头像失败：Base64 为空，UserId={UserId}", userId);
            return ApiResult<string>.Error("请提供头像图片");
        }

        try
        {
            (byte[] bytes, string extension) = ParseImageBase64(base64);
            if (bytes.Length == 0)
            {
                _logger.LogWarning("上传自定义头像失败：图片内容为空，UserId={UserId}", userId);
                return ApiResult<string>.Error("图片内容不能为空");
            }

            if (bytes.Length > MaxCustomAvatarBytes)
            {
                _logger.LogWarning(
                    "上传自定义头像失败：文件过大，UserId={UserId}，Size={Size}",
                    userId,
                    bytes.Length);
                return ApiResult<string>.Error("头像大小不能超过 2MB");
            }

            SysFile file = await _fileService.UploadBytesAsync(
                $"avatar{extension}",
                bytes,
                "avatar",
                isRename: true,
                cancellationToken);

            await _freeSql.Update<SysUser>()
                .Where(u => u.Id == userId)
                .Set(u => u.Avatar, file.LinkUrl)
                .ExecuteAffrowsAsync(cancellationToken);

            _cache.Remove($"avatar:custom-url:{userId}");
            _logger.LogInformation(
                "上传自定义头像成功，UserId={UserId}，Url={Url}",
                userId,
                file.LinkUrl);
            return ApiResult<string>.Success(file.LinkUrl, "头像上传成功");
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "上传自定义头像失败：Base64 格式无效，UserId={UserId}", userId);
            return ApiResult<string>.Error("图片格式无效");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传自定义头像异常，UserId={UserId}", userId);
            return ApiResult<string>.Error(ex.Message);
        }
    }

    private static (byte[] Bytes, string Extension) ParseImageBase64(string input)
    {
        string data = input.Trim();
        string extension = ".png";

        if (data.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            int comma = data.IndexOf(',');
            if (comma > 0)
            {
                string header = data[..comma];
                extension = header.Contains("jpeg", StringComparison.OrdinalIgnoreCase)
                            || header.Contains("jpg", StringComparison.OrdinalIgnoreCase)
                    ? ".jpg"
                    : header.Contains("webp", StringComparison.OrdinalIgnoreCase)
                        ? ".webp"
                        : header.Contains("gif", StringComparison.OrdinalIgnoreCase)
                            ? ".gif"
                            : ".png";
                data = data[(comma + 1)..];
            }
        }

        return (Convert.FromBase64String(data), extension);
    }

    /// <summary>读取站点设置中当前生效的（风格, 预设）。</summary>
    public async Task<(string StyleKey, string Preset)> GetCurrentSelectionAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await _siteSettingsService.GetAsync(cancellationToken);
        return (settings.AvatarStyle, settings.AvatarPreset);
    }

    /// <summary>按当前站点设置为指定 seed 生成 SVG（带内存缓存）。</summary>
    public async Task<string> GetSvgAsync(string seed, int size = 64, CancellationToken cancellationToken = default)
    {
        (string styleKey, string preset) = await GetCurrentSelectionAsync(cancellationToken);
        return GetSvg(seed, styleKey, preset, size);
    }

    /// <summary>按指定风格与预设为 seed 生成 SVG（预览用，可绕过站点设置）。</summary>
    public string GetSvg(string seed, string styleKey, string preset, int size = 64)
    {
        if (!StyleDefinitions.ContainsKey(styleKey))
        {
            styleKey = DefaultStyleKey;
        }

        size = Math.Clamp(size, 16, 512);
        string cacheKey = $"avatar:{styleKey}:{preset}:{seed}:{size}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            try
            {
                Style style = ParsedStyles.GetOrAdd(styleKey, key => Style.Parse(StyleDefinitions[key]()));
                JsonObject options = BuildOptions(seed, styleKey, preset, size);
                return new Avatar(style, options).ToSvg();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成头像失败，Seed={Seed}，Style={Style}，Preset={Preset}", seed, styleKey, preset);
                throw;
            }
        })!;
    }

    private static JsonObject BuildOptions(string seed, string styleKey, string preset, int size)
    {
        // 预设中的组件/颜色选项是 Clay 专属，其他风格只用 seed 随机
        PresetInfo? info = styleKey == DefaultStyleKey
            ? Presets.FirstOrDefault(p => p.Key == preset)
            : null;

        JsonObject options = JsonNode.Parse(info?.OptionsJson ?? "{}")!.AsObject();
        options["seed"] = seed;
        options["size"] = size;
        return options;
    }

    private static Dictionary<string, Func<string>> BuildStyleDefinitions()
    {
        Dictionary<string, Func<string>> map = new();
        foreach (PropertyInfo property in typeof(Styles).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (property.PropertyType != typeof(string))
            {
                continue;
            }

            map[ToKebabCase(property.Name)] = () => (string)property.GetValue(null)!;
        }

        return map;
    }

    private static List<StyleInfo> BuildStyleList()
    {
        List<StyleInfo> styles = typeof(Styles)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => new StyleInfo(ToKebabCase(p.Name), ToDisplayName(p.Name)))
            .OrderBy(s => s.Label, StringComparer.Ordinal)
            .ToList();

        // 默认风格置顶
        int clayIndex = styles.FindIndex(s => s.Key == DefaultStyleKey);
        if (clayIndex > 0)
        {
            StyleInfo clay = styles[clayIndex];
            styles.RemoveAt(clayIndex);
            styles.Insert(0, clay);
        }

        return styles;
    }

    /// <summary>PixelArtNeutral → pixel-art-neutral（与 DiceBear 官方风格名一致）。</summary>
    private static string ToKebabCase(string name)
    {
        StringBuilder builder = new(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }

    /// <summary>PixelArtNeutral → Pixel Art Neutral。</summary>
    private static string ToDisplayName(string name)
    {
        StringBuilder builder = new(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c) && i > 0)
            {
                builder.Append(' ');
            }

            builder.Append(c);
        }

        return builder.ToString();
    }
}
