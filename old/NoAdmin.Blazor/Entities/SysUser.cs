using System;
using System.Collections.Generic;
using NoAdmin.Blazor.Components;
using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;


/// <summary>
/// 用户
/// </summary>
public class SysUser : EntityCreated, IEntitySync
{
	[Navigate(ManyToMany = typeof(SysRoleUser))]
	public List<SysRole> Roles { get; set; }

	[Navigate("UserId")]
	public List<SysRoleUser> RoleUsers { get; set; }

	/// <summary>
	/// 名称
	/// </summary>
	[Column(StringLength = 50)]
	public string Username { get; set; }

	/// <summary>
	/// 昵称
	/// </summary>
	[Column(StringLength = 50)]
	public string Nickname { get; set; }

	/// <summary>
	/// 密码
	/// </summary>
	[Column(StringLength = 50)]
	public string Password { get; set; }

	/// <summary>
	/// 谷歌验证器密钥
	/// </summary>
	[Column(StringLength = 100)]
	public string GoogleSecretKey { get; set; }

	/// <summary>
	/// 是否可用
	/// </summary>
	public bool IsEnabled { get; set; }

	/// <summary>
	/// 登陆时间
	/// </summary>
	public DateTime LoginTime { get; set; }

	/// <summary>
	/// 所属组织
	/// </summary>
	public long OrgId { get; set; }

	[Navigate("OrgId")]
	public SysOrg Org { get; set; }

	/// <summary>
	/// 备注
	/// </summary>
	[Column(StringLength = 500)]
	public string Description { get; set; }

	/// <summary>
	/// 是否系统
	/// </summary>
	public bool IsSystem { get; set; }

	public string SyncSourceId { get; set; }

	public DateTime? SyncSourceUpdateTime { get; set; }

	public DateTime SyncTime { get; set; }
}