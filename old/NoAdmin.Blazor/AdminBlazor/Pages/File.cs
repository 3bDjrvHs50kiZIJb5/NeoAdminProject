using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Services;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/File")]
public class File : ComponentBase
{
	[Inject]
	private IAggregateRootRepository<SysFile> repo { get; set; }

	[Inject]
	private FileService fileService { get; set; }

	private NovaAdminTable<SysFile> adminTable2 { get; set; }

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
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "文件管理");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysFile>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(30));
		__builder.AddComponentParameter(6, "Title", "文件");
		__builder.AddComponentParameter(7, "DialogClassName", "modal-xl");
		__builder.AddComponentParameter(8, "SearchPlaceholder", "文件名..");
		__builder.AddComponentParameter(9, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysFile>>)OnQuery)));
		__builder.AddComponentParameter(10, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(11, "OnRemoving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysFile>>, Task>)OnRemoving)));
		__builder.AddComponentParameter(12, "OnSaving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<SysFile>, Task>)OnSaving)));
		__builder.AddComponentParameter(13, "SaveText", "上传至服务器");
		__builder.AddComponentParameter(14, "AddText", "上传");
		__builder.AddComponentParameter(15, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(16, "TableTd99Width", RuntimeHelpers.TypeCheck(40));
		__builder.AddComponentParameter(17, "FixedLeftColumns", RuntimeHelpers.TypeCheck(3));
		__builder.AddComponentParameter(18, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddAttribute(19, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(20, "<th>文件名</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(21, "<th>文件扩展名</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(22, "<th>文件大小格式化</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(23, "<th>创建者</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(24, "<th>创建时间</th>");
		});
		__builder.AddAttribute(25, "TableRow", (RenderFragment<SysFile>)((SysFile item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(26, "td");
			renderTreeBuilder.OpenElement(27, "a");
			renderTreeBuilder.AddAttribute(28, "href", item.LinkUrl);
			renderTreeBuilder.AddAttribute(29, "target", "_blank");
			renderTreeBuilder.AddContent(30, item.OriginFileName);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(31, "\r\n        ");
			renderTreeBuilder.OpenElement(32, "td");
			renderTreeBuilder.AddContent(33, item.Extension);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(34, "\r\n        ");
			renderTreeBuilder.OpenElement(35, "td");
			renderTreeBuilder.AddContent(36, item.SizeFormat);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(37, "\r\n        ");
			renderTreeBuilder.OpenElement(38, "td");
			renderTreeBuilder.AddContent(39, item.CreatedUserName);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(40, "\r\n        ");
			renderTreeBuilder.OpenElement(41, "td");
			renderTreeBuilder.AddContent(42, item.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(43, "EditTemplate", (RenderFragment<SysFile>)((SysFile item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(44, "div");
			renderTreeBuilder.AddAttribute(45, "style", "padding:12px;");
			renderTreeBuilder.AddMarkupContent(46, "<p>选择一个或多个文件后，再点保存。</p>\r\n            ");
			renderTreeBuilder.OpenComponent<CardUpload<string>>(47);
			renderTreeBuilder.AddComponentParameter(48, "IsMultiple", true);
			renderTreeBuilder.AddComponentParameter(49, "ShowProgress", RuntimeHelpers.TypeCheck(value: true));
			renderTreeBuilder.AddComponentParameter(50, "OnChange", new Func<UploadFile, Task>(OnCardUpload));
			renderTreeBuilder.AddComponentParameter(51, "OnDelete", (Func<UploadFile, Task<bool>>)((UploadFile fileName) => Task.FromResult(result: true)));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddComponentReferenceCapture(52, delegate(object __value)
		{
			adminTable2 = (NovaAdminTable<SysFile>)__value;
		});
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysFile> e)
	{
		e.Select.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysFile, bool>>)((SysFile a) => a.OriginFileName.Contains(e.SearchText) || a.Extension.Contains(e.SearchText) || a.CreatedUserName.Contains(e.SearchText))).OrderByDescending<DateTime?>((Expression<Func<SysFile, DateTime?>>)((SysFile x) => x.CreatedTime));
	}

	private async Task OnCardUpload(UploadFile file)
	{
		if (file != null && file.File != null)
		{
			if (UploadFileExtensions.IsImage(file, (List<string>)null, (Func<UploadFile, bool>)null))
			{
				await UploadFileExtensions.RequestBase64ImageFileAsync(file, "png", 200, 200, (long?)null, (List<string>)null, default(CancellationToken));
			}
			uploadFiles.Add(file);
		}
	}

	private async Task OnSaving(NovaAdminConfirmEventArgs<SysFile> e)
	{
		e.Cancel = true;
		int total = 0;
		foreach (UploadFile file in uploadFiles)
		{
			SysFile result = await fileService.UploadFileAsync(file, "", isRename: true, 0L);
			if (result != null)
			{
				file.PrevUrl = result.LinkUrl;
				total++;
			}
		}
		await ToastServiceExtensions.Success(ToastService, "上传文件", $"已成功上传{total}个文件。", true);
		adminTable2.item = null;
		await adminTable2.Load();
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysFile>> e)
	{
		if (!(await JS.Confirm($"确定要删除 {e.Argument.Count}行 记录吗？", "删除之后无法恢复！")))
		{
			return;
		}
		foreach (SysFile item in e.Argument)
		{
			await fileService.DeleteAsync(item.Id);
		}
		e.Cancel = true;
		await adminTable2.Load();
	}
}
