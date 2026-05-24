using System.ComponentModel.DataAnnotations;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Menus;

public sealed class MenuEditModel
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    [Required(ErrorMessage = "请输入菜单名称")]
    [StringLength(50, ErrorMessage = "菜单名称不能超过 50 个字符")]
    public string Label { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "图标不能超过 50 个字符")]
    public string Icon { get; set; } = "circle";

    [StringLength(100, ErrorMessage = "路径不能超过 100 个字符")]
    public string Path { get; set; } = string.Empty;

    public int Sort { get; set; }

    public SysMenuType Type { get; set; } = SysMenuType.菜单;

    [StringLength(500, ErrorMessage = "备注不能超过 500 个字符")]
    public string Description { get; set; } = string.Empty;

    public SysMenuSidebarStyle SidebarStyle { get; set; } = SysMenuSidebarStyle.收起;

    public bool IsHidden { get; set; }

    public bool GenerateCrudButtons { get; set; }

    public bool GenerateAuditButtons { get; set; }

    public static MenuEditModel FromEntity(SysMenu menu) => new()
    {
        Id = menu.Id,
        ParentId = menu.ParentId,
        Label = menu.Label,
        Icon = menu.Icon,
        Path = menu.Path,
        Sort = menu.Sort,
        Type = menu.Type,
        Description = menu.Description,
        SidebarStyle = menu.SidebarStyle,
        IsHidden = menu.IsHidden,
        GenerateCrudButtons = menu.Type == SysMenuType.增删改查
    };
}
