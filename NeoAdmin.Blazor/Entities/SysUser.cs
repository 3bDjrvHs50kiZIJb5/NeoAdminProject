using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

[Table(Name = "sysuser")]
public partial class SysUser : Entity
{
    [Navigate(ManyToMany = typeof(SysRoleUser))]
    public List<SysRole> Roles { get; set; } = new();

    [Column(StringLength = 50)]
    public string Username { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Nickname { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Password { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public bool IsSystem { get; set; }

    public DateTime LoginTime { get; set; }

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 用户自定义头像地址（上传后的文件 URL）；为空时使用 DiceBear 系统头像。
    /// </summary>
    [Column(StringLength = 256)]
    public string? Avatar { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}

public partial class SysUser {
    public long? OrgId { get; set; }

    [Navigate(nameof(OrgId))]
    public SysOrg? Org { get; set; }
}
