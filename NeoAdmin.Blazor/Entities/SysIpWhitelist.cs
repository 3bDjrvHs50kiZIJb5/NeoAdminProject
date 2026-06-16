using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// IP 白名单
/// </summary>
[Table(Name = "sysipwhitelist")]
public sealed class SysIpWhitelist : Entity
{
    [Column(StringLength = 300)]
    public string IpAddress { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastAccessTime { get; set; }

    public int AccessCount { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
