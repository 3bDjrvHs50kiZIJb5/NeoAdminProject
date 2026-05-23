using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using __Blazor.NoAdmin.Blazor.Pages.Org;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Org")]
public class Org : ComponentBase
{
	private NovaAdminQueryInfo q;

	private SysOrg item;

	private SysOrg orgAdd;

	[Inject]
	private IAggregateRootRepository<SysOrg> repo { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "组织结构");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysOrg>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(-1));
		__builder.AddComponentParameter(6, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(7, "TableTd1Width", RuntimeHelpers.TypeCheck(400));
		__builder.AddComponentParameter(8, "FixedLeftColumns", RuntimeHelpers.TypeCheck(2));
		__builder.AddComponentParameter(9, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(10, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(11, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysOrg>>)OnQuery)));
		__builder.AddComponentParameter(12, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysOrg, Task>)OnEdit)));
		__builder.AddComponentParameter(13, "OnRemoving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysOrg>>, Task>)OnRemoving)));
		__builder.AddAttribute(14, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(15, "<th width=\"120\">类型</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(16, "<th width=\"55\">排序</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(17, "<th width=\"55\">可用</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(18, "<th>备注</th>");
		});
		__builder.AddAttribute(19, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(20, "组织名称");
		});
		__builder.AddAttribute(21, "TableTd1", (RenderFragment<SysOrg>)((SysOrg item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(22, item.Label);
			renderTreeBuilder.AddMarkupContent(23, "\r\n        ");
			renderTreeBuilder.OpenElement(24, "button");
			renderTreeBuilder.AddAttribute(25, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginAdd(item)));
			renderTreeBuilder.AddAttribute(26, "type", "button");
			renderTreeBuilder.AddAttribute(27, "class", "ml-2 btn btn-light btn-xs float-end");
			renderTreeBuilder.AddMarkupContent(28, "<i class=\"fa fa-plus\"></i>");
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(29, "TableRow", (RenderFragment<SysOrg>)((SysOrg item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(30, "td");
			renderTreeBuilder.AddContent(31, item.Type);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(32, "\r\n        ");
			renderTreeBuilder.OpenElement(33, "td");
			renderTreeBuilder.AddContent(34, item.Sort);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(35, "\r\n        ");
			renderTreeBuilder.OpenElement(36, "td");
			renderTreeBuilder.AddContent(37, item.IsEnabled ? "-" : "否");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(38, "\r\n        ");
			renderTreeBuilder.OpenElement(39, "td");
			renderTreeBuilder.AddContent(40, item.Description);
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(41, "EditTemplate", (RenderFragment<SysOrg>)((SysOrg item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(42, "div");
			renderTreeBuilder.AddAttribute(43, "class", "row");
			renderTreeBuilder.OpenElement(44, "div");
			renderTreeBuilder.AddAttribute(45, "class", "form-group col-6");
			renderTreeBuilder.AddMarkupContent(46, "<label class=\"form-label\">组织名称</label>\r\n                ");
			renderTreeBuilder.OpenElement(47, "input");
			renderTreeBuilder.AddAttribute(48, "type", "text");
			renderTreeBuilder.AddAttribute(49, "class", "form-control");
			renderTreeBuilder.AddAttribute(50, "placeholder", "");
			renderTreeBuilder.AddAttribute(51, "maxlength", "50");
			renderTreeBuilder.AddAttribute(52, "data-valid", "true");
			renderTreeBuilder.AddAttribute(53, "value", BindConverter.FormatValue(item.Label));
			renderTreeBuilder.AddAttribute(54, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Label = __value;
			}, item.Label));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(55, "\r\n\r\n        ");
			renderTreeBuilder.OpenElement(56, "div");
			renderTreeBuilder.AddAttribute(57, "class", "row");
			renderTreeBuilder.OpenElement(58, "div");
			renderTreeBuilder.AddAttribute(59, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(60, "<label class=\"form-label\">父组织</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaInputTable<SysOrg, long>>(61);
			renderTreeBuilder.AddComponentParameter(62, "DisplayText", (Func<SysOrg, string>)((SysOrg a) => $"[{a.Id}]{a.Label}"));
			renderTreeBuilder.AddComponentParameter(63, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(64, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(NovaAdminQueryEventArgs<SysOrg> e)
			{
				e.Select.Where((Expression<Func<SysOrg, bool>>)((SysOrg a) => a.IsEnabled)).OrderBy<int>((Expression<Func<SysOrg, int>>)((SysOrg a) => a.Sort));
			})));
			renderTreeBuilder.AddComponentParameter(65, "Value", RuntimeHelpers.TypeCheck(item.ParentId));
			renderTreeBuilder.AddComponentParameter(66, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(long __value)
			{
				item.ParentId = __value;
			}, item.ParentId))));
			renderTreeBuilder.AddComponentParameter(67, "Item", RuntimeHelpers.TypeCheck(item.Parent));
			renderTreeBuilder.AddComponentParameter(68, "ItemChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysOrg __value)
			{
				item.Parent = __value;
			}, item.Parent))));
			renderTreeBuilder.AddAttribute(69, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(70, "组织名称");
			});
			renderTreeBuilder.AddAttribute(71, "TableTd1", (RenderFragment<SysOrg>)((SysOrg sysOrg) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddContent(72, sysOrg.Label);
			}));
			renderTreeBuilder.AddAttribute(73, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(74, "<th>类型</th>\r\n                        ");
				renderTreeBuilder2.AddMarkupContent(75, "<th>排序</th>\r\n                        ");
				renderTreeBuilder2.AddMarkupContent(76, "<th>备注</th>");
			});
			renderTreeBuilder.AddAttribute(77, "TableRow", (RenderFragment<SysOrg>)((SysOrg sysOrg) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(78, "td");
				renderTreeBuilder2.AddContent(79, sysOrg.Type);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(80, "\r\n                        ");
				renderTreeBuilder2.OpenElement(81, "td");
				renderTreeBuilder2.AddContent(82, sysOrg.Sort);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(83, "\r\n                        ");
				renderTreeBuilder2.OpenElement(84, "td");
				renderTreeBuilder2.AddContent(85, item.Description);
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(86, "\r\n\r\n        ");
			renderTreeBuilder.OpenElement(87, "div");
			renderTreeBuilder.AddAttribute(88, "class", "row");
			renderTreeBuilder.OpenElement(89, "div");
			renderTreeBuilder.AddAttribute(90, "class", "form-group col-6");
			renderTreeBuilder.AddMarkupContent(91, "<label class=\"form-label\">组织类型</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaSelectEnum<SysOrg.OrgType>>(92);
			renderTreeBuilder.AddComponentParameter(93, "Value", RuntimeHelpers.TypeCheck(item.Type));
			renderTreeBuilder.AddComponentParameter(94, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysOrg.OrgType __value)
			{
				item.Type = __value;
			}, item.Type))));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(95, "\r\n            ");
			renderTreeBuilder.OpenElement(96, "div");
			renderTreeBuilder.AddAttribute(97, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(98, "<label class=\"form-label\">排序</label>\r\n                ");
			renderTreeBuilder.OpenElement(99, "input");
			renderTreeBuilder.AddAttribute(100, "type", "number");
			renderTreeBuilder.AddAttribute(101, "class", "form-control");
			renderTreeBuilder.AddAttribute(102, "data-valid", "true");
			renderTreeBuilder.AddAttribute(103, "value", BindConverter.FormatValue(item.Sort, CultureInfo.InvariantCulture));
			renderTreeBuilder.AddAttribute(104, "onchange", EventCallback.Factory.CreateBinder(this, delegate(int __value)
			{
				item.Sort = __value;
			}, item.Sort, CultureInfo.InvariantCulture));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(105, "\r\n            ");
			renderTreeBuilder.OpenElement(106, "div");
			renderTreeBuilder.AddAttribute(107, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(108, "<label class=\"form-label\">是否可用</label>\r\n                ");
			TypeInference.CreateCheckbox_0(renderTreeBuilder, 109, 110, item.IsEnabled, 111, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
			{
				item.IsEnabled = __value;
			}, item.IsEnabled)), 112, () => item.IsEnabled);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(113, "\r\n\r\n        ");
			renderTreeBuilder.OpenElement(114, "div");
			renderTreeBuilder.AddAttribute(115, "class", "row");
			renderTreeBuilder.OpenElement(116, "div");
			renderTreeBuilder.AddAttribute(117, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(118, "<label class=\"form-label\">备注</label>\r\n                ");
			renderTreeBuilder.OpenElement(119, "textarea");
			renderTreeBuilder.AddAttribute(120, "class", "form-control");
			renderTreeBuilder.AddAttribute(121, "placeholder", "");
			renderTreeBuilder.AddAttribute(122, "maxlength", "500");
			renderTreeBuilder.AddAttribute(123, "rows", "5");
			renderTreeBuilder.AddAttribute(124, "value", BindConverter.FormatValue(item.Description));
			renderTreeBuilder.AddAttribute(125, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Description = __value;
			}, item.Description));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		}));
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		q = e;
		e.Filters = new NovaAdminFilterInfo[1]
		{
			new NovaAdminFilterInfo("类型", "Type", multiple: true, 12, string.Join(",", Enum.GetNames(typeof(SysOrg.OrgType))), string.Join(",", from a in Enum.GetNames(typeof(SysOrg.OrgType))
				select (int)Enum.Parse<SysOrg.OrgType>(a)))
		};
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysOrg> e)
	{
		e.Select.WhereIf(e.Filters[0].HasValue, (Expression<Func<SysOrg, bool>>)((SysOrg a) => e.Filters[0].Values<SysOrg.OrgType>().Contains(a.Type))).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysOrg, bool>>)((SysOrg a) => a.Label.Contains(e.SearchText) || a.Description.Contains(e.SearchText))).OrderBy<int>((Expression<Func<SysOrg, int>>)((SysOrg a) => a.Sort));
	}

	private async Task OnEdit(SysOrg org)
	{
		item = org;
		if (orgAdd != null)
		{
			item.ParentId = orgAdd.Id;
			item.Parent = orgAdd;
			SysOrg sysOrg = item;
			sysOrg.Sort = await ((IBaseRepository<SysOrg>)(object)repo).Where((Expression<Func<SysOrg, bool>>)((SysOrg a) => a.ParentId == orgAdd.Id)).MaxAsync<int>((Expression<Func<SysOrg, int>>)((SysOrg a) => a.Sort), default(CancellationToken)) + 1;
		}
		if (org.Parent == null && org.ParentId > 0)
		{
			SysOrg sysOrg2 = org;
			sysOrg2.Parent = await ((ISelect0<ISelect<SysOrg>, SysOrg>)(object)((IBaseRepository<SysOrg>)(object)repo).Where((Expression<Func<SysOrg, bool>>)((SysOrg a) => a.Id == org.ParentId))).FirstAsync(default(CancellationToken));
		}
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysOrg>> e)
	{
		await FreeSqlGlobalExtensions.IncludeManyAsync<SysOrg, SysOrg>(e.Argument, ((IBaseRepository)repo).Orm, (Expression<Func<SysOrg, IEnumerable<SysOrg>>>)((SysOrg a) => a.Children), (Action<ISelect<SysOrg>>)null, default(CancellationToken));
	}

	private async Task BeginAdd(SysOrg org)
	{
		orgAdd = org;
		await q.InvokeAddAsync();
		orgAdd = null;
	}
}
