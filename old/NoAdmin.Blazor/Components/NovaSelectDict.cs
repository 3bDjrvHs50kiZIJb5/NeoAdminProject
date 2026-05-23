using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaSelectDict : ComponentBase
{
	[Parameter]
	public string ParentName { get; set; }

	[Parameter]
	public string Value { get; set; }

	[Parameter]
	public Func<SysDict, string> DisplayText { get; set; } = (SysDict a) => string.IsNullOrWhiteSpace(a.Value) ? a.Name : a.Value;

	[Parameter]
	public EventCallback<string> ValueChanged { get; set; }

	[Parameter]
	public EventCallback<string> OnValueChanged { get; set; }

	[Parameter]
	public List<SysDict> Source { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	[Inject]
	private IFreeSql fsql { get; set; }

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
		__builder.OpenElement(0, "select");
		__builder.AddAttribute(1, "oninput", EventCallback.Factory.Create((object)this, (Func<ChangeEventArgs, Task>)OnInput));
		__builder.AddAttribute(2, "class", "form-control");
		__builder.AddAttribute(3, "disabled", Disabled);
		__builder.AddAttribute(4, "value", BindConverter.FormatValue(Value));
		__builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			Value = __value;
		}, Value));
		__builder.SetUpdatesAttributeName("value");
		__builder.OpenElement(6, "option");
		__builder.AddAttribute(7, "value");
		__builder.AddMarkupContent(8, "请选择..");
		__builder.CloseElement();
		if (Source != null)
		{
			foreach (SysDict item in Source)
			{
				__builder.OpenElement(9, "option");
				__builder.AddAttribute(10, "value", DisplayText?.Invoke(item));
				__builder.AddContent(11, DisplayText?.Invoke(item));
				__builder.CloseElement();
			}
		}
		__builder.CloseElement();
	}

	private async Task OnInput(ChangeEventArgs e)
	{
		string val = e.Value.ConvertTo<string>();
		await OnValueChanged.InvokeAsync(val);
		await ValueChanged.InvokeAsync(val);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
		{
			return;
		}
		if (Source == null)
		{
			Source = await ((ISelect0<ISelect<SysDict>, SysDict>)(object)fsql.Select<SysDict>().Where((Expression<Func<SysDict, bool>>)((SysDict a) => a.Parent.Name == ParentName)).OrderBy<int>((Expression<Func<SysDict, int>>)((SysDict a) => a.Sort))).ToListAsync(default(CancellationToken));
		}
		StateHasChanged();
	}
}
