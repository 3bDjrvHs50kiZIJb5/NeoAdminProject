using System;

namespace NoAdmin.Blazor.Entities;


public interface IEntitySync
{
	/// <summary>
	/// 同步源 Id
	/// </summary>
	string SyncSourceId { get; set; }

	/// <summary>
	/// 源系统记录的最后更新时间
	/// 这样下次可以根据此字段做增量查询
	/// </summary>
	DateTime? SyncSourceUpdateTime { get; set; }

	/// <summary>
	/// 本地同步执行的时间
	/// </summary>
	DateTime SyncTime { get; set; }
}