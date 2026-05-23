namespace NoAdmin.Blazor.Models;

public class NovaAdminItem<T>
{
	public T Value { get; set; }

	public bool Selected { get; set; }

	public bool Disabled { get; set; }

	internal bool DisabledSave { get; set; }

	public int Level { get; set; } = 1;

	public bool Expanded { get; set; } = true;

	public string RowClass { get; set; }

	internal string KeyString { get; set; }

	public NovaAdminItem(T item)
	{
		Value = item;
	}
}
