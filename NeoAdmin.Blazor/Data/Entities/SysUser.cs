using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

public sealed class SysUser : Entity
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

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
