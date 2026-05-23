namespace NoAdmin.Blazor.Entities;

public class SysRoleUser
{
	public long RoleId { get; set; }

	public long UserId { get; set; }

	public SysRole Role { get; set; }

	public SysUser User { get; set; }
}
