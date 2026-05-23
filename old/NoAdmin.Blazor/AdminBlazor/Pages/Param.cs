using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Param")]
public class Param : ComponentBase
{
	[Inject]
	private IAggregateRootRepository<SysParam> repo { get; set; }

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "参数配置");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysParam>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(30));
		__builder.AddComponentParameter(6, "Title", "参数配置");
		__builder.AddComponentParameter(7, "SearchPlaceholder", "编码/描述/备注..");
		__builder.AddComponentParameter(8, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(9, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysParam>>)OnQuery)));
		__builder.AddComponentParameter(10, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysParam, Task>)OnEdit)));
		__builder.AddAttribute(11, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(12, "<th>编码</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(13, "<th>描述</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(14, "<th>值1</th>\r\n        ");
			renderTreeBuilder.OpenElement(15, "th");
			renderTreeBuilder.OpenComponent<NovaAdminSort>(16);
			renderTreeBuilder.AddComponentParameter(17, "Text", "排序");
			renderTreeBuilder.AddComponentParameter(18, "Value", "Sort");
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(19, "\r\n        ");
			renderTreeBuilder.AddMarkupContent(20, "<th>启用</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(21, "<th>备注</th>");
		});
		__builder.AddAttribute(22, "TableRow", (RenderFragment<SysParam>)((SysParam item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(23, "td");
			renderTreeBuilder.AddContent(24, item.Id);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(25, "\r\n        ");
			renderTreeBuilder.OpenElement(26, "td");
			renderTreeBuilder.AddContent(27, item.Title);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(28, "\r\n        ");
			renderTreeBuilder.OpenElement(29, "td");
			renderTreeBuilder.AddContent(30, item.Value);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(31, "\r\n        ");
			renderTreeBuilder.OpenElement(32, "td");
			renderTreeBuilder.AddContent(33, item.Sort);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(34, "\r\n        ");
			renderTreeBuilder.OpenElement(35, "td");
			renderTreeBuilder.AddMarkupContent(36, item.Enabled ? "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-success-bg-subtle); --bs-border-color: var(--bs-success-border-subtle); color: var(--bs-success-text);\">是</span>" : "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-danger-bg-subtle); --bs-border-color: var(--bs-danger-border-subtle); color: var(--bs-danger-text);\">否</span>");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(37, "\r\n        ");
			renderTreeBuilder.OpenElement(38, "td");
			renderTreeBuilder.AddContent(39, item.Description);
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(39, "EditTemplate", (RenderFragment<SysParam>)((SysParam item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(40, "div");
			renderTreeBuilder.AddAttribute(41, "class", "row");
			AddTextInput(renderTreeBuilder, 42, "编码", item.Id, value => item.Id = value, "col-6", "50");
			AddTextInput(renderTreeBuilder, 60, "描述", item.Title, value => item.Title = value, "col-6", "500");
			AddNumberInput(renderTreeBuilder, 78, "排序", item.Sort, value => item.Sort = value);
			renderTreeBuilder.OpenElement(96, "div");
			renderTreeBuilder.AddAttribute(97, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(98, "<label class=\"form-label\">启用</label>\r\n                ");
			renderTreeBuilder.OpenComponent<Switch>(99);
			renderTreeBuilder.AddComponentParameter(100, "OnColor", RuntimeHelpers.TypeCheck<Color>((Color)4));
			renderTreeBuilder.AddComponentParameter(101, "Value", RuntimeHelpers.TypeCheck(item.Enabled));
			renderTreeBuilder.AddComponentParameter(102, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
			{
				item.Enabled = __value;
			}, item.Enabled))));
			renderTreeBuilder.AddComponentParameter(103, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<bool>>>(() => item.Enabled));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			AddTextInput(renderTreeBuilder, 104, "值1", item.Value, value => item.Value = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 122, "值2", item.Value2, value => item.Value2 = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 140, "值3", item.Value3, value => item.Value3 = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 158, "值4", item.Value4, value => item.Value4 = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 176, "值5", item.Value5, value => item.Value5 = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 194, "值6", item.Value6, value => item.Value6 = value, "col-12", "1024");
			AddTextInput(renderTreeBuilder, 212, "值7", item.Value7, value => item.Value7 = value, "col-12", "1024");
			AddTextarea(renderTreeBuilder, 230, "备注", item.Description, value => item.Description = value);
			renderTreeBuilder.CloseElement();
		}));
		__builder.CloseComponent();
	}

	private void AddTextInput(RenderTreeBuilder builder, int sequence, string label, string value, Action<string> setter, string columnClass, string maxLength)
	{
		builder.OpenElement(sequence, "div");
		builder.AddAttribute(sequence + 1, "class", $"form-group {columnClass}");
		builder.AddMarkupContent(sequence + 2, $"<label class=\"form-label\">{label}</label>\r\n                ");
		builder.OpenElement(sequence + 3, "input");
		builder.AddAttribute(sequence + 4, "type", "text");
		builder.AddAttribute(sequence + 5, "class", "form-control");
		builder.AddAttribute(sequence + 6, "maxlength", maxLength);
		builder.AddAttribute(sequence + 7, "value", BindConverter.FormatValue(value));
		builder.AddAttribute(sequence + 8, "onchange", EventCallback.Factory.CreateBinder(this, setter, value));
		builder.SetUpdatesAttributeName("value");
		builder.CloseElement();
		builder.CloseElement();
	}

	private void AddNumberInput(RenderTreeBuilder builder, int sequence, string label, int value, Action<int> setter)
	{
		builder.OpenElement(sequence, "div");
		builder.AddAttribute(sequence + 1, "class", "form-group col-3");
		builder.AddMarkupContent(sequence + 2, $"<label class=\"form-label\">{label}</label>\r\n                ");
		builder.OpenElement(sequence + 3, "input");
		builder.AddAttribute(sequence + 4, "type", "number");
		builder.AddAttribute(sequence + 5, "class", "form-control");
		builder.AddAttribute(sequence + 6, "value", BindConverter.FormatValue(value, CultureInfo.InvariantCulture));
		builder.AddAttribute(sequence + 7, "onchange", EventCallback.Factory.CreateBinder(this, setter, value, CultureInfo.InvariantCulture));
		builder.SetUpdatesAttributeName("value");
		builder.CloseElement();
		builder.CloseElement();
	}

	private void AddTextarea(RenderTreeBuilder builder, int sequence, string label, string value, Action<string> setter)
	{
		builder.OpenElement(sequence, "div");
		builder.AddAttribute(sequence + 1, "class", "form-group col-12");
		builder.AddMarkupContent(sequence + 2, $"<label class=\"form-label\">{label}</label>\r\n                ");
		builder.OpenElement(sequence + 3, "textarea");
		builder.AddAttribute(sequence + 4, "class", "form-control");
		builder.AddAttribute(sequence + 5, "maxlength", "500");
		builder.AddAttribute(sequence + 6, "rows", "5");
		builder.AddAttribute(sequence + 7, "value", BindConverter.FormatValue(value));
		builder.AddAttribute(sequence + 8, "onchange", EventCallback.Factory.CreateBinder(this, setter, value));
		builder.SetUpdatesAttributeName("value");
		builder.CloseElement();
		builder.CloseElement();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysParam> e)
	{
		ISelect<SysParam> query = e.Select.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysParam, bool>>)((SysParam a) => a.Id.Contains(e.SearchText) || a.Title.Contains(e.SearchText) || a.Description.Contains(e.SearchText)));
		bool hasSort = !e.Sort.IsNull();
		string sort = e.Sort?.Replace("@desc", "");
		((ISelect0<ISelect<SysParam>, SysParam>)(object)query).OrderByPropertyNameIf(hasSort, sort, e.Sort == null || !e.Sort.Contains("@desc")).OrderByIf<int>(e.Sort.IsNull(), (Expression<Func<SysParam, int>>)((SysParam a) => a.Sort));
	}

	private async Task OnEdit(SysParam item)
	{
		if (!((IBaseRepository<SysParam>)(object)repo).Select.Any((Expression<Func<SysParam, bool>>)((SysParam a) => a.Id == item.Id)))
		{
			SysParam sysParam = item;
			sysParam.Sort = await ((IBaseRepository<SysParam>)(object)repo).Select.MaxAsync<int>((Expression<Func<SysParam, int>>)((SysParam a) => a.Sort), default(CancellationToken)) + 1;
		}
		await Task.Yield();
	}
}
