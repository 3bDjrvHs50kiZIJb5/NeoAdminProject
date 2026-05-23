using FreeSql.DataAnnotations;

namespace NoAdmin.Blazor.Entities;

public class SysTenantMenu
{
	[Column(StringLength = 50)]
	public string TenantId { get; set; }

	public long MenuId { get; set; }

	public SysTenant Tenant { get; set; }

	public SysMenu Menu { get; set; }
}
