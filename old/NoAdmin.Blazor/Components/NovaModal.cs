using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaModal : ComponentBase
{
	private bool _hiding;

	private bool _visible;

	internal bool _visibleNotifyChanged;

	private int _clickIncr = 0;

	[CascadingParameter]
	public NovaAdminContext.TabInfo TabInfo { get; set; }

	[Parameter]
	public string ClientId { get; set; } = "modal-" + Guid.NewGuid().ToString("n");

	[Parameter]
	public bool IsBackdropStatic { get; set; } = true;

	[Parameter]
	public bool IsKeyboard { get; set; } = true;

	[Parameter]
	public bool IsDraggable { get; set; } = true;

	[Parameter]
	public bool IsFooter { get; set; } = true;

	[Parameter]
	public bool IsShowLoader { get; set; } = false;

	[Inject]
	private NovaAdminContext admin { get; set; }

	[Parameter]
	public string Title { get; set; } = "标题";

	[Parameter]
	public string YesButton { get; set; } = "保存";

	[Parameter]
	public string CloseButton { get; set; } = "取消";

	[Parameter]
	public string DialogClassName { get; set; }

	[Parameter]
	public NovaModalSize Size { get; set; }

	[Parameter]
	public NovaModalAnimation Animation { get; set; }

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public RenderFragment? Body { get; set; }

	[Parameter]
	public RenderFragment? Footer { get; set; }

	[Parameter]
	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible != value)
			{
				_visible = value;
				if (value)
				{
					InvokeAsync((Func<Task>)Show);
				}
				else
				{
					InvokeAsync((Func<Task>)Hide);
				}
			}
		}
	}

	[Parameter]
	public EventCallback OnYes { get; set; }

	[Parameter]
	public EventCallback OnClose { get; set; }

	[Parameter]
	public bool IsDrawer { get; set; }

	[Parameter]
	public NovaAdminDrawerPlacement DrawerPlacement { get; set; } = NovaAdminDrawerPlacement.Right;

	/// <summary>
	/// 抽屉宽度（用于 Left/Right），例如 "400px" 或 "30%"
	/// </summary>
	[Parameter]
	public string DrawerWidth { get; set; } = "30%";

	/// <summary>
	/// 抽屉高度（用于 Top/Bottom），例如 "300px" 或 "50%"
	/// </summary>
	[Parameter]
	public string DrawerHeight { get; set; } = "40%";

	/// <summary>
	/// 离对边的距离（例如 Placement=Right 时，设置离左边的距离）
	/// </summary>
	[Parameter]
	public string? DrawerOffset { get; set; }

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
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender && _visible && !_visibleNotifyChanged)
		{
			_visibleNotifyChanged = true;
			await admin.CascadeSource.NotifyChangedAsync();
		}
	}

	private async Task Show()
	{
		_visibleNotifyChanged = false;
		await admin.OpenModal(this);
		await JS.InvokeVoidAsync("novaAdminJS.modalShow", ClientId, DotNetObjectReference.Create(this));
	}

	[JSInvokable]
	public async Task ModalOnClose()
	{
		_visibleNotifyChanged = false;
		await admin.CloseModal(this);
		if (OnClose.HasDelegate)
		{
			await OnClose.InvokeAsync();
		}
	}

	internal async Task Hide()
	{
		if (!_hiding)
		{
			_hiding = true;
			await JS.InvokeVoidAsync("eval", "$('#" + ClientId + "').modal('hide')");
			if (OnClose.HasDelegate)
			{
				await OnClose.InvokeAsync();
			}
			_hiding = false;
		}
	}

	internal async Task YesClick()
	{
		if (Interlocked.Exchange(ref _clickIncr, 1) != 0)
		{
			return;
		}
		try
		{
			if (OnYes.HasDelegate)
			{
				await OnYes.InvokeAsync();
			}
		}
		finally
		{
			Interlocked.Exchange(ref _clickIncr, 0);
		}
	}

	internal string GetDrawerStyle()
	{
		if (!IsDrawer)
		{
			return null;
		}
		List<string> list = new List<string>();
		switch (DrawerPlacement)
		{
		case NovaAdminDrawerPlacement.Left:
			list.Add("width: " + DrawerWidth + ";");
			if (!string.IsNullOrEmpty(DrawerOffset))
			{
				list.Add("right: " + DrawerOffset + ";");
			}
			break;
		case NovaAdminDrawerPlacement.Right:
			list.Add("width: " + DrawerWidth + ";");
			if (!string.IsNullOrEmpty(DrawerOffset))
			{
				list.Add("left: " + DrawerOffset + ";");
			}
			break;
		case NovaAdminDrawerPlacement.Top:
			list.Add("height: " + DrawerHeight + ";");
			if (!string.IsNullOrEmpty(DrawerOffset))
			{
				list.Add("bottom: " + DrawerOffset + ";");
			}
			break;
		case NovaAdminDrawerPlacement.Bottom:
			list.Add("height: " + DrawerHeight + ";");
			if (!string.IsNullOrEmpty(DrawerOffset))
			{
				list.Add("top: " + DrawerOffset + ";");
			}
			break;
		}
		return string.Join(" ", list);
	}

	internal string GetDrawerClass()
	{
		if (!IsDrawer)
		{
			return null;
		}
		return "modal-drawer modal-drawer-" + DrawerPlacement.ToString().ToLower();
	}

	internal string GetDialogClass()
	{
		NovaModalSize size = Size;
		if (1 == 0)
		{
		}
		string result = size switch
		{
			NovaModalSize.Small => DialogClassName + " modal-sm", 
			NovaModalSize.Large => DialogClassName + " modal-lg", 
			NovaModalSize.ExtraLarge => DialogClassName + " modal-xl", 
			NovaModalSize.Extra2Large => DialogClassName + " modal-xxl", 
			NovaModalSize.FullScreen => DialogClassName + " modal-fullscreen", 
			_ => DialogClassName ?? "", 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
