using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Rougamo;
using Rougamo.Context;
using __Blazor.NoAdmin.Blazor.Components.NovaAdminTable;

namespace NoAdmin.Blazor.Components;

public class NovaAdminTable<TItem> : ComponentBase, IDisposable where TItem : class, new()
{




	private bool _IsRemove = true;

	private bool? _IsRowRemove;

	private bool showAuditEntityLog = false;

	private bool isEntityAudited = typeof(EntityAudited).IsAssignableFrom(typeof(TItem));

	private bool MultiSelectChecked = false;

	private Dictionary<INovaAdminColumn, NovaAdminColumnFixed> _fixedOverrides = new Dictionary<INovaAdminColumn, NovaAdminColumnFixed>();

	private bool showColumnSettings;

	private int settingsTab = 0;

	private NovaAdminQueryInfo q = new NovaAdminQueryInfo();

	public TItem item;

	private int itemModalStatus = 0;

	public List<NovaAdminItem<TItem>> items = new List<NovaAdminItem<TItem>>();

	private string tempid;

	private TableInfo metaTItem;

	private KeyValuePair<string, TableRef> treeNav;

	private DotNetObjectReference<NovaAdminTable<TItem>>? _objRef;

	private bool _shouldInitializeTable;

	private bool _settingsSortableInitialized;

	private bool isLoaded = false;

	private bool isShowLoader = false;

	private string boundaryName;

	private ElementReference _theadElement;

	private NovaModal editModal;

	private bool isShowLoaderSaving = false;

	private object selectedSingleValue;

	/// <summary>
	/// [新增] 内存数据源。设置此参数后，表格将对内存 List 进行 CRUD，不再查询数据库。
	/// </summary>
	[Parameter]
	public List<TItem> ItemsSource { get; set; }

	[Inject]
	private UnitOfWorkManager unitOfWorkManager { get; set; }

	[Inject]
	private IAggregateRootRepository<TItem> repository { get; set; }

	private IFreeSql fsql => ((IBaseRepository)repository).Orm;

	[Inject]
	private DownloadService downloadService { get; set; }

	[Inject]
	private NovaAdminContext admin { get; set; }

	[CascadingParameter]
	public NovaAdminContext.TabInfo tabInfo { get; set; }

	/// <summary>
	/// 打开UI调试
	/// </summary>
	[Parameter]
	public bool IsDebug { get; set; }

	/// <summary>
	/// 标题，弹框时
	/// </summary>
	[Parameter]
	public string Title { get; set; }

	/// <summary>
	/// 分页，值 -1 时不分页
	/// </summary>
	[Parameter]
	public int PageSize { get; set; } = 30;

	/// <summary>
	/// 查询条件与 URL QueryString 同步
	/// </summary>
	[Parameter]
	public bool IsQueryString { get; set; } = true;

	/// <summary>
	/// 开启删除
	/// </summary>
	[Parameter]
	public bool IsRemove
	{
		get
		{
			return _IsRemove;
		}
		set
		{
			_IsRemove = value;
			if (!_IsRowRemove.HasValue)
			{
				_IsRowRemove = value;
			}
		}
	}

	/// <summary>
	/// 开启删除（表格每行）
	/// </summary>
	[Parameter]
	public bool IsRowRemove
	{
		get
		{
			return _IsRowRemove ?? true;
		}
		set
		{
			_IsRowRemove = value;
		}
	}

	/// <summary>
	/// 开启双击查看
	/// </summary>
	[Parameter]
	public bool IsView { get; set; } = true;

	/// <summary>
	/// 开启添加
	/// </summary>
	[Parameter]
	public bool IsAdd { get; set; } = true;

	/// <summary>
	/// 开启编辑
	/// </summary>
	[Parameter]
	public bool IsEdit { get; set; } = true;

	/// <summary>
	/// 开启刷新
	/// </summary>
	[Parameter]
	public bool IsRefersh { get; set; } = true;

	/// <summary>
	/// 开启导出
	/// </summary>
	[Parameter]
	public bool IsExportExcel { get; set; } = true;

	/// <summary>
	/// 开启行号
	/// </summary>
	[Parameter]
	public bool IsRowNumber { get; set; } = true;

	/// <summary>
	/// 开启审核
	/// </summary>
	[Parameter]
	public bool IsAudit { get; set; } = true;

	private bool IsViewAuditEntityLog { get; set; } = true;

	/// <summary>
	/// 开启添加/编辑/删除通知其他端
	/// </summary>
	[Parameter]
	public bool IsNotifyChanged { get; set; } = false;

	/// <summary>
	/// 开启遮罩加载中
	/// </summary>
	[Parameter]
	public bool IsShowLoading { get; set; } = true;

	/// <summary>
	/// 开启文本搜索
	/// </summary>
	[Parameter]
	public bool IsSearchText { get; set; } = true;

	/// <summary>
	/// 文本搜索 Placeholder
	/// </summary>
	[Parameter]
	public string SearchPlaceholder { get; set; } = "Search";

	/// <summary>
	/// 开启单选
	/// </summary>
	[Parameter]
	public bool IsSingleSelect { get; set; } = false;

	/// <summary>
	/// 开启多选
	/// </summary>
	[Parameter]
	public bool IsMultiSelect { get; set; } = true;

	/// <summary>
	/// 单条是否可选
	/// </summary>
	[Parameter]
	public Func<TItem, bool> CanbeSelect { get; set; }

	/// <summary>
	/// 自动选中父
	/// </summary>
	[Parameter]
	public bool IsAutoSelectParent { get; set; } = false;

	/// <summary>
	/// 开启编辑保存时，弹框确认
	/// </summary>
	[Parameter]
	public bool IsConfirmEdit { get; set; } = true;

	/// <summary>
	/// 开启删除时，弹框确认
	/// </summary>
	[Parameter]
	public bool IsConfirmRemove { get; set; } = true;

	[Parameter]
	public EventCallback<TItem[]> OnDragRow { get; set; }

	/// <summary>
	/// 表格一行显示几条记录
	/// </summary>
	[Parameter]
	public int Colspan { get; set; } = 4;

	/// <summary>
	/// 表格高度
	/// </summary>
	[Parameter]
	public int BodyHeight { get; set; } = -1;

	/// <summary>
	/// 设置左侧固定列的数量
	/// </summary>
	[Parameter]
	public int FixedLeftColumns { get; set; } = 0;

	/// <summary>
	/// 设置右侧固定列的数量
	/// </summary>
	[Parameter]
	public int FixedRightColumns { get; set; } = 0;

	/// <summary>
	/// 弹框样式
	/// </summary>
	[Parameter]
	public string DialogClassName { get; set; }

	/// <summary>
	/// 弹框尺寸
	/// </summary>
	[Parameter]
	public NovaModalSize DialogSize { get; set; }

	/// <summary>
	/// 弹框动画
	/// </summary>
	[Parameter]
	public NovaModalAnimation DialogAnimation { get; set; }

	[Parameter]
	public bool DialogIsKeyboard { get; set; } = false;

	[Parameter]
	public bool DialogIsBackdropStatic { get; set; } = true;

	/// <summary>
	/// 弹出抽屉
	/// </summary>
	[Parameter]
	public bool IsDrawer { get; set; } = false;

	[Parameter]
	public NovaAdminDrawerPlacement DrawerPlacement { get; set; } = NovaAdminDrawerPlacement.Right;

	[Parameter]
	public string DrawerWidth { get; set; } = "30%";

	[Parameter]
	public string DrawerHeight { get; set; } = "40%";

	[Parameter]
	public string? DrawerOffset { get; set; }

	/// <summary>
	/// 暂无数据
	/// </summary>
	[Parameter]
	public string NoDataText { get; set; } = "暂无数据";

	/// <summary>
	/// 保存
	/// </summary>
	[Parameter]
	public string SaveText { get; set; } = "保存";

	/// <summary>
	/// 添加
	/// </summary>
	[Parameter]
	public string AddText { get; set; } = "添加";

	/// <summary>
	/// 表格 TR 模板
	/// </summary>
	[Parameter]
	public RenderFragment? TableHeader { get; set; }

	/// <summary>
	/// 基于列定义的模板
	/// </summary>
	[Parameter]
	public RenderFragment<TItem> TableColumns { get; set; }

	internal List<INovaAdminColumn> ColumnDefs { get; set; } = new List<INovaAdminColumn>();

	/// <summary>
	/// 表格筛选行模板（位于 Body 第一行）
	/// </summary>
	[Parameter]
	public RenderFragment? TableFilter { get; set; }

	/// <summary>
	/// 是否在顶部显示 SearchFilter (CardHeader 中)
	/// </summary>
	[Parameter]
	public bool IsShowSearchFilter { get; set; } = true;

	/// <summary>
	/// 表格 TD 模板
	/// </summary>
	[Parameter]
	public RenderFragment<TItem>? TableRow { get; set; }

	[Parameter]
	public int TableTd1Width { get; set; }

	[Parameter]
	public RenderFragment? TableTh1 { get; set; }

	[Parameter]
	public RenderFragment<TItem>? TableTd1 { get; set; }

	[Parameter]
	public int TableTd99Width { get; set; } = 75;

	[Parameter]
	public RenderFragment<TItem>? TableTd99 { get; set; }

	/// <summary>
	/// 添加/编辑 模板
	/// </summary>
	[Parameter]
	public RenderFragment<TItem>? EditTemplate { get; set; }

	/// <summary>
	/// 卡片 Header 模板
	/// </summary>
	[Parameter]
	public RenderFragment? CardHeader { get; set; }

	/// <summary>
	/// 卡片 Fotter 模板
	/// </summary>
	[Parameter]
	public RenderFragment? CardFooter { get; set; }

	/// <summary>
	/// 初始化查询
	/// </summary>
	[Parameter]
	public Func<NovaAdminQueryInfo, Task> InitQuery { get; set; }

	/// <summary>
	/// 正在查询，设置条件
	/// </summary>
	[Parameter]
	public EventCallback<NovaAdminQueryEventArgs<TItem>> OnQuery { get; set; }

	[Parameter]
	public EventCallback<List<NovaAdminItem<TItem>>> OnQueried { get; set; }

	/// <summary>
	/// 正在编辑，设置编辑对象
	/// </summary>
	[Parameter]
	public EventCallback<TItem> OnEdit { get; set; }

	[Parameter]
	public EventCallback<TItem> OnEditClose { get; set; }

	/// <summary>
	/// 保存验证
	/// </summary>
	[Parameter]
	public EventCallback<NovaAdminConfirmEventArgs<TItem>> OnSaving { get; set; }

	/// <summary>
	/// 编辑完成
	/// </summary>
	[Parameter]
	public EventCallback<TItem> OnSaved { get; set; }

	/// <summary>
	/// 正在删除
	/// </summary>
	[Parameter]
	public EventCallback<NovaAdminConfirmEventArgs<List<TItem>>> OnRemoving { get; set; }

	/// <summary>
	/// 删除完成
	/// </summary>
	[Parameter]
	public EventCallback<List<TItem>> OnRemoved { get; set; }

	/// <summary>
	/// 选择内容发生变化
	/// </summary>
	[Parameter]
	public EventCallback<List<NovaAdminItem<TItem>>> OnSelectChanged { get; set; }

	/// <summary>
	/// 单击表格行时（树形表格时：返回所有子记录）
	/// </summary>
	[Parameter]
	public EventCallback<List<NovaAdminItem<TItem>>> OnRowClick { get; set; }

	/// <summary>
	/// 审核事件
	/// </summary>
	[Parameter]
	public EventCallback<NovaNovaAdminAuditedEventArgs> OnAudited { get; set; }

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

	internal void AddColumn(INovaAdminColumn col)
	{
		ColumnDefs.Add(col);
		if (col.Order == 0)
		{
			col.Order = ColumnDefs.Count * 10;
		}
		StateHasChanged();
	}

	internal void RemoveColumn(INovaAdminColumn col)
	{
		ColumnDefs.Remove(col);
		if (_fixedOverrides.ContainsKey(col))
		{
			_fixedOverrides.Remove(col);
		}
		StateHasChanged();
	}

	public IEnumerable<INovaAdminColumn> GetRenderColumns()
	{
		IEnumerable<INovaAdminColumn> source = ColumnDefs.Where((INovaAdminColumn c) => c.Visible);
		IOrderedEnumerable<INovaAdminColumn> first = from c in source
			where c.Fixed == NovaAdminColumnFixed.Left
			orderby c.Order
			select c;
		IOrderedEnumerable<INovaAdminColumn> second = from c in source
			where c.Fixed == NovaAdminColumnFixed.None
			orderby c.Order
			select c;
		IOrderedEnumerable<INovaAdminColumn> second2 = from c in source
			where c.Fixed == NovaAdminColumnFixed.Right
			orderby c.Order
			select c;
		return first.Concat(second).Concat(second2).ToList();
	}

	private void OpenColumnSettings()
	{
		showColumnSettings = true;
	}

	private async Task SaveColumnSettings()
	{
		showColumnSettings = false;
		_shouldInitializeTable = true;
		StateHasChanged();
	}

	private async Task InitializeAdvancedTable()
	{
		try
		{
			List<INovaAdminColumn> renderColumns = GetRenderColumns().ToList();
			int fixedLeft = renderColumns.Count((INovaAdminColumn a) => a.Fixed == NovaAdminColumnFixed.Left);
			if (IsRowNumber)
			{
				fixedLeft++;
			}
			if (fixedLeft == 0 && FixedLeftColumns > 0)
			{
				fixedLeft = FixedLeftColumns;
			}
			int fixedRight = renderColumns.Count((INovaAdminColumn a) => a.Fixed == NovaAdminColumnFixed.Right);
			if (fixedRight == 0 && FixedRightColumns > 0)
			{
				fixedRight = FixedRightColumns;
			}
			await JS.InvokeVoidAsync("novaAdminJS.initializeAdvancedTable", _theadElement, fixedLeft, fixedRight, _objRef, OnDragRow.HasDelegate);
		}
		catch (JSDisconnectedException)
		{
		}
		catch (TaskCanceledException)
		{
		}
		catch (Exception)
		{
		}
	}

	private void MoveColumn(INovaAdminColumn item, bool up)
	{
		List<INovaAdminColumn> list = ColumnDefs.OrderBy((INovaAdminColumn a) => a.Order).ToList();
		int num = list.IndexOf(item);
		if (up && num > 0)
		{
			int order = list[num - 1].Order;
			list[num - 1].Order = item.Order;
			item.Order = order;
		}
		else if (!up && num < list.Count - 1)
		{
			int order2 = list[num + 1].Order;
			list[num + 1].Order = item.Order;
			item.Order = order2;
		}
	}

	private void ToggleFixed(INovaAdminColumn col, NovaAdminColumnFixed fixedType)
	{
		List<INovaAdminColumn> list = ColumnDefs.OrderBy((INovaAdminColumn c) => c.Order).ToList();
		int num = list.IndexOf(col);
		if (num == -1)
		{
			return;
		}
		if (col.Fixed != fixedType)
		{
			col.Fixed = fixedType;
			switch (fixedType)
			{
			case NovaAdminColumnFixed.Left:
			{
				for (int num3 = 0; num3 < num; num3++)
				{
					if (list[num3].Fixed != NovaAdminColumnFixed.Left)
					{
						list[num3].Fixed = NovaAdminColumnFixed.Left;
					}
				}
				break;
			}
			case NovaAdminColumnFixed.Right:
			{
				for (int num2 = num + 1; num2 < list.Count; num2++)
				{
					if (list[num2].Fixed != NovaAdminColumnFixed.Right)
					{
						list[num2].Fixed = NovaAdminColumnFixed.Right;
					}
				}
				break;
			}
			}
		}
		else
		{
			NovaAdminColumnFixed adminColumnFixed = col.Fixed;
			col.Fixed = NovaAdminColumnFixed.None;
			switch (adminColumnFixed)
			{
			case NovaAdminColumnFixed.Left:
			{
				for (int num5 = num + 1; num5 < list.Count; num5++)
				{
					if (list[num5].Fixed == NovaAdminColumnFixed.Left)
					{
						list[num5].Fixed = NovaAdminColumnFixed.None;
					}
				}
				break;
			}
			case NovaAdminColumnFixed.Right:
			{
				for (int num4 = num - 1; num4 >= 0; num4--)
				{
					if (list[num4].Fixed == NovaAdminColumnFixed.Right)
					{
						list[num4].Fixed = NovaAdminColumnFixed.None;
					}
				}
				break;
			}
			}
		}
		foreach (INovaAdminColumn columnDef in ColumnDefs)
		{
			_fixedOverrides[columnDef] = columnDef.Fixed;
		}
		List<INovaAdminColumn> list2 = (from c in ColumnDefs
			orderby (c.Fixed != NovaAdminColumnFixed.Left) ? ((c.Fixed == NovaAdminColumnFixed.None) ? 1 : 2) : 0, c.Order
			select c).ToList();
		for (int num6 = 0; num6 < list2.Count; num6++)
		{
			list2[num6].Order = num6 + 1;
		}
		_shouldInitializeTable = true;
		StateHasChanged();
	}

	[JSInvokable]
	public async Task HandleColumnReorder(int oldIndex, int newIndex)
	{
		if (oldIndex == newIndex)
		{
			return;
		}
		List<INovaAdminColumn> list = ColumnDefs.OrderBy((INovaAdminColumn a) => a.Order).ToList();
		if (oldIndex >= 0 && oldIndex < list.Count && newIndex >= 0 && newIndex < list.Count)
		{
			INovaAdminColumn item = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, item);
			INovaAdminColumn prev = ((newIndex > 0) ? list[newIndex - 1] : null);
			INovaAdminColumn next = ((newIndex < list.Count - 1) ? list[newIndex + 1] : null);
			if (prev != null && prev.Fixed == NovaAdminColumnFixed.Left)
			{
				item.Fixed = NovaAdminColumnFixed.Left;
			}
			else if (prev != null && prev.Fixed == NovaAdminColumnFixed.Right)
			{
				item.Fixed = NovaAdminColumnFixed.Right;
			}
			else if (next != null && next.Fixed == NovaAdminColumnFixed.Right)
			{
				item.Fixed = NovaAdminColumnFixed.Right;
			}
			else if (prev == null && next != null && next.Fixed == NovaAdminColumnFixed.Left)
			{
				item.Fixed = NovaAdminColumnFixed.Left;
			}
			else
			{
				item.Fixed = NovaAdminColumnFixed.None;
			}
			_fixedOverrides[item] = item.Fixed;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Order = i + 1;
			}
			_shouldInitializeTable = true;
			StateHasChanged();
		}
	}

	[JSInvokable]
	public void HandleFilterReorder(int oldIndex, int newIndex)
	{
		List<NovaAdminFilterInfo> list = q.Filters.OrderBy((NovaAdminFilterInfo a) => a.Order).ToList();
		if (oldIndex >= 0 && oldIndex < list.Count && newIndex >= 0 && newIndex < list.Count)
		{
			NovaAdminFilterInfo adminFilterInfo = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, adminFilterInfo);
			for (int num = 0; num < list.Count; num++)
			{
				list[num].Order = num + 1;
			}
			StateHasChanged();
		}
	}

	private void MoveFilter(NovaAdminFilterInfo item, bool up)
	{
		List<NovaAdminFilterInfo> list = q.Filters.OrderBy((NovaAdminFilterInfo a) => a.Order).ToList();
		int num = list.IndexOf(item);
		if (up && num > 0)
		{
			int order = list[num - 1].Order;
			list[num - 1].Order = item.Order;
			item.Order = order;
		}
		else if (!up && num < list.Count - 1)
		{
			int order2 = list[num + 1].Order;
			list[num + 1].Order = item.Order;
			item.Order = order2;
		}
	}

	private object GetItemPrimaryValue(TItem item)
	{
		return metaTItem.Primarys[0].GetValue((object)item);
	}

	protected override void OnInitialized()
	{
		tempid = "NovaAdminTable_" + Guid.NewGuid().ToString("n");
		metaTItem = fsql.CodeFirst.GetTableByEntity(typeof(TItem));
		if (metaTItem.Primarys.Length != 1)
		{
			bool flag = (IsSingleSelect = false);
			bool flag3 = (IsRowRemove = flag);
			bool flag5 = (IsNotifyChanged = flag3);
			bool flag7 = (IsMultiSelect = flag5);
			bool flag9 = (IsAudit = flag7);
			bool flag11 = (IsRemove = flag9);
			bool isAdd = (IsEdit = flag11);
			IsAdd = isAdd;
		}
		if (Title.IsNull())
		{
			Title = metaTItem.Comment;
		}
		q.PageSize = PageSize;
		q.IsQueryString = IsQueryString;
		q.Filters = new NovaAdminFilterInfo[0];
		treeNav = (from a in metaTItem.GetAllTableRef()
			where a.Value.Exception == null && (int)a.Value.RefType == 2 && a.Value.RefEntityType == typeof(TItem) && a.Value.Columns.Count == 1 && a.Value.Columns[0].Attribute.IsPrimary && a.Value.RefColumns.Count == 1
			select a).FirstOrDefault();
		if (!treeNav.Key.IsNull())
		{
			q.PageSize = -1;
		}
		boundaryName = typeof(AggregateRootRepository<TItem>).GetPropertyOrFieldValue(repository, "_boundaryName") as string;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		foreach (KeyValuePair<INovaAdminColumn, NovaAdminColumnFixed> kvp in _fixedOverrides)
		{
			if (ColumnDefs.Contains(kvp.Key))
			{
				kvp.Key.Fixed = kvp.Value;
			}
		}
		if (showColumnSettings && !_settingsSortableInitialized)
		{
			_settingsSortableInitialized = true;
			await JS.InvokeVoidAsync("novaAdminJS.initColumnSettingsSortable", tempid, _objRef);
		}
		else if (!showColumnSettings)
		{
			_settingsSortableInitialized = false;
		}
		if (_shouldInitializeTable)
		{
			_shouldInitializeTable = false;
			await InitializeAdvancedTable();
		}
		if (!firstRender)
		{
			return;
		}
		if (IsAdd)
		{
			IsAdd = admin.AuthButton(tabInfo.Menu, "add");
		}
		if (IsEdit)
		{
			IsEdit = admin.AuthButton(tabInfo.Menu, "edit");
		}
		if (IsRemove)
		{
			IsRemove = admin.AuthButton(tabInfo.Menu, "remove");
		}
		if (IsAudit)
		{
			IsAudit = isEntityAudited && (admin.AuthButton(tabInfo.Menu, "audit_00") || admin.AuthButton(tabInfo.Menu, "audit_01") || admin.AuthButton(tabInfo.Menu, "audit_02") || admin.AuthButton(tabInfo.Menu, "audit_03") || admin.AuthButton(tabInfo.Menu, "audit_04") || admin.AuthButton(tabInfo.Menu, "audit_05") || admin.AuthButton(tabInfo.Menu, "audit_98") || admin.AuthButton(tabInfo.Menu, "audit_99"));
		}
		if (IsViewAuditEntityLog)
		{
			IsViewAuditEntityLog = isEntityAudited && admin.AuthButton(tabInfo.Menu, "edit_version");
		}
		if (InitQuery != null)
		{
			await InitQuery(q);
			if (q.IsQueryString)
			{
				NovaAdminFilterInfo[] filters = q.Filters;
				foreach (NovaAdminFilterInfo filter in filters)
				{
					string[] query = Nav.GetQueryStringValues(filter.QueryStringName, tabInfo?.Url);
					string[] array = query;
					foreach (string qval in array)
					{
						for (int x = 0; x < filter.Options.Length; x++)
						{
							if (filter.Options[x].Value.Value == qval)
							{
								filter.Options[x].Selected = true;
							}
						}
					}
				}
			}
		}
		q.IsTracking = IsEdit;
		q.InvokeQueryAsync = Load;
		q.InvokeAddAsync = BeginAdd;
		await q.InvokeQueryAsync();
		if (IsNotifyChanged)
		{
			admin.RegisterNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnAdd", delegate(string blazorId, object arg)
			{
				if (!(admin.BlazorId == blazorId) && arg is TItem source && q.PageNumber <= 1)
				{
					TItem val = CloneItem(source);
					NovaAdminItem<TItem> admitem = new NovaAdminItem<TItem>(val)
					{
						RowClass = "bg-newdata"
					};
					Task.Delay(60000).ContinueWith((Task _) => admitem.RowClass = null);
					((IBaseRepository<TItem>)(object)repository).Attach(admitem.Value);
					SetItemAttribute(admitem);
					items.Insert(0, admitem);
					InvokeAsync((Action)base.StateHasChanged);
				}
			});
			admin.RegisterNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnEdit", delegate(string blazorId, object arg)
			{
				if (!(admin.BlazorId == blazorId))
				{
					TItem val = arg as TItem;
					string newitemKeyString = EntityUtilExtensions.GetEntityKeyString(fsql, typeof(TItem), (object)val, false, "*|_,[,_|*");
					NovaAdminItem<TItem> item = items.FirstOrDefault((NovaAdminItem<TItem> a) => a.KeyString == newitemKeyString);
					if (item != null)
					{
						AggregateRootUtils.MapEntityValue(boundaryName, fsql, typeof(TItem), (object)val, (object)item.Value);
						((IBaseRepository<TItem>)(object)repository).Attach(item.Value);
						item.RowClass = "bg-newdata";
						Task.Delay(60000).ContinueWith((Task _) => item.RowClass = null);
						InvokeAsync((Action)base.StateHasChanged);
					}
				}
			});
			admin.RegisterNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnRemove", delegate(string blazorId, object arg)
			{
				if (!(admin.BlazorId == blazorId))
				{
					TItem val = arg as TItem;
					string newitemKeyString = EntityUtilExtensions.GetEntityKeyString(fsql, typeof(TItem), (object)val, false, "*|_,[,_|*");
					NovaAdminItem<TItem> adminItem = items.FirstOrDefault((NovaAdminItem<TItem> a) => a.KeyString == newitemKeyString);
					if (adminItem != null)
					{
						items.Remove(adminItem);
						InvokeAsync((Action)base.StateHasChanged);
					}
				}
			});
		}
		_objRef = DotNetObjectReference.Create(this);
		await InitializeAdvancedTable();
	}

	[JSInvokable]
	public async Task HandleRowReorder(int oldIndex, int newIndex)
	{
		if (oldIndex != newIndex && oldIndex >= 0 && oldIndex < items.Count && newIndex >= 0 && newIndex < items.Count)
		{
			TItem movedItem = items[oldIndex].Value;
			TItem targetItem = items[newIndex].Value;
			if (OnDragRow.HasDelegate)
			{
				await OnDragRow.InvokeAsync(new TItem[2] { movedItem, targetItem });
			}
			await Load();
			await InitializeAdvancedTable();
		}
	}

	[NovaButton("NovaAdminTable_look")]
	public async Task Load()
	{
		await _0024Rougamo_Load();
	}

	public Task InvokeQueryAsync()
	{
		return Load();
	}

	private void SetItemAttribute(NovaAdminItem<TItem> item)
	{
		if (CanbeSelect != null)
		{
			item.Disabled = !CanbeSelect(item.Value);
		}
		if (IsAudit)
		{
			EntityAudited entityAudited = item.Value as EntityAudited;
			item.DisabledSave = entityAudited.AuditStatus != SysAuditStatus.待提交 && (entityAudited.AuditStatus != SysAuditStatus.退回 || !(entityAudited.AuditStep == "audit_00"));
		}
		if (IsNotifyChanged)
		{
			item.KeyString = EntityUtilExtensions.GetEntityKeyString(fsql, typeof(TItem), (object)item.Value, false, "*|_,[,_|*");
		}
	}

	[AntiConcurrency(100)]
	private async Task Refersh()
	{
		await _0024Rougamo_Refersh();
	}

	[NovaButton("remove")]
	[AntiConcurrency(100)]
	private async Task Remove(NovaAdminItem<TItem> opt = null)
	{
		await _0024Rougamo_Remove(opt);
	}

	private async Task OnDblClick(NovaAdminItem<TItem> opt)
	{
		if ((IsView && EditTemplate != null) || IsAdd || IsEdit)
		{
			if (IsEdit && !opt.DisabledSave)
			{
				await BeginEdit(opt.Value);
			}
			else
			{
				await BeginView(opt);
			}
		}
	}

	[NovaButton("NovaAdminTable_look")]
	private async Task BeginView(NovaAdminItem<TItem> opt)
	{
		await _0024Rougamo_BeginView(opt);
	}

	[NovaButton("add")]
	private async Task BeginAdd()
	{
		await _0024Rougamo_BeginAdd();
	}

	[NovaButton("add")]
	private async Task<bool> EndAdd()
	{
		return await _0024Rougamo_EndAdd();
	}

	[NovaButton("edit")]
	private async Task BeginEdit(TItem editItem)
	{
		await _0024Rougamo_BeginEdit(editItem);
	}

	private async Task EditClose()
	{
		if (item != null)
		{
			if (isEntityAudited)
			{
				await admin.UnlockResource((item as EntityAudited).Id);
			}
			if (OnEditClose.HasDelegate)
			{
				await OnEditClose.InvokeAsync(item);
			}
			item = null;
		}
	}

	[NovaButton("edit")]
	private async Task<bool> EndEdit()
	{
		return await _0024Rougamo_EndEdit();
	}

	public TItem CloneItem(TItem source)
	{
		if (source == null)
		{
			return null;
		}
		TItem val = new TItem();
		AggregateRootUtils.MapEntityValue((string)null, fsql, typeof(TItem), (object)source, (object)val);
		foreach (KeyValuePair<string, TableRef> item in from a in fsql.CodeFirst.GetTableByEntity(typeof(TItem)).GetAllTableRef()
			where a.Value.Exception == null && (int)a.Value.RefType == 1
			select a)
		{
			EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, typeof(TItem), (object)val, item.Key, EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, typeof(TItem), (object)source, item.Key));
		}
		foreach (KeyValuePair<string, TableRef> item2 in from a in fsql.CodeFirst.GetTableByEntity(typeof(TItem)).GetAllTableRef()
			where a.Value.Exception == null && (int)a.Value.RefType == 0
			select a)
		{
			object entityValueWithPropertyName = EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, typeof(TItem), (object)source, item2.Key);
			if (entityValueWithPropertyName == null)
			{
				continue;
			}
			object entityValueWithPropertyName2 = EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, typeof(TItem), (object)val, item2.Key);
			foreach (KeyValuePair<string, TableRef> item3 in from a in fsql.CodeFirst.GetTableByEntity(item2.Value.RefEntityType).GetAllTableRef()
				where a.Value.Exception == null && (int)a.Value.RefType == 1
				select a)
			{
				EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, item2.Value.RefEntityType, entityValueWithPropertyName2, item3.Key, EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, item2.Value.RefEntityType, entityValueWithPropertyName, item3.Key));
			}
		}
		foreach (KeyValuePair<string, TableRef> item4 in from a in fsql.CodeFirst.GetTableByEntity(typeof(TItem)).GetAllTableRef()
			where a.Value.Exception == null && (int)a.Value.RefType == 2
			select a)
		{
			if (!(EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, typeof(TItem), (object)source, item4.Key) is IEnumerable enumerable))
			{
				continue;
			}
			IEnumerable enumerable2 = EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, typeof(TItem), (object)val, item4.Key) as IEnumerable;
			List<object> list = new List<object>();
			List<object> list2 = new List<object>();
			foreach (object item5 in enumerable)
			{
				list.Add(item5);
			}
			foreach (object item6 in enumerable2)
			{
				list2.Add(item6);
			}
			if (list.Count != list2.Count)
			{
				continue;
			}
			foreach (KeyValuePair<string, TableRef> item7 in from a in fsql.CodeFirst.GetTableByEntity(item4.Value.RefEntityType).GetAllTableRef()
				where a.Value.Exception == null && (int)a.Value.RefType == 1
				select a)
			{
				for (int num = 0; num < list.Count; num++)
				{
					EntityUtilExtensions.SetEntityValueWithPropertyName(fsql, item4.Value.RefEntityType, list2[num], item7.Key, EntityUtilExtensions.GetEntityValueWithPropertyName(fsql, item4.Value.RefEntityType, list[num], item7.Key));
				}
			}
		}
		return val;
	}

	[AntiConcurrency(100)]
	private async Task Save()
	{
		await _0024Rougamo_Save();
	}

	private Dictionary<string, object> GetRowAttributes(NovaAdminItem<TItem> opt)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("class", opt.RowClass + " " + (opt.Selected ? "tr-selected" : ""));
		if (OnRowClick.HasDelegate)
		{
			dictionary.Add("onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RowClick(opt)));
		}
		else
		{
			dictionary.Add("ondblclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnDblClick(opt)));
		}
		return dictionary;
	}

	private async Task RowClick(NovaAdminItem<TItem> opt)
	{
		if (OnSelectChanged.HasDelegate)
		{
			if (opt == null)
			{
				return;
			}
			await CheckboxOnChange(opt, !opt.Selected);
		}
		if (!OnRowClick.HasDelegate || opt == null)
		{
			return;
		}
		List<NovaAdminItem<TItem>> relateItems = new List<NovaAdminItem<TItem>> { opt };
		for (int a = 0; a < items.Count; a++)
		{
			if (items[a] == opt)
			{
				for (int b = a + 1; b < items.Count && items[b].Level > opt.Level; b++)
				{
					relateItems.Add(items[b]);
				}
				break;
			}
		}
		await OnRowClick.InvokeAsync(relateItems);
	}

	private async Task AuditedChange(NovaNovaAdminAuditedEventArgs e)
	{
		if (OnAudited.HasDelegate)
		{
			await OnAudited.InvokeAsync(e);
		}
		if (IsNotifyChanged)
		{
			admin.TriggerNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnEdit", item);
		}
	}

	private async Task CheckboxOnChange(NovaAdminItem<TItem> opt, bool selected)
	{
		if (opt == null)
		{
			MultiSelectChecked = selected;
			for (int a = 0; a < items.Count; a++)
			{
				items[a].Selected = selected;
			}
		}
		else if (IsSingleSelect)
		{
			object optPkValue = GetItemPrimaryValue(opt.Value);
			TItem tmp = null;
			items.ForEach(delegate(NovaAdminItem<TItem> adminItem)
			{
				adminItem.Selected = (object.Equals(GetItemPrimaryValue(adminItem.Value), optPkValue) ? true : false);
				if (adminItem.Selected)
				{
					tmp = adminItem.Value;
				}
			});
			selectedSingleValue = optPkValue;
		}
		else
		{
			opt.Selected = selected;
			for (int a2 = 0; a2 < items.Count; a2++)
			{
				if (items[a2] != opt)
				{
					continue;
				}
				for (int b = a2 + 1; b < items.Count && items[b].Level > opt.Level; b++)
				{
					items[b].Selected = opt.Selected;
				}
				if (!IsAutoSelectParent)
				{
					break;
				}
				List<int> parents = new List<int>();
				int b2;
				for (b2 = a2 - 1; b2 >= 0; b2--)
				{
					if (items[b2].Level < opt.Level && !parents.Any((int c) => items[c].Level == items[b2].Level))
					{
						parents.Add(b2);
						if (items[b2].Level == 1)
						{
							break;
						}
					}
				}
				int selectedCount = 0;
				foreach (int parentIndex in parents)
				{
					if (opt.Selected || selectedCount > 0)
					{
						items[parentIndex].Selected = true;
						continue;
					}
					NovaAdminItem<TItem> rootMenu = items[parentIndex];
					int maxLevel = 0;
					int b3;
					for (b3 = parentIndex + 1; b3 < items.Count && items[b3].Level > rootMenu.Level; b3++)
					{
						maxLevel = items[b3].Level;
						if (items[b3].Selected)
						{
							selectedCount++;
						}
					}
					if (b3 > parentIndex + 1 && rootMenu.Level + 1 < maxLevel)
					{
						rootMenu.Selected = selectedCount > 0;
					}
				}
				break;
			}
		}
		if (CanbeSelect != null)
		{
			items.ForEach(delegate(NovaAdminItem<TItem> adminItem)
			{
				adminItem.Selected = !adminItem.Disabled && adminItem.Selected;
			});
		}
		if (OnSelectChanged.HasDelegate)
		{
			await OnSelectChanged.InvokeAsync(items);
		}
	}

	[AntiConcurrency(100)]
	private async Task ExportExcel()
	{
		await _0024Rougamo_ExportExcel();
	}

	private async Task SysAuditEntityLogInitQuery(NovaAdminQueryInfo e)
	{
		await Task.Yield();
	}

	private void SysAuditEntityLogOnQuery(NovaAdminQueryEventArgs<SysAuditEntityLog> e)
	{
		EntityAudited audit = item as EntityAudited;
		ISelect<SysAuditEntityLog> obj = e.Select.Where((Expression<Func<SysAuditEntityLog, bool>>)((SysAuditEntityLog a) => a.TableName == metaTItem.DbName)).WhereIf(audit != null, (Expression<Func<SysAuditEntityLog, bool>>)((SysAuditEntityLog a) => a.TableId == audit.Id)).WhereIf(audit == null, (Expression<Func<SysAuditEntityLog, bool>>)((SysAuditEntityLog a) => false))
			.OrderByIf<DateTime?>(e.Sort.IsNull(), (Expression<Func<SysAuditEntityLog, DateTime?>>)((SysAuditEntityLog a) => a.CreatedTime), true);
		bool num = !e.Sort.IsNull();
		string obj2 = e.Sort?.Replace("@desc", "");
		string sort = e.Sort;
		((ISelect0<ISelect<SysAuditEntityLog>, SysAuditEntityLog>)(object)obj).OrderByPropertyNameIf(num, obj2, sort == null || !sort.Contains("@desc"));
	}

	public void Dispose()
	{
		_objRef?.Dispose();
	}

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		bool value = !items.Any((NovaAdminItem<TItem> a) => a.Selected) || items.Any((NovaAdminItem<TItem> a) => a.Selected && a.DisabledSave);
		bool is1r4c = TableTd1 != null && TableRow == null && TableHeader == null && !IsEdit && !IsRemove;
		if (is1r4c)
		{
			IsView = false;
		}
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "class", "card " + (is1r4c ? "" : "card-info ") + "card-outline");
		if (IsShowSearchFilter && q.Filters.Any())
		{
			__builder.OpenElement(2, "div");
			__builder.AddAttribute(3, "class", "card-header d-block");
			__builder.OpenComponent<NovaAdminSearchFilter>(4);
			__builder.AddComponentParameter(5, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
			__builder.CloseComponent();
			__builder.CloseElement();
		}
		if (IsRemove || (IsEdit && IsSingleSelect) || IsAdd || CardHeader != null || IsSearchText)
		{
			__builder.OpenElement(6, "div");
			__builder.AddAttribute(7, "class", "card-header d-block");
			if (IsRemove && (IsMultiSelect || IsSingleSelect))
			{
				__builder.OpenElement(8, "button");
				__builder.AddAttribute(9, "disabled", value);
				__builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Remove()));
				__builder.AddAttribute(11, "type", "button");
				__builder.AddAttribute(12, "class", "mr-2 btn btn-light btn-del");
				__builder.AddMarkupContent(13, "<i class=\"far fa-trash-alt\"></i>");
				__builder.CloseElement();
			}
			if (IsEdit && IsSingleSelect)
			{
				__builder.OpenElement(14, "button");
				__builder.AddAttribute(15, "disabled", value);
				__builder.AddAttribute(16, "onclick", EventCallback.Factory.Create((object)this, (Func<MouseEventArgs, Task>)delegate
				{
					NovaAdminTable<TItem> adminTable = this;
					NovaAdminItem<TItem>? adminItem = items.FirstOrDefault((NovaAdminItem<TItem> a) => a.Selected);
					return adminTable.BeginEdit((adminItem != null) ? adminItem.Value : null);
				}));
				__builder.AddAttribute(17, "type", "button");
				__builder.AddAttribute(18, "class", "mr-2 btn btn-light btn-edit");
				__builder.AddMarkupContent(19, "<i class=\"far fa-edit\"></i>");
				__builder.CloseElement();
			}
			if (IsAdd)
			{
				__builder.OpenElement(20, "button");
				__builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)BeginAdd));
				__builder.AddAttribute(22, "type", "button");
				__builder.AddAttribute(23, "class", "mr-2 btn btn-light btn-add");
				__builder.AddMarkupContent(24, "<i class=\"fas fa-plus\"></i> ");
				__builder.AddContent(25, AddText);
				__builder.CloseElement();
			}
			if (IsRefersh)
			{
				__builder.OpenElement(26, "button");
				__builder.AddAttribute(27, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Refersh()));
				__builder.AddAttribute(28, "type", "button");
				__builder.AddAttribute(29, "class", "mr-2 btn btn-light btn-refersh");
				__builder.AddMarkupContent(30, "<i class=\"fas fa-sync-alt\"></i> 刷新");
				__builder.CloseElement();
			}
			if (IsExportExcel && TableHeader != null)
			{
				__builder.OpenElement(31, "button");
				__builder.AddAttribute(32, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => ExportExcel()));
				__builder.AddAttribute(33, "type", "button");
				__builder.AddAttribute(34, "class", "mr-2 btn btn-light btn-export");
				__builder.AddMarkupContent(35, "<i class=\"fa-solid fa-download\"></i> 导出");
				__builder.CloseElement();
			}
			__builder.AddContent(36, CardHeader);
			__builder.AddMarkupContent(37, "\r\n\r\n            ");
			__builder.OpenElement(38, "div");
			__builder.AddAttribute(39, "class", "float-end");
			if (IsSearchText)
			{
				__builder.OpenComponent<NovaAdminSearchText>(40);
				__builder.AddComponentParameter(41, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
				__builder.AddComponentParameter(42, "Placeholder", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(SearchPlaceholder));
				__builder.CloseComponent();
			}
			if (ColumnDefs.Any())
			{
				__builder.OpenElement(43, "button");
				__builder.AddAttribute(44, "class", "btn btn-sm btn-link position-absolute");
				__builder.AddAttribute(45, "style", "right: 5px; top: 0px; z-index: 10;");
				__builder.AddAttribute(46, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)OpenColumnSettings));
				__builder.AddMarkupContent(47, "<i class=\"fa fa-cog\"></i>");
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}
		__builder.OpenElement(48, "div");
		__builder.AddAttribute(49, "class", "admintable2-loader table-loader");
		__builder.AddAttribute(50, "style", isShowLoader ? "" : "display:none");
		__builder.AddMarkupContent(51, "<div class=\"spinner spinner-border text-primary\"><span class=\"visually-hidden\">Loading...</span></div>");
		__builder.CloseElement();
		__builder.AddMarkupContent(52, "\r\n    ");
		__builder.OpenElement(53, "div");
		__builder.AddAttribute(54, "class", "card-body p-0 table-shim table-scroll scroll table-fixed-column");
		__builder.AddAttribute(55, "style", "border:none;" + ((BodyHeight > 0) ? $"height:{BodyHeight}px;margin-top:0.5rem;overflow:auto;" : ""));
		TypeInference.CreateCascadingValue_0(__builder, 56, 57, q, 58, delegate(RenderTreeBuilder _builder)
		{
			TypeInference.CreateCascadingValue_1(_builder, 59, 60, this, 61, __arg1: true, 62, delegate(RenderTreeBuilder renderTreeBuilder)
			{
				//IL_3005: Unknown result type (might be due to invalid IL or missing references)
				//IL_3021: Unknown result type (might be due to invalid IL or missing references)
				//IL_3053: Unknown result type (might be due to invalid IL or missing references)
				renderTreeBuilder.OpenElement(63, "div");
				renderTreeBuilder.AddAttribute(64, "style", "display:none");
				if (TableColumns != null)
				{
					renderTreeBuilder.AddContent(65, TableColumns(new TItem()));
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(66, "\r\n                ");
				renderTreeBuilder.OpenElement(67, "table");
				renderTreeBuilder.AddAttribute(68, "class", "table " + ((is1r4c && treeNav.Key.IsNull()) ? "" : "table-hover ") + "table-bordered table-sm " + (IsRowNumber ? "table-rownumber" : "") + " m-0");
				renderTreeBuilder.OpenElement(69, "thead");
				renderTreeBuilder.AddAttribute(70, "style", is1r4c ? "display:none " : "");
				renderTreeBuilder.AddElementReferenceCapture(71, delegate(ElementReference __value)
				{
					_theadElement = __value;
				});
				if (ColumnDefs.Any())
				{
					bool flag = ColumnDefs.Any((INovaAdminColumn c) => c.Primary);
					bool flag2 = ColumnDefs.Any((INovaAdminColumn c) => c.IsOperation);
					renderTreeBuilder.OpenElement(72, "tr");
					if (IsRowNumber)
					{
						renderTreeBuilder.OpenElement(73, "th");
						renderTreeBuilder.AddAttribute(74, "class", "fixed no-resize");
						renderTreeBuilder.AddAttribute(75, "style", "left:0;width:" + Math.Max(30, (Math.Max(0, q.PageNumber - 1) * q.PageSize + items.Count).ToString().Length * 15) + "px");
						renderTreeBuilder.CloseElement();
					}
					if ((IsMultiSelect || IsSingleSelect) && !flag)
					{
						renderTreeBuilder.AddMarkupContent(76, "<th class='no-resize' style='width:30px;'></th>");
					}
					foreach (INovaAdminColumn renderColumn in GetRenderColumns())
					{
						renderTreeBuilder.OpenElement(77, "th");
						renderTreeBuilder.AddAttribute(78, "style", renderColumn.CalculatedStyle + " " + ((renderColumn.Width > 0) ? $"width:{renderColumn.Width}px;" : ((renderColumn.IsOperation && TableTd99Width > 75) ? $"width:{TableTd99Width}px;" : "")));
						if (renderColumn.Primary && IsMultiSelect && !IsSingleSelect)
						{
							renderTreeBuilder.OpenElement(79, "input");
							renderTreeBuilder.AddAttribute(80, "checked", MultiSelectChecked);
							renderTreeBuilder.AddAttribute(81, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(null, e.Value.ConvertTo<bool>())));
							renderTreeBuilder.AddAttribute(82, "type", "checkbox");
							renderTreeBuilder.AddAttribute(83, "class", "form-check-input mr-2");
							renderTreeBuilder.CloseElement();
						}
						if (!string.IsNullOrEmpty(renderColumn.Sort))
						{
							renderTreeBuilder.OpenComponent<NovaAdminSort>(84);
							renderTreeBuilder.AddComponentParameter(85, "Text", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(renderColumn.Title));
							renderTreeBuilder.AddComponentParameter(86, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(renderColumn.Sort));
							renderTreeBuilder.CloseComponent();
						}
						else
						{
							renderTreeBuilder.AddContent(87, renderColumn.Title);
						}
						renderTreeBuilder.CloseElement();
					}
					if (!flag2 && (TableTd99 != null || IsEdit || IsRemove || IsRowRemove))
					{
						renderTreeBuilder.OpenElement(88, "th");
						renderTreeBuilder.AddAttribute(89, "width", TableTd99Width);
						renderTreeBuilder.AddMarkupContent(90, "操作");
						renderTreeBuilder.CloseElement();
					}
					renderTreeBuilder.CloseElement();
				}
				else if (TableHeader != null)
				{
					renderTreeBuilder.OpenElement(91, "tr");
					if (!is1r4c && IsRowNumber)
					{
						renderTreeBuilder.OpenElement(92, "th");
						renderTreeBuilder.AddAttribute(93, "class", "fixed no-resize");
						renderTreeBuilder.AddAttribute(94, "style", "left:0;width:" + Math.Max(30, (Math.Max(0, q.PageNumber - 1) * q.PageSize + items.Count).ToString().Length * 15) + "px");
						renderTreeBuilder.CloseElement();
					}
					if (IsMultiSelect || IsSingleSelect || TableTd1 != null)
					{
						renderTreeBuilder.OpenElement(95, "th");
						renderTreeBuilder.AddAttribute(96, "class", (TableTd1 == null && TableTh1 == null) ? "no-resize" : "");
						renderTreeBuilder.AddAttribute(97, "style", (TableTd1 == null && TableTh1 == null) ? "width:30px;" : ((TableTd1Width > 0) ? $"width:{TableTd1Width}px;" : ""));
						renderTreeBuilder.AddAttribute(98, "colspan", (!is1r4c) ? 1 : Colspan);
						if (IsMultiSelect && !IsSingleSelect)
						{
							renderTreeBuilder.OpenElement(99, "input");
							renderTreeBuilder.AddAttribute(100, "checked", MultiSelectChecked);
							renderTreeBuilder.AddAttribute(101, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(null, e.Value.ConvertTo<bool>())));
							renderTreeBuilder.AddAttribute(102, "type", "checkbox");
							renderTreeBuilder.AddAttribute(103, "class", "form-check-input");
							renderTreeBuilder.CloseElement();
						}
						else if (IsSingleSelect)
						{
							renderTreeBuilder.AddMarkupContent(104, "<span>&nbsp;</span>");
						}
						if (TableTh1 != null)
						{
							if (IsMultiSelect || IsSingleSelect)
							{
								renderTreeBuilder.AddMarkupContent(105, "<span class=\"mr-2\"></span>");
							}
							renderTreeBuilder.AddContent(106, TableTh1);
						}
						renderTreeBuilder.CloseElement();
					}
					if (IsAudit)
					{
						renderTreeBuilder.AddMarkupContent(107, "<th class=\"table-column-audit no-resize\">审核状态</th>");
					}
					renderTreeBuilder.AddContent(108, TableHeader);
					if (TableTd99 != null || IsEdit || IsRemove || IsRowRemove)
					{
						renderTreeBuilder.OpenElement(109, "th");
						renderTreeBuilder.AddAttribute(110, "width", TableTd99Width);
						renderTreeBuilder.AddContent(111, "\u00a0");
						renderTreeBuilder.CloseElement();
					}
					renderTreeBuilder.CloseElement();
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(112, "\r\n                    ");
				renderTreeBuilder.OpenElement(113, "tbody");
				if (ColumnDefs.Any())
				{
					bool flag3 = ColumnDefs.Any((INovaAdminColumn c) => c.Primary);
					bool flag4 = ColumnDefs.Any((INovaAdminColumn c) => c.IsOperation);
					if (!IsShowSearchFilter)
					{
						renderTreeBuilder.OpenElement(114, "tr");
						if (IsRowNumber)
						{
							renderTreeBuilder.AddMarkupContent(115, "<th class=\"fixed\" style=\"left:0;font-weight:normal;\"></th>");
						}
						if ((IsMultiSelect || IsSingleSelect) && !flag3)
						{
							renderTreeBuilder.AddMarkupContent(116, "<td></td>");
						}
						if (IsAudit)
						{
							renderTreeBuilder.AddMarkupContent(117, "<td></td>");
						}
						foreach (INovaAdminColumn renderColumn2 in GetRenderColumns())
						{
							renderTreeBuilder.OpenElement(118, "td");
							renderTreeBuilder.AddAttribute(119, "style", renderColumn2.CalculatedStyle);
							if (!string.IsNullOrEmpty(renderColumn2.FilterKey))
							{
								renderTreeBuilder.OpenComponent<NovaAdminFilterInput>(120);
								renderTreeBuilder.AddComponentParameter(121, "FilterKey", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(renderColumn2.FilterKey));
								renderTreeBuilder.CloseComponent();
							}
							renderTreeBuilder.CloseElement();
						}
						if (!flag4 && (TableTd99 != null || IsEdit || IsRemove || IsRowRemove))
						{
							renderTreeBuilder.AddMarkupContent(122, "<td></td>");
						}
						renderTreeBuilder.CloseElement();
					}
					for (int num = 0; num < items.Count; num++)
					{
						if (num > 0 && !items[num - 1].Expanded)
						{
							for (NovaAdminItem<TItem> adminItem = items[num - 1]; num < items.Count && items[num].Level > adminItem.Level; num++)
							{
							}
							if (num >= items.Count)
							{
								break;
							}
						}
						NovaAdminItem<TItem> opt = items[num];
						renderTreeBuilder.OpenElement(123, "tr");
						renderTreeBuilder.AddMultipleAttributes(124, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck((IEnumerable<KeyValuePair<string, object>>)GetRowAttributes(opt)));
						if (IsRowNumber)
						{
							renderTreeBuilder.OpenElement(125, "th");
							renderTreeBuilder.AddAttribute(126, "class", "fixed");
							renderTreeBuilder.AddAttribute(127, "style", "left:0;font-weight:normal;");
							renderTreeBuilder.AddContent(128, Math.Max(0, q.PageNumber - 1) * q.PageSize + num + 1);
							renderTreeBuilder.CloseElement();
						}
						if ((IsMultiSelect || IsSingleSelect) && !flag3)
						{
							renderTreeBuilder.OpenElement(129, "td");
							renderTreeBuilder.AddAttribute(130, "style", (opt.Level > 1) ? $"padding-left:{(opt.Level - 1) * 24}px" : "");
							if (IsMultiSelect)
							{
								renderTreeBuilder.OpenElement(131, "input");
								renderTreeBuilder.AddAttribute(132, "disabled", opt.Disabled);
								renderTreeBuilder.AddAttribute(133, "checked", opt.Selected);
								renderTreeBuilder.AddAttribute(134, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt, (bool)e.Value)));
								renderTreeBuilder.AddAttribute(135, "type", "checkbox");
								renderTreeBuilder.AddAttribute(136, "class", "form-check-input");
								renderTreeBuilder.CloseElement();
							}
							else
							{
								renderTreeBuilder.OpenElement(137, "input");
								renderTreeBuilder.AddAttribute(138, "disabled", opt.Disabled);
								renderTreeBuilder.AddAttribute(139, "checked", opt.Selected);
								renderTreeBuilder.AddAttribute(140, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt, (bool)e.Value)));
								renderTreeBuilder.AddAttribute(141, "type", "radio");
								renderTreeBuilder.AddAttribute(142, "name", tempid + "_selectRadio");
								renderTreeBuilder.AddAttribute(143, "class", "form-check-input");
								renderTreeBuilder.CloseElement();
							}
							renderTreeBuilder.CloseElement();
						}
						foreach (INovaAdminColumn renderColumn3 in GetRenderColumns())
						{
							renderTreeBuilder.OpenElement(144, "td");
							renderTreeBuilder.AddAttribute(145, "style", renderColumn3.CalculatedStyle + " " + ((renderColumn3.Primary && opt.Level > 1) ? $"padding-left:{(opt.Level - 1) * 24}px" : ""));
							renderTreeBuilder.AddAttribute(146, "title", (renderColumn3.Width > 0 && renderColumn3.Template == null) ? renderColumn3.GetValue(opt.Value) : "");
							if (renderColumn3.Primary && (IsMultiSelect || IsSingleSelect))
							{
								if (IsMultiSelect)
								{
									renderTreeBuilder.OpenElement(147, "input");
									renderTreeBuilder.AddAttribute(148, "disabled", opt.Disabled);
									renderTreeBuilder.AddAttribute(149, "checked", opt.Selected);
									renderTreeBuilder.AddAttribute(150, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt, (bool)e.Value)));
									renderTreeBuilder.AddAttribute(151, "type", "checkbox");
									renderTreeBuilder.AddAttribute(152, "class", "form-check-input mr-2");
									renderTreeBuilder.CloseElement();
								}
								else
								{
									renderTreeBuilder.OpenElement(153, "input");
									renderTreeBuilder.AddAttribute(154, "disabled", opt.Disabled);
									renderTreeBuilder.AddAttribute(155, "checked", opt.Selected);
									renderTreeBuilder.AddAttribute(156, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt, (bool)e.Value)));
									renderTreeBuilder.AddAttribute(157, "type", "radio");
									renderTreeBuilder.AddAttribute(158, "name", tempid + "_selectRadio");
									renderTreeBuilder.AddAttribute(159, "class", "form-check-input mr-2");
									renderTreeBuilder.CloseElement();
								}
							}
							if (renderColumn3.IsOperation)
							{
								renderTreeBuilder.OpenElement(160, "div");
								renderTreeBuilder.AddAttribute(161, "class", "btn-toolbar");
								if (renderColumn3.Template != null)
								{
									renderTreeBuilder.AddContent(162, renderColumn3.Template(opt.Value));
								}
								if (TableTd99 != null)
								{
									renderTreeBuilder.AddContent(163, TableTd99(opt.Value));
								}
								if (IsEdit)
								{
									renderTreeBuilder.OpenElement(164, "button");
									renderTreeBuilder.AddAttribute(165, "disabled", opt.DisabledSave);
									renderTreeBuilder.AddAttribute(166, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginEdit(opt.Value)));
									renderTreeBuilder.AddAttribute(167, "type", "button");
									renderTreeBuilder.AddAttribute(168, "class", "mr-2 btn btn-light btn-xs btn-edit");
									renderTreeBuilder.AddMarkupContent(169, "<i class=\"fa fa-edit\"></i>");
									renderTreeBuilder.CloseElement();
								}
								if (IsRemove)
								{
									renderTreeBuilder.OpenElement(170, "button");
									renderTreeBuilder.AddAttribute(171, "disabled", opt.DisabledSave);
									renderTreeBuilder.AddAttribute(172, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Remove(opt)));
									renderTreeBuilder.AddAttribute(173, "type", "button");
									renderTreeBuilder.AddAttribute(174, "class", "btn btn-light btn-xs btn-del");
									renderTreeBuilder.AddMarkupContent(175, "<i class=\"far fa-trash-alt\"></i>");
									renderTreeBuilder.CloseElement();
								}
								renderTreeBuilder.CloseElement();
							}
							else if (renderColumn3.Template != null)
							{
								renderTreeBuilder.AddContent(176, renderColumn3.Template(opt.Value));
							}
							else
							{
								renderTreeBuilder.AddContent(177, renderColumn3.GetValue(opt.Value));
							}
							renderTreeBuilder.CloseElement();
						}
						if (!flag4 && (TableTd99 != null || IsEdit || IsRemove || IsRowRemove))
						{
							renderTreeBuilder.OpenElement(178, "td");
							renderTreeBuilder.AddAttribute(179, "width", TableTd99Width);
							renderTreeBuilder.OpenElement(180, "div");
							renderTreeBuilder.AddAttribute(181, "class", "btn-toolbar");
							if (TableTd99 != null)
							{
								renderTreeBuilder.AddContent(182, TableTd99(opt.Value));
							}
							if (IsEdit)
							{
								renderTreeBuilder.OpenElement(183, "button");
								renderTreeBuilder.AddAttribute(184, "disabled", opt.DisabledSave);
								renderTreeBuilder.AddAttribute(185, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginEdit(opt.Value)));
								renderTreeBuilder.AddAttribute(186, "type", "button");
								renderTreeBuilder.AddAttribute(187, "class", "mr-2 btn btn-light btn-xs btn-edit");
								renderTreeBuilder.AddMarkupContent(188, "<i class=\"fa fa-edit\"></i>");
								renderTreeBuilder.CloseElement();
							}
							if (IsRemove)
							{
								renderTreeBuilder.OpenElement(189, "button");
								renderTreeBuilder.AddAttribute(190, "disabled", opt.DisabledSave);
								renderTreeBuilder.AddAttribute(191, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Remove(opt)));
								renderTreeBuilder.AddAttribute(192, "type", "button");
								renderTreeBuilder.AddAttribute(193, "class", "btn btn-light btn-xs btn-del");
								renderTreeBuilder.AddMarkupContent(194, "<i class=\"far fa-trash-alt\"></i>");
								renderTreeBuilder.CloseElement();
							}
							renderTreeBuilder.CloseElement();
							renderTreeBuilder.CloseElement();
						}
						renderTreeBuilder.CloseElement();
					}
				}
				else
				{
					if (!is1r4c && TableFilter != null)
					{
						renderTreeBuilder.OpenElement(195, "tr");
						if (IsRowNumber)
						{
							renderTreeBuilder.AddMarkupContent(196, "<th class=\"fixed\" style=\"left:0;font-weight:normal;\"></th>");
						}
						if (IsMultiSelect || IsSingleSelect || TableTd1 != null)
						{
							renderTreeBuilder.AddMarkupContent(197, "<td></td>");
						}
						if (IsAudit)
						{
							renderTreeBuilder.AddMarkupContent(198, "<td></td>");
						}
						renderTreeBuilder.AddContent(199, TableFilter);
						if (TableTd99 != null || IsEdit || IsRemove || IsRowRemove)
						{
							renderTreeBuilder.AddMarkupContent(200, "<td></td>");
						}
						renderTreeBuilder.CloseElement();
					}
					if (is1r4c)
					{
						if (treeNav.Key.IsNull() || !items.Any((NovaAdminItem<TItem> a) => a.Level > 1))
						{
							for (int num2 = 0; num2 < items.Count; num2 += Colspan)
							{
								renderTreeBuilder.OpenElement(201, "tr");
								for (int num3 = 0; num3 < Colspan; num3++)
								{
									NovaAdminItem<TItem> opt2 = ((num2 + num3 < items.Count) ? items[num2 + num3] : null);
									renderTreeBuilder.OpenElement(202, "td");
									renderTreeBuilder.AddAttribute(203, "width", 100 / Colspan + "%");
									renderTreeBuilder.AddAttribute(204, "class", "pl-3");
									if (opt2 != null)
									{
										if (IsMultiSelect && !IsSingleSelect)
										{
											renderTreeBuilder.OpenElement(205, "input");
											renderTreeBuilder.AddAttribute(206, "disabled", opt2.Disabled);
											renderTreeBuilder.AddAttribute(207, "checked", opt2.Selected);
											renderTreeBuilder.AddAttribute(208, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt2, e.Value.ConvertTo<bool>())));
											renderTreeBuilder.AddAttribute(209, "type", "checkbox");
											renderTreeBuilder.AddAttribute(210, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt2.Value));
											renderTreeBuilder.AddAttribute(211, "class", "form-check-input");
											renderTreeBuilder.CloseElement();
										}
										else if (IsSingleSelect)
										{
											renderTreeBuilder.OpenElement(212, "input");
											renderTreeBuilder.AddAttribute(213, "disabled", opt2.Disabled);
											renderTreeBuilder.AddAttribute(214, "checked", opt2.Selected);
											renderTreeBuilder.AddAttribute(215, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt2, e.Value.ConvertTo<bool>())));
											renderTreeBuilder.AddAttribute(216, "type", "radio");
											renderTreeBuilder.AddAttribute(217, "name", tempid + "_selectRadio");
											renderTreeBuilder.AddAttribute(218, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt2.Value));
											renderTreeBuilder.AddAttribute(219, "class", "form-check-input");
											renderTreeBuilder.CloseElement();
										}
										renderTreeBuilder.OpenElement(220, "label");
										renderTreeBuilder.AddAttribute(221, "for", tempid + "selectRadio_" + GetItemPrimaryValue(opt2.Value));
										renderTreeBuilder.AddAttribute(222, "class", "form-check-label pl-2");
										renderTreeBuilder.AddContent(223, TableTd1(opt2.Value));
										renderTreeBuilder.CloseElement();
									}
									else
									{
										renderTreeBuilder.AddMarkupContent(224, "<span>&nbsp;</span>");
									}
									renderTreeBuilder.CloseElement();
								}
								renderTreeBuilder.CloseElement();
							}
						}
						else
						{
							for (int num4 = 0; num4 < items.Count; num4++)
							{
								NovaAdminItem<TItem> opt3 = items[num4];
								renderTreeBuilder.OpenElement(225, "tr");
								renderTreeBuilder.OpenElement(226, "td");
								renderTreeBuilder.AddAttribute(227, "style", (opt3.Level > 1) ? $"padding-left:{(opt3.Level - 1) * 24}px" : "");
								renderTreeBuilder.OpenElement(228, "div");
								renderTreeBuilder.AddAttribute(229, "style", "float:left;width:25%;");
								if (opt3 != null)
								{
									if (IsMultiSelect && !IsSingleSelect)
									{
										renderTreeBuilder.OpenElement(230, "input");
										renderTreeBuilder.AddAttribute(231, "disabled", opt3.Disabled);
										renderTreeBuilder.AddAttribute(232, "checked", opt3.Selected);
										renderTreeBuilder.AddAttribute(233, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt3, e.Value.ConvertTo<bool>())));
										renderTreeBuilder.AddAttribute(234, "type", "checkbox");
										renderTreeBuilder.AddAttribute(235, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt3.Value));
										renderTreeBuilder.AddAttribute(236, "class", "form-check-input");
										renderTreeBuilder.CloseElement();
									}
									else if (IsSingleSelect)
									{
										renderTreeBuilder.OpenElement(237, "input");
										renderTreeBuilder.AddAttribute(238, "disabled", opt3.Disabled);
										renderTreeBuilder.AddAttribute(239, "checked", opt3.Selected);
										renderTreeBuilder.AddAttribute(240, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt3, e.Value.ConvertTo<bool>())));
										renderTreeBuilder.AddAttribute(241, "type", "radio");
										renderTreeBuilder.AddAttribute(242, "name", tempid + "_selectRadio");
										renderTreeBuilder.AddAttribute(243, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt3.Value));
										renderTreeBuilder.AddAttribute(244, "class", "form-check-input");
										renderTreeBuilder.CloseElement();
									}
									renderTreeBuilder.OpenElement(245, "label");
									renderTreeBuilder.AddAttribute(246, "for", tempid + "selectRadio_" + GetItemPrimaryValue(opt3.Value));
									renderTreeBuilder.AddAttribute(247, "class", "form-check-label pl-1");
									renderTreeBuilder.AddContent(248, TableTd1(opt3.Value));
									renderTreeBuilder.CloseElement();
								}
								else
								{
									renderTreeBuilder.AddMarkupContent(249, "<span>&nbsp;</span>");
								}
								renderTreeBuilder.CloseElement();
								int num5 = 0;
								for (int num6 = num4 + 1; num6 < items.Count; num6++)
								{
									if (items[num6].Level != opt3.Level + 1)
									{
										if (items[num6].Level > opt3.Level + 1)
										{
											num5 = 0;
										}
										break;
									}
									num5 = num6;
								}
								if (num4 < num5 && (num5 + 1 >= items.Count || items[num5 + 1].Level < items[num5].Level))
								{
									renderTreeBuilder.OpenElement(250, "div");
									renderTreeBuilder.AddAttribute(251, "style", "float:left;width:75%;");
									for (int num7 = num4 + 1; num7 <= num5; num7++)
									{
										NovaAdminItem<TItem> opt4 = items[num7];
										if (IsMultiSelect && !IsSingleSelect)
										{
											renderTreeBuilder.OpenElement(252, "input");
											renderTreeBuilder.AddAttribute(253, "disabled", opt3.Disabled);
											renderTreeBuilder.AddAttribute(254, "checked", opt4.Selected);
											renderTreeBuilder.AddAttribute(255, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt4, e.Value.ConvertTo<bool>())));
											renderTreeBuilder.AddAttribute(256, "type", "checkbox");
											renderTreeBuilder.AddAttribute(257, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt4.Value));
											renderTreeBuilder.AddAttribute(258, "class", "form-check-input");
											renderTreeBuilder.CloseElement();
										}
										else if (IsSingleSelect)
										{
											renderTreeBuilder.OpenElement(259, "input");
											renderTreeBuilder.AddAttribute(260, "disabled", opt3.Disabled);
											renderTreeBuilder.AddAttribute(261, "checked", opt4.Selected);
											renderTreeBuilder.AddAttribute(262, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt4, e.Value.ConvertTo<bool>())));
											renderTreeBuilder.AddAttribute(263, "type", "radio");
											renderTreeBuilder.AddAttribute(264, "name", tempid + "_selectRadio");
											renderTreeBuilder.AddAttribute(265, "id", tempid + "selectRadio_" + GetItemPrimaryValue(opt4.Value));
											renderTreeBuilder.AddAttribute(266, "class", "form-check-input");
											renderTreeBuilder.CloseElement();
										}
										renderTreeBuilder.OpenElement(267, "label");
										renderTreeBuilder.AddAttribute(268, "for", tempid + "selectRadio_" + GetItemPrimaryValue(opt4.Value));
										renderTreeBuilder.AddAttribute(269, "class", "form-check-label pl-1 mr-2");
										renderTreeBuilder.AddContent(270, TableTd1(opt4.Value));
										renderTreeBuilder.CloseElement();
									}
									num4 = num5;
									renderTreeBuilder.CloseElement();
								}
								renderTreeBuilder.AddMarkupContent(271, "<div style=\"clear:both\"></div>");
								renderTreeBuilder.CloseElement();
								renderTreeBuilder.CloseElement();
							}
						}
					}
					else
					{
						for (int num8 = 0; num8 < items.Count; num8++)
						{
							if (num8 > 0 && !items[num8 - 1].Expanded)
							{
								for (NovaAdminItem<TItem> adminItem2 = items[num8 - 1]; num8 < items.Count && items[num8].Level > adminItem2.Level; num8++)
								{
								}
								if (num8 >= items.Count)
								{
									break;
								}
							}
							NovaAdminItem<TItem> opt5 = items[num8];
							renderTreeBuilder.OpenElement(272, "tr");
							renderTreeBuilder.AddMultipleAttributes(273, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck((IEnumerable<KeyValuePair<string, object>>)GetRowAttributes(opt5)));
							if (IsRowNumber)
							{
								renderTreeBuilder.OpenElement(274, "th");
								renderTreeBuilder.AddAttribute(275, "class", "fixed");
								renderTreeBuilder.AddAttribute(276, "style", "left:0;font-weight:normal;");
								renderTreeBuilder.AddContent(277, Math.Max(0, q.PageNumber - 1) * q.PageSize + num8 + 1);
								renderTreeBuilder.CloseElement();
							}
							if (IsMultiSelect || IsSingleSelect || TableTd1 != null)
							{
								renderTreeBuilder.OpenElement(278, "td");
								renderTreeBuilder.AddAttribute(279, "style", (opt5.Level > 1) ? $"padding-left:{(opt5.Level - 1) * 24}px" : "");
								if (!treeNav.Key.IsNull())
								{
									if (num8 + 1 < items.Count && items[num8 + 1].Level > opt5.Level)
									{
										renderTreeBuilder.OpenElement(280, "i");
										renderTreeBuilder.AddAttribute(281, "class", "fa fa-caret-" + (opt5.Expanded ? "down" : "right"));
										renderTreeBuilder.AddAttribute(282, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
										{
											opt5.Expanded = !opt5.Expanded;
										}));
										renderTreeBuilder.AddEventStopPropagationAttribute(283, "onclick", value: true);
										renderTreeBuilder.AddEventStopPropagationAttribute(284, "ondblclick", value: true);
										renderTreeBuilder.AddAttribute(285, "style", "cursor:pointer;width:18px;font-size:14px;padding-right:3px;");
										renderTreeBuilder.CloseElement();
									}
									else
									{
										renderTreeBuilder.AddMarkupContent(286, "<i class=\"fa \" style=\"cursor:pointer;width:18px;font-size:14px;padding-right:3px;\"></i>");
									}
								}
								if (IsMultiSelect && !IsSingleSelect)
								{
									renderTreeBuilder.OpenElement(287, "input");
									renderTreeBuilder.AddAttribute(288, "disabled", opt5.Disabled);
									renderTreeBuilder.AddAttribute(289, "checked", opt5.Selected);
									renderTreeBuilder.AddAttribute(290, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt5, e.Value.ConvertTo<bool>())));
									renderTreeBuilder.AddEventStopPropagationAttribute(291, "onclick", value: true);
									renderTreeBuilder.AddEventStopPropagationAttribute(292, "ondblclick", value: true);
									renderTreeBuilder.AddAttribute(293, "type", "checkbox");
									renderTreeBuilder.AddAttribute(294, "class", "form-check-input");
									renderTreeBuilder.CloseElement();
								}
								else if (IsSingleSelect)
								{
									renderTreeBuilder.OpenElement(295, "input");
									renderTreeBuilder.AddAttribute(296, "disabled", opt5.Disabled);
									renderTreeBuilder.AddAttribute(297, "checked", opt5.Selected);
									renderTreeBuilder.AddAttribute(298, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => CheckboxOnChange(opt5, e.Value.ConvertTo<bool>())));
									renderTreeBuilder.AddEventStopPropagationAttribute(299, "onclick", value: true);
									renderTreeBuilder.AddEventStopPropagationAttribute(300, "ondblclick", value: true);
									renderTreeBuilder.AddAttribute(301, "type", "radio");
									renderTreeBuilder.AddAttribute(302, "name", tempid + "_selectRadio");
									renderTreeBuilder.AddAttribute(303, "class", "form-check-input");
									renderTreeBuilder.CloseElement();
								}
								if (TableTd1 != null)
								{
									if (IsMultiSelect || IsSingleSelect)
									{
										renderTreeBuilder.AddMarkupContent(304, "<span class=\"mr-2\"></span>");
									}
									renderTreeBuilder.AddContent(305, TableTd1(opt5.Value));
								}
								renderTreeBuilder.CloseElement();
							}
							if (IsAudit)
							{
								renderTreeBuilder.OpenElement(306, "td");
								renderTreeBuilder.AddAttribute(307, "class", "table-column-audit");
								renderTreeBuilder.AddEventStopPropagationAttribute(308, "onclick", value: true);
								renderTreeBuilder.AddEventStopPropagationAttribute(309, "ondblclick", value: true);
								renderTreeBuilder.OpenComponent<NovaAdminAudit>(310);
								renderTreeBuilder.AddComponentParameter(311, "Item", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(opt5.Value as EntityAudited));
								renderTreeBuilder.AddComponentParameter(312, "OnAudited", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, (NovaNovaAdminAuditedEventArgs e) => AuditedChange(e))));
								renderTreeBuilder.CloseComponent();
								renderTreeBuilder.CloseElement();
							}
							if (TableRow != null)
							{
								renderTreeBuilder.AddContent(313, TableRow(opt5.Value));
							}
							if (TableTd99 != null || IsEdit || IsRemove || IsRowRemove)
							{
								renderTreeBuilder.OpenElement(314, "td");
								renderTreeBuilder.AddEventStopPropagationAttribute(315, "onclick", value: true);
								renderTreeBuilder.AddEventStopPropagationAttribute(316, "ondblclick", value: true);
								renderTreeBuilder.OpenElement(317, "div");
								renderTreeBuilder.AddAttribute(318, "class", "btn-toolbar");
								if (TableTd99 != null)
								{
									renderTreeBuilder.AddContent(319, TableTd99(opt5.Value));
								}
								if (IsEdit)
								{
									renderTreeBuilder.OpenElement(320, "button");
									renderTreeBuilder.AddAttribute(321, "disabled", opt5.DisabledSave);
									renderTreeBuilder.AddAttribute(322, "onclick", EventCallback.Factory.Create((object)this, (Func<MouseEventArgs, Task>)delegate
									{
										NovaAdminTable<TItem> adminTable = this;
										NovaAdminItem<TItem> adminItem3 = opt5;
										return adminTable.BeginEdit((adminItem3 != null) ? adminItem3.Value : null);
									}));
									renderTreeBuilder.AddAttribute(323, "type", "button");
									renderTreeBuilder.AddAttribute(324, "class", "mr-2 btn btn-light btn-xs btn-edit");
									renderTreeBuilder.AddMarkupContent(325, "<i class=\"fa fa-edit\"></i>");
									renderTreeBuilder.CloseElement();
								}
								if (IsRemove || IsRowRemove)
								{
									if (IsConfirmRemove && !opt5.DisabledSave)
									{
										renderTreeBuilder.OpenComponent<PopConfirmButton>(326);
										renderTreeBuilder.AddComponentParameter(327, "Color", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Color>((Color)8));
										renderTreeBuilder.AddComponentParameter(328, "Size", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Size>((Size)1));
										renderTreeBuilder.AddComponentParameter(329, "class", "btn-del");
										renderTreeBuilder.AddComponentParameter(330, "ConfirmButtonColor", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Color>((Color)5));
										renderTreeBuilder.AddComponentParameter(331, "ConfirmIcon", "fa-solid fa-triangle-exclamation text-danger");
										renderTreeBuilder.AddComponentParameter(332, "Text", "");
										renderTreeBuilder.AddComponentParameter(333, "Icon", "far fa-trash-alt");
										renderTreeBuilder.AddComponentParameter(334, "Content", "确定删除吗？");
										renderTreeBuilder.AddComponentParameter(335, "OnConfirm", (Func<Task>)(() => Remove(opt5)));
										renderTreeBuilder.CloseComponent();
									}
									else
									{
										renderTreeBuilder.OpenElement(336, "button");
										renderTreeBuilder.AddAttribute(337, "disabled", opt5.DisabledSave);
										renderTreeBuilder.AddAttribute(338, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Remove(opt5)));
										renderTreeBuilder.AddAttribute(339, "type", "button");
										renderTreeBuilder.AddAttribute(340, "class", "btn btn-light btn-xs btn-del");
										renderTreeBuilder.AddMarkupContent(341, "<i class=\"far fa-trash-alt\"></i>");
										renderTreeBuilder.CloseElement();
									}
								}
								renderTreeBuilder.CloseElement();
								renderTreeBuilder.CloseElement();
							}
							renderTreeBuilder.CloseElement();
						}
					}
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				if (!isLoaded)
				{
					renderTreeBuilder.AddMarkupContent(342, "<div style=\"text-align:center;padding:18px;color:#6a6a6a;\">玩命加载中...</div>");
				}
				else if (!items.Any())
				{
					renderTreeBuilder.OpenElement(343, "div");
					renderTreeBuilder.AddAttribute(344, "style", "text-align:center;padding:18px;color:#6a6a6a;");
					renderTreeBuilder.AddContent(345, NoDataText);
					renderTreeBuilder.CloseElement();
				}
			});
		});
		__builder.CloseElement();
		if ((q.PageSize > 0 && q.Total > q.PageSize) || CardFooter != null)
		{
			__builder.OpenElement(346, "div");
			__builder.AddAttribute(347, "class", "card-footer");
			if (q.PageSize > 0 && q.Total > q.PageSize)
			{
				__builder.OpenComponent<NovaAdminPagination>(348);
				__builder.AddComponentParameter(349, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
				__builder.CloseComponent();
			}
			__builder.AddContent(350, CardFooter);
			__builder.CloseElement();
		}
		__builder.CloseElement();
		if (TableHeader == null && TableRow == null && TableColumns != null)
		{
			__builder.OpenComponent<NovaModal>(351);
			__builder.AddComponentParameter(352, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(showColumnSettings));
			__builder.AddComponentParameter(353, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)SaveColumnSettings)));
			__builder.AddComponentParameter(354, "Title", "表格设置");
			__builder.AddComponentParameter(355, "IsFooter", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			__builder.AddComponentParameter(356, "IsBackdropStatic", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			__builder.AddComponentParameter(357, "IsDrawer", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
			__builder.AddComponentParameter(358, "DrawerPlacement", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(NovaAdminDrawerPlacement.Bottom));
			__builder.AddAttribute(359, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				renderTreeBuilder.OpenComponent<Tab>(360);
				renderTreeBuilder.AddComponentParameter(361, "IsCard", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
				renderTreeBuilder.AddComponentParameter(362, "IsLazyLoadTabItem", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
				renderTreeBuilder.AddComponentParameter(363, "TabStyle", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<TabStyle>((TabStyle)1));
				renderTreeBuilder.AddAttribute(364, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
				{
					renderTreeBuilder2.OpenComponent<TabItem>(365);
					renderTreeBuilder2.AddComponentParameter(366, "Text", "列显示");
					renderTreeBuilder2.AddComponentParameter(367, "Icon", "fa-solid fa-table-columns");
					renderTreeBuilder2.AddAttribute(368, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
					{
						renderTreeBuilder3.OpenElement(369, "div");
						renderTreeBuilder3.AddAttribute(370, "class", "column-setting-body");
						renderTreeBuilder3.OpenElement(371, "div");
						renderTreeBuilder3.AddAttribute(372, "id", tempid + "_columns");
						renderTreeBuilder3.AddAttribute(373, "style", "display: grid; grid-template-columns: repeat(10, 1fr); gap: 0.5rem; overflow-y: auto; padding:0.5rem;");
						foreach (INovaAdminColumn col in ColumnDefs.OrderBy((INovaAdminColumn a) => a.Order))
						{
							renderTreeBuilder3.OpenElement(374, "div");
							renderTreeBuilder3.AddAttribute(375, "class", "col");
							renderTreeBuilder3.AddAttribute(376, "data-id", col.GetHashCode());
							renderTreeBuilder3.OpenElement(377, "div");
							renderTreeBuilder3.AddAttribute(378, "class", "card h-100 column-setting-item position-relative " + ((col.Fixed == NovaAdminColumnFixed.Left) ? "fixed-left" : ((col.Fixed == NovaAdminColumnFixed.Right) ? "fixed-right" : (col.Visible ? "border-primary" : ""))));
							renderTreeBuilder3.OpenElement(379, "div");
							renderTreeBuilder3.AddAttribute(380, "class", "card-body p-2 d-flex align-items-center");
							renderTreeBuilder3.AddMarkupContent(381, "<div class=\"drag-handle text-secondary me-2\" style=\"cursor: move;\"><i class=\"fa-solid fa-grip-vertical\"></i></div>\r\n                                    ");
							renderTreeBuilder3.OpenElement(382, "div");
							renderTreeBuilder3.AddAttribute(383, "class", "form-check mb-0 text-truncate flex-fill ps-0");
							renderTreeBuilder3.AddAttribute(384, "title", col.Title);
							renderTreeBuilder3.OpenElement(385, "input");
							renderTreeBuilder3.AddAttribute(386, "class", "form-check-input ms-0");
							renderTreeBuilder3.AddAttribute(387, "type", "checkbox");
							renderTreeBuilder3.AddAttribute(388, "id", "col_" + col.GetHashCode());
							renderTreeBuilder3.AddAttribute(389, "checked", col.Visible);
							renderTreeBuilder3.AddAttribute(390, "onchange", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
							{
								col.Visible = (bool)e.Value;
							}));
							renderTreeBuilder3.AddAttribute(391, "style", "cursor: pointer;");
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.AddMarkupContent(392, "\r\n                                       ");
							renderTreeBuilder3.OpenElement(393, "label");
							renderTreeBuilder3.AddAttribute(394, "class", "form-check-label ps-2 text-truncate w-100");
							renderTreeBuilder3.AddAttribute(395, "for", "col_" + col.GetHashCode());
							renderTreeBuilder3.AddAttribute(396, "style", "cursor: pointer; vertical-align: text-bottom;");
							renderTreeBuilder3.AddContent(397, col.Title);
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.AddMarkupContent(398, "\r\n                                    ");
							renderTreeBuilder3.OpenElement(399, "div");
							renderTreeBuilder3.AddAttribute(400, "class", "column-setting-actions position-absolute top-0 end-0 p-1 " + ((col.Fixed != NovaAdminColumnFixed.None) ? "show-actions" : ""));
							renderTreeBuilder3.OpenElement(401, "i");
							renderTreeBuilder3.AddAttribute(402, "class", "fa-solid fa-thumbtack me-1 action-icon " + ((col.Fixed == NovaAdminColumnFixed.Left) ? "text-primary active" : ""));
							renderTreeBuilder3.AddAttribute(403, "title", "固定到左侧");
							renderTreeBuilder3.AddAttribute(404, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
							{
								ToggleFixed(col, NovaAdminColumnFixed.Left);
							}));
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.AddMarkupContent(405, "\r\n                                        ");
							renderTreeBuilder3.OpenElement(406, "i");
							renderTreeBuilder3.AddAttribute(407, "class", "fa-solid fa-thumbtack fa-rotate-90 action-icon " + ((col.Fixed == NovaAdminColumnFixed.Right) ? "text-danger active" : ""));
							renderTreeBuilder3.AddAttribute(408, "title", "固定到右侧");
							renderTreeBuilder3.AddAttribute(409, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
							{
								ToggleFixed(col, NovaAdminColumnFixed.Right);
							}));
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
						}
						renderTreeBuilder3.CloseElement();
						renderTreeBuilder3.CloseElement();
					});
					renderTreeBuilder2.CloseComponent();
					renderTreeBuilder2.AddMarkupContent(410, "\r\n              ");
					renderTreeBuilder2.OpenComponent<TabItem>(411);
					renderTreeBuilder2.AddComponentParameter(412, "Text", "高级筛选");
					renderTreeBuilder2.AddComponentParameter(413, "Icon", "fa-solid fa-filter");
					renderTreeBuilder2.AddAttribute(414, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
					{
						renderTreeBuilder3.OpenElement(415, "div");
						renderTreeBuilder3.AddAttribute(416, "class", "column-setting-body");
						renderTreeBuilder3.OpenElement(417, "div");
						renderTreeBuilder3.AddAttribute(418, "id", tempid + "_filters");
						renderTreeBuilder3.AddAttribute(419, "style", "display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.5rem; max-height: 320px; overflow-y: auto;");
						foreach (NovaAdminFilterInfo f in q.Filters.OrderBy((NovaAdminFilterInfo a) => a.Order))
						{
							renderTreeBuilder3.OpenElement(420, "div");
							renderTreeBuilder3.AddAttribute(421, "class", "col");
							renderTreeBuilder3.AddAttribute(422, "data-id", f.GetHashCode());
							renderTreeBuilder3.OpenElement(423, "div");
							renderTreeBuilder3.AddAttribute(424, "class", "card h-100 column-setting-item position-relative " + (f.Visible ? "border-primary" : ""));
							renderTreeBuilder3.OpenElement(425, "div");
							renderTreeBuilder3.AddAttribute(426, "class", "card-body p-2 d-flex align-items-center");
							renderTreeBuilder3.AddMarkupContent(427, "<div class=\"drag-handle text-secondary me-2\" style=\"cursor: move;\"><i class=\"fa-solid fa-grip-vertical\"></i></div>\r\n                                    ");
							renderTreeBuilder3.OpenElement(428, "div");
							renderTreeBuilder3.AddAttribute(429, "class", "form-check mb-0 text-truncate flex-fill ps-0");
							renderTreeBuilder3.AddAttribute(430, "title", f.Label);
							renderTreeBuilder3.OpenElement(431, "input");
							renderTreeBuilder3.AddAttribute(432, "class", "form-check-input ms-0");
							renderTreeBuilder3.AddAttribute(433, "type", "checkbox");
							renderTreeBuilder3.AddAttribute(434, "id", "filter_" + f.GetHashCode());
							renderTreeBuilder3.AddAttribute(435, "checked", f.Visible);
							renderTreeBuilder3.AddAttribute(436, "onchange", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
							{
								f.Visible = (bool)e.Value;
							}));
							renderTreeBuilder3.AddAttribute(437, "style", "cursor: pointer;");
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.AddMarkupContent(438, "\r\n                                       ");
							renderTreeBuilder3.OpenElement(439, "label");
							renderTreeBuilder3.AddAttribute(440, "class", "form-check-label ps-2 text-truncate w-100");
							renderTreeBuilder3.AddAttribute(441, "for", "filter_" + f.GetHashCode());
							renderTreeBuilder3.AddAttribute(442, "style", "cursor: pointer; vertical-align: text-bottom;");
							renderTreeBuilder3.AddContent(443, f.Label);
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
							renderTreeBuilder3.CloseElement();
						}
						renderTreeBuilder3.CloseElement();
						renderTreeBuilder3.CloseElement();
					});
					renderTreeBuilder2.CloseComponent();
				});
				renderTreeBuilder.CloseComponent();
				renderTreeBuilder.AddMarkupContent(444, "\r\n           ");
				renderTreeBuilder.AddMarkupContent(445, "<style>\r\n               /* Google Modern Style Simulation */\r\n               .column-setting-item {\r\n                   transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);\r\n                   border: 1px solid #dadce0;\r\n                   background-color: #fff;\r\n                   border-radius: 8px;\r\n                   box-shadow: none;\r\n               }\r\n               \r\n               /* Hover state */\r\n               .column-setting-item:hover {\r\n                   box-shadow: 0 1px 2px 0 rgba(60,64,67,0.3), 0 1px 3px 1px rgba(60,64,67,0.15);\r\n                   border-color: transparent;\r\n                   z-index: 1;\r\n               }\r\n\r\n               /* Fixed Left State */\r\n               .column-setting-item.fixed-left {\r\n                   border: 1px solid #e3d7fb !important;\r\n                   background-color: #f8faff;\r\n               }\r\n\r\n               /* Fixed Right State */\r\n               .column-setting-item.fixed-right {\r\n                   border: 1px solid #fdcbc6 !important;\r\n                   background-color: #fce8e6;\r\n               }\r\n               \r\n               /* Actions Container */\r\n               .column-setting-actions { \r\n                   display: none; \r\n                   background: #fff; \r\n                   border-bottom-left-radius: 8px; \r\n                   border-top-right-radius: 8px; \r\n                   box-shadow: -2px 2px 4px rgba(0,0,0,0.1); \r\n               }\r\n               \r\n               .column-setting-item:hover .column-setting-actions,\r\n               .column-setting-actions.show-actions { display: flex !important; }\r\n               \r\n               /* Icons */\r\n               .column-setting-item .action-icon {\r\n                   cursor: pointer; \r\n                   opacity: 0.5; \r\n                   padding: 2px;\r\n                   color: #5f6368; /* Google Grey */\r\n                   transition: all 0.2s; \r\n                   font-size: 0.9rem;\r\n               }\r\n               .column-setting-item .action-icon:hover { \r\n                   opacity: 1; \r\n                   background-color: rgba(60,64,67,0.08);\r\n                   border-radius: 50%;\r\n               }\r\n               .column-setting-item .action-icon.text-primary.active { \r\n                   opacity: 1; \r\n                   color: #1a73e8 !important; /* Google Blue */\r\n               }\r\n               .column-setting-item .action-icon.text-danger.active { \r\n                   opacity: 1; \r\n                   color: #ea4335 !important; /* Google Red */\r\n               }\r\n           </style>");
			});
			__builder.CloseComponent();
		}
		if ((IsView && EditTemplate != null) || IsAdd || IsEdit)
		{
			__builder.OpenComponent<NovaModal>(446);
			__builder.AddComponentParameter(447, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(item != null));
			__builder.AddComponentParameter(448, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)EditClose)));
			__builder.AddComponentParameter(449, "IsShowLoader", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(isShowLoaderSaving));
			__builder.AddComponentParameter(450, "IsKeyboard", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogIsKeyboard && (itemModalStatus == 1 || itemModalStatus == 2)));
			__builder.AddComponentParameter(451, "IsBackdropStatic", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogIsBackdropStatic && (itemModalStatus == 1 || itemModalStatus == 2)));
			__builder.AddComponentParameter(452, "DialogClassName", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogClassName));
			__builder.AddComponentParameter(453, "Size", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogSize));
			__builder.AddComponentParameter(454, "Animation", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogAnimation));
			__builder.AddComponentParameter(455, "IsDrawer", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(IsDrawer));
			__builder.AddComponentParameter(456, "DrawerPlacement", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DrawerPlacement));
			__builder.AddComponentParameter(457, "DrawerWidth", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DrawerWidth));
			__builder.AddComponentParameter(458, "DrawerHeight", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DrawerHeight));
			__builder.AddComponentParameter(459, "DrawerOffset", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DrawerOffset));
			__builder.AddComponentParameter(460, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【" + ((itemModalStatus == 1) ? AddText : ((itemModalStatus == 2) ? "编辑" : "查看")) + "】" + Title));
			__builder.AddAttribute(461, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				if (EditTemplate != null)
				{
					if (item != null)
					{
						renderTreeBuilder.AddContent(462, EditTemplate(item));
					}
				}
				else
				{
					renderTreeBuilder.AddMarkupContent(463, "<span>请设置 &lt;EditTemplate&gt; 编辑内容</span>");
				}
			});
			__builder.AddAttribute(464, "Footer", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.OpenElement(465, "div");
				renderTreeBuilder.AddAttribute(466, "style", "width:100%;display:block;");
				renderTreeBuilder.OpenElement(467, "div");
				renderTreeBuilder.AddAttribute(468, "style", "float:left;");
				if ((itemModalStatus == 0 || itemModalStatus == 2) && IsViewAuditEntityLog)
				{
					renderTreeBuilder.OpenElement(469, "button");
					renderTreeBuilder.AddAttribute(470, "type", "button");
					renderTreeBuilder.AddAttribute(471, "class", "btn btn-secondary");
					renderTreeBuilder.AddAttribute(472, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
					{
						showAuditEntityLog = true;
					}));
					renderTreeBuilder.AddMarkupContent(473, "<i class=\"fa fa-clock-rotate-left\"></i> 查看历史变化");
					renderTreeBuilder.CloseElement();
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(474, "\r\n                ");
				renderTreeBuilder.OpenElement(475, "div");
				renderTreeBuilder.AddAttribute(476, "style", "float:right;");
				renderTreeBuilder.OpenElement(477, "button");
				renderTreeBuilder.AddAttribute(478, "type", "button");
				renderTreeBuilder.AddAttribute(479, "class", "btn btn-secondary");
				renderTreeBuilder.AddAttribute(480, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)EditClose));
				renderTreeBuilder.AddMarkupContent(481, "<i class=\"fa fa-close\"></i> 取消");
				renderTreeBuilder.CloseElement();
				if (itemModalStatus == 1 || itemModalStatus == 2)
				{
					if (item != null && isEntityAudited && admin.LockResources.TryGetValue((item as EntityAudited).Id, out NovaAdminLockResourceInfo value2) && value2.BlazorId != admin.BlazorId)
					{
						renderTreeBuilder.OpenElement(482, "button");
						renderTreeBuilder.AddAttribute(483, "disabled");
						renderTreeBuilder.AddAttribute(484, "type", "button");
						renderTreeBuilder.AddAttribute(485, "class", "btn btn-primary pl-4 pr-4");
						renderTreeBuilder.AddMarkupContent(486, "<i class=\"fa fa-save\"></i> [");
						renderTreeBuilder.AddContent(487, value2.LockUsername);
						renderTreeBuilder.AddMarkupContent(488, "]正在编辑...");
						renderTreeBuilder.CloseElement();
					}
					else
					{
						renderTreeBuilder.OpenElement(489, "button");
						renderTreeBuilder.AddAttribute(490, "type", "button");
						renderTreeBuilder.AddAttribute(491, "class", "btn btn-primary pl-4 pr-4");
						renderTreeBuilder.AddAttribute(492, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Save));
						renderTreeBuilder.AddMarkupContent(493, "<i class=\"fa fa-save\"></i> ");
						renderTreeBuilder.AddContent(494, SaveText);
						renderTreeBuilder.CloseElement();
					}
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(495, "\r\n                <div style=\"clear:both\"></div>");
				renderTreeBuilder.CloseElement();
			});
			__builder.AddComponentReferenceCapture(496, delegate(object __value)
			{
				editModal = (NovaModal)__value;
			});
			__builder.CloseComponent();
		}
		if (IsViewAuditEntityLog)
		{
			__builder.OpenComponent<NovaModal>(497);
			__builder.AddComponentParameter(498, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(item != null && showAuditEntityLog));
			__builder.AddComponentParameter(499, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
			{
				showAuditEntityLog = false;
			})));
			__builder.AddComponentParameter(500, "DialogClassName", "modal-fullscreen");
			__builder.AddComponentParameter(501, "IsDraggable", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			__builder.AddComponentParameter(502, "IsFooter", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			__builder.AddComponentParameter(503, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【查看历史变化】" + Title));
			__builder.AddAttribute(504, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.OpenComponent<NovaAdminTable<SysAuditEntityLog>>(505);
				renderTreeBuilder.AddComponentParameter(506, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(20));
				renderTreeBuilder.AddComponentParameter(507, "DialogClassName", "modal-lg modal-audit-entity-log");
				renderTreeBuilder.AddComponentParameter(508, "IsAdd", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(509, "IsEdit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(510, "IsRemove", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(511, "IsSingleSelect", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(512, "IsMultiSelect", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(513, "IsQueryString", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(514, "IsRefersh", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(515, "IsSearchText", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(516, "IsExportExcel", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(517, "InitQuery", new Func<NovaAdminQueryInfo, Task>(SysAuditEntityLogInitQuery));
				renderTreeBuilder.AddComponentParameter(518, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysAuditEntityLog>>)SysAuditEntityLogOnQuery)));
				renderTreeBuilder.AddAttribute(519, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
				{
					renderTreeBuilder2.OpenElement(520, "th");
					renderTreeBuilder2.AddAttribute(521, "width", "160");
					renderTreeBuilder2.OpenComponent<NovaAdminSort>(522);
					renderTreeBuilder2.AddComponentParameter(523, "Text", "时间");
					renderTreeBuilder2.AddComponentParameter(524, "Value", "CreatedTime");
					renderTreeBuilder2.CloseComponent();
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.AddMarkupContent(525, "\r\n                    ");
					renderTreeBuilder2.AddMarkupContent(526, "<th width=\"80\">操作人</th>\r\n                    ");
					renderTreeBuilder2.AddMarkupContent(527, "<th width=\"80\">操作类型</th>\r\n                    ");
					renderTreeBuilder2.AddMarkupContent(528, "<th>新数据</th>\r\n                    ");
					renderTreeBuilder2.AddMarkupContent(529, "<th>旧数据</th>");
				});
				renderTreeBuilder.AddAttribute(530, "TableRow", (RenderFragment<SysAuditEntityLog>)((SysAuditEntityLog item) => delegate(RenderTreeBuilder renderTreeBuilder2)
				{
					renderTreeBuilder2.OpenElement(531, "td");
					renderTreeBuilder2.AddContent(532, item.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.AddMarkupContent(533, "\r\n                    ");
					renderTreeBuilder2.OpenElement(534, "td");
					renderTreeBuilder2.AddContent(535, item.CreatedUserName);
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.AddMarkupContent(536, "\r\n                    ");
					renderTreeBuilder2.OpenElement(537, "td");
					if (item.LogType == "add")
					{
						renderTreeBuilder2.OpenElement(538, "span");
						renderTreeBuilder2.AddAttribute(539, "class", "px-1 rounded-1 border");
						renderTreeBuilder2.AddAttribute(540, "style", "background-color: var(--bs-success-bg-subtle); --bs-border-color: var(--bs-success-border-subtle); color: var(--bs-success-text);");
						renderTreeBuilder2.AddContent(541, AddText);
						renderTreeBuilder2.CloseElement();
					}
					else if (item.LogType == "edit")
					{
						renderTreeBuilder2.AddMarkupContent(542, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-warning-bg-subtle); --bs-border-color: var(--bs-warning-border-subtle); color: var(--bs-warning-text);\">修改</span>");
					}
					else if (item.LogType == "remove")
					{
						renderTreeBuilder2.AddMarkupContent(543, "<span class=\"px-1 rounded-1 border\" style=\"background-color: var(--bs-danger-bg-subtle); --bs-border-color: var(--bs-danger-border-subtle); color: var(--bs-danger-text);\">删除</span>");
					}
					else
					{
						renderTreeBuilder2.OpenElement(544, "span");
						renderTreeBuilder2.AddAttribute(545, "class", "px-1 rounded-1 border");
						renderTreeBuilder2.AddAttribute(546, "style", "background-color: var(--bs-secondary-bg-subtle); --bs-border-color: var(--bs-secondary-border-subtle); color: var(--bs-secondary-text);");
						renderTreeBuilder2.AddContent(547, item.LogType);
						renderTreeBuilder2.CloseElement();
					}
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.AddMarkupContent(548, "\r\n                    ");
					renderTreeBuilder2.OpenElement(549, "td");
					renderTreeBuilder2.OpenElement(550, "div");
					renderTreeBuilder2.AddAttribute(551, "class", "table-cell");
					renderTreeBuilder2.AddContent(552, item.Data);
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.AddMarkupContent(553, "\r\n                    ");
					renderTreeBuilder2.OpenElement(554, "td");
					renderTreeBuilder2.OpenElement(555, "div");
					renderTreeBuilder2.AddAttribute(556, "class", "table-cell");
					renderTreeBuilder2.AddContent(557, item.DataOld);
					renderTreeBuilder2.CloseElement();
					renderTreeBuilder2.CloseElement();
				}));
				renderTreeBuilder.AddAttribute(558, "EditTemplate", (RenderFragment<SysAuditEntityLog>)((SysAuditEntityLog item) => delegate(RenderTreeBuilder renderTreeBuilder2)
				{
					renderTreeBuilder2.OpenElement(559, "div");
					renderTreeBuilder2.AddAttribute(560, "style", "height:660px;");
					renderTreeBuilder2.OpenComponent<Split>(561);
					renderTreeBuilder2.AddComponentParameter(562, "ShowBarHandle", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
					renderTreeBuilder2.AddComponentParameter(563, "Basis", "50%");
					renderTreeBuilder2.AddAttribute(564, "FirstPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
					{
						renderTreeBuilder3.OpenElement(565, "div");
						renderTreeBuilder3.AddAttribute(566, "class", "groupbox");
						renderTreeBuilder3.AddAttribute(567, "style", "height:100%;");
						renderTreeBuilder3.AddMarkupContent(568, "<label class=\"legend\">修改前</label>\r\n                                    ");
						renderTreeBuilder3.OpenElement(569, "textarea");
						renderTreeBuilder3.AddAttribute(570, "class", "form-control");
						renderTreeBuilder3.AddAttribute(571, "placeholder", "");
						renderTreeBuilder3.AddAttribute(572, "style", "height:100%;");
						renderTreeBuilder3.AddAttribute(573, "readonly");
						renderTreeBuilder3.AddAttribute(574, "value", BindConverter.FormatValue(item.DataOld));
						renderTreeBuilder3.AddAttribute(575, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
						{
							item.DataOld = __value;
						}, item.DataOld));
						renderTreeBuilder3.SetUpdatesAttributeName("value");
						renderTreeBuilder3.CloseElement();
						renderTreeBuilder3.CloseElement();
					});
					renderTreeBuilder2.AddAttribute(576, "SecondPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
					{
						renderTreeBuilder3.OpenElement(577, "div");
						renderTreeBuilder3.AddAttribute(578, "class", "groupbox");
						renderTreeBuilder3.AddAttribute(579, "style", "height:100%;");
						renderTreeBuilder3.AddMarkupContent(580, "<label class=\"legend\">修改后</label>\r\n                                    ");
						renderTreeBuilder3.OpenElement(581, "textarea");
						renderTreeBuilder3.AddAttribute(582, "class", "form-control");
						renderTreeBuilder3.AddAttribute(583, "placeholder", "");
						renderTreeBuilder3.AddAttribute(584, "style", "height:100%;");
						renderTreeBuilder3.AddAttribute(585, "readonly");
						renderTreeBuilder3.AddAttribute(586, "value", BindConverter.FormatValue(item.Data));
						renderTreeBuilder3.AddAttribute(587, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
						{
							item.Data = __value;
						}, item.Data));
						renderTreeBuilder3.SetUpdatesAttributeName("value");
						renderTreeBuilder3.CloseElement();
						renderTreeBuilder3.CloseElement();
					});
					renderTreeBuilder2.CloseComponent();
					renderTreeBuilder2.CloseElement();
				}));
				renderTreeBuilder.CloseComponent();
			});
			__builder.CloseComponent();
		}
		if (IsDebug)
		{
			__builder.OpenElement(588, "input");
			__builder.AddAttribute(589, "type", "checkbox");
			__builder.AddAttribute(590, "id", "check_IsQueryString");
			__builder.AddAttribute(591, "checked", BindConverter.FormatValue(q.IsQueryString));
			__builder.AddAttribute(592, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				q.IsQueryString = __value;
			}, q.IsQueryString));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(593, "<label for=\"check_IsQueryString\" class=\"form-check-label\">IsQueryString</label>\r\n    ");
			__builder.OpenElement(594, "input");
			__builder.AddAttribute(595, "type", "checkbox");
			__builder.AddAttribute(596, "id", "check_IsRemove");
			__builder.AddAttribute(597, "checked", BindConverter.FormatValue(IsRemove));
			__builder.AddAttribute(598, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsRemove = __value;
			}, IsRemove));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(599, "<label for=\"check_IsRemove\" class=\"form-check-label\">IsRemove</label>\r\n    ");
			__builder.OpenElement(600, "input");
			__builder.AddAttribute(601, "type", "checkbox");
			__builder.AddAttribute(602, "id", "check_IsAdd");
			__builder.AddAttribute(603, "checked", BindConverter.FormatValue(IsAdd));
			__builder.AddAttribute(604, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsAdd = __value;
			}, IsAdd));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(605, "<label for=\"check_IsAdd\" class=\"form-check-label\">IsAdd</label>\r\n    ");
			__builder.OpenElement(606, "input");
			__builder.AddAttribute(607, "type", "checkbox");
			__builder.AddAttribute(608, "id", "check_IsEdit");
			__builder.AddAttribute(609, "checked", BindConverter.FormatValue(IsEdit));
			__builder.AddAttribute(610, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsEdit = __value;
			}, IsEdit));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(611, "<label for=\"check_IsEdit\" class=\"form-check-label\">IsEdit</label>\r\n    ");
			__builder.OpenElement(612, "input");
			__builder.AddAttribute(613, "type", "checkbox");
			__builder.AddAttribute(614, "id", "check_IsSearchText");
			__builder.AddAttribute(615, "checked", BindConverter.FormatValue(IsSearchText));
			__builder.AddAttribute(616, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsSearchText = __value;
			}, IsSearchText));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(617, "<label for=\"check_IsSearchText\" class=\"form-check-label\">IsSearchText</label>\r\n    ");
			__builder.OpenElement(618, "input");
			__builder.AddAttribute(619, "type", "checkbox");
			__builder.AddAttribute(620, "id", "check_IsSingleSelect");
			__builder.AddAttribute(621, "checked", BindConverter.FormatValue(IsSingleSelect));
			__builder.AddAttribute(622, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsSingleSelect = __value;
			}, IsSingleSelect));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(623, "<label for=\"check_IsSingleSelect\" class=\"form-check-label\">IsSingleSelect</label>\r\n    ");
			__builder.OpenElement(624, "input");
			__builder.AddAttribute(625, "type", "checkbox");
			__builder.AddAttribute(626, "id", "check_IsConfirmEdit");
			__builder.AddAttribute(627, "checked", BindConverter.FormatValue(IsConfirmEdit));
			__builder.AddAttribute(628, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsConfirmEdit = __value;
			}, IsConfirmEdit));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(629, "<label for=\"check_IsConfirmEdit\" class=\"form-check-label\">IsConfirmEdit</label>\r\n    ");
			__builder.OpenElement(630, "input");
			__builder.AddAttribute(631, "type", "checkbox");
			__builder.AddAttribute(632, "id", "check_IsConfirmRemove");
			__builder.AddAttribute(633, "checked", BindConverter.FormatValue(IsConfirmRemove));
			__builder.AddAttribute(634, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				IsConfirmRemove = __value;
			}, IsConfirmRemove));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(635, "<label for=\"check_IsConfirmRemove\" class=\"form-check-label\">IsConfirmRemove</label>\r\n    ");
			__builder.OpenComponent<NovaSelectEnum<NovaModalSize>>(636);
			__builder.AddComponentParameter(637, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogSize));
			__builder.AddComponentParameter(638, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(NovaModalSize __value)
			{
				DialogSize = __value;
			}, DialogSize))));
			__builder.CloseComponent();
			__builder.AddMarkupContent(639, "\r\n    ");
			__builder.OpenComponent<NovaSelectEnum<NovaModalAnimation>>(640);
			__builder.AddComponentParameter(641, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DialogAnimation));
			__builder.AddComponentParameter(642, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(NovaModalAnimation __value)
			{
				DialogAnimation = __value;
			}, DialogAnimation))));
			__builder.CloseComponent();
			__builder.AddMarkupContent(643, "\r\n    ");
			__builder.OpenComponent<NovaSelectEnum<NovaAdminDrawerPlacement>>(644);
			__builder.AddComponentParameter(645, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(DrawerPlacement));
			__builder.AddComponentParameter(646, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(NovaAdminDrawerPlacement __value)
			{
				DrawerPlacement = __value;
			}, DrawerPlacement))));
			__builder.CloseComponent();
		}
	}

	private async Task _0024Rougamo_Load()
	{
		if (IsShowLoading)
		{
			isShowLoader = true;
			StateHasChanged();
		}
		if (ItemsSource != null)
		{
			IQueryable<TItem> query = ItemsSource.AsQueryable();
			if (OnQuery.HasDelegate)
			{
				NovaAdminQueryEventArgs<TItem> args = new NovaAdminQueryEventArgs<TItem>(null, q.SearchText?.Trim(), q.Filters, q.Sort)
				{
					Queryable = query
				};
				await OnQuery.InvokeAsync(args);
				query = args.Queryable;
			}
			if (q.PageSize > 0)
			{
				q.Total = query.Count();
				query = query.Skip(Math.Max(0, q.PageNumber - 1) * q.PageSize).Take(q.PageSize);
			}
			items.Clear();
			items = query.ToList().ToNovaAdminItemList(fsql);
			if (OnQueried.HasDelegate)
			{
				await OnQueried.InvokeAsync(items);
			}
			isLoaded = true;
			MultiSelectChecked = false;
			if (CanbeSelect != null || IsAudit || IsNotifyChanged)
			{
				items.ForEach(SetItemAttribute);
			}
			if (OnSelectChanged.HasDelegate)
			{
				await OnSelectChanged.InvokeAsync(items);
			}
		}
		else
		{
			ISelect<TItem> select = ((IBaseRepository<TItem>)(object)repository).Select;
			if (OnQuery.HasDelegate)
			{
				await OnQuery.InvokeAsync(new NovaAdminQueryEventArgs<TItem>(select, q.SearchText?.Trim(), q.Filters, q.Sort));
			}
			if (!q.IsTracking)
			{
				FreeSqlDbContextExtensions.NoTracking<TItem>(select);
			}
			if (q.PageSize > 0)
			{
				NovaAdminQueryInfo adminQueryInfo = q;
				adminQueryInfo.Total = await ((ISelect0<ISelect<TItem>, TItem>)(object)select).CountAsync(default(CancellationToken));
				((ISelect0<ISelect<TItem>, TItem>)(object)select).Page(q.PageNumber, q.PageSize);
			}
			List<TItem> list = await ((ISelect0<ISelect<TItem>, TItem>)(object)select).ToListAsync(default(CancellationToken));
			if (q.PageSize <= 0)
			{
				q.Total = list.Count;
			}
			items.Clear();
			items = list.ToNovaAdminItemList(fsql);
			if (OnQueried.HasDelegate)
			{
				await OnQueried.InvokeAsync(items);
			}
			isLoaded = true;
			MultiSelectChecked = false;
			if (CanbeSelect != null || IsAudit || IsNotifyChanged)
			{
				items.ForEach(SetItemAttribute);
			}
			if (OnSelectChanged.HasDelegate)
			{
				await OnSelectChanged.InvokeAsync(items);
			}
		}
		if (IsShowLoading)
		{
			await Task.Delay(200);
			isShowLoader = false;
		}
		StateHasChanged();
	}

	private async Task _0024Rougamo_Refersh()
	{
		await Load();
	}

	private async Task _0024Rougamo_Remove([Optional] NovaAdminItem<TItem> opt)
	{
		List<TItem> list = new List<TItem>();
		if (opt != null && opt.Value != null)
		{
			if (opt.DisabledSave)
			{
				await MessageService.Error("正在审核中...无法删除！");
				return;
			}
			list.Add(opt.Value);
		}
		else
		{
			List<NovaAdminItem<TItem>> opts = items.Where((NovaAdminItem<TItem> a) => a.Selected).ToList();
			if (!opts.Any())
			{
				return;
			}
			if (opts.Any((NovaAdminItem<TItem> a) => a.DisabledSave))
			{
				await MessageService.Error("正在审核中...无法删除！");
				return;
			}
			list = opts.Select((NovaAdminItem<TItem> a) => a.Value).ToList();
		}
		if (!list.Any())
		{
			return;
		}
		if (OnRemoving.HasDelegate)
		{
			NovaAdminConfirmEventArgs<List<TItem>> args = new NovaAdminConfirmEventArgs<List<TItem>>
			{
				Argument = list
			};
			await OnRemoving.InvokeAsync(args);
			if (args.Cancel)
			{
				return;
			}
		}
		bool flag = opt == null && IsConfirmRemove;
		bool flag2 = flag;
		if (flag2)
		{
			flag2 = !(await JS.Confirm($"确定要删除 {list.Count}行 记录吗？", "删除之后无法恢复！"));
		}
		if (flag2)
		{
			return;
		}
		if (ItemsSource != null)
		{
			int removeCount = 0;
			foreach (TItem itemToRemove in list)
			{
				object pkValue = GetItemPrimaryValue(itemToRemove);
				TItem itemInSource = ItemsSource.FirstOrDefault((TItem x) => object.Equals(GetItemPrimaryValue(x), pkValue));
				if (itemInSource != null)
				{
					ItemsSource.Remove(itemInSource);
					removeCount++;
				}
			}
			q.Total -= removeCount;
			await Load();
			return;
		}
		if (isEntityAudited)
		{
			EntityAudited[] audits = list.Select((TItem a) => a as EntityAudited).ToArray();
			SysAuditEntityLog[] logs = list.Select(delegate(TItem a)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				AggregateRootUtils.MapEntityValue(boundaryName, ((IBaseRepository)repository).Orm, typeof(TItem), (object)a, (object)dictionary);
				return new SysAuditEntityLog
				{
					TableName = metaTItem.DbName,
					TableId = (a as EntityAudited).Id,
					LogType = "remove",
					DataOld = JsonConvert.SerializeObject((object)dictionary)
				};
			}).ToArray();
			bool succuess = false;
			IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
			try
			{
				int affrows = await uow.Orm.Update<EntityAudited>().AsTable(metaTItem.DbName).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => audits.Any((EntityAudited audit) => a.Id == audit.Id && a.AuditVersion == audit.AuditVersion)))
					.Set<int>((Expression<Func<EntityAudited, int>>)((EntityAudited a) => a.AuditVersion + 1))
					.ExecuteAffrowsAsync(default(CancellationToken));
				if (affrows == list.Count)
				{
					await uow.Orm.Insert<SysAuditEntityLog>(logs).ExecuteAffrowsAsync(default(CancellationToken));
					await ((IBaseRepository<TItem>)(object)repository).DeleteAsync((IEnumerable<TItem>)list, default(CancellationToken));
					await uow.Orm.Delete<SysAuditLog>().Where((Expression<Func<SysAuditLog, bool>>)((SysAuditLog a) => a.TableName == metaTItem.DbName && audits.Select((EntityAudited audit) => audit.Id).Contains(a.TableId))).ExecuteAffrowsAsync(default(CancellationToken));
					succuess = true;
					q.Total -= affrows;
					uow.Commit();
				}
			}
			finally
			{
				((IDisposable)uow)?.Dispose();
			}
			if (!succuess)
			{
				await MessageService.Error("数据发生变化...请刷新后再操作！");
				return;
			}
		}
		else
		{
			await ((IBaseRepository<TItem>)(object)repository).DeleteAsync((IEnumerable<TItem>)list, default(CancellationToken));
			q.Total -= list.Count;
		}
		if (OnRemoved.HasDelegate)
		{
			await OnRemoved.InvokeAsync(list);
		}
		await Load();
		if (!IsNotifyChanged)
		{
			return;
		}
		foreach (TItem item in list)
		{
			admin.TriggerNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnRemove", item);
		}
	}

	private async Task _0024Rougamo_BeginView(NovaAdminItem<TItem> opt)
	{
		item = CloneItem(opt.Value);
		itemModalStatus = 0;
		if (OnEdit.HasDelegate)
		{
			await OnEdit.InvokeAsync(item);
		}
	}

	private async Task _0024Rougamo_BeginAdd()
	{
		item = new TItem();
		itemModalStatus = 1;
		if (OnEdit.HasDelegate)
		{
			await OnEdit.InvokeAsync(item);
		}
	}

	private async Task<bool> _0024Rougamo_EndAdd()
	{
		if (ItemsSource != null)
		{
			ItemsSource.Insert(0, item);
			await Load();
			return true;
		}
		if (OnSaving.HasDelegate)
		{
			NovaAdminConfirmEventArgs<TItem> args = new NovaAdminConfirmEventArgs<TItem>
			{
				Argument = item
			};
			await OnSaving.InvokeAsync(args);
			if (args.Cancel)
			{
				return false;
			}
		}
		if (!isEntityAudited)
		{
			await ((IBaseRepository<TItem>)(object)repository).InsertAsync(item, default(CancellationToken));
		}
		else
		{
			EntityAudited audit = item as EntityAudited;
			Dictionary<string, object> logData = new Dictionary<string, object>();
			AggregateRootUtils.MapEntityValue(boundaryName, ((IBaseRepository)repository).Orm, typeof(TItem), (object)item, (object)logData);
			SysAuditEntityLog log = new SysAuditEntityLog
			{
				TableName = metaTItem.DbName,
				TableId = audit.Id,
				LogType = "add",
				Data = JsonConvert.SerializeObject((object)logData)
			};
			IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
			try
			{
				await uow.Orm.Insert<SysAuditEntityLog>(log).ExecuteAffrowsAsync(default(CancellationToken));
				await ((IBaseRepository<TItem>)(object)repository).InsertAsync(item, default(CancellationToken));
				uow.Commit();
			}
			finally
			{
				((IDisposable)uow)?.Dispose();
			}
		}
		if (IsNotifyChanged)
		{
			admin.TriggerNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnAdd", item);
		}
		return true;
	}

	private async Task _0024Rougamo_BeginEdit(TItem editItem)
	{
		item = CloneItem(editItem);
		if (item != null)
		{
			if (isEntityAudited)
			{
				await admin.LockResource((item as EntityAudited).Id);
			}
			itemModalStatus = 2;
			if (OnEdit.HasDelegate)
			{
				await OnEdit.InvokeAsync(item);
			}
		}
	}

	private async Task<bool> _0024Rougamo_EndEdit()
	{
		if (IsAudit)
		{
			EntityAudited audit = item as EntityAudited;
			if (audit.AuditStatus != SysAuditStatus.待提交 && (audit.AuditStatus != SysAuditStatus.退回 || !(audit.AuditStep == "audit_00")))
			{
				await MessageService.Error("正在审核中...无法修改！");
				return false;
			}
		}
		if (OnSaving.HasDelegate)
		{
			NovaAdminConfirmEventArgs<TItem> args = new NovaAdminConfirmEventArgs<TItem>
			{
				Argument = item
			};
			await OnSaving.InvokeAsync(args);
			if (args.Cancel)
			{
				return false;
			}
		}
		bool isConfirmEdit = IsConfirmEdit;
		bool flag = isConfirmEdit;
		if (flag)
		{
			flag = !(await JS.Confirm("确定要修改数据吗？"));
		}
		if (flag)
		{
			return false;
		}
		if (ItemsSource != null)
		{
			object pkValue = GetItemPrimaryValue(item);
			TItem originalItem = ItemsSource.FirstOrDefault((TItem x) => object.Equals(GetItemPrimaryValue(x), pkValue));
			if (originalItem != null)
			{
				AggregateRootUtils.MapEntityValue(boundaryName, ((IBaseRepository)repository).Orm, typeof(TItem), (object)item, (object)originalItem);
				await Load();
				return true;
			}
			await MessageService.Error("在内存列表中未找到该记录，无法更新！");
			return false;
		}
		if (!isEntityAudited)
		{
			await ((IBaseRepository<TItem>)(object)repository).UpdateAsync(item, default(CancellationToken));
		}
		else
		{
			Dictionary<string, object[]> changelog = ((IBaseRepository<TItem>)(object)repository).CompareState(item);
			if (!changelog.Any())
			{
				return true;
			}
			EntityAudited audit2 = item as EntityAudited;
			IDictionary _states = ((object)repository).GetType().GetPropertyOrFieldValue(repository, "_states") as IDictionary;
			string stateKey = EntityUtilExtensions.GetEntityKeyString(((IBaseRepository)repository).Orm, typeof(TItem), (object)item, false, "*|_,[,_|*");
			if (_states == null || stateKey == null)
			{
				await MessageService.Error("未能获取状态管理...无法修改！");
				return false;
			}
			if (!(_states[stateKey]?.GetType().GetPropertyOrFieldValue(_states[stateKey], "Value") is TItem stateValue))
			{
				await MessageService.Error("状态管理为空...无法修改！");
				return false;
			}
			Dictionary<string, object> logDataOld = new Dictionary<string, object>();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			AggregateRootUtils.MapEntityValue(boundaryName, ((IBaseRepository)repository).Orm, typeof(TItem), (object)stateValue, (object)logDataOld);
			AggregateRootUtils.MapEntityValue(boundaryName, ((IBaseRepository)repository).Orm, typeof(TItem), (object)item, (object)logData);
			logData["AuditVersion"] = audit2.AuditVersion + 1;
			LocalRemoveSameValue(logDataOld, logData);
			SysAuditEntityLog log = new SysAuditEntityLog
			{
				TableName = metaTItem.DbName,
				TableId = audit2.Id,
				LogType = "edit",
				DataOld = JsonConvert.SerializeObject((object)logDataOld, (Formatting)1),
				Data = JsonConvert.SerializeObject((object)logData, (Formatting)1)
			};
			bool succuess = false;
			IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
			try
			{
				if (await uow.Orm.Update<EntityAudited>().AsTable(metaTItem.DbName).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == audit2.Id && a.AuditVersion == audit2.AuditVersion))
					.Set<int>((Expression<Func<EntityAudited, int>>)((EntityAudited a) => a.AuditVersion + 1))
					.ExecuteAffrowsAsync(default(CancellationToken)) == 1)
				{
					await uow.Orm.Insert<SysAuditEntityLog>(log).ExecuteAffrowsAsync(default(CancellationToken));
					await ((IBaseRepository<TItem>)(object)repository).UpdateAsync(item, default(CancellationToken));
					succuess = true;
					uow.Commit();
				}
			}
			finally
			{
				((IDisposable)uow)?.Dispose();
			}
			if (!succuess)
			{
				await MessageService.Error("数据发生变化...请刷新后再操作！");
				return false;
			}
			audit2.AuditVersion += 1;
		}
		if (IsNotifyChanged)
		{
			admin.TriggerNotifyChanged(admin.Tenant.Id + "/" + metaTItem.DbName + "/OnEdit", item);
		}
		return true;
		static void LocalRemoveSameValue(Dictionary<string, object> obj1, Dictionary<string, object> obj2)
		{
			string[] array = obj1.Keys.ToArray();
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (obj2.ContainsKey(key))
				{
					if (obj1[key] is Dictionary<string, object> dictionary && obj2[key] is Dictionary<string, object> dictionary2)
					{
						LocalRemoveSameValue(dictionary, dictionary2);
						if (dictionary.Count == 0 && dictionary2.Count == 0)
						{
							obj1.Remove(key);
							obj2.Remove(key);
						}
					}
					else if (object.Equals(obj1[key], obj2[key]))
					{
						obj1.Remove(key);
						obj2.Remove(key);
					}
				}
			}
		}
	}

	private async Task _0024Rougamo_Save()
	{
		try
		{
			isShowLoaderSaving = true;
			StateHasChanged();
			bool isReturn;
			switch (itemModalStatus)
			{
			case 1:
				isReturn = !(await EndAdd());
				break;
			case 2:
				isReturn = !(await EndEdit());
				break;
			default:
				await MessageService.Success("系统异常！");
				isReturn = true;
				break;
			}
			if (isReturn)
			{
				await Task.Delay(200);
				isShowLoaderSaving = false;
				StateHasChanged();
				return;
			}
			if (OnSaved.HasDelegate)
			{
				await OnSaved.InvokeAsync(item);
			}
			if (itemModalStatus == 2 && isEntityAudited)
			{
				await admin.UnlockResource((item as EntityAudited).Id);
			}
			item = null;
			await Load();
			await MessageService.Success("保存成功！");
		}
		finally
		{
			isShowLoaderSaving = false;
		}
	}

	private async Task _0024Rougamo_ExportExcel()
	{
		StringBuilder excel = new StringBuilder();
		RenderTreeBuilder renderTreeBuilder = new RenderTreeBuilder();
		if (TableTd1 != null)
		{
			if (TableTh1 == null)
			{
				excel.Append(",");
			}
			else
			{
				TableTh1(renderTreeBuilder);
				string text = renderTreeBuilder.GetFrames().Array[0].MarkupContent.Trim();
				appendExcelContent(text);
				renderTreeBuilder.Clear();
			}
		}
		TableHeader(renderTreeBuilder);
		RenderTreeFrame[] frameArray = renderTreeBuilder.GetFrames().Array;
		for (int x = 0; x < frameArray.Length; x++)
		{
			RenderTreeFrame frame = frameArray[x];
			if (frame.FrameType == RenderTreeFrameType.None)
			{
				break;
			}
			if (frame.FrameType == RenderTreeFrameType.Markup && frame.MarkupContent.Contains("<th>"))
			{
				string text2 = frame.MarkupContent.Replace("<th>", "").Replace("</th>", "").Trim();
				appendExcelContent(text2);
			}
			else
			{
				if (frame.FrameType != RenderTreeFrameType.Element || !(frame.AttributeName?.ToLower() == "td"))
				{
					continue;
				}
				string text3 = "";
				for (x++; x < frameArray.Length; x++)
				{
					frame = frameArray[x];
					if (frame.FrameType == RenderTreeFrameType.None)
					{
						break;
					}
					if (frame.FrameType != RenderTreeFrameType.Markup)
					{
						if (frame.FrameType == RenderTreeFrameType.Element && frame.AttributeName?.ToLower() == "td")
						{
							x--;
							break;
						}
						if (frame.FrameType == RenderTreeFrameType.Text)
						{
							text3 += frame.TextContent;
						}
					}
				}
				appendExcelContent(text3);
			}
		}
		if (excel.Length > 0)
		{
			excel.Remove(excel.Length - 1, 1).Append("\n");
		}
		new List<TItem>();
		List<TItem> list;
		if (ItemsSource != null)
		{
			IQueryable<TItem> query = ItemsSource.AsQueryable();
			if (OnQuery.HasDelegate)
			{
				NovaAdminQueryEventArgs<TItem> args = new NovaAdminQueryEventArgs<TItem>(null, q.SearchText?.Trim(), q.Filters, q.Sort)
				{
					Queryable = query
				};
				await OnQuery.InvokeAsync(args);
				query = args.Queryable;
			}
			list = query.ToList();
		}
		else
		{
			ISelect<TItem> select = ((IBaseRepository<TItem>)(object)repository).Select;
			if (OnQuery.HasDelegate)
			{
				await OnQuery.InvokeAsync(new NovaAdminQueryEventArgs<TItem>(select, q.SearchText?.Trim(), q.Filters, q.Sort));
			}
			list = await ((ISelect0<ISelect<TItem>, TItem>)(object)FreeSqlDbContextExtensions.NoTracking<TItem>(select)).ToListAsync(default(CancellationToken));
		}
		list = (from a in list.ToNovaAdminItemList(fsql)
			select a.Value).ToList();
		renderTreeBuilder.Clear();
		foreach (TItem item in list)
		{
			bool isRow = false;
			renderTreeBuilder.Clear();
			if (TableTd1 != null)
			{
				TableTd1(item)(renderTreeBuilder);
				string text4 = renderTreeBuilder.GetFrames().Array[0].MarkupContent.Trim();
				appendExcelContent(text4);
				renderTreeBuilder.Clear();
			}
			TableRow(item)(renderTreeBuilder);
			frameArray = renderTreeBuilder.GetFrames().Array;
			for (int x2 = 0; x2 < frameArray.Length; x2++)
			{
				RenderTreeFrame frame2 = frameArray[x2];
				if (frame2.FrameType == RenderTreeFrameType.None)
				{
					break;
				}
				if (frame2.FrameType != RenderTreeFrameType.Element || !(frame2.AttributeName?.ToLower() == "td"))
				{
					continue;
				}
				string text5 = "";
				for (x2++; x2 < frameArray.Length; x2++)
				{
					frame2 = frameArray[x2];
					if (frame2.FrameType == RenderTreeFrameType.None)
					{
						break;
					}
					if (frame2.FrameType != RenderTreeFrameType.Markup)
					{
						if (frame2.FrameType == RenderTreeFrameType.Element && frame2.AttributeName?.ToLower() == "td")
						{
							x2--;
							break;
						}
						if (frame2.FrameType == RenderTreeFrameType.Text)
						{
							text5 += frame2.TextContent;
						}
					}
				}
				appendExcelContent(text5);
				isRow = true;
			}
			if (isRow)
			{
				excel.Remove(excel.Length - 1, 1).Append("\n");
			}
		}
		using (MemoryStream ms = new MemoryStream())
		{
			StreamWriter writer = new StreamWriter(ms, Encoding.UTF8);
			await writer.WriteAsync(excel.ToString());
			await writer.FlushAsync();
			ms.Position = 0L;
			await DownloadServiceExtensions.DownloadFromStreamAsync(downloadService, Title + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xls", (Stream)ms);
			ms.Close();
		}
		excel.Clear();
		void appendExcelContent(string text6)
		{
			if (text6.Contains(","))
			{
				excel.Append("\"").Append(text6.Trim().Replace("\n", " ").Replace("\"", "\\\"")).Append("\"")
					.Append(",");
			}
			else
			{
				excel.Append(text6.Trim().Replace("\n", " ")).Append(",");
			}
		}
	}
}

internal static class NovaAdminTableTypeInference
{
	public static void CreateCascadingValue_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, RenderFragment __arg1)
	{
		__builder.OpenComponent<CascadingValue<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "Value", __arg0);
		__builder.AddComponentParameter(__seq1, "ChildContent", __arg1);
		__builder.CloseComponent();
	}

	public static void CreateCascadingValue_1<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, bool __arg1, int __seq2, RenderFragment __arg2)
	{
		__builder.OpenComponent<CascadingValue<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "Value", __arg0);
		__builder.AddComponentParameter(__seq1, "IsFixed", __arg1);
		__builder.AddComponentParameter(__seq2, "ChildContent", __arg2);
		__builder.CloseComponent();
	}
}
