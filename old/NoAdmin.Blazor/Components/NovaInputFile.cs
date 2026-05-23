using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaInputFile : ComponentBase
{
	private string ClientId = "modal-" + Guid.NewGuid().ToString("n");

	private bool showModal;

	/// <summary>
	/// 值
	/// </summary>
	[Parameter]
	public string Value { get; set; }

	[Parameter]
	public EventCallback<string> ValueChanged { get; set; }

	/// <summary>
	/// 值变化时
	/// </summary>
	[Parameter]
	public EventCallback<string> OnValueChanged { get; set; }

	/// <summary>
	/// 弹框标题
	/// </summary>
	[Parameter]
	public string ModalTitle { get; set; } = "选择..";

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
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "class", "input-group");
		__builder.OpenElement(2, "input");
		__builder.AddAttribute(3, "class", "form-control disabled");
		__builder.AddAttribute(4, "readonly", "readonly");
		__builder.AddAttribute(5, "value", BindConverter.FormatValue(Value));
		__builder.AddAttribute(6, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			Value = __value;
		}, Value));
		__builder.SetUpdatesAttributeName("value");
		__builder.CloseElement();
		__builder.AddMarkupContent(7, "\r\n    ");
		__builder.OpenElement(8, "button");
		__builder.AddAttribute(9, "type", "button");
		__builder.AddAttribute(10, "class", "btn btn-outline-secondary");
		__builder.AddAttribute(11, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => OnItemClick(null)));
		__builder.AddAttribute(12, "role", "button");
		__builder.AddAttribute(13, "aria-disabled", "false");
		__builder.AddMarkupContent(14, "×");
		__builder.CloseElement();
		__builder.AddMarkupContent(15, "\r\n    ");
		__builder.OpenElement(16, "button");
		__builder.AddAttribute(17, "type", "button");
		__builder.AddAttribute(18, "class", "btn btn-secondary");
		__builder.AddAttribute(19, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)OpenModal));
		__builder.AddAttribute(20, "role", "button");
		__builder.AddAttribute(21, "aria-disabled", "false");
		__builder.AddMarkupContent(22, "<i class=\"fa-solid fa-folder-open\"></i>");
		__builder.AddMarkupContent(23, "<span>选择</span>");
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(24, "\r\n\r\n");
		__builder.OpenComponent<NovaModal>(25);
		__builder.AddComponentParameter(26, "Visible", RuntimeHelpers.TypeCheck(showModal));
		__builder.AddComponentParameter(27, "ClientId", RuntimeHelpers.TypeCheck(ClientId));
		__builder.AddComponentParameter(28, "IsBackdropStatic", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(29, "DialogClassName", "modal-xl modal-inputtable2");
		__builder.AddComponentParameter(30, "Title", RuntimeHelpers.TypeCheck(ModalTitle));
		__builder.AddComponentParameter(31, "OnClose", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action)OnClose)));
		__builder.AddAttribute(32, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			if (showModal)
			{
				renderTreeBuilder.OpenComponent<NovaAdminFilePicker>(33);
				renderTreeBuilder.AddComponentParameter(34, "OnItemClick", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysFile, Task>)OnItemClick)));
				renderTreeBuilder.CloseComponent();
			}
		});
		__builder.CloseComponent();
	}

	private void OpenModal()
	{
		showModal = true;
	}

	private async Task OnItemClick(SysFile file)
	{
		Value = file?.LinkUrl;
		if (ValueChanged.HasDelegate)
		{
			await ValueChanged.InvokeAsync(Value);
			if (OnValueChanged.HasDelegate)
			{
				await OnValueChanged.InvokeAsync(Value);
			}
		}
		OnClose();
	}

	private void OnClose()
	{
		showModal = false;
	}
}
