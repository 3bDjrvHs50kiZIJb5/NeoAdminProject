using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

public sealed class SysRoleUser
{
    [Column(IsPrimary = true)]
    public long RoleId { get; set; }

    [Column(IsPrimary = true)]
    public long UserId { get; set; }

    [Navigate(nameof(RoleId))]
    public SysRole? Role { get; set; }

    [Navigate(nameof(UserId))]
    public SysUser? User { get; set; }
}
