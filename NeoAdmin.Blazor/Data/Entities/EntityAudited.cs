using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

/// <summary>
/// 带审核信息的实体基类（博客频道、收藏集等示例）。
/// </summary>
public abstract class EntityAudited : EntityModified
{
    [Column(MapType = typeof(int), CanUpdate = false)]
    public SysAuditStatus AuditStatus { get; set; }

    [Column(StringLength = 10, CanUpdate = false)]
    public string AuditStep { get; set; } = string.Empty;

    [Column(CanUpdate = false)]
    public int AuditVersion { get; set; }
}
