using System;

namespace NoAdmin.Blazor.Models;

public class NovaAdminLockResourceInfo
{
	public DateTime LockTime { get; set; }

	public string BlazorId { get; set; }

	public long LockUserId { get; set; }

	public string LockUsername { get; set; }
}
