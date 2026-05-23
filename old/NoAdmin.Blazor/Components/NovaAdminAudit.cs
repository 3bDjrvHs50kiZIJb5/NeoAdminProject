using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAdminAudit : ComponentBase
{
	private bool show = false;

	private string opinion;

	private bool showButton1;

	private bool showButton2;

	private bool showButton3;

	private bool showButton4;

	private bool showButton5;

	private List<SysUser> users;

	private string tableName;

	[Parameter]
	public EntityAudited Item { get; set; }

	[Parameter]
	public string Title { get; set; } = "审核";

	[Parameter]
	public EventCallback<NovaNovaAdminAuditedEventArgs> OnAudited { get; set; }

	[CascadingParameter]
	public NovaAdminQueryInfo AdminQuery { get; set; }

	[CascadingParameter]
	public NovaAdminContext.TabInfo TabInfo { get; set; }

	private NovaAdminQueryInfo q => AdminQuery;

	[Inject]
	private UnitOfWorkManager unitOfWorkManager { get; set; }

	[Inject]
	private FreeSqlCloud fsql { get; set; }

	[Inject]
	private IAggregateRootRepository<SysAuditLog> repositoryAuditLog { get; set; }

	[Inject]
	private NovaAdminContext admin { get; set; }

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
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (Item != null && TabInfo.AuditButtons != null)
		{
			if (Item.AuditStatus == SysAuditStatus.待提交)
			{
				__builder.OpenElement(0, "button");
				__builder.AddAttribute(1, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					show = true;
				}));
				__builder.AddAttribute(2, "type", "button");
				__builder.AddAttribute(3, "class", "btn btn-outline-primary");
				__builder.AddMarkupContent(4, "<i class=\"fas fa-arrow-circle-right\"></i>");
				__builder.AddContent(5, Item.AuditStatus);
				__builder.CloseElement();
			}
			else if (Item.AuditStatus == SysAuditStatus.审核中)
			{
				__builder.OpenComponent<Tooltip>(6);
				__builder.AddComponentParameter(7, "Title", RuntimeHelpers.TypeCheck("审核中：" + TabInfo.AuditButtons.Find((SysMenu a) => a.Path == Item.AuditStep)?.Label));
				__builder.AddComponentParameter(8, "Placement", RuntimeHelpers.TypeCheck<Placement>((Placement)16));
				__builder.AddAttribute(9, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
				{
					renderTreeBuilder.OpenElement(10, "button");
					renderTreeBuilder.AddAttribute(11, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
					{
						show = true;
					}));
					renderTreeBuilder.AddAttribute(12, "type", "button");
					renderTreeBuilder.AddAttribute(13, "class", "btn btn-outline-info");
					renderTreeBuilder.AddMarkupContent(14, "<i class=\"fas fa-refresh\"></i>");
					renderTreeBuilder.AddContent(15, Item.AuditStatus);
					renderTreeBuilder.CloseElement();
				});
				__builder.CloseComponent();
			}
			else if (Item.AuditStatus == SysAuditStatus.通过)
			{
				__builder.OpenElement(16, "button");
				__builder.AddAttribute(17, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					show = true;
				}));
				__builder.AddAttribute(18, "type", "button");
				__builder.AddAttribute(19, "class", "btn btn-outline-success");
				__builder.AddMarkupContent(20, "<i class=\"fas fa-circle-check\"></i>");
				__builder.AddContent(21, Item.AuditStatus);
				__builder.CloseElement();
			}
			else if (Item.AuditStatus == SysAuditStatus.退回)
			{
				__builder.OpenElement(22, "button");
				__builder.AddAttribute(23, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					show = true;
				}));
				__builder.AddAttribute(24, "type", "button");
				__builder.AddAttribute(25, "class", "btn btn-outline-danger");
				__builder.AddMarkupContent(26, "<i class=\"fas fa-undo\"></i>");
				__builder.AddContent(27, Item.AuditStatus);
				__builder.CloseElement();
			}
			else if (Item.AuditStatus == SysAuditStatus.拒绝)
			{
				__builder.OpenElement(28, "button");
				__builder.AddAttribute(29, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					show = true;
				}));
				__builder.AddAttribute(30, "type", "button");
				__builder.AddAttribute(31, "class", "btn btn-outline-danger");
				__builder.AddMarkupContent(32, "<i class=\"fas fa-ban\"></i>");
				__builder.AddContent(33, Item.AuditStatus);
				__builder.CloseElement();
			}
		}
		__builder.OpenComponent<NovaModal>(34);
		__builder.AddComponentParameter(35, "Visible", RuntimeHelpers.TypeCheck(show));
		__builder.AddComponentParameter(36, "Title", RuntimeHelpers.TypeCheck(Title));
		__builder.AddComponentParameter(37, "DialogClassName", "modal-xl modal-audit");
		__builder.AddComponentParameter(38, "IsBackdropStatic", RuntimeHelpers.TypeCheck(showButton1 || showButton2 || showButton3 || showButton4 || showButton5));
		__builder.AddComponentParameter(39, "OnClose", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			show = false;
		})));
		__builder.AddComponentParameter(40, "IsFooter", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddAttribute(41, "Body", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			if (showButton1 || showButton2 || showButton3 || showButton4 || showButton5)
			{
				renderTreeBuilder.OpenElement(42, "div");
				renderTreeBuilder.AddAttribute(43, "class", "row");
				renderTreeBuilder.OpenElement(44, "div");
				renderTreeBuilder.AddAttribute(45, "class", "form-group col-12");
				renderTreeBuilder.AddMarkupContent(46, "<label class=\"form-label\">处理意见：</label>\r\n                    ");
				renderTreeBuilder.OpenElement(47, "textarea");
				renderTreeBuilder.AddAttribute(48, "class", "form-control");
				renderTreeBuilder.AddAttribute(49, "placeholder", "");
				renderTreeBuilder.AddAttribute(50, "maxlength", "500");
				renderTreeBuilder.AddAttribute(51, "rows", "5");
				renderTreeBuilder.AddAttribute(52, "value", BindConverter.FormatValue(opinion));
				renderTreeBuilder.AddAttribute(53, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					opinion = __value;
				}, opinion));
				renderTreeBuilder.SetUpdatesAttributeName("value");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(54, "\r\n            ");
				renderTreeBuilder.OpenElement(55, "div");
				renderTreeBuilder.AddAttribute(56, "class", "row");
				renderTreeBuilder.OpenElement(57, "div");
				renderTreeBuilder.AddAttribute(58, "class", "form-group col-12");
				if (showButton1)
				{
					renderTreeBuilder.OpenElement(59, "button");
					renderTreeBuilder.AddAttribute(60, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Submit));
					renderTreeBuilder.AddAttribute(61, "type", "button");
					renderTreeBuilder.AddAttribute(62, "class", "btn btn-outline-success");
					renderTreeBuilder.AddMarkupContent(63, "<i class=\"fas fa-check\"></i>提交");
					renderTreeBuilder.CloseElement();
				}
				if (showButton2)
				{
					renderTreeBuilder.OpenElement(64, "button");
					renderTreeBuilder.AddAttribute(65, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Submit));
					renderTreeBuilder.AddAttribute(66, "type", "button");
					renderTreeBuilder.AddAttribute(67, "class", "btn btn-outline-success");
					renderTreeBuilder.AddMarkupContent(68, "<i class=\"fas fa-check\"></i>通过");
					renderTreeBuilder.CloseElement();
				}
				if (showButton3)
				{
					renderTreeBuilder.OpenElement(69, "button");
					renderTreeBuilder.AddAttribute(70, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitBack));
					renderTreeBuilder.AddAttribute(71, "type", "button");
					renderTreeBuilder.AddAttribute(72, "class", "btn btn-outline-danger");
					renderTreeBuilder.AddMarkupContent(73, "<i class=\"fas fa-undo\"></i>退回");
					renderTreeBuilder.CloseElement();
				}
				if (showButton4)
				{
					renderTreeBuilder.OpenElement(74, "button");
					renderTreeBuilder.AddAttribute(75, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitRefuse));
					renderTreeBuilder.AddAttribute(76, "type", "button");
					renderTreeBuilder.AddAttribute(77, "class", "btn btn-outline-danger");
					renderTreeBuilder.AddMarkupContent(78, "<i class=\"fas fa-ban\"></i>拒绝");
					renderTreeBuilder.CloseElement();
				}
				if (showButton5)
				{
					renderTreeBuilder.OpenElement(79, "button");
					renderTreeBuilder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitBack));
					renderTreeBuilder.AddAttribute(81, "type", "button");
					renderTreeBuilder.AddAttribute(82, "class", "btn btn-outline-danger");
					renderTreeBuilder.AddMarkupContent(83, "<i class=\"fas fa-angle-left\"></i>反审");
					renderTreeBuilder.CloseElement();
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			}
			else if (users != null)
			{
				renderTreeBuilder.OpenElement(84, "div");
				renderTreeBuilder.AddAttribute(85, "class", "row");
				renderTreeBuilder.OpenElement(86, "div");
				renderTreeBuilder.AddAttribute(87, "class", "form-group col-12");
				if (Item.AuditStatus == SysAuditStatus.待提交)
				{
					renderTreeBuilder.AddMarkupContent(88, "<label class=\"form-label\">待提交人员：</label>");
				}
				else if (Item.AuditStatus == SysAuditStatus.审核中)
				{
					renderTreeBuilder.AddMarkupContent(89, "<label class=\"form-label\">待审核人员：</label>");
				}
				else if (Item.AuditStatus == SysAuditStatus.通过)
				{
					renderTreeBuilder.AddMarkupContent(90, "<label class=\"form-label\">可反审人员：</label>");
				}
				else if (Item.AuditStatus == SysAuditStatus.退回)
				{
					renderTreeBuilder.AddMarkupContent(91, "<label class=\"form-label\">已反审退回，待重新提交人员：</label>");
				}
				else if (Item.AuditStatus == SysAuditStatus.拒绝)
				{
					renderTreeBuilder.AddMarkupContent(92, "<label class=\"form-label\">已拒绝，可反审人员：</label>");
				}
				renderTreeBuilder.OpenElement(93, "div");
				renderTreeBuilder.AddAttribute(94, "style", "min-height:50px;");
				renderTreeBuilder.AddMarkupContent(95, "<b class=\"text-info\">管理员</b>");
				foreach (SysUser user in users)
				{
					renderTreeBuilder.OpenElement(96, "b");
					renderTreeBuilder.AddAttribute(97, "class", "ml-3 text-info");
					renderTreeBuilder.AddContent(98, user.Username);
					renderTreeBuilder.CloseElement();
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(99, "\r\n            ");
				renderTreeBuilder.OpenElement(100, "div");
				renderTreeBuilder.AddAttribute(101, "class", "row");
				renderTreeBuilder.OpenElement(102, "div");
				renderTreeBuilder.AddAttribute(103, "class", "form-group col-12");
				renderTreeBuilder.OpenElement(104, "button");
				renderTreeBuilder.AddAttribute(105, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
				{
					show = false;
				}));
				renderTreeBuilder.AddAttribute(106, "type", "button");
				renderTreeBuilder.AddAttribute(107, "class", "btn btn-light");
				renderTreeBuilder.AddMarkupContent(108, "<i class=\"fas fa-check\"></i>知道了");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			}
			renderTreeBuilder.OpenElement(109, "div");
			renderTreeBuilder.AddAttribute(110, "class", "row-status");
			renderTreeBuilder.OpenElement(111, "span");
			renderTreeBuilder.AddMarkupContent(112, "当前审核状态：");
			renderTreeBuilder.OpenElement(113, "b");
			renderTreeBuilder.AddAttribute(114, "class", "text-primary");
			renderTreeBuilder.AddContent(115, Item.AuditStatus);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			if (Item.AuditStatus == SysAuditStatus.待提交)
			{
				renderTreeBuilder.OpenElement(116, "span");
				renderTreeBuilder.AddMarkupContent(117, "下一步骤：");
				renderTreeBuilder.OpenElement(118, "b");
				renderTreeBuilder.AddAttribute(119, "class", "text-primary");
				renderTreeBuilder.AddContent(120, TabInfo.AuditButtons.FirstOrDefault()?.Label ?? "结束");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			}
			else if (Item.AuditStatus == SysAuditStatus.审核中)
			{
				int num = TabInfo.AuditButtons.FindIndex((SysMenu a) => a.Path == Item.AuditStep);
				if (num == -1)
				{
					renderTreeBuilder.AddMarkupContent(121, "<span>当前步骤：<b class=\"text-primary\">未知</b></span>");
				}
				else
				{
					renderTreeBuilder.OpenElement(122, "span");
					renderTreeBuilder.AddMarkupContent(123, "当前步骤：");
					renderTreeBuilder.OpenElement(124, "b");
					renderTreeBuilder.AddAttribute(125, "class", "text-primary");
					renderTreeBuilder.AddContent(126, TabInfo.AuditButtons[num].Label);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(127, "\r\n                    ");
					renderTreeBuilder.OpenElement(128, "span");
					renderTreeBuilder.AddMarkupContent(129, "下一步骤：");
					renderTreeBuilder.OpenElement(130, "b");
					renderTreeBuilder.AddAttribute(131, "class", "text-primary");
					renderTreeBuilder.AddContent(132, (num + 1 < TabInfo.AuditButtons.Count) ? TabInfo.AuditButtons[num + 1].Label : "结束");
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
				}
			}
			else if (Item.AuditStatus == SysAuditStatus.退回)
			{
				renderTreeBuilder.OpenElement(133, "span");
				renderTreeBuilder.AddMarkupContent(134, "当前步骤：");
				renderTreeBuilder.OpenElement(135, "b");
				renderTreeBuilder.AddAttribute(136, "class", "text-primary");
				renderTreeBuilder.AddContent(137, GetAuditButtonLabel(Item.AuditStep, 0));
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			}
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(138, "\r\n        ");
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysAuditLog>>(139);
			renderTreeBuilder.AddComponentParameter(140, "PageSize", RuntimeHelpers.TypeCheck(0));
			renderTreeBuilder.AddComponentParameter(141, "IsExportExcel", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(142, "IsRefersh", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(143, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(144, "IsAudit", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(145, "IsView", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(146, "IsAdd", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(147, "IsEdit", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(148, "IsRemove", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(149, "IsSingleSelect", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(150, "IsMultiSelect", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(151, "IsQueryString", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(152, "NoDataText", "暂无审核记录！");
			renderTreeBuilder.AddComponentParameter(153, "BodyHeight", RuntimeHelpers.TypeCheck(200));
			renderTreeBuilder.AddComponentParameter(154, "InitQuery", new Func<NovaAdminQueryInfo, Task>(SysAuditHistoryInitQuery));
			renderTreeBuilder.AddComponentParameter(155, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysAuditLog>>)SysAuditHistoryOnQuery)));
			renderTreeBuilder.AddAttribute(156, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(157, "<th width=\"68\">处理步骤</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(158, "<th width=\"68\">接收步骤</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(159, "<th>处理意见</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(160, "<th width=\"80\">审核人</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(161, "<th width=\"150\">审核时间</th>");
			});
			renderTreeBuilder.AddAttribute(162, "TableRow", (RenderFragment<SysAuditLog>)((SysAuditLog item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(163, "td");
				renderTreeBuilder2.AddContent(164, GetAuditButtonLabel(item.Step, 1));
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(165, "\r\n                ");
				renderTreeBuilder2.OpenElement(166, "td");
				if (item.NextStep == "")
				{
					renderTreeBuilder2.AddMarkupContent(167, "<b class=\"text-success\">【结束】</b>");
				}
				else if (item.NextStep == "audit_98")
				{
					renderTreeBuilder2.AddMarkupContent(168, "<b class=\"text-danger\">【拒绝】</b>");
				}
				else
				{
					renderTreeBuilder2.AddContent(169, GetAuditButtonLabel(item.NextStep, 0));
				}
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(170, "\r\n                ");
				renderTreeBuilder2.OpenElement(171, "td");
				renderTreeBuilder2.AddContent(172, item.Opinion);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(173, "\r\n                ");
				renderTreeBuilder2.OpenElement(174, "td");
				renderTreeBuilder2.AddContent(175, item.CreatedUserName);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(176, "\r\n                ");
				renderTreeBuilder2.OpenElement(177, "td");
				renderTreeBuilder2.AddContent(178, item.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.CloseComponent();
	}

	protected override async Task OnInitializedAsync()
	{
		if (TabInfo.AuditButtons == null)
		{
			System.Console.WriteLine("NovaAdminAudit -> OnInitializedAsync ...");
			List<SysMenu> auditButtons = await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((FreeSqlCloud<string>)(object)fsql).Use("main").Select<SysMenu>().Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.ParentId == TabInfo.Menu.Id && (int)a.Type == 1 && a.IsSystem == true && a.Path.StartsWith("audit_")))
				.OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort))).ToListAsync(default(CancellationToken));
			TabInfo.AuditAllButtons = auditButtons.Where((SysMenu a) => Regex.IsMatch(a.Path, "audit_\\d\\d")).ToList();
			TabInfo.AuditButtons = TabInfo.AuditAllButtons.Where((SysMenu a) => a.Path != "audit_00" && a.Path != "audit_98" && a.Path != "audit_99").ToList();
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (TabInfo.AuditButtons == null)
		{
			List<SysMenu> auditButtons = await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((FreeSqlCloud<string>)(object)fsql).Use("main").Select<SysMenu>().Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.ParentId == TabInfo.Menu.Id && (int)a.Type == 1 && a.IsSystem == true && a.Path.StartsWith("audit_")))
				.OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort))).ToListAsync(default(CancellationToken));
			TabInfo.AuditAllButtons = auditButtons.Where((SysMenu a) => Regex.IsMatch(a.Path, "audit_\\d\\d")).ToList();
			TabInfo.AuditButtons = TabInfo.AuditAllButtons.Where((SysMenu a) => a.Path != "audit_00" && a.Path != "audit_98" && a.Path != "audit_99").ToList();
		}
	}

	private string GetAuditButtonLabel(string step, int style)
	{
		if (step == "")
		{
			return (style == 1) ? "【结束】" : "结束";
		}
		if (step == "audit_98")
		{
			return (style == 1) ? "【拒绝】" : "拒绝";
		}
		if (step == "audit_00")
		{
			return "提交";
		}
		return TabInfo.AuditButtons.Find((SysMenu a) => a.Path == step)?.Label;
	}

	private async Task SysAuditHistoryInitQuery(NovaAdminQueryInfo e)
	{
		tableName = ((FreeSqlCloud<string>)(object)fsql).CodeFirst.GetTableByEntity(Item.GetType()).DbName;
		await InitButton();
	}

	private void SysAuditHistoryOnQuery(NovaAdminQueryEventArgs<SysAuditLog> e)
	{
		e.Select.Where((Expression<Func<SysAuditLog, bool>>)((SysAuditLog a) => a.TableName == tableName && a.TableId == Item.Id));
	}

	private async Task InitButton()
	{
		if (TabInfo.AuditAllButtons.Any())
		{
			showButton1 = (Item.AuditStatus == SysAuditStatus.待提交 || Item.AuditStatus == SysAuditStatus.退回) && admin.AuthButton(TabInfo.Menu, "audit_00");
			showButton2 = Item.AuditStatus == SysAuditStatus.审核中 && admin.AuthButton(TabInfo.Menu, Item.AuditStep);
			showButton3 = Item.AuditStatus == SysAuditStatus.审核中 && TabInfo.AuditButtons.Any((SysMenu a) => a.Path == Item.AuditStep) && TabInfo.AuditAllButtons.Any((SysMenu a) => a.Path == "audit_99") && admin.AuthButton(TabInfo.Menu, "audit_99");
			showButton4 = Item.AuditStatus == SysAuditStatus.审核中 && TabInfo.AuditAllButtons.Any((SysMenu a) => a.Path == "audit_98") && admin.AuthButton(TabInfo.Menu, "audit_98");
			showButton5 = (Item.AuditStatus == SysAuditStatus.通过 || Item.AuditStatus == SysAuditStatus.拒绝) && TabInfo.AuditAllButtons.Any((SysMenu a) => a.Path == "audit_99") && admin.AuthButton(TabInfo.Menu, "audit_99");
			if (!showButton1 && !showButton2 && !showButton3 && !showButton4 && !showButton5)
			{
				if (Item.AuditStep == "")
				{
					users = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)((FreeSqlCloud<string>)(object)fsql).Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Roles.Any((SysRole b) => b.Menus.Any((SysMenu c) => c.ParentId == TabInfo.Menu.Id && c.Path == "audit_99"))))).ToListAsync(default(CancellationToken));
				}
				else
				{
					users = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)((FreeSqlCloud<string>)(object)fsql).Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Roles.Any((SysRole b) => b.Menus.Any((SysMenu c) => c.ParentId == TabInfo.Menu.Id && c.Path == Item.AuditStep))))).ToListAsync(default(CancellationToken));
				}
			}
		}
		await Task.Delay(300).ContinueWith((Task e) => InvokeAsync((Action)base.StateHasChanged));
	}

	private async Task Submit()
	{
		string buttonName = "提交";
		SysAuditLog auditLog = null;
		if (Item.AuditStatus == SysAuditStatus.待提交 || Item.AuditStatus == SysAuditStatus.退回)
		{
			if (!admin.AuthButton(TabInfo.Menu, "audit_00"))
			{
				await MessageService.Error("您没有权限进行此操作！");
				return;
			}
			auditLog = new SysAuditLog
			{
				TableName = tableName,
				TableId = Item.Id,
				Step = (Item.AuditStep ?? "audit_00"),
				Opinion = opinion,
				Result = SysAuditStatus.通过,
				NextStep = (TabInfo.AuditButtons.FirstOrDefault()?.Path ?? "")
			};
		}
		else if (Item.AuditStatus == SysAuditStatus.审核中)
		{
			if (!admin.AuthButton(TabInfo.Menu, Item.AuditStep))
			{
				await MessageService.Error("您没有权限进行此操作！");
				return;
			}
			buttonName = "审核";
			int index = TabInfo.AuditButtons.FindIndex((SysMenu a) => a.Path == Item.AuditStep);
			auditLog = new SysAuditLog
			{
				TableName = tableName,
				TableId = Item.Id,
				Step = Item.AuditStep,
				Opinion = opinion,
				Result = SysAuditStatus.通过,
				NextStep = ((index >= 0 && index + 1 < TabInfo.AuditButtons.Count) ? TabInfo.AuditButtons[index + 1].Path : "")
			};
		}
		if (auditLog == null)
		{
			return;
		}
		bool succuess = false;
		IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
		try
		{
			if (await uow.Orm.Update<EntityAudited>().AsTable(tableName).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id && a.AuditVersion == Item.AuditVersion))
				.Set<SysAuditStatus>((Expression<Func<EntityAudited, SysAuditStatus>>)((EntityAudited a) => a.AuditStatus), (!(auditLog.NextStep == "")) ? SysAuditStatus.审核中 : SysAuditStatus.通过)
				.Set<string>((Expression<Func<EntityAudited, string>>)((EntityAudited a) => a.AuditStep), auditLog.NextStep)
				.Set<int>((Expression<Func<EntityAudited, int>>)((EntityAudited a) => a.AuditVersion + 1))
				.ExecuteAffrowsAsync(default(CancellationToken)) == 1)
			{
				await ((IBaseRepository<SysAuditLog>)(object)repositoryAuditLog).InsertAsync(auditLog, default(CancellationToken));
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
			await MessageService.Error(buttonName + "失败，数据发生变化！");
			EntityAudited newitem = await ((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((IBaseRepository)repositoryAuditLog).Orm.Select<EntityAudited>()).AsTable((Func<Type, string, string>)((Type _, string old) => tableName)).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id))).FirstAsync(default(CancellationToken));
			Item.AuditStatus = newitem.AuditStatus;
			Item.AuditStep = newitem.AuditStep;
			Item.AuditVersion = newitem.AuditVersion;
			await InitButton();
		}
		else
		{
			if (OnAudited.HasDelegate)
			{
				await OnAudited.InvokeAsync(new NovaNovaAdminAuditedEventArgs
				{
					Entity = Item,
					AuditLog = auditLog
				});
			}
			await MessageService.Success(buttonName + "成功！");
			await q.InvokeQueryAsync();
			show = false;
			StateHasChanged();
		}
	}

	private async Task SubmitBack()
	{
		if (!admin.AuthButton(TabInfo.Menu, "audit_99"))
		{
			await MessageService.Error("您没有权限进行此操作！");
			return;
		}
		if (opinion.IsNull())
		{
			await MessageService.Error("处理意见不能为空！");
			return;
		}
		string buttonName = "退回";
		SysAuditStatus backAuditStatus = SysAuditStatus.退回;
		SysAuditLog auditLog = null;
		if (Item.AuditStatus == SysAuditStatus.审核中)
		{
			int index = TabInfo.AuditButtons.FindIndex((SysMenu a) => a.Path == Item.AuditStep);
			if (index >= 1)
			{
				backAuditStatus = SysAuditStatus.审核中;
			}
			auditLog = new SysAuditLog
			{
				TableName = tableName,
				TableId = Item.Id,
				Step = Item.AuditStep,
				Opinion = opinion,
				Result = SysAuditStatus.退回,
				NextStep = ((index - 1 >= 0) ? TabInfo.AuditButtons[index - 1].Path : "audit_00")
			};
		}
		else if (Item.AuditStatus == SysAuditStatus.通过 || Item.AuditStatus == SysAuditStatus.拒绝)
		{
			buttonName = "反审";
			auditLog = new SysAuditLog
			{
				TableName = tableName,
				TableId = Item.Id,
				Step = ((Item.AuditStatus == SysAuditStatus.通过) ? "" : Item.AuditStep),
				Opinion = opinion,
				Result = SysAuditStatus.退回,
				NextStep = "audit_00"
			};
		}
		if (auditLog == null)
		{
			return;
		}
		bool succuess = false;
		IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
		try
		{
			if (await uow.Orm.Update<EntityAudited>().AsTable(tableName).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id && a.AuditVersion == Item.AuditVersion))
				.Set<SysAuditStatus>((Expression<Func<EntityAudited, SysAuditStatus>>)((EntityAudited a) => a.AuditStatus), backAuditStatus)
				.Set<string>((Expression<Func<EntityAudited, string>>)((EntityAudited a) => a.AuditStep), auditLog.NextStep)
				.Set<int>((Expression<Func<EntityAudited, int>>)((EntityAudited a) => a.AuditVersion + 1))
				.ExecuteAffrowsAsync(default(CancellationToken)) == 1)
			{
				await ((IBaseRepository<SysAuditLog>)(object)repositoryAuditLog).InsertAsync(auditLog, default(CancellationToken));
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
			await MessageService.Error(buttonName + "失败，数据发生变化！");
			EntityAudited newitem = await ((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((IBaseRepository)repositoryAuditLog).Orm.Select<EntityAudited>()).AsTable((Func<Type, string, string>)((Type _, string old) => tableName)).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id))).FirstAsync(default(CancellationToken));
			Item.AuditStatus = newitem.AuditStatus;
			Item.AuditStep = newitem.AuditStep;
			Item.AuditVersion = newitem.AuditVersion;
			await InitButton();
		}
		else
		{
			if (OnAudited.HasDelegate)
			{
				await OnAudited.InvokeAsync(new NovaNovaAdminAuditedEventArgs
				{
					Entity = Item,
					AuditLog = auditLog
				});
			}
			await MessageService.Success(buttonName + "成功！");
			await q.InvokeQueryAsync();
			show = false;
			StateHasChanged();
		}
	}

	private async Task SubmitRefuse()
	{
		if (!admin.AuthButton(TabInfo.Menu, "audit_98"))
		{
			await MessageService.Error("您没有权限进行此操作！");
			return;
		}
		if (opinion.IsNull())
		{
			await MessageService.Error("处理意见不能为空！");
			return;
		}
		string buttonName = "拒绝";
		SysAuditLog auditLog = null;
		if (Item.AuditStatus == SysAuditStatus.审核中)
		{
			auditLog = new SysAuditLog
			{
				TableName = tableName,
				TableId = Item.Id,
				Step = Item.AuditStep,
				Opinion = opinion,
				Result = SysAuditStatus.拒绝,
				NextStep = "audit_98"
			};
		}
		if (auditLog == null)
		{
			return;
		}
		bool succuess = false;
		IUnitOfWork uow = unitOfWorkManager.Begin((Propagation)5, (IsolationLevel?)null);
		try
		{
			if (await uow.Orm.Update<EntityAudited>().AsTable(tableName).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id && a.AuditVersion == Item.AuditVersion))
				.Set<SysAuditStatus>((Expression<Func<EntityAudited, SysAuditStatus>>)((EntityAudited a) => a.AuditStatus), auditLog.Result)
				.Set<string>((Expression<Func<EntityAudited, string>>)((EntityAudited a) => a.AuditStep), auditLog.NextStep)
				.Set<int>((Expression<Func<EntityAudited, int>>)((EntityAudited a) => a.AuditVersion + 1))
				.ExecuteAffrowsAsync(default(CancellationToken)) == 1)
			{
				await ((IBaseRepository<SysAuditLog>)(object)repositoryAuditLog).InsertAsync(auditLog, default(CancellationToken));
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
			await MessageService.Error(buttonName + "失败，数据发生变化！");
			EntityAudited newitem = await ((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((ISelect0<ISelect<EntityAudited>, EntityAudited>)(object)((IBaseRepository)repositoryAuditLog).Orm.Select<EntityAudited>()).AsTable((Func<Type, string, string>)((Type _, string old) => tableName)).Where((Expression<Func<EntityAudited, bool>>)((EntityAudited a) => a.Id == Item.Id))).FirstAsync(default(CancellationToken));
			Item.AuditStatus = newitem.AuditStatus;
			Item.AuditStep = newitem.AuditStep;
			Item.AuditVersion = newitem.AuditVersion;
			await InitButton();
		}
		else
		{
			if (OnAudited.HasDelegate)
			{
				await OnAudited.InvokeAsync(new NovaNovaAdminAuditedEventArgs
				{
					Entity = Item,
					AuditLog = auditLog
				});
			}
			await MessageService.Success(buttonName + "成功！");
			await q.InvokeQueryAsync();
			show = false;
			StateHasChanged();
		}
	}
}
