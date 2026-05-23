namespace NoAdmin.Blazor.Models;

public class NovaAdminOptionsItem
{
	public string Label { get; set; }

	public string Value { get; set; }

	public NovaAdminOptionsItem(string label, string value)
	{
		Label = label;
		Value = value;
	}
}
