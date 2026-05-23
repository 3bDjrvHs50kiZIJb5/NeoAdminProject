using System;
using System.Collections.Generic;
using System.Linq;
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
using __Blazor.NoAdmin.Blazor.Pages.Tenant;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Tenant")]
public class Tenant : ComponentBase
{
	private SysTenant editItem;

	private string editItemId;

	private string editItemHost;

	private UploadFile uploadFileLogo1 = null;

	private List<UploadFile> defaultFiles1 = new List<UploadFile>();

	private UploadFile uploadFileLoginImage1 = null;

	private List<UploadFile> defaultFiles2 = new List<UploadFile>();

	private SysTenant modifyPasswordItem;

	private string modifyUsername;

	private string modifyPassword;

	[Inject]
	private NovaAdminContext admin { get; set; }

	[Inject]
	private IAggregateRootRepository<SysTenant> repo { get; set; }

	[Inject]
	private FileService fileService { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "租户");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysTenant>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(30));
		__builder.AddComponentParameter(6, "Title", "租户");
		__builder.AddComponentParameter(7, "DialogClassName", "modal-xl");
		__builder.AddComponentParameter(8, "TableTd99Width", RuntimeHelpers.TypeCheck(178));
		__builder.AddComponentParameter(9, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(10, "SearchPlaceholder", "名称/域名/标题..");
		__builder.AddComponentParameter(11, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysTenant>>)OnQuery)));
		__builder.AddComponentParameter(12, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(13, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysTenant, Task>)OnEdit)));
		__builder.AddComponentParameter(14, "OnSaving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<SysTenant>, Task>)OnSaving)));
		__builder.AddComponentParameter(15, "OnSaved", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysTenant, Task>)OnSaved)));
		__builder.AddComponentParameter(16, "OnRemoving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysTenant>>, Task>)OnRemoving)));
		__builder.AddAttribute(17, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(18, "<th width=\"200\">标题</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(19, "<th>域名</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(20, "<th>名称</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(21, "<th>数据库(业务)</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(22, "<th>是否启用</th>\r\n        ");
			renderTreeBuilder.OpenElement(23, "th");
			renderTreeBuilder.AddAttribute(24, "width", "165");
			renderTreeBuilder.OpenComponent<NovaAdminSort>(25);
			renderTreeBuilder.AddComponentParameter(26, "Text", "创建时间");
			renderTreeBuilder.AddComponentParameter(27, "Value", "CreatedTime");
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
		});
		__builder.AddAttribute(28, "TableRow", (RenderFragment<SysTenant>)((SysTenant item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(29, "td");
			renderTreeBuilder.OpenElement(30, "img");
			renderTreeBuilder.AddAttribute(31, "src", item.Logo);
			renderTreeBuilder.AddAttribute(32, "style", "width:40px;height:40px;border-radius:49px;");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddContent(33, " ");
			renderTreeBuilder.AddContent(34, item.Title);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(35, "\r\n        ");
			renderTreeBuilder.OpenElement(36, "td");
			renderTreeBuilder.AddContent(37, string.Join(", ", new string[3] { item.Host, item.Host2, item.Host3 }.Where((string a) => !a.IsNull())));
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(38, "\r\n        ");
			renderTreeBuilder.OpenElement(39, "td");
			renderTreeBuilder.AddContent(40, item.Id);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(41, "\r\n        ");
			renderTreeBuilder.OpenElement(42, "td");
			if (item.Id == "main")
			{
				renderTreeBuilder.AddMarkupContent(43, "<i class=\"text-secondary\">&lt;在代码&gt;</i>");
			}
			else
			{
				renderTreeBuilder.AddContent(44, item.Database?.Label);
			}
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(45, "\r\n        ");
			renderTreeBuilder.OpenElement(46, "td");
			renderTreeBuilder.AddContent(47, item.IsEnabled ? "是" : "否");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(48, "\r\n        ");
			renderTreeBuilder.OpenElement(49, "td");
			renderTreeBuilder.AddContent(50, item?.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(51, "TableTd99", (RenderFragment<SysTenant>)((SysTenant item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(52, "button");
			renderTreeBuilder.AddAttribute(53, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginModifyPassword(item)));
			renderTreeBuilder.AddAttribute(54, "type", "button");
			renderTreeBuilder.AddAttribute(55, "class", "mr-2 btn btn-light btn-xs");
			renderTreeBuilder.AddMarkupContent(56, "<i class=\"fas fa-rotate-right\"></i> 修改密码");
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(57, "EditTemplate", (RenderFragment<SysTenant>)((SysTenant item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			renderTreeBuilder.OpenElement(58, "div");
			renderTreeBuilder.AddAttribute(59, "class", "row");
			renderTreeBuilder.OpenElement(60, "div");
			renderTreeBuilder.AddAttribute(61, "class", "form-group col-5");
			renderTreeBuilder.AddMarkupContent(62, "<label class=\"form-label\">标题</label>\r\n                ");
			renderTreeBuilder.OpenElement(63, "input");
			renderTreeBuilder.AddAttribute(64, "type", "text");
			renderTreeBuilder.AddAttribute(65, "class", "form-control");
			renderTreeBuilder.AddAttribute(66, "placeholder", "");
			renderTreeBuilder.AddAttribute(67, "maxlength", "255");
			renderTreeBuilder.AddAttribute(68, "value", BindConverter.FormatValue(item.Title));
			renderTreeBuilder.AddAttribute(69, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Title = __value;
			}, item.Title));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(70, "\r\n            ");
			renderTreeBuilder.OpenElement(71, "div");
			renderTreeBuilder.AddAttribute(72, "class", "form-group col-5");
			renderTreeBuilder.AddMarkupContent(73, "<label class=\"form-label\">域名</label>\r\n                ");
			renderTreeBuilder.OpenElement(74, "input");
			renderTreeBuilder.AddAttribute(75, "type", "text");
			renderTreeBuilder.AddAttribute(76, "class", "form-control");
			renderTreeBuilder.AddAttribute(77, "placeholder", "");
			renderTreeBuilder.AddAttribute(78, "maxlength", "50");
			renderTreeBuilder.AddAttribute(79, "value", BindConverter.FormatValue(editItemHost));
			renderTreeBuilder.AddAttribute(80, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				editItemHost = __value;
			}, editItemHost));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(81, "\r\n            ");
			renderTreeBuilder.OpenElement(82, "div");
			renderTreeBuilder.AddAttribute(83, "class", "form-group col-2");
			renderTreeBuilder.AddMarkupContent(84, "<label class=\"form-label\">是否启用</label>\r\n                ");
			renderTreeBuilder.OpenComponent<Switch>(85);
			renderTreeBuilder.AddComponentParameter(86, "OnColor", RuntimeHelpers.TypeCheck<Color>((Color)4));
			renderTreeBuilder.AddComponentParameter(87, "IsDisabled", RuntimeHelpers.TypeCheck(item.Id == "main"));
			renderTreeBuilder.AddComponentParameter(88, "Value", RuntimeHelpers.TypeCheck(item.IsEnabled));
			renderTreeBuilder.AddComponentParameter(89, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
			{
				item.IsEnabled = __value;
			}, item.IsEnabled))));
			renderTreeBuilder.AddComponentParameter(90, "ValueExpression", RuntimeHelpers.TypeCheck<Expression<Func<bool>>>(() => item.IsEnabled));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(91, "\r\n            ");
			renderTreeBuilder.OpenElement(92, "div");
			renderTreeBuilder.AddAttribute(93, "class", "form-group col-5");
			renderTreeBuilder.AddMarkupContent(94, "<label class=\"form-label\">名称(唯一)</label>\r\n                ");
			renderTreeBuilder.OpenElement(95, "input");
			renderTreeBuilder.AddAttribute(96, "type", "text");
			renderTreeBuilder.AddAttribute(97, "class", "form-control");
			renderTreeBuilder.AddAttribute(98, "placeholder", "");
			renderTreeBuilder.AddAttribute(99, "maxlength", "50");
			renderTreeBuilder.AddAttribute(100, "disabled", editItemId != null);
			renderTreeBuilder.AddAttribute(101, "value", BindConverter.FormatValue(item.Id));
			renderTreeBuilder.AddAttribute(102, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Id = __value;
			}, item.Id));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(103, "\r\n            ");
			renderTreeBuilder.OpenElement(104, "div");
			renderTreeBuilder.AddAttribute(105, "class", "form-group col-5");
			if (item.Id == "main")
			{
				renderTreeBuilder.AddMarkupContent(106, "<label class=\"form-label\">数据库(定时任务)</label>\r\n                    ");
				renderTreeBuilder.OpenComponent<NovaSelectEntity<SysTenantDatabase, long>>(107);
				renderTreeBuilder.AddComponentParameter(108, "DisplayText", (Func<SysTenantDatabase, string>)((SysTenantDatabase e) => ((object)e.DataType/*cast due to .constrained prefix*/).ToString() + "|" + e.Label));
				renderTreeBuilder.AddComponentParameter(109, "Value", RuntimeHelpers.TypeCheck(item.TaskDatabaseId));
				renderTreeBuilder.AddComponentParameter(110, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(long __value)
				{
					item.TaskDatabaseId = __value;
				}, item.TaskDatabaseId))));
				renderTreeBuilder.CloseComponent();
			}
			else
			{
				renderTreeBuilder.AddMarkupContent(111, "<label class=\"form-label\">数据库(业务)</label>\r\n                    ");
				renderTreeBuilder.OpenComponent<NovaSelectEntity<SysTenantDatabase, long>>(112);
				renderTreeBuilder.AddComponentParameter(113, "DisplayText", (Func<SysTenantDatabase, string>)((SysTenantDatabase e) => ((object)e.DataType/*cast due to .constrained prefix*/).ToString() + "|" + e.Label));
				renderTreeBuilder.AddComponentParameter(114, "Value", RuntimeHelpers.TypeCheck(item.DatabaseId));
				renderTreeBuilder.AddComponentParameter(115, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(long __value)
				{
					item.DatabaseId = __value;
				}, item.DatabaseId))));
				renderTreeBuilder.CloseComponent();
			}
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(116, "\r\n            <div class=\"form-group col-2\"></div>\r\n            ");
			renderTreeBuilder.OpenElement(117, "div");
			renderTreeBuilder.AddAttribute(118, "class", "form-group col-5");
			renderTreeBuilder.AddMarkupContent(119, "<label class=\"form-label\">说明</label>\r\n                ");
			renderTreeBuilder.OpenElement(120, "textarea");
			renderTreeBuilder.AddAttribute(121, "class", "form-control");
			renderTreeBuilder.AddAttribute(122, "placeholder", "");
			renderTreeBuilder.AddAttribute(123, "maxlength", "500");
			renderTreeBuilder.AddAttribute(124, "style", "height:80px;");
			renderTreeBuilder.AddAttribute(125, "value", BindConverter.FormatValue(item.Description));
			renderTreeBuilder.AddAttribute(126, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Description = __value;
			}, item.Description));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(127, "\r\n            ");
			renderTreeBuilder.OpenElement(128, "div");
			renderTreeBuilder.AddAttribute(129, "class", "form-group col-auto");
			renderTreeBuilder.AddMarkupContent(130, "<label class=\"form-label\">登陆图片</label>\r\n                ");
			TypeInference.CreateAvatarUpload_0(renderTreeBuilder, 131, 132, defaultFiles2, 133, OnCardUpload2, 134, 80, 135, 80, 136, __arg4: true, 137, "3px", 138, "image/*", 139, item.LoginImage, 140, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(string __value)
			{
				item.LoginImage = __value;
			}, item.LoginImage)), 141, () => item.LoginImage);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(142, "\r\n            ");
			renderTreeBuilder.OpenElement(143, "div");
			renderTreeBuilder.AddAttribute(144, "class", "form-group col-auto");
			renderTreeBuilder.AddMarkupContent(145, "<label class=\"form-label\">LOGO</label>\r\n                ");
			TypeInference.CreateAvatarUpload_1(renderTreeBuilder, 146, 147, defaultFiles1, 148, OnCardUpload1, 149, 80, 150, 80, 151, __arg4: true, 152, "49px", 153, "image/*", 154, item.Logo, 155, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(string __value)
			{
				item.Logo = __value;
			}, item.Logo)), 156, () => item.Logo);
			renderTreeBuilder.CloseElement();
			if (item.Id != "main")
			{
				renderTreeBuilder.OpenElement(157, "div");
				renderTreeBuilder.AddAttribute(158, "class", "form-group col-12");
				renderTreeBuilder.AddMarkupContent(159, "<label class=\"form-label\">菜单</label>\r\n                    ");
				renderTreeBuilder.OpenComponent<NovaSelectTable<SysMenu, long>>(160);
				renderTreeBuilder.AddComponentParameter(161, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.AddComponentParameter(162, "PageSize", RuntimeHelpers.TypeCheck(-1));
				renderTreeBuilder.AddComponentParameter(163, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminQueryEventArgs<SysMenu>, Task>)OnEditMenuOnQuery)));
				renderTreeBuilder.AddComponentParameter(164, "Items", RuntimeHelpers.TypeCheck(item.Menus));
				renderTreeBuilder.AddComponentParameter(165, "ItemsChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(List<SysMenu> __value)
				{
					item.Menus = __value;
				}, item.Menus))));
				renderTreeBuilder.AddAttribute(166, "ChildContent", (RenderFragment<SysMenu>)((SysMenu context) => delegate(RenderTreeBuilder renderTreeBuilder2)
				{
					renderTreeBuilder2.AddContent(167, context.Label);
				}));
				renderTreeBuilder.CloseComponent();
				renderTreeBuilder.CloseElement();
			}
			renderTreeBuilder.CloseElement();
		}));
		__builder.CloseComponent();
		__builder.AddMarkupContent(168, "\r\n\r\n");
		__builder.OpenComponent<NovaModal>(169);
		__builder.AddComponentParameter(170, "Visible", RuntimeHelpers.TypeCheck(modifyPasswordItem != null));
		__builder.AddComponentParameter(171, "Title", RuntimeHelpers.TypeCheck("【修改密码】" + modifyPasswordItem?.Title));
		__builder.AddComponentParameter(172, "OnYes", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)ModifyPasswordFinish)));
		__builder.AddComponentParameter(173, "DialogClassName", "modal-sm");
		__builder.AddComponentParameter(174, "IsBackdropStatic", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(175, "OnClose", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			modifyPasswordItem = null;
			modifyPassword = null;
		})));
		__builder.AddAttribute(176, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(177, "div");
			renderTreeBuilder.AddAttribute(178, "class", "row");
			renderTreeBuilder.OpenElement(179, "div");
			renderTreeBuilder.AddAttribute(180, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(181, "<label class=\"form-label\">账号</label>\r\n            ");
			renderTreeBuilder.OpenElement(182, "input");
			renderTreeBuilder.AddAttribute(183, "type", "text");
			renderTreeBuilder.AddAttribute(184, "class", "form-control");
			renderTreeBuilder.AddAttribute(185, "placeholder", "");
			renderTreeBuilder.AddAttribute(186, "maxlength", "50");
			renderTreeBuilder.AddAttribute(187, "value", BindConverter.FormatValue(modifyUsername));
			renderTreeBuilder.AddAttribute(188, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				modifyUsername = __value;
			}, modifyUsername));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(189, "\r\n        ");
			renderTreeBuilder.OpenElement(190, "div");
			renderTreeBuilder.AddAttribute(191, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(192, "<label class=\"form-label\">密码</label>\r\n            ");
			renderTreeBuilder.OpenElement(193, "input");
			renderTreeBuilder.AddAttribute(194, "type", "text");
			renderTreeBuilder.AddAttribute(195, "class", "form-control");
			renderTreeBuilder.AddAttribute(196, "placeholder", "");
			renderTreeBuilder.AddAttribute(197, "maxlength", "50");
			renderTreeBuilder.AddAttribute(198, "value", BindConverter.FormatValue(modifyPassword));
			renderTreeBuilder.AddAttribute(199, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				modifyPassword = __value;
			}, modifyPassword));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		});
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		List<SysTenantDatabase> allTenantDatabases = await ((ISelect0<ISelect<SysTenantDatabase>, SysTenantDatabase>)(object)((IBaseRepository)repo).Orm.Select<SysTenantDatabase>()).ToListAsync(default(CancellationToken));
		e.Filters = new NovaAdminFilterInfo[1]
		{
			new NovaAdminFilterInfo("数据库", "DatabaseId", multiple: true, 12, string.Join(",", allTenantDatabases.Select((SysTenantDatabase a) => $"[{a.DataType}]{a.Label}")), string.Join(",", allTenantDatabases.Select((SysTenantDatabase a) => a.Id)))
		};
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysTenant> e)
	{
		ISelect<SysTenant> obj = e.Select.Include<SysTenantDatabase>((Expression<Func<SysTenant, SysTenantDatabase>>)((SysTenant a) => a.Database)).WhereIf(e.Filters[0].HasValue, (Expression<Func<SysTenant, bool>>)((SysTenant a) => e.Filters[0].Values<long>().Contains(a.DatabaseId))).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysTenant, bool>>)((SysTenant a) => a.Id.Contains(e.SearchText) || a.Host.Contains(e.SearchText) || a.Title.Contains(e.SearchText)));
		bool num = !e.Sort.IsNull();
		string obj2 = e.Sort?.Replace("@desc", "");
		string sort = e.Sort;
		((ISelect0<ISelect<SysTenant>, SysTenant>)(object)obj).OrderByPropertyNameIf(num, obj2, sort == null || !sort.Contains("@desc"));
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysTenant>> e)
	{
		if (e.Argument.Any((SysTenant a) => a.Id == "main"))
		{
			await MessageService.Error("不能删除 main 租户记录!");
			e.Cancel = true;
		}
	}

	private async Task OnEditMenuOnQuery(NovaAdminQueryEventArgs<SysMenu> e)
	{
		IEnumerable<long> menuIds = (await admin.GenerateTenantMenus(editItem.Id, isAdministrator: true)).Select((SysMenu a) => a.Id);
		e.Select.Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => menuIds.Contains(a.Id))).OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort));
	}

	private async Task OnEdit(SysTenant item)
	{
		editItem = item;
		editItemId = item.Id;
		editItemHost = string.Join(", ", new string[3] { item.Host, item.Host2, item.Host3 }.Where((string a) => !a.IsNull()));
		if (item.Menus == null)
		{
			if (item.Id != null)
			{
				await FreeSqlGlobalExtensions.IncludeManyAsync<SysTenant, SysMenu>(new List<SysTenant> { item }, ((IBaseRepository)repo).Orm, (Expression<Func<SysTenant, IEnumerable<SysMenu>>>)((SysTenant a) => a.Menus), (Action<ISelect<SysMenu>>)null, default(CancellationToken));
			}
			else
			{
				item.Menus = new List<SysMenu>();
			}
			((IBaseRepository<SysTenant>)(object)repo).Attach(item);
		}
		if (item.Id != null)
		{
			defaultFiles1.Clear();
			if (!string.IsNullOrWhiteSpace(item.Logo))
			{
				defaultFiles1.Add(new UploadFile
				{
					PrevUrl = item.Logo
				});
			}
			defaultFiles2.Clear();
			if (!string.IsNullOrWhiteSpace(item.LoginImage))
			{
				defaultFiles2.Add(new UploadFile
				{
					PrevUrl = item.LoginImage
				});
			}
		}
		await Task.Yield();
	}

	private async Task OnCardUpload1(UploadFile file)
	{
		if (file.File != null)
		{
			defaultFiles1.Clear();
			if (UploadFileExtensions.IsImage(file, (List<string>)null, (Func<UploadFile, bool>)null))
			{
				await UploadFileExtensions.RequestBase64ImageFileAsync(file, "png", 150, 150, (long?)5120000L, (List<string>)null, default(CancellationToken));
			}
			uploadFileLogo1 = file;
		}
	}

	private async Task OnCardUpload2(UploadFile file)
	{
		if (file.File != null)
		{
			defaultFiles2.Clear();
			if (UploadFileExtensions.IsImage(file, (List<string>)null, (Func<UploadFile, bool>)null))
			{
				await UploadFileExtensions.RequestBase64ImageFileAsync(file, "png", 150, 150, (long?)5120000L, (List<string>)null, default(CancellationToken));
			}
			uploadFileLoginImage1 = file;
		}
	}

	private async Task OnSaving(NovaAdminConfirmEventArgs<SysTenant> e)
	{
		if (editItemId != e.Argument.Id && new string[2] { "main", "main_task" }.Contains(e.Argument.Id))
		{
			await MessageService.Error("请使用其他名称!");
			e.Cancel = true;
		}
		string[] hosts = (from a in editItemHost.Split(',')
			select a.Trim() into a
			where !a.IsNull()
			select a).ToArray();
		editItem.Host = ((hosts.Length != 0) ? hosts[0] : "");
		editItem.Host2 = ((hosts.Length > 1) ? hosts[1] : "");
		editItem.Host3 = ((hosts.Length > 2) ? hosts[2] : "");
		if (!defaultFiles1.Any())
		{
			e.Argument.Logo = null;
		}
		if (uploadFileLogo1 != null)
		{
			SysFile result = await fileService.UploadFileAsync(uploadFileLogo1, "tenant", isRename: true, 0L);
			if (result != null)
			{
				SysTenant argument = e.Argument;
				string logo = (uploadFileLogo1.PrevUrl = result.LinkUrl);
				argument.Logo = logo;
			}
		}
		if (!defaultFiles2.Any())
		{
			e.Argument.LoginImage = null;
		}
		if (uploadFileLoginImage1 != null)
		{
			SysFile result2 = await fileService.UploadFileAsync(uploadFileLoginImage1, "tenant", isRename: true, 0L);
			if (result2 != null)
			{
				SysTenant argument2 = e.Argument;
				string logo = (uploadFileLoginImage1.PrevUrl = result2.LinkUrl);
				argument2.LoginImage = logo;
			}
		}
		await Task.Yield();
	}

	private async Task OnSaved(SysTenant item)
	{
		IFreeSql fsql = admin.GetTenantFreeSql(item.Id);
		if (!(await ((ISelect0<ISelect<SysRole>, SysRole>)(object)fsql.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => a.IsAdministrator))).AnyAsync(default(CancellationToken))))
		{
			await fsql.Insert<SysRole>(new SysRole
			{
				Name = "Administrator",
				Description = "管理员角色",
				IsAdministrator = true
			}).ExecuteAffrowsAsync(default(CancellationToken));
		}
		if (!(await ((ISelect0<ISelect<SysUser>, SysUser>)(object)fsql.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Roles.Any((SysRole b) => b.IsAdministrator)))).AnyAsync(default(CancellationToken))))
		{
			SysUser adminUser = new SysUser
			{
				Username = "admin",
				Password = "admin",
				Nickname = "管理员"
			};
			SysUser sysUser = adminUser;
			List<SysRole> list = new List<SysRole>(1);
			List<SysRole> list2 = list;
			list2.Add(await ((ISelect0<ISelect<SysRole>, SysRole>)(object)fsql.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => a.IsAdministrator))).FirstAsync(default(CancellationToken)));
			sysUser.Roles = list;
			await FreeSqlAggregateRootRepositoryGlobalExtensions.GetAggregateRootRepository<SysUser>(fsql).InsertAsync(adminUser, default(CancellationToken));
		}
		if (item.Id != "main")
		{
			IBaseRepository<SysMenu> repoMenu = FreeSqlDbContextExtensions.GetRepository<SysMenu>(fsql);
			IBaseRepository<SysMenu> val = repoMenu;
			val.BeginEdit(await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>()).ToListAsync(default(CancellationToken)));
			IBaseRepository<SysMenu> val2 = repoMenu;
			val2.EndEdit(await admin.GenerateTenantMenus(item.Id));
		}
	}

	private async Task BeginModifyPassword(SysTenant item)
	{
		IFreeSql fsql = admin.GetTenantFreeSql(item.Id);
		modifyUsername = await fsql.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Roles.Any((SysRole b) => b.IsAdministrator))).FirstAsync<string>((Expression<Func<SysUser, string>>)((SysUser a) => a.Username), default(CancellationToken));
		modifyPassword = null;
		modifyPasswordItem = item;
	}

	private async Task ModifyPasswordFinish()
	{
		if (modifyUsername.IsNull() || modifyPassword.IsNull())
		{
			await MessageService.Error("账号或密码不能为空!");
			return;
		}
		IFreeSql fsql = admin.GetTenantFreeSql(modifyPasswordItem.Id);
		await fsql.Update<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Username == modifyUsername)).Set<string>((Expression<Func<SysUser, string>>)((SysUser a) => a.Password), modifyPassword)
			.Set<DateTime>((Expression<Func<SysUser, DateTime>>)((SysUser a) => a.LoginTime), DateTime.Now)
			.ExecuteAffrowsAsync(default(CancellationToken));
		await MessageService.Success("密码修改成功!");
		modifyPasswordItem = null;
	}
}
