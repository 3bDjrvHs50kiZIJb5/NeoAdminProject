using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;


public abstract class Entity<TKey> : IEntity<TKey>
{
	/// <summary>
	/// 主键Id
	/// </summary>
	[Snowflake]
	[UuidV7]
	[Column(Position = 1, IsIdentity = false, IsPrimary = true, StringLength = 50)]
	public virtual TKey Id { get; set; }
}
public abstract class Entity : Entity<long>
{
}