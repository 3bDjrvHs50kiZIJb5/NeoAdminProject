using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;


/// <summary>
/// 审计实体日志
/// </summary>
public class SysAuditEntityLog : EntityCreated<long>
{
	/// <summary>
	/// 审计表名
	/// </summary>
	[Column(StringLength = 50)]
	public string TableName { get; set; }

	/// <summary>
	/// 审计表Id值
	/// </summary>
	public long TableId { get; set; }

	/// <summary>
	/// 日志类型: add/edit/remove
	/// </summary>
	[Column(StringLength = 10)]
	public string LogType { get; set; }

	/// <summary>
	/// 实体数据(旧)
	/// </summary>
	[Column(StringLength = -1)]
	public string DataOld { get; set; }

	/// <summary>
	/// 实体数据
	/// </summary>
	[Column(StringLength = -1)]
	public string Data { get; set; }
}