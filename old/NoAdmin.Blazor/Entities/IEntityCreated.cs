using System;

namespace NoAdmin.Blazor.Entities;


/// <summary>
/// 实体基类-创建信息
/// </summary>
public interface IEntityCreated
{
	/// <summary>
	/// 创建者用户Id
	/// </summary>
	long? CreatedUserId { get; set; }

	/// <summary>
	/// 创建者
	/// </summary>
	string CreatedUserName { get; set; }

	/// <summary>
	/// 创建时间
	/// </summary>
	DateTime? CreatedTime { get; set; }
}