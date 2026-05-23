using FreeSql.DataAnnotations;
using NeoAdmin.Blazor.Data.Attributes;

namespace NeoAdmin.Blazor.Data.Entities;

public abstract class Entity
{
    [Snowflake]
    [Column(IsIdentity = false, IsPrimary = true)]
    public long Id { get; set; }
}
