using FreeSql.DataAnnotations;
using NeoAdmin.Blazor.Attributes;

namespace NeoAdmin.Blazor.Entities;

public abstract class Entity
{
    [Snowflake]
    [Column(IsIdentity = false, IsPrimary = true)]
    public long Id { get; set; }
}
