using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaSelectEnum<TEnum> : ComponentBase where TEnum : Enum
{
	[Parameter]
	public TEnum Value { get; set; }

	[Parameter]
	public EventCallback<TEnum> ValueChanged { get; set; }

	[Parameter]
	public EventCallback<TEnum> OnValueChanged { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

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
		__builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder(this, delegate(TEnum __value)
		{
			Value = __value;
		}, Value));
		__builder.SetUpdatesAttributeName("value");
		string[] enumNames = typeof(TEnum).GetEnumNames();
		foreach (string text in enumNames)
		{
			__builder.OpenElement(6, "option");
			__builder.AddAttribute(7, "value", text);
			__builder.AddContent(8, DisplayText(text));
			__builder.CloseElement();
		}
		__builder.CloseElement();
	}

	private async Task OnInput(ChangeEventArgs e)
	{
		TEnum val = e.Value.ConvertTo<TEnum>();
		await OnValueChanged.InvokeAsync(val);
		await ValueChanged.InvokeAsync(val);
	}

	private string DisplayText(string enumName)
	{
		TEnum val = enumName.ConvertTo<TEnum>();
		string s = val.ToDescription();
		return s.IsNull(enumName);
	}
}
