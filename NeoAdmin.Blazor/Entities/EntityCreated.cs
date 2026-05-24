using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 带创建信息的实体基类。
/// </summary>
public abstract class EntityCreated : Entity
{
    [Column(CanUpdate = false)]
    public long? CreatedUserId { get; set; }

    [Column(CanUpdate = false, StringLength = 50)]
    public string CreatedUserName { get; set; } = string.Empty;

    [Column(CanUpdate = false, ServerTime = DateTimeKind.Local)]
    public DateTime? CreatedTime { get; set; }
}
