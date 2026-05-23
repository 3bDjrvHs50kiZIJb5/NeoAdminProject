using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAdminSearchText : ComponentBase
{
	private string _value;

	[Parameter]
	public NovaAdminQueryInfo AdminQuery { get; set; }

	[Parameter]
	public string Placeholder { get; set; } = "Search";

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
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "class", "input-group input-group-sm");
		__builder.OpenElement(2, "input");
		__builder.AddAttribute(3, "onkeyup", EventCallback.Factory.Create((object)this, (Func<KeyboardEventArgs, Task>)KeyHandler));
		__builder.AddEventPreventDefaultAttribute(4, "onkeyup", value: true);
		__builder.AddAttribute(5, "type", "text");
		__builder.AddAttribute(6, "class", "form-control form-control-xs");
		__builder.AddAttribute(7, "placeholder", Placeholder);
		__builder.AddAttribute(8, "value", BindConverter.FormatValue(_value));
		__builder.AddAttribute(9, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			_value = __value;
		}, _value));
		__builder.SetUpdatesAttributeName("value");
		__builder.CloseElement();
		__builder.AddMarkupContent(10, "\r\n    ");
		__builder.OpenElement(11, "button");
		__builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SearchClick));
		__builder.AddAttribute(13, "type", "button");
		__builder.AddAttribute(14, "class", "btn btn-primary d-inline-flex align-items-center justify-content-center");
		__builder.AddMarkupContent(15, "<i class=\"fas fa-search\"></i>");
		__builder.CloseElement();
		__builder.CloseElement();
	}

	protected override void OnInitialized()
	{
		if (q.IsQueryString)
		{
			_value = Nav.GetQueryStringValue(q.SearchTextQueryStringName, TabInfo?.Url);
			if (!_value.IsNull())
			{
				q.SearchText = _value;
			}
		}
	}

	private async Task KeyHandler(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
		{
			await SearchClick();
		}
	}

	private async Task SearchClick()
	{
		q.SearchText = (_value.IsNull() ? null : _value);
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
