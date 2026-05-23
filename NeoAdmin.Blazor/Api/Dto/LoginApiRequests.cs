using System.ComponentModel.DataAnnotations;

namespace NeoAdmin.Blazor.Api.Dto;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "请输入用户名")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [StringLength(50, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Nickname { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required]
    [StringLength(50)]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class UpdateMemberInfoRequest
{
    [StringLength(50)]
    public string? Nickname { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public sealed class DeleteAccountRequest
{
    [Required]
    [StringLength(50)]
    public string Password { get; set; } = string.Empty;
}

public sealed class UploadAvatarRequest
{
    public string? Base64 { get; set; }
}

public sealed class UploadBadgePhotoRequest
{
    public string? Base64 { get; set; }
}

public sealed class SendResetPasswordCodeRequest
{
    public string? Phone { get; set; }
}

public sealed class ResetPasswordRequest
{
    public string? Phone { get; set; }

    public string? Code { get; set; }

    public string? NewPassword { get; set; }
}

public sealed class SetAIAlarmLevelRequest
{
    public int Level { get; set; }
}
