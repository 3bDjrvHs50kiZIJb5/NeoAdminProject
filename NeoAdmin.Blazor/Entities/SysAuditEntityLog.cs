using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 实体变更审计日志（增删改快照）。
/// </summary>
[Table(Name = "sysauditentitylog")]
public class SysAuditEntityLog : EntityCreated
{
    [Column(StringLength = 50)]
    public string TableName { get; set; } = string.Empty;

    public long TableId { get; set; }

    /// <summary>add / edit / remove</summary>
    [Column(StringLength = 10)]
    public string LogType { get; set; } = string.Empty;

    [Column(StringLength = -1)]
    public string DataOld { get; set; } = string.Empty;

    [Column(StringLength = -1)]
    public string Data { get; set; } = string.Empty;
}
