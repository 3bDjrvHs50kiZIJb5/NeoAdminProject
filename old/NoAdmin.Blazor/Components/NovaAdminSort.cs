using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAdminSort : ComponentBase
{
	[Parameter]
	public string Text { get; set; }

	[Parameter]
	public string Value { get; set; }

	[Parameter]
	public bool Sortable { get; set; } = true;

	[CascadingParameter]
	public NovaAdminQueryInfo AdminQuery { get; set; }

	[CascadingParameter]
	public NovaAdminContext.TabInfo TabInfo { get; set; }

	private NovaAdminQueryInfo q => AdminQuery;

	[Inject]
	private ToastService ToastService { get; set; } = null;

	[Inject]
	private MessageService MessageService { get; set; } = null;

	[Inject]
	private NavigationManager Nav { get; set; } = null;

	[Inject]
	private IJSRuntime JS { get; set; } = null;

	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null;

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		if (Sortable)
		{
			if (q.Sort == Value || q.Sort == Value + "@desc")
			{
				__builder.OpenElement(0, "div");
				__builder.AddAttribute(1, "class", "table-cell table-cell-sort-active");
				__builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => SortButtonClick(0)));
				__builder.OpenElement(3, "span");
				__builder.AddAttribute(4, "class", "table-text pr-3");
				__builder.AddContent(5, Text);
				__builder.CloseElement();
				__builder.AddMarkupContent(6, "\r\n            ");
				__builder.OpenElement(7, "span");
				__builder.AddAttribute(8, "class", "filter-icon");
				if (q.Sort == Value)
				{
					__builder.AddMarkupContent(9, "<i class=\"fas fa-sort-asc\" style=\"color:var(--bs-primary);\"></i>");
				}
				else if (q.Sort == Value + "@desc")
				{
					__builder.AddMarkupContent(10, "<i class=\"fas fa-sort-desc\" style=\"color:var(--bs-primary);\"></i>");
				}
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else
			{
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "table-cell");
				__builder.AddAttribute(13, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => SortButtonClick(0)));
				__builder.OpenElement(14, "span");
				__builder.AddAttribute(15, "class", "table-text pr-3");
				__builder.AddContent(16, Text);
				__builder.CloseElement();
				__builder.AddMarkupContent(17, "\r\n            ");
				__builder.AddMarkupContent(18, "<span class=\"filter-icon\"><i class=\"fas fa-sort\" style=\"color:#dddddd;\"></i></span>");
				__builder.CloseElement();
			}
		}
		else
		{
			__builder.AddContent(19, Text);
		}
	}

	protected override void OnInitialized()
	{
		if (q.IsQueryString)
		{
			string queryStringValue = Nav.GetQueryStringValue(q.SortQueryStringName, TabInfo?.Url);
			if (!queryStringValue.IsNull())
			{
				q.Sort = queryStringValue;
			}
		}
	}

	private async Task SortButtonClick(int desc)
	{
		switch (desc)
		{
		case 0:
			if (q.Sort == Value)
			{
				q.Sort = Value + "@desc";
			}
			else if (q.Sort == Value + "@desc")
			{
				q.Sort = null;
			}
			else
			{
				q.Sort = Value;
			}
			break;
		case 2:
			q.Sort = Value + "@desc";
			break;
		case 1:
			q.Sort = Value;
			break;
		default:
			q.Sort = null;
			break;
		}
		if (q.IsQueryString)
		{
			string url = Nav.GetUriWithQueryParameters(new Dictionary<string, object>
			{
				[q.PageNumberQueryStringName] = null,
				[q.SearchTextQueryStringName] = q.SearchText,
				[q.SortQueryStringName] = q.Sort
			});
			Nav.NavigateTo(url);
		}
		q.PageNumber = 1;
		if (q.InvokeQueryAsync != null)
		{
			await q.InvokeQueryAsync();
		}
	}
}
