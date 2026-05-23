using Microsoft.AspNetCore.Components;

namespace NoAdmin.Blazor.Models;

public interface INovaAdminColumn
{
	string Title { get; set; }

	int Width { get; set; }

	string Sort { get; set; }

	string FilterKey { get; set; }

	NovaAdminColumnFixed Fixed { get; set; }

	bool Visible { get; set; }

	int Order { get; set; }

	string CalculatedStyle { get; set; }

	RenderFragment<object>? Template { get; set; }

	bool Primary { get; set; }

	bool IsOperation { get; set; }

	object GetValue(object item);
}
