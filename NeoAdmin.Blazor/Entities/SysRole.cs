using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 角色。
/// </summary>
[Table(Name = "sysrole")]
public sealed class SysRole : Entity
{
    [Required(ErrorMessage = "请输入角色名称")]
    [Column(StringLength = 50)]
    public string Name { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public bool IsAdministrator { get; set; }

    [Column(StringLength = 500)]
    public string IpWhiteList { get; set; } = string.Empty;

    [Navigate(ManyToMany = typeof(SysRoleUser))]
    public List<SysUser> Users { get; set; } = new();

    [Navigate(ManyToMany = typeof(SysRoleMenu))]
    public List<SysMenu> Menus { get; set; } = new();
}
