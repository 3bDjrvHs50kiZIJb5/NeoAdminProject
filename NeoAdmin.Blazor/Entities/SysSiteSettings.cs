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
}
