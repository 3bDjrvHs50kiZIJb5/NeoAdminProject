using System;

namespace NoAdmin.Blazor.Models;

public class NovaAdminMessageInfo
{
	public DateTime SendTime { get; set; }

	public long SendUserId { get; set; }

	public string SendUsername { get; set; }

	public string Content { get; set; }
}
