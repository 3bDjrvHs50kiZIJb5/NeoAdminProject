namespace NoAdmin.Blazor.Entities;

public class SysRoleMenu
{
	public long RoleId { get; set; }

	public long MenuId { get; set; }

	public SysRole Role { get; set; }

	public SysMenu Menu { get; set; }
}
