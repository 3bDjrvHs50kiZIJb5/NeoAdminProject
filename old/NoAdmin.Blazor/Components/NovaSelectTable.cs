using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaSelectTable<TItem, TKey> : ComponentBase where TItem : class, new()
{
	private TableInfo metaTItem;

	private bool isPrametersSet;

	private List<NovaAdminItem<TItem>> currentItems;

	private Dictionary<TKey, NovaAdminItem<TItem>> allItems = new Dictionary<TKey, NovaAdminItem<TItem>>();

	[Parameter]
	public TKey Value { get; set; }

	[Parameter]
	public EventCallback<TKey> ValueChanged { get; set; }

	[Parameter]
	public List<TItem> Items { get; set; }

	[Parameter]
	public EventCallback<List<TItem>> ItemsChanged { get; set; }

	[Parameter]
	public int PageSize { get; set; } = 20;

	[Parameter]
	public bool IsSearchText { get; set; } = true;

	[Parameter]
	public RenderFragment<TItem>? ChildContent { get; set; }

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

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		__builder.OpenComponent<NovaAdminTable<TItem>>(0);
		__builder.AddComponentParameter(1, "PageSize", RuntimeHelpers.TypeCheck(PageSize));
		__builder.AddComponentParameter(2, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(3, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(4, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(5, "IsSearchText", RuntimeHelpers.TypeCheck(IsSearchText));
		__builder.AddComponentParameter(6, "IsSingleSelect", RuntimeHelpers.TypeCheck(ValueChanged.HasDelegate));
		__builder.AddComponentParameter(7, "IsAutoSelectParent", RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(8, "OnSelectChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<List<NovaAdminItem<TItem>>, Task>)OnSelectChanged)));
		__builder.AddComponentParameter(9, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, OnQuery)));
		__builder.AddAttribute(10, "TableTd1", (RenderFragment<TItem>)((TItem item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(11, ChildContent(item));
		}));
		__builder.CloseComponent();
	}

	private TKey GetPrimaryValue(TItem item)
	{
		return metaTItem.Primarys[0].GetValue((object)item).ConvertTo<TKey>();
	}

	protected override void OnInitialized()
	{
		metaTItem = fsql.CodeFirst.GetTableByEntity(typeof(TItem));
		if (metaTItem.Primarys.Length != 1)
		{
			throw new ArgumentException("NovaSelectTable 要求使用类型必须使用单一主键");
		}
		if (FreeSqlGlobalExtensions.NullableTypeOrThis(metaTItem.Primarys[0].CsType) != FreeSqlGlobalExtensions.NullableTypeOrThis(typeof(TKey)))
		{
			throw new ArgumentException("NovaSelectTable 要求使用类型的主键，必须与 TKey 类型相同");
		}
	}

	protected override async Task OnParametersSetAsync()
	{
		allItems.Clear();
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
		isPrametersSet = true;
		if (currentItems != null)
		{
			await OnSelectChanged(currentItems);
		}
	}

	private async Task OnSelectChanged(List<NovaAdminItem<TItem>> e)
	{
		if (currentItems != e || isPrametersSet)
		{
			if (currentItems != e)
			{
				currentItems = e;
			}
			isPrametersSet = false;
			currentItems.ForEach(delegate(NovaAdminItem<TItem> a)
			{
				TKey primaryValue = GetPrimaryValue(a.Value);
				if (ValueChanged.HasDelegate)
				{
					a.Selected = object.Equals(Value, primaryValue);
				}
				else if (ItemsChanged.HasDelegate)
				{
					a.Selected = allItems.TryGetValue(primaryValue, out NovaAdminItem<TItem> value) && value.Selected;
				}
				allItems[primaryValue] = a;
			});
			StateHasChanged();
		}
		await Finish();
	}

	private async Task Finish()
	{
		if (ValueChanged.HasDelegate)
		{
			NovaSelectTable<TItem, TKey> selectTable = this;
			NovaSelectTable<TItem, TKey> selectTable2 = this;
			NovaAdminItem<TItem>? adminItem = allItems.Values.Where((NovaAdminItem<TItem> a) => a.Selected).FirstOrDefault();
			selectTable.Value = selectTable2.GetPrimaryValue((adminItem != null) ? adminItem.Value : null);
			await ValueChanged.InvokeAsync(Value);
		}
		else if (ItemsChanged.HasDelegate)
		{
			bool ischanged = false;
			if (Items == null)
			{
				Items = new List<TItem>();
				ischanged = true;
			}
			Items.Clear();
			Items.AddRange(from a in allItems.Values
				where a.Selected
				select a.Value);
			if (ischanged)
			{
				await ItemsChanged.InvokeAsync(Items);
			}
		}
	}
}
