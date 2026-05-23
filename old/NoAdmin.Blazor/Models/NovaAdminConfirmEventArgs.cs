namespace NoAdmin.Blazor.Models;

public class NovaAdminConfirmEventArgs<TItem>
{
	public TItem Argument { get; set; }

	public bool Cancel { get; set; }
}
