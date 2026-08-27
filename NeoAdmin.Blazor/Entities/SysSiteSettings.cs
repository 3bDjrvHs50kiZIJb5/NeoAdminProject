using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 站点设置（单条记录，非多租户）。
/// </summary>
[Table(Name = "syssitesettings")]
public sealed class SysSiteSettings : EntityCreated
{
    /// <summary>
    /// 站点标题（浏览器标题、侧栏等）。
    /// </summary>
    [Column(StringLength = 255)]
    public string Title { get; set; } = "NeoAdmin";

    /// <summary>
    /// 主域名。
    /// </summary>
    [Column(StringLength = 50)]
    public string Host { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Host2 { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Host3 { get; set; } = string.Empty;

    /// <summary>
    /// 说明。
    /// </summary>
    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 侧栏 LOGO 图片地址。
    /// </summary>
    [Column(StringLength = 256)]
    public string? Logo { get; set; }

    /// <summary>
    /// 登录页左侧配图地址。
    /// </summary>
    [Column(StringLength = 256)]
    public string? LoginImage { get; set; }

    /// <summary>
    /// 登录页「注册」链接地址；为空时不显示。
    /// </summary>
    [Column(StringLength = 500)]
    public string? RegisterUrl { get; set; }

    /// <summary>
    /// 登录页「忘记密码」链接地址；为空时不显示。
    /// </summary>
    [Column(StringLength = 500)]
    public string? ForgotPasswordUrl { get; set; }

    /// <summary>
    /// 站点是否启用（关闭后可在中间件等处扩展拦截逻辑）。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 白名单人工审核：开启后拦截页提交的 IP 需管理员审核；关闭则提交后立即生效。
    /// </summary>
    public bool IpWhitelistManualApproval { get; set; } = false;

    private string _avatarStyle = "clay";

    /// <summary>
    /// DiceBear 头像风格 key（kebab-case，如 clay、shapes、pixel-art-neutral）。
    /// 旧库该列为 NULL，读写时统一归一为 clay。
    /// </summary>
    [Column(StringLength = 32)]
    public string AvatarStyle
    {
        get => _avatarStyle;
        set => _avatarStyle = string.IsNullOrWhiteSpace(value) ? "clay" : value;
    }

    private string _avatarPreset = string.Empty;

    /// <summary>
    /// DiceBear Clay 头像预设名（如 bare、sepia、animated）；空表示默认随机。仅 clay 风格生效。
    /// 旧库该列为 NULL，读写时统一归一为空字符串。
    /// </summary>
    [Column(StringLength = 32)]
    public string AvatarPreset
    {
        get => _avatarPreset;
        set => _avatarPreset = value ?? string.Empty;
    }
}
