using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Data.Entities;

/// <summary>
/// 带修改信息的实体基类。
/// </summary>
public abstract class EntityModified : EntityCreated
{
    [Column(CanInsert = false)]
    public long? ModifiedUserId { get; set; }

    [Column(CanUpdate = false, StringLength = 50)]
    public string ModifiedUserName { get; set; } = string.Empty;

    [Column(CanInsert = false, ServerTime = DateTimeKind.Local)]
    public DateTime? ModifiedTime { get; set; }
}
