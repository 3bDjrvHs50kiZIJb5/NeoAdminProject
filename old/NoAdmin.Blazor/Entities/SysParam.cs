using System;
using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;

/// <summary>
/// 参数配置
/// </summary>
public class SysParam : Entity<string>, IEntityCreated, IEntityModified
{
	/// <summary>
	/// 描述
	/// </summary>
	[Column(StringLength = 500)]
	public string Title { get; set; }

	/// <summary>
	/// 启用
	/// </summary>
	public bool Enabled { get; set; } = true;

	/// <summary>
	/// 排序
	/// </summary>
	public int Sort { get; set; }

	/// <summary>
	/// 值1
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value { get; set; }

	/// <summary>
	/// 值2
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value2 { get; set; }

	/// <summary>
	/// 值3
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value3 { get; set; }

	/// <summary>
	/// 值4
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value4 { get; set; }

	/// <summary>
	/// 值5
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value5 { get; set; }

	/// <summary>
	/// 值6
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value6 { get; set; }

	/// <summary>
	/// 值7
	/// </summary>
	[Column(StringLength = 1024)]
	public string Value7 { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	[Column(StringLength = 500)]
	public string Description { get; set; }

	/// <summary>
	/// 创建者Id
	/// </summary>
	[Column(Position = -22, CanUpdate = false)]
	public long? CreatedUserId { get; set; }

	/// <summary>
	/// 创建者
	/// </summary>
	[Column(Position = -21, CanUpdate = false, StringLength = 50)]
	public string CreatedUserName { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	[Column(Position = -20, CanUpdate = false, ServerTime = DateTimeKind.Local)]
	public DateTime? CreatedTime { get; set; }

	/// <summary>
	/// 修改者Id
	/// </summary>
	[Column(Position = -12, CanInsert = false)]
	public long? ModifiedUserId { get; set; }

	/// <summary>
	/// 修改者
	/// </summary>
	[Column(Position = -11, CanInsert = false, StringLength = 50)]
	public string ModifiedUserName { get; set; }

	/// <summary>
	/// 修改时间
	/// </summary>
	[Column(Position = -10, CanInsert = false, ServerTime = DateTimeKind.Local)]
	public DateTime? ModifiedTime { get; set; }
}
