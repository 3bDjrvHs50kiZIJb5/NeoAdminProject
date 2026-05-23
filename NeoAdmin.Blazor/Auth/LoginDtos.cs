using System.ComponentModel.DataAnnotations;

namespace NeoAdmin.Blazor.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "请输入用户名")]
    [StringLength(50, ErrorMessage = "用户名不能超过 50 个字符")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [StringLength(50, ErrorMessage = "密码不能超过 50 个字符")]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;

    public UserSummaryResponse User { get; init; } = new();
}

public sealed class UserSummaryResponse
{
    public long Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }

    public DateTime LoginTime { get; init; }

    public List<string> Roles { get; init; } = [];
}
