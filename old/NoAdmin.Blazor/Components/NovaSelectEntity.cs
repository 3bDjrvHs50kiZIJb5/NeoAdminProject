using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaSelectEntity<TItem, TKey> : ComponentBase where TItem : class
{
	private List<NovaAdminItem<TItem>> items;

	private ColumnInfo firstStringColumn;

	private object TKeyDefaultValue = default(TKey);

	private TableInfo metaTItem;

	[Parameter]
	public TKey Value { get; set; }

	[Parameter]
	public Func<TItem, string> DisplayText { get; set; }

	[Parameter]
	public EventCallback<TKey> ValueChanged { get; set; }

	[Parameter]
	public EventCallback<TKey> OnValueChanged { get; set; }

	[Parameter]
	public List<TItem> Source { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public Action<ISelect<TItem>> OnQuery { get; set; }

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
		__builder.OpenElement(0, "select");
		__builder.AddAttribute(1, "oninput", EventCallback.Factory.Create((object)this, (Func<ChangeEventArgs, Task>)OnInput));
		__builder.AddAttribute(2, "class", "form-control");
		__builder.AddAttribute(3, "disabled", Disabled);
		__builder.AddAttribute(4, "value", BindConverter.FormatValue(Value));
		__builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder(this, delegate(TKey __value)
		{
			Value = __value;
		}, Value));
		__builder.SetUpdatesAttributeName("value");
		if (TKeyDefaultValue == null)
		{
			__builder.OpenElement(6, "option");
			__builder.AddAttribute(7, "value");
			__builder.AddMarkupContent(8, "请选择..");
			__builder.CloseElement();
		}
		else
		{
			__builder.OpenElement(9, "option");
			__builder.AddAttribute(10, "value", TKeyDefaultValue);
			__builder.AddMarkupContent(11, "请选择..");
			__builder.CloseElement();
		}
		if (items != null)
		{
			foreach (NovaAdminItem<TItem> item in items)
			{
				__builder.OpenElement(12, "option");
				__builder.AddAttribute(13, "value", GetPrimaryValue(item.Value));
				__builder.AddContent(14, (item.Level > 1) ? "".PadRight(item.Level - 1, '\u3000') : "");
				object obj = DisplayText?.Invoke(item.Value);
				if (obj == null)
				{
					ColumnInfo obj2 = firstStringColumn;
					obj = ((obj2 != null) ? obj2.GetValue((object)item.Value) : null) ?? GetPrimaryValue(item.Value);
				}
				__builder.AddContent(15, obj);
				__builder.CloseElement();
			}
		}
		__builder.CloseElement();
	}

	private object GetPrimaryValue(TItem item)
	{
		return metaTItem.Primarys[0].GetValue((object)item);
	}

	protected override void OnInitialized()
	{
		metaTItem = fsql.CodeFirst.GetTableByEntity(typeof(TItem));
		if (metaTItem.Primarys.Length != 1)
		{
			throw new ArgumentException("NovaSelectEntity 要求使用类型必须使用单一主键");
		}
		if (FreeSqlGlobalExtensions.NullableTypeOrThis(metaTItem.Primarys[0].CsType) != FreeSqlGlobalExtensions.NullableTypeOrThis(typeof(TKey)))
		{
			throw new ArgumentException("NovaSelectEntity 要求使用类型的主键，必须与 TKey 类型相同");
		}
	}

	private async Task OnInput(ChangeEventArgs e)
	{
		TKey val = e.Value.ConvertTo<TKey>();
		await OnValueChanged.InvokeAsync(val);
		await ValueChanged.InvokeAsync(val);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
		{
			return;
		}
		firstStringColumn = ((IEnumerable<ColumnInfo>)fsql.CodeFirst.GetTableByEntity(typeof(TItem)).ColumnsByPosition).FirstOrDefault((Func<ColumnInfo, bool>)((ColumnInfo a) => a.CsType == typeof(string)));
		if (Source == null)
		{
			ISelect<TItem> query = fsql.Select<TItem>();
			OnQuery?.Invoke(query);
			Source = (await ((ISelect0<ISelect<TItem>, TItem>)(object)query).ToListAsync(default(CancellationToken))).Select((TItem a) => a).ToList();
		}
		items = Source.ToNovaAdminItemList(fsql);
		StateHasChanged();
	}
}
