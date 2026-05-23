using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeScheduler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;
using Rougamo;
using Rougamo.Context;
using ResultGetClusterLogs = FreeScheduler.Datafeed.ResultGetClusterLogs;
using ResultGetLogs = FreeScheduler.Datafeed.ResultGetLogs;
using ResultGetPage = FreeScheduler.Datafeed.ResultGetPage;
using ClusterInfo = FreeScheduler.Datafeed.ResultGetPage.ClusterInfo;
using ClusterLog = FreeScheduler.Datafeed.ClusterLog;
using TaskStatus = FreeScheduler.TaskStatus;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/TaskScheduler")]
public class TaskScheduler : ComponentBase
{
	private NovaAdminQueryInfo q = new NovaAdminQueryInfo();

	private bool isLoaded = false;

	private bool isShowLoader = false;

	private ResultGetPage loadResult;

	private ElementReference _theadElement;

	private TaskInfo task;

	private bool roundNever = true;

	private IEnumerable<SelectedItem> templateList = (IEnumerable<SelectedItem>)(object)new SelectedItem[3]
	{
		new SelectedItem("0", "自定义"),
		new SelectedItem("1", "HTTP请求"),
		new SelectedItem("2", "清理任务数据")
	};

	private ResultGetLogs taskLog;

	private NovaAdminQueryInfo qLog = new NovaAdminQueryInfo
	{
		IsQueryString = false,
		PageSize = 10
	};

	private TaskInfo logTask;

	private ResultGetClusterLogs clusterLog;

	private NovaAdminQueryInfo qCluster = new NovaAdminQueryInfo
	{
		IsQueryString = false,
		PageSize = 10
	};

	[Inject]
	private IFreeSql fsql { get; set; }

	[Inject]
	private Scheduler scheduler { get; set; }

	[Inject]
	private IHostEnvironment env { get; set; }

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
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Invalid comparison between Unknown and I4
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Invalid comparison between Unknown and I4
		//IL_06c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Invalid comparison between Unknown and I4
		//IL_07ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Invalid comparison between Unknown and I4
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0902: Unknown result type (might be due to invalid IL or missing references)
		//IL_0908: Invalid comparison between Unknown and I4
		//IL_0929: Unknown result type (might be due to invalid IL or missing references)
		//IL_092f: Invalid comparison between Unknown and I4
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		//IL_0956: Invalid comparison between Unknown and I4
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "定时任务");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenElement(4, "div");
		__builder.AddAttribute(5, "class", "card card-info card-outline");
		__builder.OpenElement(6, "div");
		__builder.AddAttribute(7, "class", "card-header d-block");
		__builder.OpenElement(8, "blockquote");
		__builder.AddAttribute(9, "class", "quote-info m-0 p-0 mt-2 mb-3 pl-2");
		ResultGetPage obj = loadResult;
		__builder.AddContent(10, (obj != null) ? obj.Description : null);
		if (env.IsDevelopment())
		{
			__builder.AddContent(11, ", 未启动任务(开发环境)");
		}
		__builder.CloseElement();
		__builder.AddMarkupContent(12, "\r\n        ");
		__builder.OpenComponent<NovaAdminSearchFilter>(13);
		__builder.AddComponentParameter(14, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
		__builder.CloseComponent();
		__builder.CloseElement();
		__builder.AddMarkupContent(15, "\r\n    ");
		__builder.OpenElement(16, "div");
		__builder.AddAttribute(17, "class", "card-header d-block");
		__builder.OpenElement(18, "button");
		__builder.AddAttribute(19, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)BeginAdd));
		__builder.AddAttribute(20, "type", "button");
		__builder.AddAttribute(21, "class", "mr-2 btn btn-light");
		__builder.AddMarkupContent(22, "<i class=\"fas fa-plus\"></i> 添加");
		__builder.CloseElement();
		__builder.AddMarkupContent(23, "\r\n        ");
		__builder.OpenElement(24, "button");
		__builder.AddAttribute(25, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => Load()));
		__builder.AddAttribute(26, "type", "button");
		__builder.AddAttribute(27, "class", "mr-2 btn btn-light");
		__builder.AddMarkupContent(28, "<i class=\"fas fa-sync-alt\"></i> 刷新");
		__builder.CloseElement();
		ResultGetPage obj2 = loadResult;
		if (obj2 != null && obj2.Clusters?.Any() == true)
		{
			__builder.OpenElement(29, "button");
			__builder.AddAttribute(30, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)LoadClusterLog));
			__builder.AddAttribute(31, "type", "button");
			__builder.AddAttribute(32, "class", "mr-2 btn btn-light");
			__builder.AddMarkupContent(33, "<i class=\"fas fa-log\"></i> 集群日志");
			__builder.CloseElement();
		}
		__builder.OpenElement(34, "div");
		__builder.AddAttribute(35, "class", "float-end");
		__builder.OpenComponent<NovaAdminSearchText>(36);
		__builder.AddComponentParameter(37, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
		__builder.AddComponentParameter(38, "Placeholder", "标题..");
		__builder.CloseComponent();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(39, "\r\n    ");
		__builder.OpenElement(40, "div");
		__builder.AddAttribute(41, "class", "admintable2-loader table-loader");
		__builder.AddAttribute(42, "style", isShowLoader ? "" : "display:none");
		__builder.AddMarkupContent(43, "<div class=\"spinner spinner-border text-primary\"><span class=\"visually-hidden\">Loading...</span></div>");
		__builder.CloseElement();
		__builder.AddMarkupContent(44, "\r\n    ");
		__builder.OpenElement(45, "div");
		__builder.AddAttribute(46, "class", "card-body p-0 table-shim table-scroll scroll table-fixed-column");
		__builder.AddAttribute(47, "style", "border:none;");
		__builder.OpenElement(48, "table");
		__builder.AddAttribute(49, "class", "table table-hover table-bordered table-sm m-0");
		__builder.OpenElement(50, "thead");
		__builder.AddElementReferenceCapture(51, delegate(ElementReference __value)
		{
			_theadElement = __value;
		});
		__builder.AddMarkupContent(52, "<tr><th class=\"fixed no-resize\" style=\"left:0;width:30px;\"></th>\r\n                    <th></th>\r\n                    <th>标题</th>\r\n                    <th>定时</th>\r\n                    <th>轮次</th>\r\n                    <th>内容</th>\r\n                    <th>状态</th>\r\n                    <th>错误</th>\r\n                    <th>最后运行</th>\r\n                    <th>下次运行</th>\r\n                    <th>创建时间</th></tr>");
		__builder.CloseElement();
		__builder.AddMarkupContent(53, "\r\n            ");
		__builder.OpenElement(54, "tbody");
		int num = 0;
		while (true)
		{
			int num2 = num;
			ResultGetPage obj3 = loadResult;
			if (!(num2 < ((obj3 != null) ? new int?(obj3.Tasks.Count) : ((int?)null))))
			{
				break;
			}
			TaskInfo item = loadResult.Tasks[num];
			__builder.OpenElement(55, "tr");
			__builder.OpenElement(56, "th");
			__builder.AddAttribute(57, "class", "fixed");
			__builder.AddAttribute(58, "style", "left:0;font-weight:normal;");
			__builder.AddContent(59, Math.Max(0, q.PageNumber - 1) * q.PageSize + num + 1);
			__builder.CloseElement();
			__builder.AddMarkupContent(60, "\r\n                        ");
			__builder.OpenElement(61, "td");
			__builder.OpenElement(62, "input");
			__builder.AddAttribute(63, "type", "button");
			__builder.AddAttribute(64, "class", "btn btn-xs btn-danger");
			__builder.AddAttribute(65, "value", "删除");
			__builder.AddAttribute(66, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => RemoveTask(item)));
			__builder.CloseElement();
			if ((int)item.Status == 1)
			{
				__builder.OpenElement(67, "input");
				__builder.AddAttribute(68, "type", "button");
				__builder.AddAttribute(69, "class", "ml-2 btn btn-xs btn-success");
				__builder.AddAttribute(70, "value", "恢复");
				__builder.AddAttribute(71, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => ResumeTask(item)));
				__builder.CloseElement();
			}
			if ((int)item.Status == 0)
			{
				__builder.OpenElement(72, "input");
				__builder.AddAttribute(73, "type", "button");
				__builder.AddAttribute(74, "class", "ml-2 btn btn-xs btn-warning");
				__builder.AddAttribute(75, "value", "暂停");
				__builder.AddAttribute(76, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => PauseTask(item)));
				__builder.CloseElement();
			}
			if ((int)item.Status != 2)
			{
				__builder.OpenElement(77, "input");
				__builder.AddAttribute(78, "type", "button");
				__builder.AddAttribute(79, "class", "ml-2 btn btn-xs btn-light");
				__builder.AddAttribute(80, "value", "立即触发");
				__builder.AddAttribute(81, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => RunNowTask(item)));
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.AddMarkupContent(82, "\r\n                        ");
			__builder.OpenElement(83, "td");
			__builder.AddContent(84, item.Topic);
			__builder.CloseElement();
			__builder.AddMarkupContent(85, "\r\n                        ");
			__builder.OpenElement(86, "td");
			if ((int)item.Interval == 21)
			{
				__builder.AddContent(87, item.IntervalArgument);
			}
			else
			{
				__builder.AddContent(88, item.Interval);
				__builder.AddContent(89, " ");
				__builder.AddContent(90, item.IntervalArgument);
			}
			__builder.CloseElement();
			__builder.AddMarkupContent(91, "\r\n                        ");
			__builder.OpenElement(92, "td");
			__builder.OpenComponent<Button>(93);
			__builder.AddComponentParameter(94, "class", "btn btn-xs btn-light");
			__builder.AddComponentParameter(95, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => LoadTaskLog(item)));
			__builder.AddAttribute(96, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				if (item.Round == -1)
				{
					renderTreeBuilder.OpenElement(97, "span");
					renderTreeBuilder.AddContent(98, item.CurrentRound);
					renderTreeBuilder.AddContent(99, "/∞");
					renderTreeBuilder.CloseElement();
				}
				else
				{
					renderTreeBuilder.OpenElement(100, "span");
					renderTreeBuilder.AddContent(101, item.CurrentRound);
					renderTreeBuilder.AddContent(102, "/");
					renderTreeBuilder.AddContent(103, item.Round);
					renderTreeBuilder.CloseElement();
				}
			});
			__builder.CloseComponent();
			__builder.CloseElement();
			__builder.AddMarkupContent(104, "\r\n                        ");
			__builder.OpenElement(105, "td");
			__builder.AddContent(106, item.Body);
			__builder.CloseElement();
			__builder.AddMarkupContent(107, "\r\n                        ");
			__builder.OpenElement(108, "td");
			if ((int)item.Status == 1)
			{
				__builder.AddContent(109, "已暂停");
			}
			else if ((int)item.Status == 0)
			{
				__builder.AddContent(110, "运行中");
			}
			else if ((int)item.Status != 2)
			{
				__builder.AddContent(111, "已结束");
			}
			__builder.CloseElement();
			__builder.AddMarkupContent(112, "\r\n                        ");
			__builder.OpenElement(113, "td");
			__builder.AddContent(114, item.ErrorTimes);
			__builder.CloseElement();
			__builder.AddMarkupContent(115, "\r\n                        ");
			__builder.OpenElement(116, "td");
			__builder.AddContent(117, item.LastRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
			__builder.CloseElement();
			__builder.AddMarkupContent(118, "\r\n                        ");
			__builder.OpenElement(119, "td");
			__builder.AddContent(120, loadResult.NextTimes[num]?.ToString("yyyy-MM-dd HH:mm:ss"));
			__builder.CloseElement();
			__builder.AddMarkupContent(121, "\r\n                        ");
			__builder.OpenElement(122, "td");
			__builder.AddContent(123, item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			__builder.CloseElement();
			__builder.CloseElement();
			num++;
		}
		__builder.CloseElement();
		__builder.CloseElement();
		if (!isLoaded)
		{
			__builder.AddMarkupContent(124, "<div style=\"text-align:center;padding-top:18px;color:#6a6a6a;\">玩命加载中...</div>");
		}
		else
		{
			ResultGetPage obj4 = loadResult;
			if (obj4 != null && !obj4.Tasks.Any())
			{
				__builder.AddMarkupContent(125, "<div style=\"text-align:center;padding-top:18px;color:#6a6a6a;\">暂无数据.</div>");
			}
		}
		__builder.CloseElement();
		__builder.AddMarkupContent(126, "\r\n    ");
		__builder.OpenElement(127, "div");
		__builder.AddAttribute(128, "class", "card-footer");
		__builder.OpenComponent<NovaAdminPagination>(129);
		__builder.AddComponentParameter(130, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(q));
		__builder.CloseComponent();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(131, "\r\n\r\n");
		__builder.OpenComponent<NovaModal>(132);
		__builder.AddComponentParameter(133, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(task != null));
		__builder.AddComponentParameter(134, "Title", "【添加】定时任务");
		__builder.AddComponentParameter(135, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			task = null;
		})));
		__builder.AddComponentParameter(136, "OnYes", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)OnSave)));
		__builder.AddAttribute(137, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			//IL_042a: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0676: Unknown result type (might be due to invalid IL or missing references)
			//IL_067c: Invalid comparison between Unknown and I4
			//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b2: Invalid comparison between Unknown and I4
			//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e8: Invalid comparison between Unknown and I4
			//IL_0792: Unknown result type (might be due to invalid IL or missing references)
			//IL_0799: Invalid comparison between Unknown and I4
			//IL_0859: Unknown result type (might be due to invalid IL or missing references)
			//IL_0860: Invalid comparison between Unknown and I4
			if (task != null)
			{
				renderTreeBuilder.OpenElement(138, "div");
				renderTreeBuilder.AddAttribute(139, "class", "row");
				renderTreeBuilder.OpenElement(140, "div");
				renderTreeBuilder.AddAttribute(141, "class", "form-group col-12");
				renderTreeBuilder.AddMarkupContent(142, "<label class=\"form-label\">模板</label>\r\n                ");
				renderTreeBuilder.OpenComponent<RadioList<string>>(143);
				renderTreeBuilder.AddComponentParameter(144, "Items", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(templateList));
				renderTreeBuilder.AddComponentParameter(145, "OnSelectedChanged", new Func<IEnumerable<SelectedItem>, string, Task>(OnTemplateChanged));
				renderTreeBuilder.AddComponentParameter(146, "IsVertical", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder.CloseComponent();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(147, "\r\n            ");
				renderTreeBuilder.OpenElement(148, "div");
				renderTreeBuilder.AddAttribute(149, "class", "form-group col-12");
				renderTreeBuilder.AddMarkupContent(150, "<label class=\"form-label\">标题</label>\r\n                ");
				renderTreeBuilder.OpenElement(151, "input");
				renderTreeBuilder.AddAttribute(152, "type", "text");
				renderTreeBuilder.AddAttribute(153, "class", "form-control");
				renderTreeBuilder.AddAttribute(154, "placeholder", "");
				renderTreeBuilder.AddAttribute(155, "maxlength", "255");
				renderTreeBuilder.AddAttribute(156, "value", BindConverter.FormatValue(task.Topic));
				renderTreeBuilder.AddAttribute(157, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					task.Topic = __value;
				}, task.Topic));
				renderTreeBuilder.SetUpdatesAttributeName("value");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(158, "\r\n            ");
				renderTreeBuilder.OpenElement(159, "div");
				renderTreeBuilder.AddAttribute(160, "class", "form-group col-12");
				renderTreeBuilder.OpenElement(161, "label");
				renderTreeBuilder.AddAttribute(162, "class", "form-label");
				renderTreeBuilder.AddContent(163, (task.Topic == "[系统预留]清理任务数据") ? "清理多久之前的记录(单位:秒)" : "内容");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(164, "\r\n                ");
				renderTreeBuilder.OpenElement(165, "textarea");
				renderTreeBuilder.AddAttribute(166, "class", "form-control");
				renderTreeBuilder.AddAttribute(167, "placeholder", "");
				renderTreeBuilder.AddAttribute(168, "maxlength", "1024");
				renderTreeBuilder.AddAttribute(169, "rows", "5");
				renderTreeBuilder.AddAttribute(170, "value", BindConverter.FormatValue(task.Body));
				renderTreeBuilder.AddAttribute(171, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					task.Body = __value;
				}, task.Body));
				renderTreeBuilder.SetUpdatesAttributeName("value");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(172, "\r\n            ");
				renderTreeBuilder.OpenElement(173, "div");
				renderTreeBuilder.AddAttribute(174, "class", "form-group col-3");
				renderTreeBuilder.AddMarkupContent(175, "<label class=\"form-label\">定时</label>\r\n                ");
				renderTreeBuilder.OpenElement(176, "select");
				renderTreeBuilder.AddAttribute(177, "oninput", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
				{
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					IntervalChanged(e.Value.ConvertTo<TaskInterval>());
				}));
				renderTreeBuilder.AddAttribute(178, "class", "form-control");
				renderTreeBuilder.AddAttribute(179, "value", BindConverter.FormatValue<TaskInterval>(task.Interval));
				renderTreeBuilder.AddAttribute(180, "onchange", EventCallback.Factory.CreateBinder(this, delegate(TaskInterval __value)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					task.Interval = __value;
				}, task.Interval));
				renderTreeBuilder.SetUpdatesAttributeName("value");
				renderTreeBuilder.OpenElement(181, "option");
				renderTreeBuilder.AddAttribute(182, "value", (object)(TaskInterval)1);
				renderTreeBuilder.AddMarkupContent(183, "秒");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(184, "\r\n                    ");
				renderTreeBuilder.OpenElement(185, "option");
				renderTreeBuilder.AddAttribute(186, "value", (object)(TaskInterval)11);
				renderTreeBuilder.AddMarkupContent(187, "每天");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(188, "\r\n                    ");
				renderTreeBuilder.OpenElement(189, "option");
				renderTreeBuilder.AddAttribute(190, "value", (object)(TaskInterval)12);
				renderTreeBuilder.AddMarkupContent(191, "每周");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(192, "\r\n                    ");
				renderTreeBuilder.OpenElement(193, "option");
				renderTreeBuilder.AddAttribute(194, "value", (object)(TaskInterval)13);
				renderTreeBuilder.AddMarkupContent(195, "每月");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(196, "\r\n                    ");
				renderTreeBuilder.OpenElement(197, "option");
				renderTreeBuilder.AddAttribute(198, "value", (object)(TaskInterval)21);
				renderTreeBuilder.AddContent(199, "Cron");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(200, "\r\n            ");
				renderTreeBuilder.OpenElement(201, "div");
				renderTreeBuilder.AddAttribute(202, "class", "form-group col-6");
				renderTreeBuilder.OpenElement(203, "label");
				renderTreeBuilder.AddAttribute(204, "class", "form-label");
				if ((int)task.Interval == 1)
				{
					renderTreeBuilder.AddContent(205, task.IntervalArgument + "秒");
				}
				if ((int)task.Interval == 11)
				{
					renderTreeBuilder.AddContent(206, "每天" + task.IntervalArgument);
				}
				if ((int)task.Interval == 12)
				{
					string[] array = task.IntervalArgument.Split(':');
					if (array.Length == 4)
					{
						renderTreeBuilder.AddContent(207, "每周" + (new string[7] { "日", "一", "二", "三", "四", "五", "六" })[array[0].ConvertTo<int>()] + " " + string.Join(":", array.Skip(1)));
					}
				}
				if ((int)task.Interval == 13)
				{
					string[] array2 = task.IntervalArgument.Split(':');
					if (array2.Length == 4)
					{
						renderTreeBuilder.AddContent(208, $"每月{((array2[0].ConvertTo<int>() > 0) ? "第" : "最后第")}{Math.Abs(array2[0].ConvertTo<int>())}天 {string.Join(":", array2.Skip(1))}");
					}
				}
				if ((int)task.Interval == 21)
				{
					renderTreeBuilder.AddContent(209, "Cron 表达式");
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(210, "\r\n                ");
				renderTreeBuilder.OpenElement(211, "input");
				renderTreeBuilder.AddAttribute(212, "oninput", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
				{
					task.IntervalArgument = e.Value.ConvertTo<string>();
				}));
				renderTreeBuilder.AddAttribute(213, "type", "text");
				renderTreeBuilder.AddAttribute(214, "class", "form-control");
				renderTreeBuilder.AddAttribute(215, "placeholder", "");
				renderTreeBuilder.AddAttribute(216, "value", BindConverter.FormatValue(task.IntervalArgument));
				renderTreeBuilder.AddAttribute(217, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					task.IntervalArgument = __value;
				}, task.IntervalArgument));
				renderTreeBuilder.SetUpdatesAttributeName("value");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(218, "\r\n            ");
				renderTreeBuilder.OpenElement(219, "div");
				renderTreeBuilder.AddAttribute(220, "class", "form-group col-3");
				renderTreeBuilder.AddMarkupContent(221, "<label class=\"form-label\">轮次</label>\r\n                ");
				renderTreeBuilder.OpenElement(222, "div");
				renderTreeBuilder.AddAttribute(223, "class", "float-end");
				renderTreeBuilder.OpenElement(224, "input");
				renderTreeBuilder.AddAttribute(225, "oninput", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
				{
					task.Round = ((!e.Value.ConvertTo<bool>()) ? 1 : (-1));
				}));
				renderTreeBuilder.AddAttribute(226, "id", "roundNever");
				renderTreeBuilder.AddAttribute(227, "type", "checkbox");
				renderTreeBuilder.AddAttribute(228, "checked", roundNever);
				renderTreeBuilder.AddAttribute(229, "class", "form-check-input");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(230, "<label for=\"roundNever\">永久</label>");
				renderTreeBuilder.CloseElement();
				if (task.Round == -1)
				{
					renderTreeBuilder.AddMarkupContent(231, "<input type=\"text\" class=\"form-control\" value=\"∞\" disabled=\"disabled\">");
				}
				else
				{
					renderTreeBuilder.OpenComponent<BootstrapInput<int>>(232);
					renderTreeBuilder.AddComponentParameter(233, "IsDisabled", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(task.Round == -1));
					renderTreeBuilder.AddComponentParameter(234, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(task.Round));
					renderTreeBuilder.AddComponentParameter(235, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(int __value)
					{
						task.Round = __value;
					}, task.Round))));
					renderTreeBuilder.AddComponentParameter(236, "ValueExpression", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<Expression<Func<int>>>(() => task.Round));
					renderTreeBuilder.CloseComponent();
				}
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			}
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(237, "\r\n\r\n");
		__builder.OpenComponent<NovaModal>(238);
		__builder.AddComponentParameter(239, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(taskLog != null));
		__builder.AddComponentParameter(240, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			taskLog = null;
		})));
		TaskInfo obj5 = logTask;
		__builder.AddComponentParameter(241, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【日志】" + ((obj5 != null) ? obj5.Topic : null)));
		__builder.AddComponentParameter(242, "IsBackdropStatic", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(243, "DialogClassName", "modal-xl");
		__builder.AddAttribute(244, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(245, "div");
			renderTreeBuilder.AddAttribute(246, "class", "card card-info card-outline");
			renderTreeBuilder.OpenElement(247, "div");
			renderTreeBuilder.AddAttribute(248, "class", "card-header d-block");
			renderTreeBuilder.OpenComponent<NovaAdminSearchFilter>(249);
			renderTreeBuilder.AddComponentParameter(250, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(qLog));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(251, "\r\n        ");
			renderTreeBuilder.OpenElement(252, "div");
			renderTreeBuilder.AddAttribute(253, "class", "card-header d-block");
			renderTreeBuilder.OpenElement(254, "button");
			renderTreeBuilder.AddAttribute(255, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => LoadTaskLog()));
			renderTreeBuilder.AddAttribute(256, "type", "button");
			renderTreeBuilder.AddAttribute(257, "class", "mr-2 btn btn-light");
			renderTreeBuilder.AddMarkupContent(258, "<i class=\"fas fa-sync-alt\"></i> 刷新");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(259, "\r\n        ");
			renderTreeBuilder.OpenElement(260, "div");
			renderTreeBuilder.AddAttribute(261, "class", "card-body p-0");
			renderTreeBuilder.AddAttribute(262, "style", "border:none;");
			renderTreeBuilder.OpenElement(263, "table");
			renderTreeBuilder.AddAttribute(264, "class", "table table-hover table-bordered table-sm m-0");
			renderTreeBuilder.AddMarkupContent(265, "<thead><tr><th>第几轮</th>\r\n                        <th>耗时</th>\r\n                        <th>成功</th>\r\n                        <th>异常信息</th>\r\n                        <th>备注</th>\r\n                        <th>时间</th></tr></thead>\r\n                ");
			renderTreeBuilder.OpenElement(266, "tbody");
			if (taskLog != null)
			{
				foreach (TaskLog log in taskLog.Logs)
				{
					renderTreeBuilder.OpenElement(267, "tr");
					renderTreeBuilder.OpenElement(268, "td");
					renderTreeBuilder.AddContent(269, log.Round);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(270, "\r\n                                ");
					renderTreeBuilder.OpenElement(271, "td");
					renderTreeBuilder.AddContent(272, log.ElapsedMilliseconds);
					renderTreeBuilder.AddContent(273, " ms");
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(274, "\r\n                                ");
					renderTreeBuilder.OpenElement(275, "td");
					if (log.Success)
					{
						renderTreeBuilder.AddMarkupContent(276, "<span class=\"text-success\">是</span>");
					}
					else
					{
						renderTreeBuilder.AddMarkupContent(277, "<span class=\"text-danger\">是</span>");
					}
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(278, "\r\n                                ");
					renderTreeBuilder.OpenElement(279, "td");
					renderTreeBuilder.AddAttribute(280, "style", "overflow-wrap:break-word;word-break:break-all;");
					renderTreeBuilder.AddContent(281, log.Exception);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(282, "\r\n                                ");
					renderTreeBuilder.OpenElement(283, "td");
					renderTreeBuilder.AddAttribute(284, "style", "overflow-wrap:break-word;word-break:break-all;");
					renderTreeBuilder.AddContent(285, log.Remark.Truncate(2048));
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(286, "\r\n                                ");
					renderTreeBuilder.OpenElement(287, "td");
					renderTreeBuilder.AddContent(288, log.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
				}
			}
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(289, "\r\n        ");
			renderTreeBuilder.OpenElement(290, "div");
			renderTreeBuilder.AddAttribute(291, "class", "card-footer");
			renderTreeBuilder.OpenComponent<NovaAdminPagination>(292);
			renderTreeBuilder.AddComponentParameter(293, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(qLog));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		});
		__builder.CloseComponent();
		ResultGetPage obj6 = loadResult;
		if (obj6 == null || obj6.Clusters?.Any() != true)
		{
			return;
		}
		__builder.OpenComponent<NovaModal>(294);
		__builder.AddComponentParameter(295, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(clusterLog != null));
		__builder.AddComponentParameter(296, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			clusterLog = null;
		})));
		__builder.AddComponentParameter(297, "Title", "集群日志");
		__builder.AddComponentParameter(298, "IsBackdropStatic", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(299, "DialogClassName", "modal-xl");
		__builder.AddAttribute(300, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(301, "div");
			renderTreeBuilder.AddAttribute(302, "class", "card card-info card-outline");
			renderTreeBuilder.OpenElement(303, "div");
			renderTreeBuilder.AddAttribute(304, "class", "card-header d-block");
			renderTreeBuilder.OpenComponent<NovaAdminSearchFilter>(305);
			renderTreeBuilder.AddComponentParameter(306, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(qCluster));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(307, "\r\n            ");
			renderTreeBuilder.OpenElement(308, "div");
			renderTreeBuilder.AddAttribute(309, "class", "card-header d-block");
			renderTreeBuilder.OpenElement(310, "button");
			renderTreeBuilder.AddAttribute(311, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => LoadClusterLog()));
			renderTreeBuilder.AddAttribute(312, "type", "button");
			renderTreeBuilder.AddAttribute(313, "class", "mr-2 btn btn-light");
			renderTreeBuilder.AddMarkupContent(314, "<i class=\"fas fa-sync-alt\"></i> 刷新");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(315, "\r\n            ");
			renderTreeBuilder.OpenElement(316, "div");
			renderTreeBuilder.AddAttribute(317, "class", "card-body p-0");
			renderTreeBuilder.AddAttribute(318, "style", "border:none;");
			renderTreeBuilder.OpenElement(319, "table");
			renderTreeBuilder.AddAttribute(320, "class", "table table-hover table-bordered table-sm m-0");
			renderTreeBuilder.AddMarkupContent(321, "<thead><tr><th>时间</th>\r\n                            <th>集群Id</th>\r\n                            <th>集群名称</th>\r\n                            <th>内容</th></tr></thead>\r\n                    ");
			renderTreeBuilder.OpenElement(322, "tbody");
			if (clusterLog != null)
			{
				foreach (ClusterLog log2 in clusterLog.Logs)
				{
					renderTreeBuilder.OpenElement(323, "tr");
					renderTreeBuilder.OpenElement(324, "td");
					renderTreeBuilder.AddContent(325, log2.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(326, "\r\n                                    ");
					renderTreeBuilder.OpenElement(327, "td");
					renderTreeBuilder.AddContent(328, log2.ClusterId);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(329, "\r\n                                    ");
					renderTreeBuilder.OpenElement(330, "td");
					renderTreeBuilder.AddContent(331, log2.ClusterName);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.AddMarkupContent(332, "\r\n                                    ");
					renderTreeBuilder.OpenElement(333, "td");
					renderTreeBuilder.AddAttribute(334, "style", "overflow-wrap:break-word;word-break:break-all;");
					renderTreeBuilder.AddContent(335, log2.Message);
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
				}
			}
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(336, "\r\n            ");
			renderTreeBuilder.OpenElement(337, "div");
			renderTreeBuilder.AddAttribute(338, "class", "card-footer");
			renderTreeBuilder.OpenComponent<NovaAdminPagination>(339);
			renderTreeBuilder.AddComponentParameter(340, "AdminQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(qCluster));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
		});
		__builder.CloseComponent();
	}

	protected override void OnInitialized()
	{
		q.Filters = new NovaAdminFilterInfo[2]
		{
			new NovaAdminFilterInfo("集群", "ClusterId", multiple: false, 12, "", ""),
			new NovaAdminFilterInfo("状态", "Status", multiple: false, 12, "运行中,暂停中,已结束", "0,1,2")
		};
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await fsql.Select<SysDict>().Where((Expression<Func<SysDict, bool>>)((SysDict a) => a.Parent.Name == "TaskScheduler_GroupNames")).OrderBy<int>((Expression<Func<SysDict, int>>)((SysDict a) => a.Sort))
				.ToListAsync<string>((Expression<Func<SysDict, string>>)((SysDict a) => a.Name), default(CancellationToken));
			q.InvokeQueryAsync = Load;
			await Load();
			await JS.InvokeVoidAsync("novaAdminJS.initializeAdvancedTable", _theadElement, 2, 0);
		}
	}

	private async Task Load()
	{
		isShowLoader = true;
		StateHasChanged();
		loadResult = Datafeed.GetPage(scheduler, q.Filters[0].HasValue ? q.Filters[0].Value<string>() : null, q.SearchText, q.Filters[1].HasValue ? q.Filters[1].Value<TaskStatus?>() : ((TaskStatus?)null), (DateTime?)null, (DateTime?)null, q.PageSize, q.PageNumber);
		q.Total = loadResult.Total;
		if (loadResult.Clusters?.Any() ?? false)
		{
			q.Filters[0] = new NovaAdminFilterInfo("集群", "ClusterId", multiple: false, 12, string.Join(",", loadResult.Clusters.Select((ClusterInfo a) => $"{a.Name}({a.TaskCount})")), string.Join(",", loadResult.Clusters.Select((ClusterInfo a) => a.Id)));
		}
		isLoaded = true;
		await Task.Delay(200);
		isShowLoader = false;
		StateHasChanged();
		await Task.Yield();
	}

	private async Task OnSave()
	{
		Datafeed.AddTask(scheduler, task.Topic, task.Body, task.Round, task.Interval, task.IntervalArgument);
		task = null;
		await q.InvokeQueryAsync();
	}

	[NovaButton("add")]
	private async Task BeginAdd()
	{
		await _0024Rougamo_BeginAdd();
	}

	private void IntervalChanged(TaskInterval interval)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		task.Interval = interval;
		TaskInterval interval2 = task.Interval;
		TaskInterval val = interval2;
		if ((int)val != 1)
		{
			switch ((int)val - 11)
			{
			case 0:
				task.IntervalArgument = "22:00:00";
				return;
			case 1:
				task.IntervalArgument = "5:22:00:00";
				return;
			case 2:
				task.IntervalArgument = "1:22:00:00";
				return;
			}
			if ((int)val == 21)
			{
				task.IntervalArgument = "* * * * * *";
			}
		}
		else
		{
			task.IntervalArgument = "60";
		}
	}

	private async Task OnTemplateChanged(IEnumerable<SelectedItem> items, string value)
	{
		if (!value.IsNull())
		{
			switch (value)
			{
			case "0":
				task.Topic = "";
				task.Body = "";
				break;
			case "1":
				task.Topic = "[系统预留]Http请求";
				task.Body = "{\r\n\t\"method\": \"get\",\r\n\t\"url\": \"https://freesql.net/guide/freescheduler.html\",\r\n\t\"timeout\": \"30\",\r\n\t\"header\":\r\n\t{\r\n\t\t\"Content-Type\": \"application/json\",\r\n\t},\r\n\t\"body\": \"{}\",\r\n}";
				break;
			case "2":
				task.Topic = "[系统预留]清理任务数据";
				task.Body = "86400";
				break;
			}
			StateHasChanged();
			await Task.Yield();
		}
	}

	[NovaButton("remove")]
	private async Task RemoveTask(TaskInfo task)
	{
		await _0024Rougamo_RemoveTask(task);
	}

	[NovaButton("resume")]
	private async Task ResumeTask(TaskInfo task)
	{
		await _0024Rougamo_ResumeTask(task);
	}

	[NovaButton("pause")]
	private async Task PauseTask(TaskInfo task)
	{
		await _0024Rougamo_PauseTask(task);
	}

	[NovaButton("runnow")]
	private async Task RunNowTask(TaskInfo task)
	{
		await _0024Rougamo_RunNowTask(task);
	}

	[NovaButton("tasklog")]
	private async Task LoadTaskLog(TaskInfo task = null)
	{
		await _0024Rougamo_LoadTaskLog(task);
	}

	[NovaButton("clusterlog")]
	private async Task LoadClusterLog()
	{
		await _0024Rougamo_LoadClusterLog();
	}

	private async Task _0024Rougamo_BeginAdd()
	{
		task = new TaskInfo();
		if ((int)task.Interval == 0)
		{
			IntervalChanged((TaskInterval)1);
		}
		if (task.Round == 0)
		{
			task.Round = -1;
		}
		roundNever = task.Round < 0;
		await Task.Yield();
	}

	private async Task _0024Rougamo_RemoveTask(TaskInfo task)
	{
		if (await JS.Confirm("确定删除任务吗？"))
		{
			scheduler.RemoveTask(task.Id);
			await Load();
		}
	}

	private async Task _0024Rougamo_ResumeTask(TaskInfo task)
	{
		scheduler.ResumeTask(task.Id);
		await Load();
	}

	private async Task _0024Rougamo_PauseTask(TaskInfo task)
	{
		scheduler.PauseTask(task.Id);
		await Load();
	}

	private async Task _0024Rougamo_RunNowTask(TaskInfo task)
	{
		scheduler.RunNowTask(task.Id);
		await Load();
	}

	private async Task _0024Rougamo_LoadTaskLog([Optional] TaskInfo task)
	{
		if (qLog.InvokeQueryAsync == null)
		{
			qLog.InvokeQueryAsync = () => LoadTaskLog(logTask);
		}
		if (task != null)
		{
			logTask = task;
		}
		taskLog = Datafeed.GetLogs(scheduler, logTask.Id, qLog.PageSize, qLog.PageNumber);
		qLog.Total = taskLog.Total;
		StateHasChanged();
		await Task.Yield();
	}

	private async Task _0024Rougamo_LoadClusterLog()
	{
		if (qLog.InvokeQueryAsync == null)
		{
			qLog.InvokeQueryAsync = LoadClusterLog;
		}
		clusterLog = Datafeed.GetClusterLogs(scheduler, qCluster.PageSize, qCluster.PageNumber);
		qCluster.Total = clusterLog.Total;
		StateHasChanged();
		await Task.Yield();
	}
}
