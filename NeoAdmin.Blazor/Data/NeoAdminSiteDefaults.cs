namespace NeoAdmin.Blazor.Data;

/// <summary>站点品牌资源默认值（LOGO、登录页配图）。</summary>
public static class NeoAdminSiteDefaults
{
    public const string LogoPath = "/_content/NeoAdmin.Blazor/images/logo.png";

    public const string LoginImagePath = "/_content/NeoAdmin.Blazor/images/login_bg.png";

    public static string ResolveLogo(string? logo) =>
        string.IsNullOrWhiteSpace(logo) ? LogoPath : logo;

    public static string ResolveLoginImage(string? loginImage) =>
        string.IsNullOrWhiteSpace(loginImage) ? LoginImagePath : loginImage;
}
