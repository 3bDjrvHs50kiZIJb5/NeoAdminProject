using System;
using System.Reflection;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeScheduler;
using FreeSql;

public class NovaAdminOptionsItem
{
	internal static string Global_CookieName;

	internal static SysTenant Global_FixedTenant;

	internal bool IsDevelopment;

	internal bool? _IsSwagger;

	public Func<NovaAdminContext, string, Task<long>> TokenParse;

	public Func<NovaAdminContext, DbContextAuditValueEventArgs, Task> AuditValue;

	public ushort WorkId { get; set; } = 1;

	public string CookieName { get; set; } = "NovaAdmin";

	public Assembly[] Assemblies { get; set; }

	public Action<IServiceProvider, TaskInfo> SchedulerExecuting { get; set; }

	public Action<FreeSqlBuilder> FreeSqlBuilder { get; set; }

	public bool IsSwagger
	{
		get
		{
			return _IsSwagger == true;
		}
		set
		{
			_IsSwagger = value;
		}
	}

	/// <summary>
	/// 固定租户，不进行多租户识别
	/// </summary>
	public string FixedTenantId { get; set; }

	public string ConfigsPath { get; set; } = "Configs";

	public string[] SwaggerHides { get; set; }

	public string LoginImageBackground { get; set; } = "";

	public bool TokenCheckLoginTime { get; set; } = true;

	public bool TokenCheckApiFingerprint { get; set; } = false;

	public bool IsSampleLayout { get; set; } = true;
}
