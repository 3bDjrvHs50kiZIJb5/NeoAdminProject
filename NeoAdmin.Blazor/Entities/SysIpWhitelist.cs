using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// IP 白名单
/// </summary>
[Table(Name = "sysipwhitelist")]
public sealed class SysIpWhitelist : Entity
{
    /// <summary>
    /// 所属用户；列表与校验按此字段隔离。管理员可查看全部，普通用户仅能管理自己的记录。
    /// </summary>
    public long? UserId { get; set; }

    [Navigate(nameof(UserId))]
    public SysUser? User { get; set; }

    [Column(StringLength = 300)]
    public string IpAddress { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastAccessTime { get; set; }

    public int AccessCount { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
