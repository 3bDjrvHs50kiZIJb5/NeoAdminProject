using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAllocTable<TItem, TChild> : ComponentBase where TItem : class, new() where TChild : class, new()
{
	private TableInfo metaTItem;

	private TableInfo metaTChild;

	private List<NovaAdminItem<TChild>> currentItems;

	private Dictionary<string, NovaAdminItem<TChild>> allItems = new Dictionary<string, NovaAdminItem<TChild>>();

	/// <summary>
	/// 被分配的对象
	/// </summary>
	[Parameter]
	public TItem Item { get; set; }

	/// <summary>
	/// 被分配的对象的 List&lt;TChild&gt; 属性
	/// </summary>
	[Parameter]
	public string ChildProperty { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	[Parameter]
	public string Title { get; set; }

	/// <summary>
	/// 分配变化时
	/// </summary>
	[Parameter]
	public EventCallback<TItem> ItemChanged { get; set; }

	/// <summary>
	/// TChild 分页，值 -1 时不分页
	/// </summary>
	[Parameter]
	public int PageSize { get; set; } = 20;

	/// <summary>
	/// TChild 开启文本搜索
	/// </summary>
	[Parameter]
	public bool IsSearchText { get; set; } = true;

	/// <summary>
	/// 开启添加/编辑/删除通知其他端
	/// </summary>
	[Parameter]
	public bool IsNotifyChanged { get; set; } = false;

	/// <summary>
	/// TChild 表格 TR 模板
	/// </summary>
	[Parameter]
	public RenderFragment? TableHeader { get; set; }

	/// <summary>
	/// TChild 表格 TD 模板
	/// </summary>
	[Parameter]
	public RenderFragment<TChild>? TableRow { get; set; }

	[Parameter]
	public RenderFragment? TableTh1 { get; set; }

	[Parameter]
	public RenderFragment<TChild>? TableTd1 { get; set; }

	/// <summary>
	/// TChild 正在查询，设置条件
	/// </summary>
	[Parameter]
	public EventCallback<NovaAdminQueryEventArgs<TChild>> OnQuery { get; set; }

	[Inject]
	private NovaAdminContext admin { get; set; }

	[Inject]
	private IAggregateRootRepository<TItem> repo { get; set; }

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

	private string GetTChildPrimaryValue(TChild child)
	{
		return metaTChild.Primarys[0].GetValue((object)child).ConvertTo<string>() ?? "";
	}

	protected override void OnInitialized()
	{
		metaTItem = ((IBaseRepository)repo).Orm.CodeFirst.GetTableByEntity(typeof(TItem));
		if (metaTItem.Primarys.Length != 1)
		{
			throw new ArgumentException("NovaAllocTable 要求使用类型必须使用单一主键");
		}
		metaTChild = ((IBaseRepository)repo).Orm.CodeFirst.GetTableByEntity(typeof(TChild));
		if (metaTItem.Primarys.Length != 1)
		{
			throw new ArgumentException("NovaAllocTable 要求使用类型必须使用单一主键");
		}
	}

	protected override async Task OnParametersSetAsync()
	{
		if (Item == null || ChildProperty.IsNull())
		{
			return;
		}
		object childs = metaTItem.Properties[ChildProperty].GetValue(Item);
		if (childs == null)
		{
			await FreeSqlGlobalExtensions.IncludeByPropertyNameAsync<TItem>(new List<TItem> { Item }, ((IBaseRepository)repo).Orm, ChildProperty, (string)null, 0, (string)null, (Expression<Action<ISelect<object>>>)null);
			((IBaseRepository<TItem>)(object)repo).Attach(Item);
			childs = metaTItem.Properties[ChildProperty].GetValue(Item);
		}
		if (!(childs is IEnumerable childsEnumerable))
		{
			return;
		}
		foreach (object child in childsEnumerable)
		{
			if (child is TChild subi)
			{
				allItems[GetTChildPrimaryValue(subi)] = new NovaAdminItem<TChild>(subi)
				{
					Selected = true
				};
			}
		}
	}

	private void AllocChanged(List<NovaAdminItem<TChild>> e)
	{
		if (currentItems != e)
		{
			currentItems = e;
			currentItems.ForEach(delegate(NovaAdminItem<TChild> a)
			{
				string tChildPrimaryValue = GetTChildPrimaryValue(a.Value);
				a.Selected = allItems.TryGetValue(tChildPrimaryValue, out NovaAdminItem<TChild> value) && value.Selected;
				allItems[tChildPrimaryValue] = a;
			});
			StateHasChanged();
		}
	}

	private async Task AllocFinish()
	{
		List<TChild> childs = metaTItem.Properties[ChildProperty].GetValue(Item) as List<TChild>;
		childs.Clear();
		childs.AddRange(from a in allItems.Values
			where a.Selected
			select a.Value);
		await ((IBaseRepository<TItem>)(object)repo).UpdateAsync(Item, default(CancellationToken));
		await InvokeAsync(() => MessageService.Show(new MessageOption
		{
			Color = (Color)4,
			Icon = "fa-solid fa-circle-info",
			Content = "保存成功！",
			Delay = 2000
		}, (Message)null));
		if (IsNotifyChanged)
		{
			admin.TriggerNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnEdit", Item);
		}
		await OnClose();
	}

	private async Task OnClose()
	{
		if (ItemChanged.HasDelegate)
		{
			await ItemChanged.InvokeAsync(null);
		}
		currentItems?.Clear();
		currentItems = null;
		allItems.Clear();
		Item = null;
		await Task.Yield();
	}

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		__builder.OpenComponent<NovaModal>(0);
		__builder.AddComponentParameter(1, "Visible", RuntimeHelpers.TypeCheck(Item != null));
		__builder.AddComponentParameter(2, "IsBackdropStatic", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(3, "DialogClassName", "modal-lg modal-height0 modal-alloctable");
		__builder.AddComponentParameter(4, "Title", RuntimeHelpers.TypeCheck(Title.IsNull() ? "【分配】" : Title));
		__builder.AddComponentParameter(5, "OnClose", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)OnClose)));
		__builder.AddAttribute(6, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			if (Item != null)
			{
				if (TableTd1 != null && TableRow != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TChild>>(7);
					renderTreeBuilder.AddComponentParameter(8, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(9, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(10, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(11, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(12, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(13, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(14, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(15, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TChild>>>)AllocChanged)));
					renderTreeBuilder.AddComponentParameter(16, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(17, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(18, TableTh1);
					});
					renderTreeBuilder.AddAttribute(19, "TableTd1", (RenderFragment<TChild>)((TChild item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(20, TableTd1(item2));
					}));
					renderTreeBuilder.AddAttribute(21, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(22, TableHeader);
					});
					renderTreeBuilder.AddAttribute(23, "TableRow", (RenderFragment<TChild>)((TChild item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(24, TableRow?.Invoke(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
				else if (TableTd1 != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TChild>>(25);
					renderTreeBuilder.AddComponentParameter(26, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(27, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(28, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(29, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(30, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(31, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(32, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(33, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TChild>>>)AllocChanged)));
					renderTreeBuilder.AddComponentParameter(34, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(35, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(36, TableTh1);
					});
					renderTreeBuilder.AddAttribute(37, "TableTd1", (RenderFragment<TChild>)((TChild item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(38, TableTd1(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
				else if (TableRow != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TChild>>(39);
					renderTreeBuilder.AddComponentParameter(40, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(41, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(42, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(43, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(44, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(45, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(46, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(47, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TChild>>>)AllocChanged)));
					renderTreeBuilder.AddComponentParameter(48, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(49, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(50, TableHeader);
					});
					renderTreeBuilder.AddAttribute(51, "TableRow", (RenderFragment<TChild>)((TChild item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(52, TableRow?.Invoke(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
			}
		});
		__builder.AddAttribute(53, "Footer", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			int num = allItems.Values.Where((NovaAdminItem<TChild> a) => a.Selected).Count();
			if (num >= 0)
			{
				renderTreeBuilder.OpenElement(54, "button");
				renderTreeBuilder.AddAttribute(55, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					foreach (NovaAdminItem<TChild> value in allItems.Values)
					{
						value.Selected = false;
					}
				}));
				renderTreeBuilder.AddAttribute(56, "type", "button");
				renderTreeBuilder.AddAttribute(57, "class", "ml-2 btn btn-light");
				renderTreeBuilder.AddMarkupContent(58, "<i class=\"far fa-square\"></i> 重置");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(59, "\r\n            ");
				renderTreeBuilder.OpenElement(60, "button");
				renderTreeBuilder.AddAttribute(61, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)AllocFinish));
				renderTreeBuilder.AddAttribute(62, "type", "button");
				renderTreeBuilder.AddAttribute(63, "class", "ml-2 btn btn-success");
				renderTreeBuilder.AddMarkupContent(64, "<i class=\"fas fa-plus\"></i> 确认选择 ");
				renderTreeBuilder.AddContent(65, num);
				renderTreeBuilder.AddMarkupContent(66, " 项");
				renderTreeBuilder.CloseElement();
			}
			else
			{
				renderTreeBuilder.AddMarkupContent(67, "<button type=\"button\" class=\"ml-2 btn btn-light disabled\"><i class=\"far fa-square\"></i> 重置</button>\r\n            ");
				renderTreeBuilder.OpenElement(68, "button");
				renderTreeBuilder.AddAttribute(69, "type", "button");
				renderTreeBuilder.AddAttribute(70, "class", "ml-2 btn btn-success disabled");
				renderTreeBuilder.AddMarkupContent(71, "<i class=\"fas fa-plus\"></i> 确认选择 ");
				renderTreeBuilder.AddContent(72, num);
				renderTreeBuilder.AddMarkupContent(73, " 项");
				renderTreeBuilder.CloseElement();
			}
		});
		__builder.CloseComponent();
	}
}
