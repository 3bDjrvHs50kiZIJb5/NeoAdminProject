using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaInputTable<TItem, TKey> : ComponentBase where TItem : class, new()
{
	private TableInfo metaTItem;

	private string ClientId = "modal-" + Guid.NewGuid().ToString("n");

	private bool showModal;

	private List<NovaAdminItem<TItem>> currentItems;

	private Dictionary<TKey, NovaAdminItem<TItem>> allItems = new Dictionary<TKey, NovaAdminItem<TItem>>();

	/// <summary>
	/// 值
	/// </summary>
	[Parameter]
	public TKey Value { get; set; }

	[Parameter]
	public EventCallback<TKey> ValueChanged { get; set; }

	/// <summary>
	/// 值变化时
	/// </summary>
	[Parameter]
	public EventCallback<TKey> OnValueChanged { get; set; }

	/// <summary>
	/// 【多对一】导航属性
	/// </summary>
	[Parameter]
	public TItem Item { get; set; }

	[Parameter]
	public EventCallback<TItem> ItemChanged { get; set; }

	/// <summary>
	/// 【多对一】导航属性变化时
	/// </summary>
	[Parameter]
	public EventCallback<TItem> OnItemChanged { get; set; }

	/// <summary>
	/// 【多对多】导航属性
	/// </summary>
	[Parameter]
	public List<TItem> Items { get; set; }

	[Parameter]
	public EventCallback<List<TItem>> ItemsChanged { get; set; }

	/// <summary>
	/// 【多对多】导航属性变化时
	/// </summary>
	[Parameter]
	public EventCallback<List<TItem>> OnItemsChanged { get; set; }

	/// <summary>
	/// 文本框显示内容
	/// </summary>
	[Parameter]
	public Func<TItem, string> DisplayText { get; set; }

	/// <summary>
	/// 弹框标题
	/// </summary>
	[Parameter]
	public string ModalTitle { get; set; } = "选择..";

	/// <summary>
	/// 弹框显示 TItem 分页，值 -1 时不分页
	/// </summary>
	[Parameter]
	public int PageSize { get; set; } = 20;

	/// <summary>
	/// 弹框显示 TItem 开启文本搜索
	/// </summary>
	[Parameter]
	public bool IsSearchText { get; set; } = true;

	[Parameter]
	public string SearchPlaceholder { get; set; }

	[Parameter]
	public string DialogClassName { get; set; } = "modal-lg";

	/// <summary>
	/// 自定义UI
	/// </summary>
	[Parameter]
	public bool IsCustomUI { get; set; }

	/// <summary>
	/// 弹框显示 TItem 表格 TR 模板
	/// </summary>
	[Parameter]
	public RenderFragment? TableHeader { get; set; }

	/// <summary>
	/// 弹框显示 TItem 表格 TD 模板
	/// </summary>
	[Parameter]
	public RenderFragment<TItem>? TableRow { get; set; }

	[Parameter]
	public RenderFragment? TableTh1 { get; set; }

	[Parameter]
	public RenderFragment<TItem>? TableTd1 { get; set; }

	/// <summary>
	/// 弹框显示 TItem 正在查询，设置条件
	/// </summary>
	[Parameter]
	public EventCallback<NovaAdminQueryEventArgs<TItem>> OnQuery { get; set; }

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

	private TKey GetPrimaryValue(TItem item)
	{
		return metaTItem.Primarys[0].GetValue((object)item).ConvertTo<TKey>();
	}

	protected override void OnInitialized()
	{
		metaTItem = fsql.CodeFirst.GetTableByEntity(typeof(TItem));
		if (metaTItem.Primarys.Length != 1)
		{
			throw new ArgumentException("NovaInputTable 要求使用类型必须使用单一主键");
		}
		if (FreeSqlGlobalExtensions.NullableTypeOrThis(metaTItem.Primarys[0].CsType) != FreeSqlGlobalExtensions.NullableTypeOrThis(typeof(TKey)))
		{
			throw new ArgumentException("NovaInputTable 要求使用类型的主键，必须与 TKey 类型相同");
		}
	}

	public void OpenModal()
	{
		allItems.Clear();
		if (Item != null)
		{
			allItems[GetPrimaryValue(Item)] = new NovaAdminItem<TItem>(Item)
			{
				Selected = true
			};
		}
		if (Items != null)
		{
			foreach (TItem item in Items)
			{
				allItems[GetPrimaryValue(item)] = new NovaAdminItem<TItem>(item)
				{
					Selected = true
				};
			}
		}
		showModal = true;
	}

	private void OnSelectChanged(List<NovaAdminItem<TItem>> e)
	{
		if (currentItems == e)
		{
			return;
		}
		currentItems = e;
		TKey itemPkval = default(TKey);
		if (ItemChanged.HasDelegate)
		{
			itemPkval = GetPrimaryValue(Item);
			foreach (KeyValuePair<TKey, NovaAdminItem<TItem>> allItem in allItems)
			{
				if (allItem.Value.Selected)
				{
					allItem.Value.Selected = false;
				}
			}
		}
		currentItems.ForEach(delegate(NovaAdminItem<TItem> a)
		{
			TKey primaryValue = GetPrimaryValue(a.Value);
			if (ItemChanged.HasDelegate)
			{
				a.Selected = object.Equals(itemPkval, primaryValue);
			}
			else if (ItemsChanged.HasDelegate)
			{
				a.Selected = allItems.TryGetValue(primaryValue, out NovaAdminItem<TItem> value) && value.Selected;
			}
			else if (ValueChanged.HasDelegate)
			{
				a.Selected = object.Equals(Value, primaryValue);
			}
			allItems[primaryValue] = a;
		});
		StateHasChanged();
	}

	private async Task Finish()
	{
		if (ItemChanged.HasDelegate)
		{
			Item = (from a in allItems.Values
				where a.Selected
				select a.Value).FirstOrDefault();
			await ItemChanged.InvokeAsync(Item);
			if (OnItemChanged.HasDelegate)
			{
				await OnItemChanged.InvokeAsync(Item);
			}
			TKey pkval = GetPrimaryValue(Item);
			await ValueChanged.InvokeAsync(pkval);
			if (OnValueChanged.HasDelegate)
			{
				await OnValueChanged.InvokeAsync(pkval);
			}
		}
		else if (ItemsChanged.HasDelegate)
		{
			bool ischanged;
			if (Items == null)
			{
				Items = new List<TItem>();
				ischanged = true;
			}
			else
			{
				TKey[] oldItems = (from a in Items
					select GetPrimaryValue(a) into a
					orderby a
					select a).ToArray();
				TKey[] newItems = (from a in allItems.Values
					where a.Selected
					select GetPrimaryValue(a.Value) into a
					orderby a
					select a).ToArray();
				ischanged = !oldItems.SequenceEqual(newItems);
			}
			Items.Clear();
			Items.AddRange(from a in allItems.Values
				where a.Selected
				select a.Value);
			if (ischanged)
			{
				await ItemsChanged.InvokeAsync(Items);
				if (OnItemsChanged.HasDelegate)
				{
					await OnItemsChanged.InvokeAsync(Items);
				}
			}
		}
		else if (ValueChanged.HasDelegate)
		{
			NovaInputTable<TItem, TKey> inputTable = this;
			NovaInputTable<TItem, TKey> inputTable2 = this;
			NovaAdminItem<TItem>? adminItem = allItems.Values.Where((NovaAdminItem<TItem> a) => a.Selected).FirstOrDefault();
			inputTable.Value = inputTable2.GetPrimaryValue((adminItem != null) ? adminItem.Value : null);
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
		allItems.Clear();
	}

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		if (!IsCustomUI)
		{
			__builder.OpenElement(0, "div");
			__builder.AddAttribute(1, "class", "input-group");
			if (ItemChanged.HasDelegate)
			{
				if (Item == null)
				{
					__builder.AddMarkupContent(2, "<input value class=\"form-control disabled\">");
				}
				else
				{
					__builder.OpenElement(3, "input");
					__builder.AddAttribute(4, "value", DisplayText(Item));
					__builder.AddAttribute(5, "class", "form-control disabled");
					__builder.CloseElement();
				}
			}
			else if (ItemsChanged.HasDelegate)
			{
				if (Items == null)
				{
					__builder.AddMarkupContent(6, "<input value class=\"form-control disabled\">");
				}
				else
				{
					__builder.OpenElement(7, "input");
					__builder.AddAttribute(8, "value", string.Join(",", Items.Select((TItem a) => DisplayText(a))));
					__builder.AddAttribute(9, "class", "form-control disabled");
					__builder.CloseElement();
				}
			}
			else if (ValueChanged.HasDelegate)
			{
				__builder.OpenElement(10, "input");
				__builder.AddAttribute(11, "oninput", EventCallback.Factory.Create(this, async delegate(ChangeEventArgs e)
				{
					await ValueChanged.InvokeAsync(e.Value.ConvertTo<TKey>());
				}));
				__builder.AddAttribute(12, "class", "form-control disabled");
				__builder.AddAttribute(13, "value", BindConverter.FormatValue(Value));
				__builder.AddAttribute(14, "onchange", EventCallback.Factory.CreateBinder(this, delegate(TKey __value)
				{
					Value = __value;
				}, Value));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
			}
			__builder.OpenElement(15, "button");
			__builder.AddAttribute(16, "type", "button");
			__builder.AddAttribute(17, "class", "btn btn-outline-secondary");
			__builder.AddAttribute(18, "onclick", EventCallback.Factory.Create((object)this, (Func<MouseEventArgs, Task>)async delegate
			{
				allItems.Clear();
				await Finish();
			}));
			__builder.AddAttribute(19, "role", "button");
			__builder.AddAttribute(20, "aria-disabled", "false");
			__builder.AddMarkupContent(21, "×");
			__builder.CloseElement();
			__builder.AddMarkupContent(22, "\r\n        ");
			__builder.OpenElement(23, "button");
			__builder.AddAttribute(24, "type", "button");
			__builder.AddAttribute(25, "class", "btn btn-secondary");
			__builder.AddAttribute(26, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)OpenModal));
			__builder.AddAttribute(27, "role", "button");
			__builder.AddAttribute(28, "aria-disabled", "false");
			__builder.AddMarkupContent(29, "<i class=\"fa-solid fa-folder-open\"></i>");
			__builder.AddMarkupContent(30, "<span>选择</span>");
			__builder.CloseElement();
			__builder.CloseElement();
		}
		__builder.OpenComponent<NovaModal>(31);
		__builder.AddComponentParameter(32, "Visible", RuntimeHelpers.TypeCheck(showModal));
		__builder.AddComponentParameter(33, "ClientId", RuntimeHelpers.TypeCheck(ClientId));
		__builder.AddComponentParameter(34, "IsBackdropStatic", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(35, "DialogClassName", RuntimeHelpers.TypeCheck(DialogClassName + " modal-height0 modal-novainputtable"));
		__builder.AddComponentParameter(36, "Title", RuntimeHelpers.TypeCheck(ModalTitle));
		__builder.AddComponentParameter(37, "OnClose", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action)OnClose)));
		__builder.AddAttribute(38, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			if (showModal)
			{
				if (TableTd1 != null && TableRow != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TItem>>(39);
					renderTreeBuilder.AddComponentParameter(40, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(41, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(42, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(43, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(44, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(45, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(46, "SearchPlaceholder", RuntimeHelpers.TypeCheck(SearchPlaceholder));
					renderTreeBuilder.AddComponentParameter(47, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(48, "IsSingleSelect", RuntimeHelpers.TypeCheck(ItemChanged.HasDelegate || ValueChanged.HasDelegate));
					renderTreeBuilder.AddComponentParameter(49, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(50, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TItem>>>)OnSelectChanged)));
					renderTreeBuilder.AddComponentParameter(51, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(52, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(53, TableTh1);
					});
					renderTreeBuilder.AddAttribute(54, "TableTd1", (RenderFragment<TItem>)((TItem item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(55, TableTd1(item2));
					}));
					renderTreeBuilder.AddAttribute(56, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(57, TableHeader);
					});
					renderTreeBuilder.AddAttribute(58, "TableRow", (RenderFragment<TItem>)((TItem item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(59, TableRow?.Invoke(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
				else if (TableTd1 != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TItem>>(60);
					renderTreeBuilder.AddComponentParameter(61, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(62, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(63, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(64, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(65, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(66, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(67, "SearchPlaceholder", RuntimeHelpers.TypeCheck(SearchPlaceholder));
					renderTreeBuilder.AddComponentParameter(68, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(69, "IsSingleSelect", RuntimeHelpers.TypeCheck(ItemChanged.HasDelegate || ValueChanged.HasDelegate));
					renderTreeBuilder.AddComponentParameter(70, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(71, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TItem>>>)OnSelectChanged)));
					renderTreeBuilder.AddComponentParameter(72, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(73, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(74, TableTh1);
					});
					renderTreeBuilder.AddAttribute(75, "TableTd1", (RenderFragment<TItem>)((TItem item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(76, TableTd1(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
				else if (TableRow != null)
				{
					renderTreeBuilder.OpenComponent<NovaAdminTable<TItem>>(77);
					renderTreeBuilder.AddComponentParameter(78, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
					renderTreeBuilder.AddComponentParameter(79, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(80, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(81, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(82, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(83, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
					renderTreeBuilder.AddComponentParameter(84, "SearchPlaceholder", RuntimeHelpers.TypeCheck(SearchPlaceholder));
					renderTreeBuilder.AddComponentParameter(85, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
					renderTreeBuilder.AddComponentParameter(86, "IsSingleSelect", RuntimeHelpers.TypeCheck(ItemChanged.HasDelegate || ValueChanged.HasDelegate));
					renderTreeBuilder.AddComponentParameter(87, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder.AddComponentParameter(88, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<List<NovaAdminItem<TItem>>>)OnSelectChanged)));
					renderTreeBuilder.AddComponentParameter(89, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
					renderTreeBuilder.AddAttribute(90, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(91, TableHeader);
					});
					renderTreeBuilder.AddAttribute(92, "TableRow", (RenderFragment<TItem>)((TItem item2) => delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						renderTreeBuilder2.AddContent(93, TableRow?.Invoke(item2));
					}));
					renderTreeBuilder.CloseComponent();
				}
			}
		});
		__builder.AddAttribute(94, "Footer", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			int num = (ItemsChanged.HasDelegate ? allItems.Values.Where((NovaAdminItem<TItem> a) => a.Selected).Count() : (currentItems?.Where((NovaAdminItem<TItem> a) => a.Selected).Count() ?? 0));
			if (num > 0)
			{
				renderTreeBuilder.OpenElement(95, "button");
				renderTreeBuilder.AddAttribute(96, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					foreach (NovaAdminItem<TItem> value in allItems.Values)
					{
						value.Selected = false;
					}
				}));
				renderTreeBuilder.AddAttribute(97, "type", "button");
				renderTreeBuilder.AddAttribute(98, "class", "ml-2 btn btn-light");
				renderTreeBuilder.AddMarkupContent(99, "<i class=\"far fa-square\"></i> 重置");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(100, "\r\n            ");
				renderTreeBuilder.OpenElement(101, "button");
				renderTreeBuilder.AddAttribute(102, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Finish));
				renderTreeBuilder.AddAttribute(103, "type", "button");
				renderTreeBuilder.AddAttribute(104, "class", "ml-2 btn btn-success");
				renderTreeBuilder.AddMarkupContent(105, "<i class=\"fas fa-plus\"></i> 确认选择 ");
				renderTreeBuilder.AddContent(106, num);
				renderTreeBuilder.AddMarkupContent(107, " 项");
				renderTreeBuilder.CloseElement();
			}
			else
			{
				renderTreeBuilder.AddMarkupContent(108, "<button type=\"button\" class=\"ml-2 btn btn-light disabled\"><i class=\"far fa-square\"></i> 重置</button>\r\n            ");
				renderTreeBuilder.OpenElement(109, "button");
				renderTreeBuilder.AddAttribute(110, "type", "button");
				renderTreeBuilder.AddAttribute(111, "class", "ml-2 btn btn-success disabled");
				renderTreeBuilder.AddMarkupContent(112, "<i class=\"fas fa-plus\"></i> 确认选择 ");
				renderTreeBuilder.AddContent(113, num);
				renderTreeBuilder.AddMarkupContent(114, " 项");
				renderTreeBuilder.CloseElement();
			}
		});
		__builder.CloseComponent();
	}
}
