namespace NoAdmin.Blazor.Models;

public class NovaNovaAdminAuditedEventArgs
{
	public EntityAudited Entity { get; set; }

	public SysAuditLog AuditLog { get; set; }
}
