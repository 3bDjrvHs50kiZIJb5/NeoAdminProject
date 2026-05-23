using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

public sealed class SysRoleMenu
{
    [Column(IsPrimary = true)]
    public long RoleId { get; set; }

    [Column(IsPrimary = true)]
    public long MenuId { get; set; }

    [Navigate(nameof(RoleId))]
    public SysRole? Role { get; set; }

    [Navigate(nameof(MenuId))]
    public SysMenu? Menu { get; set; }
}
