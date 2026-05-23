using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Rougamo;
using Rougamo.Context;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Role")]
public class Role : ComponentBase
{
	private SysRole allocItemUsers;

	private SysRole allocItemMenus;

	[Inject]
	private NovaAdminContext admin { get; set; }

	[Inject]
	private IAggregateRootRepository<SysRole> repo { get; set; }

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
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "角色");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysRole>>(4);
		__builder.AddComponentParameter(5, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(30));
		__builder.AddComponentParameter(6, "SearchPlaceholder", "名称/备注..");
		__builder.AddComponentParameter(7, "TableTd99Width", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(265));
		__builder.AddComponentParameter(8, "FixedRightColumns", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(9, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysRole>>)OnQuery)));
		__builder.AddComponentParameter(10, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(11, "OnEdit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysRole, Task>)OnEdit)));
		__builder.AddComponentParameter(12, "OnSaving", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<SysRole>, Task>)OnSaving)));
		__builder.AddComponentParameter(13, "OnRemoving", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysRole>>, Task>)OnRemoving)));
		__builder.AddAttribute(14, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(15, "<th>名称</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(16, "<th>备注</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(17, "<th>管理员</th>");
		});
		__builder.AddAttribute(18, "TableRow", (RenderFragment<SysRole>)((SysRole item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(19, "td");
			renderTreeBuilder.AddContent(20, item.Name);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(21, "\r\n        ");
			renderTreeBuilder.OpenElement(22, "td");
			renderTreeBuilder.AddContent(23, item.Description);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(24, "\r\n        ");
			renderTreeBuilder.OpenElement(25, "td");
			if (item.IsAdministrator)
			{
				renderTreeBuilder.AddMarkupContent(26, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-success-bg-subtle); --bs-border-color: var(--bs-success-border-subtle); color: var(--bs-success-text);\">是</span>");
			}
			else
			{
				renderTreeBuilder.AddMarkupContent(27, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-danger-bg-subtle); --bs-border-color: var(--bs-danger-border-subtle); color: var(--bs-danger-text);\">否</span>");
			}
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(28, "TableTd99", (RenderFragment<SysRole>)((SysRole item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(29, "button");
			renderTreeBuilder.AddAttribute(30, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginAllocUsers(item)));
			renderTreeBuilder.AddAttribute(31, "type", "button");
			renderTreeBuilder.AddAttribute(32, "class", "mr-2 btn btn-light btn-xs");
			renderTreeBuilder.AddMarkupContent(33, "<i class=\"fa fa-user-secret\"></i>分配用户");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(34, "\r\n        ");
			renderTreeBuilder.OpenElement(35, "button");
			renderTreeBuilder.AddAttribute(36, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginAllocMenus(item)));
			renderTreeBuilder.AddAttribute(37, "type", "button");
			renderTreeBuilder.AddAttribute(38, "class", "mr-2 btn btn-light btn-xs");
			renderTreeBuilder.AddAttribute(39, "disabled", item.IsAdministrator);
			renderTreeBuilder.AddMarkupContent(40, "<i class=\"fa fa-user-secret\"></i>分配菜单");
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(41, "EditTemplate", (RenderFragment<SysRole>)((SysRole item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(42, "div");
			renderTreeBuilder.AddAttribute(43, "class", "row");
			renderTreeBuilder.OpenElement(44, "div");
			renderTreeBuilder.AddAttribute(45, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(46, "<label class=\"form-label\">名称</label>\r\n                ");
			renderTreeBuilder.OpenElement(47, "input");
			renderTreeBuilder.AddAttribute(48, "type", "text");
			renderTreeBuilder.AddAttribute(49, "class", "form-control");
			renderTreeBuilder.AddAttribute(50, "placeholder", "");
			renderTreeBuilder.AddAttribute(51, "maxlength", "50");
			renderTreeBuilder.AddAttribute(52, "value", BindConverter.FormatValue(item.Name));
			renderTreeBuilder.AddAttribute(53, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Name = __value;
			}, item.Name));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(54, "\r\n            ");
			renderTreeBuilder.OpenElement(55, "div");
			renderTreeBuilder.AddAttribute(56, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(57, "<label class=\"form-label\">备注</label>\r\n                ");
			renderTreeBuilder.OpenElement(58, "textarea");
			renderTreeBuilder.AddAttribute(59, "class", "form-control");
			renderTreeBuilder.AddAttribute(60, "placeholder", "");
			renderTreeBuilder.AddAttribute(61, "maxlength", "500");
			renderTreeBuilder.AddAttribute(62, "style", "height:80px");
			renderTreeBuilder.AddAttribute(63, "value", BindConverter.FormatValue(item.Description));
			renderTreeBuilder.AddAttribute(64, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Description = __value;
			}, item.Description));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(65, "\r\n            ");
			renderTreeBuilder.OpenElement(66, "div");
			renderTreeBuilder.AddAttribute(67, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(68, "<label class=\"form-label\">IP白名单 <small class=\"text-muted\">(登陆后台 -&gt; 谷歌验证码)</small></label>\r\n                ");
			renderTreeBuilder.OpenElement(69, "textarea");
			renderTreeBuilder.AddAttribute(70, "class", "form-control");
			renderTreeBuilder.AddAttribute(71, "placeholder", "");
			renderTreeBuilder.AddAttribute(72, "maxlength", "500");
			renderTreeBuilder.AddAttribute(73, "style", "height:80px");
			renderTreeBuilder.AddAttribute(74, "value", BindConverter.FormatValue(item.IpWhiteList));
			renderTreeBuilder.AddAttribute(75, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.IpWhiteList = __value;
			}, item.IpWhiteList));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(76, "\r\n                ");
			renderTreeBuilder.OpenElement(77, "pre");
			renderTreeBuilder.AddMarkupContent(78, "当前IP: ");
			renderTreeBuilder.AddContent(79, admin.RemoteIp);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		}));
		__builder.CloseComponent();
		__builder.AddMarkupContent(80, "\r\n\r\n");
		__builder.OpenComponent<NovaAllocTable<SysRole, SysUser>>(81);
		__builder.AddComponentParameter(82, "ChildProperty", "Users");
		__builder.AddComponentParameter(83, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【分配用户】" + allocItemUsers?.Name));
		__builder.AddComponentParameter(84, "IsNotifyChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(85, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(50));
		__builder.AddComponentParameter(86, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(NovaAdminQueryEventArgs<SysUser> e)
		{
			e.Select.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysUser, bool>>)((SysUser a) => a.Username.Contains(e.SearchText)));
		})));
		__builder.AddComponentParameter(87, "Item", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(allocItemUsers));
		__builder.AddComponentParameter(88, "ItemChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysRole __value)
		{
			allocItemUsers = __value;
		}, allocItemUsers))));
		__builder.AddAttribute(89, "TableTd1", (RenderFragment<SysUser>)((SysUser context) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(90, context.Username);
		}));
		__builder.CloseComponent();
		__builder.AddMarkupContent(91, "\r\n\r\n");
		__builder.OpenComponent<NovaAllocTable<SysRole, SysMenu>>(92);
		__builder.AddComponentParameter(93, "ChildProperty", "Menus");
		__builder.AddComponentParameter(94, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【分配菜单】" + allocItemMenus?.Name));
		__builder.AddComponentParameter(95, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysMenu>>)BeginAllocMenusOnQuery)));
		__builder.AddComponentParameter(96, "Item", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(allocItemMenus));
		__builder.AddComponentParameter(97, "ItemChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysRole __value)
		{
			allocItemMenus = __value;
		}, allocItemMenus))));
		__builder.AddAttribute(98, "TableTd1", (RenderFragment<SysMenu>)((SysMenu context) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(99, context.Label);
		}));
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		e.Filters = new NovaAdminFilterInfo[0];
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysRole> e)
	{
		e.Select.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysRole, bool>>)((SysRole a) => a.Name.Contains(e.SearchText) || a.Description.Contains(e.SearchText)));
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysRole>> e)
	{
		if (e.Argument.Any((SysRole a) => a.IsAdministrator))
		{
			await MessageService.Error("不能删除系统角色!");
			e.Cancel = true;
		}
	}

	private async Task OnEdit(SysRole item)
	{
		await Task.Yield();
	}

	private async Task OnSaving(NovaAdminConfirmEventArgs<SysRole> e)
	{
		e.Argument.IpWhiteList = Regex.Replace(e.Argument.IpWhiteList?.Trim() ?? "", "\\b(\\r\\n|\\n|;)\\b", "; ");
	}

	[NovaButton("alloc_users")]
	private async Task BeginAllocUsers(SysRole item)
	{
		await _0024Rougamo_BeginAllocUsers(item);
	}

	[NovaButton("alloc_menus")]
	private async Task BeginAllocMenus(SysRole item)
	{
		await _0024Rougamo_BeginAllocMenus(item);
	}

	private void BeginAllocMenusOnQuery(NovaAdminQueryEventArgs<SysMenu> e)
	{
		IEnumerable<long> menuIds = admin.RoleMenus.Select((SysMenu b) => b.Id);
		e.Select.WhereIf(admin.Tenant.Id != "main", (Expression<Func<SysMenu, bool>>)((SysMenu a) => menuIds.Contains(a.Id))).OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort));
	}

	private async Task _0024Rougamo_BeginAllocUsers(SysRole item)
	{
		allocItemUsers = item;
		await Task.Yield();
	}

	private async Task _0024Rougamo_BeginAllocMenus(SysRole item)
	{
		allocItemMenus = item;
		await Task.Yield();
	}
}
