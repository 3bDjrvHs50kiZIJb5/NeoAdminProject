using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/TenantDatabase")]
public class TenantDatabase : ComponentBase
{
	private SysTenantDatabase editItem;

	[Inject]
	private IAggregateRootRepository<SysTenantDatabase> repo { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "数据库");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysTenantDatabase>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(30));
		__builder.AddComponentParameter(6, "Title", "数据库");
		__builder.AddComponentParameter(7, "SearchPlaceholder", "显示名..");
		__builder.AddComponentParameter(8, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(9, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysTenantDatabase>>)OnQuery)));
		__builder.AddComponentParameter(10, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(11, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysTenantDatabase, Task>)OnEdit)));
		__builder.AddAttribute(12, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(13, "<th>显示名</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(14, "<th>数据库</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(15, "<th>连接串</th>\r\n        ");
			renderTreeBuilder.OpenElement(16, "th");
			renderTreeBuilder.OpenComponent<NovaAdminSort>(17);
			renderTreeBuilder.AddComponentParameter(18, "Text", "创建时间");
			renderTreeBuilder.AddComponentParameter(19, "Value", "CreatedTime");
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(20, "\r\n        ");
			renderTreeBuilder.OpenElement(21, "th");
			renderTreeBuilder.OpenComponent<NovaAdminSort>(22);
			renderTreeBuilder.AddComponentParameter(23, "Text", "修改时间");
			renderTreeBuilder.AddComponentParameter(24, "Value", "ModifiedTime");
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
		});
		__builder.AddAttribute(25, "TableRow", (RenderFragment<SysTenantDatabase>)((SysTenantDatabase item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			renderTreeBuilder.OpenElement(26, "td");
			renderTreeBuilder.AddContent(27, item.Label);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(28, "\r\n        ");
			renderTreeBuilder.OpenElement(29, "td");
			renderTreeBuilder.AddContent(30, item.DataType);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(31, "\r\n        ");
			renderTreeBuilder.OpenElement(32, "td");
			renderTreeBuilder.AddContent(33, item.ConenctionString);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(34, "\r\n        ");
			renderTreeBuilder.OpenElement(35, "td");
			renderTreeBuilder.AddContent(36, item?.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(37, "\r\n        ");
			renderTreeBuilder.OpenElement(38, "td");
			renderTreeBuilder.AddContent(39, item?.ModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(40, "EditTemplate", (RenderFragment<SysTenantDatabase>)((SysTenantDatabase item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			renderTreeBuilder.OpenElement(41, "div");
			renderTreeBuilder.AddAttribute(42, "class", "row");
			renderTreeBuilder.OpenElement(43, "div");
			renderTreeBuilder.AddAttribute(44, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(45, "<label class=\"form-label\">显示名</label>\r\n                ");
			renderTreeBuilder.OpenElement(46, "input");
			renderTreeBuilder.AddAttribute(47, "type", "text");
			renderTreeBuilder.AddAttribute(48, "class", "form-control");
			renderTreeBuilder.AddAttribute(49, "placeholder", "");
			renderTreeBuilder.AddAttribute(50, "maxlength", "50");
			renderTreeBuilder.AddAttribute(51, "value", BindConverter.FormatValue(item.Label));
			renderTreeBuilder.AddAttribute(52, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Label = __value;
			}, item.Label));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(53, "\r\n            ");
			renderTreeBuilder.OpenElement(54, "div");
			renderTreeBuilder.AddAttribute(55, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(56, "<label class=\"form-label\">数据库</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaSelectEnum<DataType>>(57);
			renderTreeBuilder.AddComponentParameter(58, "Value", RuntimeHelpers.TypeCheck<DataType>(item.DataType));
			renderTreeBuilder.AddComponentParameter(59, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(DataType __value)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				item.DataType = __value;
			}, item.DataType))));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(60, "\r\n            ");
			renderTreeBuilder.OpenElement(61, "div");
			renderTreeBuilder.AddAttribute(62, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(63, "<label class=\"form-label\">连接串</label>\r\n                ");
			renderTreeBuilder.OpenElement(64, "textarea");
			renderTreeBuilder.AddAttribute(65, "class", "form-control");
			renderTreeBuilder.AddAttribute(66, "placeholder", "");
			renderTreeBuilder.AddAttribute(67, "maxlength", "500");
			renderTreeBuilder.AddAttribute(68, "rows", "5");
			renderTreeBuilder.AddAttribute(69, "value", BindConverter.FormatValue(item.ConenctionString));
			renderTreeBuilder.AddAttribute(70, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.ConenctionString = __value;
			}, item.ConenctionString));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(71, "\r\n                ConenctionString.Replace(\"{database}\", 租户名称)\r\n            ");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		}));
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		e.Filters = new NovaAdminFilterInfo[1]
		{
			new NovaAdminFilterInfo("数据库", "DataType", "MySql,SqlServer,PostgreSQL,Oracle,Sqlite,达梦,人大金仓,南大通用,虚谷,神通,QuestDB,TDengine,Firebird,ClickHouse,DuckDB", "0,1,2,3,4,12,15,19,21,14,20,27,16,18,26")
		};
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysTenantDatabase> e)
	{
		ISelect<SysTenantDatabase> obj = e.Select.WhereIf(e.Filters[0].HasValue, (Expression<Func<SysTenantDatabase, bool>>)((SysTenantDatabase a) => (int)a.DataType == (int)e.Filters[0].Value<DataType>())).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysTenantDatabase, bool>>)((SysTenantDatabase a) => a.Label.Contains(e.SearchText)));
		bool num = !e.Sort.IsNull();
		string obj2 = e.Sort?.Replace("@desc", "");
		string sort = e.Sort;
		((ISelect0<ISelect<SysTenantDatabase>, SysTenantDatabase>)(object)obj).OrderByPropertyNameIf(num, obj2, sort == null || !sort.Contains("@desc"));
	}

	private async Task OnEdit(SysTenantDatabase item)
	{
		editItem = item;
		await Task.Yield();
	}
}
