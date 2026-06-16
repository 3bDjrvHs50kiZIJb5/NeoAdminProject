using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 组织结构。
/// </summary>
[Table(Name = "sysorg")]
public sealed class SysOrg : Entity
{
    [Navigate(nameof(ParentId))]
    public SysOrg? Parent { get; set; }

    [Navigate(nameof(ParentId))]
    public List<SysOrg> Children { get; set; } = new();

    [Navigate("OrgId")]
    public List<SysUser> Users { get; set; } = new();

    public long ParentId { get; set; }

    [Required(ErrorMessage = "请输入组织名称")]
    [Column(StringLength = 50)]
    public string Label { get; set; } = string.Empty;

    public SysOrgType Type { get; set; }

    public int Sort { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;
}
