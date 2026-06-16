using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 审批操作日志。
/// </summary>
[Table(Name = "sysauditlog")]
public class SysAuditLog : EntityCreated
{
    [Column(StringLength = 50)]
    public string TableName { get; set; } = string.Empty;

    public long TableId { get; set; }

    [Column(StringLength = 10)]
    public string Step { get; set; } = string.Empty;

    [Column(MapType = typeof(int))]
    public SysAuditStatus Result { get; set; }

    [Column(StringLength = 500)]
    public string Opinion { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Tag { get; set; } = string.Empty;

    [Column(StringLength = 10)]
    public string NextStep { get; set; } = string.Empty;
}
