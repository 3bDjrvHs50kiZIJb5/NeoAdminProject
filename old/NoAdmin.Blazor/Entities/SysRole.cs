using System;
using System.Collections.Generic;
using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;

/// <summary>
/// 角色
/// </summary>
public class SysRole : Entity, IEntitySync
{
	/// <summary>
	/// 名称
	/// </summary>
	[Column(StringLength = 50)]
	public string Name { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	[Column(StringLength = 500)]
	public string Description { get; set; }

	/// <summary>
	/// 系统
	/// </summary>
	public bool IsAdministrator { get; set; }

	/// <summary>
	/// IP白名单
	/// </summary>
	[Column(StringLength = 200)]
	public string IpWhiteList { get; set; }

	public string SyncSourceId { get; set; }

	public DateTime? SyncSourceUpdateTime { get; set; }

	public DateTime SyncTime { get; set; }

	[Navigate(ManyToMany = typeof(SysRoleUser))]
	public List<SysUser> Users { get; set; }

	[Navigate(ManyToMany = typeof(SysRoleMenu))]
	public List<SysMenu> Menus { get; set; }
}
