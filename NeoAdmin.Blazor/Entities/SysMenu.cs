using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

[Table(Name = "sysmenu")]
public sealed class SysMenu : Entity
{
    [Navigate(nameof(ParentId))]
    public SysMenu? Parent { get; set; }

    [Navigate(nameof(ParentId))]
    public List<SysMenu> Children { get; set; } = new();

    public long ParentId { get; set; }

    [Required(ErrorMessage = "请输入菜单名称")]
    [StringLength(50, ErrorMessage = "菜单名称不能超过 50 个字符")]
    [Column(StringLength = 50)]
    public string Label { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "图标不能超过 50 个字符")]
    [Column(StringLength = 50)]
    public string Icon { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "路径不能超过 100 个字符")]
    [Column(StringLength = 100)]
    public string Path { get; set; } = string.Empty;

    public int Sort { get; set; }

    public SysMenuType Type { get; set; }

    [StringLength(500, ErrorMessage = "备注不能超过 500 个字符")]
    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public SysMenuSidebarStyle SidebarStyle { get; set; }

    public bool IsSystem { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    [Navigate(ManyToMany = typeof(SysRoleMenu))]
    public List<SysRole> Roles { get; set; } = new();
}
