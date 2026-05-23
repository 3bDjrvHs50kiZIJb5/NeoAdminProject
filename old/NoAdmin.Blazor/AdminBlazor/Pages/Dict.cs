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

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Dict")]
public class Dict : ComponentBase
{
	private List<NovaAdminItem<SysDict>> selectedLefts = new List<NovaAdminItem<SysDict>>();

	private NovaAdminQueryInfo queryRight;

	private bool queryRightUsedSelectedLefts;

	[Inject]
	private IAggregateRootRepository<SysDict> repo { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "数据字典");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<Split>(4);
		__builder.AddComponentParameter(5, "ShowBarHandle", RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(6, "Basis", "22%");
		__builder.AddComponentParameter(7, "FirstPaneMinimumSize", "330px");
		__builder.AddAttribute(8, "FirstPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysDict>>(9);
			renderTreeBuilder.AddComponentParameter(10, "PageSize", RuntimeHelpers.TypeCheck(30));
			renderTreeBuilder.AddComponentParameter(11, "Title", "字典分类");
			renderTreeBuilder.AddComponentParameter(12, "SearchPlaceholder", "编码/备注..");
			renderTreeBuilder.AddComponentParameter(13, "IsMultiSelect", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(14, "IsSingleSelect", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(15, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(16, "IsRefersh", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(17, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(18, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysDict>>)OnQueryLeft)));
			renderTreeBuilder.AddComponentParameter(19, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysDict, Task>)OnEdit)));
			renderTreeBuilder.AddComponentParameter(20, "OnRemoving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysDict>>, Task>)OnRemoving)));
			renderTreeBuilder.AddComponentParameter(21, "OnRowClick", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<List<NovaAdminItem<SysDict>>, Task>)LeftRowClick)));
			renderTreeBuilder.AddAttribute(22, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(23, "<th>编码</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(24, "<th>备注</th>");
			});
			renderTreeBuilder.AddAttribute(25, "TableRow", (RenderFragment<SysDict>)((SysDict item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(26, "td");
				renderTreeBuilder2.AddContent(27, item.Name);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(28, "\r\n                ");
				renderTreeBuilder2.OpenElement(29, "td");
				renderTreeBuilder2.AddContent(30, item.Description);
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.AddAttribute(31, "EditTemplate", (RenderFragment<SysDict>)((SysDict item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(32, "div");
				renderTreeBuilder2.AddAttribute(33, "class", "row");
				renderTreeBuilder2.OpenElement(34, "div");
				renderTreeBuilder2.AddAttribute(35, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(36, "<label class=\"form-label\">编码</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(37, "input");
				renderTreeBuilder2.AddAttribute(38, "type", "text");
				renderTreeBuilder2.AddAttribute(39, "class", "form-control");
				renderTreeBuilder2.AddAttribute(40, "placeholder", "");
				renderTreeBuilder2.AddAttribute(41, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(42, "value", BindConverter.FormatValue(item.Name));
				renderTreeBuilder2.AddAttribute(43, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Name = __value;
				}, item.Name));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(44, "\r\n                    ");
				renderTreeBuilder2.OpenElement(45, "div");
				renderTreeBuilder2.AddAttribute(46, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(47, "<label class=\"form-label\">备注</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(48, "textarea");
				renderTreeBuilder2.AddAttribute(49, "class", "form-control");
				renderTreeBuilder2.AddAttribute(50, "placeholder", "");
				renderTreeBuilder2.AddAttribute(51, "maxlength", "500");
				renderTreeBuilder2.AddAttribute(52, "rows", "5");
				renderTreeBuilder2.AddAttribute(53, "value", BindConverter.FormatValue(item.Description));
				renderTreeBuilder2.AddAttribute(54, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Description = __value;
				}, item.Description));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.AddAttribute(55, "SecondPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysDict>>(56);
			renderTreeBuilder.AddComponentParameter(57, "PageSize", RuntimeHelpers.TypeCheck(20));
			renderTreeBuilder.AddComponentParameter(58, "Title", "字典数据");
			renderTreeBuilder.AddComponentParameter(59, "SearchPlaceholder", "名称/备注..");
			renderTreeBuilder.AddComponentParameter(60, "FixedLeftColumns", RuntimeHelpers.TypeCheck(3));
			renderTreeBuilder.AddComponentParameter(61, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
			renderTreeBuilder.AddComponentParameter(62, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(63, "IsRefersh", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(64, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(65, "IsAdd", RuntimeHelpers.TypeCheck(selectedLefts.Any()));
			renderTreeBuilder.AddComponentParameter(66, "IsEdit", RuntimeHelpers.TypeCheck(selectedLefts.Any()));
			renderTreeBuilder.AddComponentParameter(67, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysDict>>)OnQueryRight)));
			renderTreeBuilder.AddComponentParameter(68, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQueryRight));
			renderTreeBuilder.AddComponentParameter(69, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysDict, Task>)OnEditRight)));
			renderTreeBuilder.AddAttribute(70, "CardHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				if (selectedLefts.Any())
				{
					renderTreeBuilder2.OpenElement(71, "span");
					renderTreeBuilder2.AddMarkupContent(72, "选中: ");
					renderTreeBuilder2.AddContent(73, selectedLefts[0].Value.Name);
					renderTreeBuilder2.CloseElement();
				}
			});
			renderTreeBuilder.AddAttribute(74, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(75, "<th>名称</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(76, "<th>值1</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(77, "<th>值2</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(78, "<th>值3</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(79, "<th>值4</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(80, "<th>值5</th>\r\n                ");
				renderTreeBuilder2.OpenElement(81, "th");
				renderTreeBuilder2.OpenComponent<NovaAdminSort>(82);
				renderTreeBuilder2.AddComponentParameter(83, "Text", "排序");
				renderTreeBuilder2.AddComponentParameter(84, "Value", "Sort");
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(85, "\r\n                ");
				renderTreeBuilder2.AddMarkupContent(86, "<th>启用</th>");
			});
			renderTreeBuilder.AddAttribute(87, "TableRow", (RenderFragment<SysDict>)((SysDict item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(88, "td");
				renderTreeBuilder2.AddContent(89, item.Name);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(90, "\r\n                ");
				renderTreeBuilder2.OpenElement(91, "td");
				renderTreeBuilder2.AddContent(92, item.Value);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(93, "\r\n                ");
				renderTreeBuilder2.OpenElement(94, "td");
				renderTreeBuilder2.AddContent(95, item.Value2);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(96, "\r\n                ");
				renderTreeBuilder2.OpenElement(97, "td");
				renderTreeBuilder2.AddContent(98, item.Value3);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(99, "\r\n                ");
				renderTreeBuilder2.OpenElement(100, "td");
				renderTreeBuilder2.AddContent(101, item.Value4);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(102, "\r\n                ");
				renderTreeBuilder2.OpenElement(103, "td");
				renderTreeBuilder2.AddContent(104, item.Value5);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(105, "\r\n                ");
				renderTreeBuilder2.OpenElement(106, "td");
				renderTreeBuilder2.AddContent(107, item.Sort);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(108, "\r\n                ");
				renderTreeBuilder2.OpenElement(109, "td");
				if (item.Enabled)
				{
					renderTreeBuilder2.AddMarkupContent(110, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-success-bg-subtle); --bs-border-color: var(--bs-success-border-subtle); color: var(--bs-success-text);\">是</span>");
				}
				else
				{
					renderTreeBuilder2.AddMarkupContent(111, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-danger-bg-subtle); --bs-border-color: var(--bs-danger-border-subtle); color: var(--bs-danger-text);\">否</span>");
				}
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.AddAttribute(112, "EditTemplate", (RenderFragment<SysDict>)((SysDict item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				//IL_028b: Unknown result type (might be due to invalid IL or missing references)
				renderTreeBuilder2.OpenElement(113, "div");
				renderTreeBuilder2.AddAttribute(114, "class", "row");
				renderTreeBuilder2.OpenElement(115, "div");
				renderTreeBuilder2.AddAttribute(116, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(117, "<label class=\"form-label\">名称</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(118, "input");
				renderTreeBuilder2.AddAttribute(119, "type", "text");
				renderTreeBuilder2.AddAttribute(120, "class", "form-control");
				renderTreeBuilder2.AddAttribute(121, "placeholder", "");
				renderTreeBuilder2.AddAttribute(122, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(123, "value", BindConverter.FormatValue(item.Name));
				renderTreeBuilder2.AddAttribute(124, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Name = __value;
				}, item.Name));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(125, "\r\n                    ");
				renderTreeBuilder2.OpenElement(126, "div");
				renderTreeBuilder2.AddAttribute(127, "class", "form-group col-3");
				renderTreeBuilder2.AddMarkupContent(128, "<label class=\"form-label\">排序</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(129, "input");
				renderTreeBuilder2.AddAttribute(130, "type", "number");
				renderTreeBuilder2.AddAttribute(131, "class", "form-control");
				renderTreeBuilder2.AddAttribute(132, "placeholder", "");
				renderTreeBuilder2.AddAttribute(133, "value", BindConverter.FormatValue(item.Sort, CultureInfo.InvariantCulture));
				renderTreeBuilder2.AddAttribute(134, "onchange", EventCallback.Factory.CreateBinder(this, delegate(int __value)
				{
					item.Sort = __value;
				}, item.Sort, CultureInfo.InvariantCulture));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(135, "\r\n                    ");
				renderTreeBuilder2.OpenElement(136, "div");
				renderTreeBuilder2.AddAttribute(137, "class", "form-group col-3");
				renderTreeBuilder2.AddMarkupContent(138, "<label class=\"form-label\">启用</label>\r\n                        ");
				renderTreeBuilder2.OpenComponent<Switch>(139);
				renderTreeBuilder2.AddComponentParameter(140, "OnColor", RuntimeHelpers.TypeCheck<Color>((Color)4));
				renderTreeBuilder2.AddComponentParameter(141, "Value", RuntimeHelpers.TypeCheck(item.Enabled));
				renderTreeBuilder2.AddComponentParameter(142, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
				{
					item.Enabled = __value;
				}, item.Enabled))));
				renderTreeBuilder2.AddComponentParameter(143, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<bool>>>(() => item.Enabled));
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(144, "\r\n                    ");
				renderTreeBuilder2.OpenElement(145, "div");
				renderTreeBuilder2.AddAttribute(146, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(147, "<label class=\"form-label\">值1</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(148, "input");
				renderTreeBuilder2.AddAttribute(149, "type", "text");
				renderTreeBuilder2.AddAttribute(150, "class", "form-control");
				renderTreeBuilder2.AddAttribute(151, "placeholder", "");
				renderTreeBuilder2.AddAttribute(152, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(153, "value", BindConverter.FormatValue(item.Value));
				renderTreeBuilder2.AddAttribute(154, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Value = __value;
				}, item.Value));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(155, "\r\n                    ");
				renderTreeBuilder2.OpenElement(156, "div");
				renderTreeBuilder2.AddAttribute(157, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(158, "<label class=\"form-label\">值2</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(159, "input");
				renderTreeBuilder2.AddAttribute(160, "type", "text");
				renderTreeBuilder2.AddAttribute(161, "class", "form-control");
				renderTreeBuilder2.AddAttribute(162, "placeholder", "");
				renderTreeBuilder2.AddAttribute(163, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(164, "value", BindConverter.FormatValue(item.Value2));
				renderTreeBuilder2.AddAttribute(165, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Value2 = __value;
				}, item.Value2));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(166, "\r\n                    ");
				renderTreeBuilder2.OpenElement(167, "div");
				renderTreeBuilder2.AddAttribute(168, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(169, "<label class=\"form-label\">值3</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(170, "input");
				renderTreeBuilder2.AddAttribute(171, "type", "text");
				renderTreeBuilder2.AddAttribute(172, "class", "form-control");
				renderTreeBuilder2.AddAttribute(173, "placeholder", "");
				renderTreeBuilder2.AddAttribute(174, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(175, "value", BindConverter.FormatValue(item.Value3));
				renderTreeBuilder2.AddAttribute(176, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Value3 = __value;
				}, item.Value3));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(177, "\r\n                    ");
				renderTreeBuilder2.OpenElement(178, "div");
				renderTreeBuilder2.AddAttribute(179, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(180, "<label class=\"form-label\">值4</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(181, "input");
				renderTreeBuilder2.AddAttribute(182, "type", "text");
				renderTreeBuilder2.AddAttribute(183, "class", "form-control");
				renderTreeBuilder2.AddAttribute(184, "placeholder", "");
				renderTreeBuilder2.AddAttribute(185, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(186, "value", BindConverter.FormatValue(item.Value4));
				renderTreeBuilder2.AddAttribute(187, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Value4 = __value;
				}, item.Value4));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(188, "\r\n                    ");
				renderTreeBuilder2.OpenElement(189, "div");
				renderTreeBuilder2.AddAttribute(190, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(191, "<label class=\"form-label\">值5</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(192, "input");
				renderTreeBuilder2.AddAttribute(193, "type", "text");
				renderTreeBuilder2.AddAttribute(194, "class", "form-control");
				renderTreeBuilder2.AddAttribute(195, "placeholder", "");
				renderTreeBuilder2.AddAttribute(196, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(197, "value", BindConverter.FormatValue(item.Value5));
				renderTreeBuilder2.AddAttribute(198, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Value5 = __value;
				}, item.Value5));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.CloseComponent();
	}

	private void OnQueryLeft(NovaAdminQueryEventArgs<SysDict> e)
	{
		selectedLefts.Clear();
		if (queryRightUsedSelectedLefts && queryRight?.InvokeQueryAsync != null)
		{
			InvokeAsync(queryRight.InvokeQueryAsync);
		}
		e.Select.Where((Expression<Func<SysDict, bool>>)((SysDict a) => a.ParentId == 0)).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysDict, bool>>)((SysDict a) => a.Name.Contains(e.SearchText) || a.Description.Contains(e.SearchText)));
	}

	private async Task OnEdit(SysDict item)
	{
		item.ParentId = 0L;
		await Task.Yield();
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysDict>> e)
	{
		long[] ids = e.Argument.Select((SysDict a) => a.Id).ToArray();
		await ((IBaseRepository)repo).Orm.Delete<SysDict>().Where((Expression<Func<SysDict, bool>>)((SysDict a) => ids.Contains(a.ParentId))).ExecuteAffrowsAsync(default(CancellationToken));
		SysDict left = await ((ISelect0<ISelect<SysDict>, SysDict>)(object)((IBaseRepository)repo).Orm.Select<SysDict>().Where((Expression<Func<SysDict, bool>>)((SysDict a) => a.ParentId == 0 && !ids.Contains(a.Id)))).FirstAsync(default(CancellationToken));
		selectedLefts.Clear();
		if (left != null)
		{
			selectedLefts.Add(new NovaAdminItem<SysDict>(left)
			{
				Selected = true
			});
		}
		queryRight.IsTracking = selectedLefts.Any();
		await queryRight.InvokeQueryAsync();
	}

	private async Task LeftRowClick(List<NovaAdminItem<SysDict>> relateItems)
	{
		if (selectedLefts.FirstOrDefault() != relateItems[0])
		{
			if (selectedLefts.Any())
			{
				selectedLefts[0].RowClass = "";
			}
			relateItems[0].RowClass = "active";
			selectedLefts = relateItems;
			if (queryRight?.InvokeQueryAsync != null)
			{
				await queryRight.InvokeQueryAsync();
			}
		}
		await Task.Yield();
	}

	private async Task InitQueryRight(NovaAdminQueryInfo e)
	{
		queryRight = e;
		await Task.Yield();
	}

	private void OnQueryRight(NovaAdminQueryEventArgs<SysDict> e)
	{
		queryRightUsedSelectedLefts = selectedLefts.Any();
		ISelect<SysDict> obj = ((ISelect0<ISelect<SysDict>, SysDict>)(object)e.Select.WhereIf(selectedLefts.Any(), (Expression<Func<SysDict, bool>>)((SysDict a) => a.ParentId == selectedLefts[0].Value.Id))).Cancel((Func<bool>)(() => !selectedLefts.Any())).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysDict, bool>>)((SysDict a) => a.Name.Contains(e.SearchText) || a.Description.Contains(e.SearchText)));
		bool num = !e.Sort.IsNull();
		string obj2 = e.Sort?.Replace("@desc", "");
		string sort = e.Sort;
		((ISelect0<ISelect<SysDict>, SysDict>)(object)obj).OrderByPropertyNameIf(num, obj2, sort == null || !sort.Contains("@desc")).OrderByIf<int>(e.Sort.IsNull(), (Expression<Func<SysDict, int>>)((SysDict a) => a.Sort), false);
	}

	private async Task OnEditRight(SysDict item)
	{
		item.ParentId = selectedLefts[0].Value.Id;
		if (item.Id == 0)
		{
			SysDict sysDict = item;
			sysDict.Sort = await ((IBaseRepository)repo).Orm.Select<SysDict>().Where((Expression<Func<SysDict, bool>>)((SysDict a) => a.ParentId == item.ParentId)).MaxAsync<int>((Expression<Func<SysDict, int>>)((SysDict a) => a.Sort), default(CancellationToken)) + 1;
		}
		await Task.Yield();
	}
}
