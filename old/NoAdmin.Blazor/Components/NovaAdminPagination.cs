using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAdminPagination : ComponentBase
{
	private int _page;

	private int _pageSize;

	private int _initPageSize;

	[Parameter]
	public NovaAdminQueryInfo AdminQuery { get; set; }

	[CascadingParameter]
	public NovaAdminContext.TabInfo TabInfo { get; set; }

	private NovaAdminQueryInfo q => AdminQuery;

	private int _forStart => Math.Max(1, q.PageNumber - 3);

	private int _forEnd => Math.Min(q.PageNumber + 3, q.MaxPageNumber);

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
		__builder.AddAttribute(1, "class", "adm-pg-container");
		__builder.OpenElement(2, "div");
		__builder.AddAttribute(3, "class", "adm-pg-statis");
		__builder.AddMarkupContent(4, "\r\n        共 ");
		__builder.OpenElement(5, "span");
		__builder.AddAttribute(6, "class", "adm-pg-total");
		__builder.AddContent(7, q.Total);
		__builder.CloseElement();
		__builder.AddMarkupContent(8, " 条记录\r\n    ");
		__builder.CloseElement();
		__builder.AddMarkupContent(9, "\r\n\r\n    ");
		__builder.OpenElement(10, "div");
		__builder.AddAttribute(11, "class", "adm-pg-pager");
		if (q.MaxPageNumber > 1)
		{
			__builder.OpenElement(12, "div");
			__builder.AddAttribute(13, "class", "adm-pg-node " + ((q.PageNumber <= 1) ? "is-disabled" : ""));
			__builder.AddAttribute(14, "title", "首页");
			__builder.AddAttribute(15, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnNodeClick(1)));
			__builder.AddMarkupContent(16, "<i class=\"fas fa-angles-left\"></i>");
			__builder.CloseElement();
			__builder.AddMarkupContent(17, "\r\n            ");
			__builder.OpenElement(18, "div");
			__builder.AddAttribute(19, "class", "adm-pg-node " + ((q.PageNumber <= 1) ? "is-disabled" : ""));
			__builder.AddAttribute(20, "title", "上一页");
			__builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnNodeClick(q.PageNumber - 1)));
			__builder.AddMarkupContent(22, "<i class=\"fas fa-angle-left\"></i>");
			__builder.CloseElement();
			if (_forStart > 1)
			{
				__builder.AddMarkupContent(23, "<div class=\"adm-pg-node is-ellipsis\">•••</div>");
			}
			for (int num = _forStart; num <= _forEnd; num++)
			{
				int capturedIndex = num;
				__builder.OpenElement(24, "div");
				__builder.AddAttribute(25, "class", "adm-pg-node " + ((num == q.PageNumber) ? "is-active" : ""));
				__builder.AddAttribute(26, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnNodeClick(capturedIndex)));
				__builder.AddContent(27, num);
				__builder.CloseElement();
			}
			if (_forEnd < q.MaxPageNumber)
			{
				__builder.AddMarkupContent(28, "<div class=\"adm-pg-node is-ellipsis\">•••</div>");
			}
			__builder.OpenElement(29, "div");
			__builder.AddAttribute(30, "class", "adm-pg-node " + ((q.PageNumber >= q.MaxPageNumber) ? "is-disabled" : ""));
			__builder.AddAttribute(31, "title", "下一页");
			__builder.AddAttribute(32, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnNodeClick(q.PageNumber + 1)));
			__builder.AddMarkupContent(33, "<i class=\"fas fa-angle-right\"></i>");
			__builder.CloseElement();
			__builder.AddMarkupContent(34, "\r\n            ");
			__builder.OpenElement(35, "div");
			__builder.AddAttribute(36, "class", "adm-pg-node " + ((q.PageNumber >= q.MaxPageNumber) ? "is-disabled" : ""));
			__builder.AddAttribute(37, "title", "尾页");
			__builder.AddAttribute(38, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnNodeClick(q.MaxPageNumber)));
			__builder.AddMarkupContent(39, "<i class=\"fas fa-angles-right\"></i>");
			__builder.CloseElement();
		}
		__builder.CloseElement();
		__builder.AddMarkupContent(40, "\r\n\r\n    ");
		__builder.OpenElement(41, "div");
		__builder.AddAttribute(42, "class", "adm-pg-ops");
		__builder.OpenElement(43, "div");
		__builder.AddAttribute(44, "class", "adm-pg-select-wrapper");
		__builder.OpenElement(45, "select");
		__builder.AddAttribute(46, "class", "adm-pg-select");
		__builder.AddAttribute(47, "onchange", EventCallback.Factory.Create((object)this, (Func<ChangeEventArgs, Task>)OnPageSizeChange));
		__builder.AddAttribute(48, "value", _pageSize);
		int[] array = new int[7] { 20, 30, 50, 100, 500, 1000, 5000 };
		foreach (int num3 in array)
		{
			__builder.OpenElement(49, "option");
			__builder.AddAttribute(50, "value", num3);
			__builder.AddContent(51, num3);
			__builder.AddMarkupContent(52, " 条/页");
			__builder.CloseElement();
		}
		__builder.CloseElement();
		__builder.CloseElement();
		if (q.MaxPageNumber >= 10)
		{
			__builder.OpenElement(53, "div");
			__builder.AddAttribute(54, "class", "adm-pg-jumper");
			__builder.AddMarkupContent(55, "<span>前往</span>\r\n                ");
			__builder.OpenElement(56, "input");
			__builder.AddAttribute(57, "type", "number");
			__builder.AddAttribute(58, "class", "adm-pg-input");
			__builder.AddAttribute(59, "onkeyup", EventCallback.Factory.Create((object)this, (Func<KeyboardEventArgs, Task>)OnJumpKeyHandler));
			__builder.AddEventPreventDefaultAttribute(60, "onkeyup", value: true);
			__builder.AddAttribute(61, "value", BindConverter.FormatValue(_page, CultureInfo.InvariantCulture));
			__builder.AddAttribute(62, "onchange", EventCallback.Factory.CreateBinder(this, delegate(int __value)
			{
				_page = __value;
			}, _page, CultureInfo.InvariantCulture));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(63, "\r\n                ");
			__builder.AddMarkupContent(64, "<span>页</span>");
			__builder.CloseElement();
		}
		__builder.CloseElement();
		__builder.CloseElement();
	}

	protected override void OnInitialized()
	{
		_page = q.PageNumber;
		_initPageSize = q.PageSize;
		if (q.IsQueryString)
		{
			_page = Nav.GetQueryStringValue(q.PageNumberQueryStringName, TabInfo?.Url).ConvertTo<int>();
			if (q.PageSize >= 0)
			{
				_pageSize = Nav.GetQueryStringValue(q.PageNumberQueryStringName + "size", TabInfo?.Url).ConvertTo<int>();
				if (_pageSize <= 0)
				{
					_pageSize = q.PageSize;
				}
			}
			q.PageNumber = (_page = Math.Max(1, _page));
			q.PageSize = _pageSize;
		}
		else
		{
			_pageSize = q.PageSize;
		}
	}

	private async Task OnNodeClick(int targetPage)
	{
		if (targetPage >= 1 && targetPage <= q.MaxPageNumber && targetPage != q.PageNumber)
		{
			q.PageNumber = (_page = targetPage);
			await NavigateToQuery();
		}
	}

	private async Task OnJumpKeyHandler(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
		{
			if (_page < 1)
			{
				_page = 1;
			}
			if (_page > q.MaxPageNumber)
			{
				_page = q.MaxPageNumber;
			}
			q.PageNumber = _page;
			await NavigateToQuery();
		}
	}

	private async Task OnPageSizeChange(ChangeEventArgs e)
	{
		_pageSize = e.Value.ConvertTo<int>();
		q.PageSize = _pageSize;
		q.PageNumber = (_page = 1);
		q.Total = 0L;
		await NavigateToQuery();
	}

	private async Task NavigateToQuery()
	{
		if (q.IsQueryString)
		{
			string url = Nav.GetUriWithQueryParameters(new Dictionary<string, object>
			{
				[q.PageNumberQueryStringName] = q.PageNumber,
				[q.PageNumberQueryStringName + "size"] = ((q.PageSize == _initPageSize) ? null : ((object)q.PageSize))
			});
			Nav.NavigateTo(url);
		}
		if (q.InvokeQueryAsync != null)
		{
			await q.InvokeQueryAsync();
		}
	}
}
