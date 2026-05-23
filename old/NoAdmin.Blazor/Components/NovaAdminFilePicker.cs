using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Services;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using __Blazor.NoAdmin.Blazor.Components.NovaAdminFilePicker;

namespace NoAdmin.Blazor.Components;

public class NovaAdminFilePicker : ComponentBase
{
	private ListView<SysFile> child;

	private ButtonUpload<string> buttonUpload;

	private List<TreeViewItem<string>> groups = new List<TreeViewItem<string>>();

	private TreeViewItem<string> selectTree = null;

	private bool fileReName;

	[Inject]
	private IAggregateRootRepository<SysFile> repo { get; set; }

	[Inject]
	private FileService fileService { get; set; }

	[Parameter]
	public EventCallback<SysFile> OnItemClick { get; set; }

	private List<UploadFile> uploadFiles { get; set; } = new List<UploadFile>();

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
		__builder.OpenComponent<Split>(0);
		__builder.AddComponentParameter(1, "ShowBarHandle", RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(2, "Basis", "22%");
		__builder.AddComponentParameter(3, "FirstPaneMinimumSize", "230px");
		__builder.AddAttribute(4, "FirstPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(5, "h5");
			renderTreeBuilder.AddMarkupContent(6, "分组 ");
			renderTreeBuilder.OpenElement(7, "a");
			renderTreeBuilder.AddAttribute(8, "href", "javascript:;");
			renderTreeBuilder.AddAttribute(9, "style", "font-size:14px;");
			renderTreeBuilder.AddAttribute(10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OpenGroupInput()));
			renderTreeBuilder.AddMarkupContent(11, "添加分组");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(12, "\r\n        ");
			TypeInference.CreateTreeView_0_CaptureParameters<string>(groups, out var __arg0_out, OnTreeItemClick, out var __arg1_out);
			TypeInference.CreateTreeView_0(renderTreeBuilder, 13, 14, __arg0_out, 15, __arg1_out);
			__arg0_out = null;
			__arg1_out = null;
		});
		__builder.AddAttribute(16, "SecondPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<ListView<SysFile>>(17);
			renderTreeBuilder.AddComponentParameter(18, "IsPagination", RuntimeHelpers.TypeCheck(value: true));
			renderTreeBuilder.AddComponentParameter(19, "PageItems", RuntimeHelpers.TypeCheck(15));
			renderTreeBuilder.AddComponentParameter(20, "OnQueryAsync", new Func<QueryPageOptions, Task<QueryData<SysFile>>>(OnQueryAsync));
			renderTreeBuilder.AddComponentParameter(21, "style", "height:550px;");
			renderTreeBuilder.AddAttribute(22, "HeaderTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				//IL_0195: Unknown result type (might be due to invalid IL or missing references)
				//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
				renderTreeBuilder2.OpenElement(23, "div");
				renderTreeBuilder2.AddAttribute(24, "class", "row");
				renderTreeBuilder2.OpenElement(25, "div");
				renderTreeBuilder2.AddAttribute(26, "class", "col-4");
				renderTreeBuilder2.OpenComponent<ButtonUpload<string>>(27);
				renderTreeBuilder2.AddComponentParameter(28, "IsMultiple", RuntimeHelpers.TypeCheck(value: true));
				renderTreeBuilder2.AddComponentParameter(29, "ShowProgress", RuntimeHelpers.TypeCheck(value: true));
				renderTreeBuilder2.AddComponentParameter(30, "ShowUploadFileList", RuntimeHelpers.TypeCheck(value: true));
				renderTreeBuilder2.AddComponentParameter(31, "OnAllFileUploaded", new Func<IReadOnlyCollection<UploadFile>, Task>(OnAllFileUploaded));
				renderTreeBuilder2.AddComponentParameter(32, "OnChange", new Func<UploadFile, Task>(OnClickToUpload));
				renderTreeBuilder2.AddComponentParameter(33, "OnDelete", (Func<UploadFile, Task<bool>>)((UploadFile fileName) => Task.FromResult(result: true)));
				renderTreeBuilder2.AddComponentParameter(34, "BrowserButtonText", "点击上传");
				renderTreeBuilder2.AddComponentReferenceCapture(35, delegate(object __value)
				{
					buttonUpload = (ButtonUpload<string>)__value;
				});
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(36, "\r\n                    ");
				renderTreeBuilder2.OpenElement(37, "div");
				renderTreeBuilder2.AddAttribute(38, "class", "col-4");
				renderTreeBuilder2.OpenComponent<Switch>(39);
				renderTreeBuilder2.AddComponentParameter(40, "OnText", "重命名文件");
				renderTreeBuilder2.AddComponentParameter(41, "OffText", "保持原文件名");
				renderTreeBuilder2.AddComponentParameter(42, "OnColor", RuntimeHelpers.TypeCheck<Color>((Color)4));
				renderTreeBuilder2.AddComponentParameter(43, "Value", RuntimeHelpers.TypeCheck(fileReName));
				renderTreeBuilder2.AddComponentParameter(44, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
				{
					fileReName = __value;
				}, fileReName))));
				renderTreeBuilder2.AddComponentParameter(45, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<bool>>>(() => fileReName));
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				if (child.Items.Where((SysFile x) => x.IsSelect).Count() > 0)
				{
					renderTreeBuilder2.OpenElement(46, "div");
					renderTreeBuilder2.AddAttribute(47, "class", "col-4");
					renderTreeBuilder2.OpenComponent<Button>(48);
					renderTreeBuilder2.AddComponentParameter(49, "ButtonType", RuntimeHelpers.TypeCheck<ButtonType>((ButtonType)0));
					renderTreeBuilder2.AddComponentParameter(50, "Color", RuntimeHelpers.TypeCheck<Color>((Color)5));
					renderTreeBuilder2.AddComponentParameter(51, "Icon", "fa-fw fa-solid fa-trash-alt");
					renderTreeBuilder2.AddComponentParameter(52, "Text", "删除所选文件");
					renderTreeBuilder2.AddComponentParameter(53, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)async delegate
					{
						await OnDeleteFiles();
					}));
					renderTreeBuilder2.CloseComponent();
					renderTreeBuilder2.CloseElement();
				}
				renderTreeBuilder2.CloseElement();
			});
			renderTreeBuilder.AddAttribute(54, "BodyTemplate", (RenderFragment<SysFile>)((SysFile context) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenComponent<Card>(55);
				renderTreeBuilder2.AddAttribute(56, "BodyTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
				{
					renderTreeBuilder3.OpenElement(57, "img");
					renderTreeBuilder3.AddAttribute(58, "src", context.LinkUrl);
					renderTreeBuilder3.AddAttribute(59, "height", "120");
					renderTreeBuilder3.AddAttribute(60, "width", "180");
					renderTreeBuilder3.AddAttribute(61, "title", "点击选择");
					renderTreeBuilder3.AddAttribute(62, "alt", context.OriginFileName);
					renderTreeBuilder3.AddAttribute(63, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnListViewItemClick(context)));
					renderTreeBuilder3.CloseElement();
					renderTreeBuilder3.AddMarkupContent(64, "\r\n                        ");
					renderTreeBuilder3.OpenElement(65, "p");
					renderTreeBuilder3.AddContent(66, context.OriginFileName);
					renderTreeBuilder3.CloseElement();
					renderTreeBuilder3.AddMarkupContent(67, "\r\n                        ");
					renderTreeBuilder3.OpenElement(68, "span");
					renderTreeBuilder3.AddAttribute(69, "style", "position:absolute;right:10px; bottom:10px;");
					renderTreeBuilder3.OpenComponent<Checkbox<bool>>(70);
					renderTreeBuilder3.AddComponentParameter(71, "Value", RuntimeHelpers.TypeCheck(context.IsSelect));
					renderTreeBuilder3.AddComponentParameter(72, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
					{
						context.IsSelect = __value;
					}, context.IsSelect))));
					renderTreeBuilder3.AddComponentParameter(73, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<bool>>>(() => context.IsSelect));
					renderTreeBuilder3.CloseComponent();
					renderTreeBuilder3.CloseElement();
				});
				renderTreeBuilder2.CloseComponent();
			}));
			renderTreeBuilder.AddComponentReferenceCapture(74, delegate(object __value)
			{
				child = (ListView<SysFile>)__value;
			});
			renderTreeBuilder.CloseComponent();
		});
		__builder.CloseComponent();
	}

	protected override void OnInitialized()
	{
		groups = fileService.GetGroups();
	}

	private async Task OnDeleteFiles()
	{
		IEnumerable<SysFile> selectIdList = child.Items.Where((SysFile x) => x.IsSelect);
		if (!(await JS.Confirm($"确定要删除选择的{selectIdList.Count()}个文件吗？", "删除之后无法恢复！")))
		{
			return;
		}
		foreach (SysFile item in selectIdList)
		{
			await fileService.DeleteAsync(item.Id);
		}
		await child.QueryAsync(1, false);
	}

	private async Task<QueryData<SysFile>> OnQueryAsync(QueryPageOptions options)
	{
		ISelect<SysFile> select = ((ISelect0<ISelect<SysFile>, SysFile>)(object)((IBaseRepository<SysFile>)(object)repo).Select.WhereIf(selectTree != null && ((TreeNodeBase<string>)(object)selectTree).Text != "所有文件", (Expression<Func<SysFile, bool>>)((SysFile x) => x.LinkUrl.Contains(((TreeNodeBase<string>)(object)selectTree).Text))).OrderByDescending<DateTime?>((Expression<Func<SysFile, DateTime?>>)((SysFile x) => x.CreatedTime))).Page(options.PageIndex, options.PageItems);
		QueryData<SysFile> val = new QueryData<SysFile>();
		QueryData<SysFile> val2 = val;
		val2.Items = await ((ISelect0<ISelect<SysFile>, SysFile>)(object)select).ToListAsync(default(CancellationToken));
		QueryData<SysFile> val3 = val;
		val3.TotalCount = Convert.ToInt32(await ((ISelect0<ISelect<SysFile>, SysFile>)(object)select).CountAsync(default(CancellationToken)));
		return val;
	}

	private async Task OnTreeItemClick(TreeViewItem<string> item)
	{
		selectTree = item;
		await child.QueryAsync(1, false);
	}

	private async Task OnListViewItemClick(SysFile item)
	{
		if (OnItemClick.HasDelegate)
		{
			await OnItemClick.InvokeAsync(item);
		}
	}

	private async Task OnClickToUpload(UploadFile file)
	{
		if (file != null && file.File != null)
		{
			SysFile result = await fileService.UploadFileAsync(file, ((TreeNodeBase<string>)(object)selectTree)?.Text, fileReName, 0L);
			if (result != null)
			{
				file.PrevUrl = result.LinkUrl;
			}
			uploadFiles.Add(file);
		}
	}

	private async Task OnAllFileUploaded(IReadOnlyCollection<UploadFile> files)
	{
		await child.QueryAsync(1, false);
		((IUpload)buttonUpload).UploadFiles.Clear();
	}

	private async Task OpenGroupInput()
	{
		string str = await JSRuntimeExtensions.InvokeAsync<string>(JS, "prompt", new object[1] { "请输入分组名称：" });
		if (!string.IsNullOrEmpty(str))
		{
			fileService.AddGroup(str);
			groups = fileService.GetGroups();
		}
	}
}
