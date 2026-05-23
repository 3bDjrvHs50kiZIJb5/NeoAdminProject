using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Rougamo;
using Rougamo.Context;
using __Blazor.NoAdmin.Blazor.Pages.User;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/User")]
public class User : ComponentBase
{
	private List<NovaAdminItem<SysOrg>> selectedOrgs;

	private NovaAdminQueryInfo queryUser;

	private bool queryUserUsedSelectedOrgs;

	private SysUser allocItemRoles;

	private bool showUserLogin;

	[Inject]
	private IAggregateRootRepository<SysUser> repo { get; set; }

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
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "用户");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<Split>(4);
		__builder.AddComponentParameter(5, "ShowBarHandle", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(6, "Basis", "22%");
		__builder.AddComponentParameter(7, "FirstPaneMinimumSize", "330px");
		__builder.AddAttribute(8, "FirstPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysOrg>>(9);
			renderTreeBuilder.AddComponentParameter(10, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(-1));
			renderTreeBuilder.AddComponentParameter(11, "IsSearchText", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(12, "IsRemove", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(13, "IsEdit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(14, "IsAdd", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(15, "IsExportExcel", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(16, "IsMultiSelect", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(17, "IsShowLoading", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
			renderTreeBuilder.AddComponentParameter(18, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysOrg>>)OnQueryOrg)));
			renderTreeBuilder.AddComponentParameter(19, "OnRowClick", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<List<NovaAdminItem<SysOrg>>, Task>)OrgRowClick)));
			renderTreeBuilder.AddAttribute(20, "CardHeader", (RenderFragment)delegate
			{
			});
			renderTreeBuilder.AddAttribute(21, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(22, "<th width=\"52\">类型</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(23, "<th width=\"36\">排序</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(24, "<th width=\"36\">可用</th>");
			});
			renderTreeBuilder.AddAttribute(25, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(26, "组织名称");
			});
			renderTreeBuilder.AddAttribute(27, "TableTd1", (RenderFragment<SysOrg>)((SysOrg item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddContent(28, item.Label);
			}));
			renderTreeBuilder.AddAttribute(29, "TableRow", (RenderFragment<SysOrg>)((SysOrg item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(30, "td");
				renderTreeBuilder2.AddContent(31, item.Type);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(32, "\r\n                ");
				renderTreeBuilder2.OpenElement(33, "td");
				renderTreeBuilder2.AddContent(34, item.Sort);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(35, "\r\n                ");
				renderTreeBuilder2.OpenElement(36, "td");
				renderTreeBuilder2.AddContent(37, item.IsEnabled ? "-" : "否");
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.AddAttribute(38, "SecondPaneTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysUser>>(39);
			renderTreeBuilder.AddComponentParameter(40, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(30));
			renderTreeBuilder.AddComponentParameter(41, "TableTd99Width", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(175));
			renderTreeBuilder.AddComponentParameter(42, "FixedRightColumns", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(1));
			renderTreeBuilder.AddComponentParameter(43, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysUser>>)OnQueryUser)));
			renderTreeBuilder.AddComponentParameter(44, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQueryUser));
			renderTreeBuilder.AddComponentParameter(45, "OnEdit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysUser, Task>)OnEdit)));
			renderTreeBuilder.AddComponentParameter(46, "OnRemoving", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysUser>>, Task>)OnRemoving)));
			renderTreeBuilder.AddComponentParameter(47, "SearchPlaceholder", "账号/姓名..");
			renderTreeBuilder.AddAttribute(48, "CardHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(49, "button");
				renderTreeBuilder2.AddAttribute(50, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)ShowUserLogin));
				renderTreeBuilder2.AddAttribute(51, "type", "button");
				renderTreeBuilder2.AddAttribute(52, "class", "mr-2 btn btn-light");
				renderTreeBuilder2.AddMarkupContent(53, "<i class=\"fas fa-camera\"></i> 登陆日志");
				renderTreeBuilder2.CloseElement();
			});
			renderTreeBuilder.AddAttribute(54, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(55, "<th>账号</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(56, "<th>姓名</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(57, "<th>组织</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(58, "<th>角色</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(59, "<th>可用</th>\r\n                ");
				renderTreeBuilder2.AddMarkupContent(60, "<th>备注</th>\r\n                ");
				renderTreeBuilder2.OpenElement(61, "th");
				renderTreeBuilder2.OpenComponent<NovaAdminSort>(62);
				renderTreeBuilder2.AddComponentParameter(63, "Text", "登陆时间");
				renderTreeBuilder2.AddComponentParameter(64, "Value", "LoginTime");
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(65, "\r\n                ");
				renderTreeBuilder2.OpenElement(66, "th");
				renderTreeBuilder2.OpenComponent<NovaAdminSort>(67);
				renderTreeBuilder2.AddComponentParameter(68, "Text", "创建时间");
				renderTreeBuilder2.AddComponentParameter(69, "Value", "CreatedTime");
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
			});
			renderTreeBuilder.AddAttribute(70, "TableRow", (RenderFragment<SysUser>)((SysUser item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(71, "td");
				renderTreeBuilder2.AddContent(72, item.Username);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(73, "\r\n                ");
				renderTreeBuilder2.OpenElement(74, "td");
				renderTreeBuilder2.AddContent(75, item.Nickname);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(76, "\r\n                ");
				renderTreeBuilder2.OpenElement(77, "td");
				renderTreeBuilder2.AddContent(78, item.Org?.Label);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(79, "\r\n                ");
				renderTreeBuilder2.OpenElement(80, "td");
				renderTreeBuilder2.AddContent(81, string.Join(", ", item.Roles.Select((SysRole a) => a.Name)));
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(82, "\r\n                ");
				renderTreeBuilder2.OpenElement(83, "td");
				renderTreeBuilder2.AddContent(84, item.IsEnabled ? "-" : "否");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(85, "\r\n                ");
				renderTreeBuilder2.OpenElement(86, "td");
				renderTreeBuilder2.AddContent(87, item.Description);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(88, "\r\n                ");
				renderTreeBuilder2.OpenElement(89, "td");
				renderTreeBuilder2.AddContent(90, (item.LoginTime > DateTime.UnixEpoch) ? item.LoginTime.ToString("yyyy-MM-dd HH:mm:ss") : "-");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(91, "\r\n                ");
				renderTreeBuilder2.OpenElement(92, "td");
				renderTreeBuilder2.AddContent(93, item.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.AddAttribute(94, "TableTd99", (RenderFragment<SysUser>)((SysUser item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(95, "button");
				renderTreeBuilder2.AddAttribute(96, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginAllocRoles(item)));
				renderTreeBuilder2.AddAttribute(97, "type", "button");
				renderTreeBuilder2.AddAttribute(98, "class", "mr-2 btn btn-light btn-xs");
				renderTreeBuilder2.AddAttribute(99, "disabled", item.IsSystem);
				renderTreeBuilder2.AddMarkupContent(100, "<i class=\"fa fa-user-secret\"></i>分配角色");
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.AddAttribute(101, "EditTemplate", (RenderFragment<SysUser>)((SysUser item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(102, "div");
				renderTreeBuilder2.AddAttribute(103, "class", "row");
				renderTreeBuilder2.OpenElement(104, "div");
				renderTreeBuilder2.AddAttribute(105, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(106, "<label class=\"form-label\">账号</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(107, "input");
				renderTreeBuilder2.AddAttribute(108, "type", "text");
				renderTreeBuilder2.AddAttribute(109, "class", "form-control");
				renderTreeBuilder2.AddAttribute(110, "placeholder", "");
				renderTreeBuilder2.AddAttribute(111, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(112, "value", BindConverter.FormatValue(item.Username));
				renderTreeBuilder2.AddAttribute(113, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Username = __value;
				}, item.Username));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(114, "\r\n                    ");
				renderTreeBuilder2.OpenElement(115, "div");
				renderTreeBuilder2.AddAttribute(116, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(117, "<label class=\"form-label\">密码</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(118, "input");
				renderTreeBuilder2.AddAttribute(119, "type", "text");
				renderTreeBuilder2.AddAttribute(120, "class", "form-control");
				renderTreeBuilder2.AddAttribute(121, "placeholder", "");
				renderTreeBuilder2.AddAttribute(122, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(123, "value", BindConverter.FormatValue(item.Password));
				renderTreeBuilder2.AddAttribute(124, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Password = __value;
				}, item.Password));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(125, "\r\n                    ");
				renderTreeBuilder2.OpenElement(126, "div");
				renderTreeBuilder2.AddAttribute(127, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(128, "<label class=\"form-label\">姓名</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(129, "input");
				renderTreeBuilder2.AddAttribute(130, "type", "text");
				renderTreeBuilder2.AddAttribute(131, "class", "form-control");
				renderTreeBuilder2.AddAttribute(132, "placeholder", "");
				renderTreeBuilder2.AddAttribute(133, "maxlength", "50");
				renderTreeBuilder2.AddAttribute(134, "value", BindConverter.FormatValue(item.Nickname));
				renderTreeBuilder2.AddAttribute(135, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Nickname = __value;
				}, item.Nickname));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(136, "\r\n                    ");
				renderTreeBuilder2.OpenElement(137, "div");
				renderTreeBuilder2.AddAttribute(138, "class", "form-group col-6");
				renderTreeBuilder2.AddMarkupContent(139, "<label class=\"form-label\">所属组织</label>\r\n                        ");
				renderTreeBuilder2.OpenComponent<NovaInputTable<SysOrg, long>>(140);
				renderTreeBuilder2.AddComponentParameter(141, "DisplayText", (Func<SysOrg, string>)((SysOrg a) => $"[{a.Id}]{a.Label}"));
				renderTreeBuilder2.AddComponentParameter(142, "IsSearchText", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
				renderTreeBuilder2.AddComponentParameter(143, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(NovaAdminQueryEventArgs<SysOrg> e)
				{
					e.Select.Where((Expression<Func<SysOrg, bool>>)((SysOrg a) => a.IsEnabled)).OrderBy<int>((Expression<Func<SysOrg, int>>)((SysOrg a) => a.Sort));
				})));
				renderTreeBuilder2.AddComponentParameter(144, "Value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(item.OrgId));
				renderTreeBuilder2.AddComponentParameter(145, "ValueChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(long __value)
				{
					item.OrgId = __value;
				}, item.OrgId))));
				renderTreeBuilder2.AddComponentParameter(146, "Item", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(item.Org));
				renderTreeBuilder2.AddComponentParameter(147, "ItemChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysOrg __value)
				{
					item.Org = __value;
				}, item.Org))));
				renderTreeBuilder2.AddAttribute(148, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
				{
					renderTreeBuilder3.AddMarkupContent(149, "组织名称");
				});
				renderTreeBuilder2.AddAttribute(150, "TableTd1", (RenderFragment<SysOrg>)((SysOrg sysOrg) => delegate(RenderTreeBuilder renderTreeBuilder3)
				{
					renderTreeBuilder3.AddContent(151, sysOrg.Label);
				}));
				renderTreeBuilder2.AddAttribute(152, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder3)
				{
					renderTreeBuilder3.AddMarkupContent(153, "<th>组织类型</th>\r\n                                ");
					renderTreeBuilder3.AddMarkupContent(154, "<th>排序</th>\r\n                                ");
					renderTreeBuilder3.AddMarkupContent(155, "<th>备注</th>");
				});
				renderTreeBuilder2.AddAttribute(156, "TableRow", (RenderFragment<SysOrg>)((SysOrg sysOrg) => delegate(RenderTreeBuilder renderTreeBuilder3)
				{
					renderTreeBuilder3.OpenElement(157, "td");
					renderTreeBuilder3.AddContent(158, sysOrg.Type);
					renderTreeBuilder3.CloseElement();
					renderTreeBuilder3.AddMarkupContent(159, "\r\n                                ");
					renderTreeBuilder3.OpenElement(160, "td");
					renderTreeBuilder3.AddContent(161, sysOrg.Sort);
					renderTreeBuilder3.CloseElement();
					renderTreeBuilder3.AddMarkupContent(162, "\r\n                                ");
					renderTreeBuilder3.OpenElement(163, "td");
					renderTreeBuilder3.AddContent(164, sysOrg.Description);
					renderTreeBuilder3.CloseElement();
				}));
				renderTreeBuilder2.CloseComponent();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(165, "\r\n                    ");
				renderTreeBuilder2.OpenElement(166, "div");
				renderTreeBuilder2.AddAttribute(167, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(168, "<label class=\"form-label\">是否可用</label>\r\n                        ");
				TypeInference.CreateCheckbox_0(renderTreeBuilder2, 169, 170, item.IsEnabled, 171, EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
				{
					item.IsEnabled = __value;
				}, item.IsEnabled)), 172, () => item.IsEnabled);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(173, "\r\n                    ");
				renderTreeBuilder2.OpenElement(174, "div");
				renderTreeBuilder2.AddAttribute(175, "class", "form-group col-12");
				renderTreeBuilder2.AddMarkupContent(176, "<label class=\"form-label\">备注</label>\r\n                        ");
				renderTreeBuilder2.OpenElement(177, "textarea");
				renderTreeBuilder2.AddAttribute(178, "class", "form-control");
				renderTreeBuilder2.AddAttribute(179, "placeholder", "");
				renderTreeBuilder2.AddAttribute(180, "maxlength", "500");
				renderTreeBuilder2.AddAttribute(181, "style", "height:80px");
				renderTreeBuilder2.AddAttribute(182, "value", BindConverter.FormatValue(item.Description));
				renderTreeBuilder2.AddAttribute(183, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					item.Description = __value;
				}, item.Description));
				renderTreeBuilder2.SetUpdatesAttributeName("value");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(184, "\r\n\r\n\r\n");
		__builder.OpenComponent<NovaAllocTable<SysUser, SysRole>>(185);
		__builder.AddComponentParameter(186, "ChildProperty", "Roles");
		__builder.AddComponentParameter(187, "Title", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck("【分配角色】" + allocItemRoles?.Username));
		__builder.AddComponentParameter(188, "IsNotifyChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
		__builder.AddComponentParameter(189, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(50));
		__builder.AddComponentParameter(190, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(NovaAdminQueryEventArgs<SysRole> e)
		{
			e.Select.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysRole, bool>>)((SysRole a) => a.Name.Contains(e.SearchText)));
		})));
		__builder.AddComponentParameter(191, "Item", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(allocItemRoles));
		__builder.AddComponentParameter(192, "ItemChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysUser __value)
		{
			allocItemRoles = __value;
		}, allocItemRoles))));
		__builder.AddAttribute(193, "TableTd1", (RenderFragment<SysRole>)((SysRole context) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(194, context.Name);
		}));
		__builder.CloseComponent();
		__builder.AddMarkupContent(195, "\r\n\r\n");
		__builder.OpenComponent<NovaModal>(196);
		__builder.AddComponentParameter(197, "Visible", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(showUserLogin));
		__builder.AddComponentParameter(198, "Title", "登陆日志");
		__builder.AddComponentParameter(199, "OnClose", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<object>)delegate
		{
			showUserLogin = false;
		})));
		__builder.AddComponentParameter(200, "DialogClassName", "modal-xxl");
		__builder.AddComponentParameter(201, "IsBackdropStatic", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
		__builder.AddAttribute(202, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenComponent<NovaAdminTable<SysUserLoginLog>>(203);
			renderTreeBuilder.AddComponentParameter(204, "PageSize", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(20));
			renderTreeBuilder.AddComponentParameter(205, "IsAdd", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(206, "IsEdit", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(207, "IsRemove", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(208, "IsSingleSelect", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(209, "IsMultiSelect", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(210, "IsQueryString", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(211, "OnQuery", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysUserLoginLog>>)OnQueryUserLogin)));
			renderTreeBuilder.AddComponentParameter(212, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQueryUserLogin));
			renderTreeBuilder.AddAttribute(213, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(214, "<th>登陆时间</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(215, "<th>用户名</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(216, "<th>日志类型</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(217, "<th>IP</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(218, "<th>地点</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(219, "<th>操作系统</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(220, "<th>设备类型</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(221, "<th>浏览器</th>\r\n            ");
				renderTreeBuilder2.AddMarkupContent(222, "<th>浏览器语言</th>");
			});
			renderTreeBuilder.AddAttribute(223, "TableRow", (RenderFragment<SysUserLoginLog>)((SysUserLoginLog item) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
				renderTreeBuilder2.OpenElement(224, "td");
				renderTreeBuilder2.AddContent(225, item.LoginTime.ToString("yyyy-MM-dd HH:mm:ss"));
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(226, "\r\n            ");
				renderTreeBuilder2.OpenElement(227, "td");
				renderTreeBuilder2.AddContent(228, item.Username);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(229, "\r\n            ");
				renderTreeBuilder2.OpenElement(230, "td");
				renderTreeBuilder2.AddContent(231, item.Type);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(232, "\r\n            ");
				renderTreeBuilder2.OpenElement(233, "td");
				renderTreeBuilder2.AddContent(234, item.Ip);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(235, "\r\n            ");
				renderTreeBuilder2.OpenElement(236, "td");
				renderTreeBuilder2.AddContent(237, item.City);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(238, "\r\n            ");
				renderTreeBuilder2.OpenElement(239, "td");
				renderTreeBuilder2.AddContent(240, item.OS);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(241, "\r\n            ");
				renderTreeBuilder2.OpenElement(242, "td");
				renderTreeBuilder2.AddContent(243, item.Device);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(244, "\r\n            ");
				renderTreeBuilder2.OpenElement(245, "td");
				renderTreeBuilder2.AddContent(246, item.Browser);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(247, "\r\n            ");
				renderTreeBuilder2.OpenElement(248, "td");
				renderTreeBuilder2.AddContent(249, item.Language);
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
		});
		__builder.CloseComponent();
	}

	private void OnQueryOrg(NovaAdminQueryEventArgs<SysOrg> e)
	{
		selectedOrgs = null;
		if (queryUserUsedSelectedOrgs && queryUser?.InvokeQueryAsync != null)
		{
			InvokeAsync(queryUser.InvokeQueryAsync);
		}
		e.Select.Where((Expression<Func<SysOrg, bool>>)((SysOrg a) => a.IsEnabled)).OrderBy<int>((Expression<Func<SysOrg, int>>)((SysOrg a) => a.Sort));
	}

	private async Task OrgRowClick(List<NovaAdminItem<SysOrg>> relateItems)
	{
		if (selectedOrgs?.FirstOrDefault() != relateItems[0])
		{
			if (selectedOrgs != null)
			{
				selectedOrgs[0].RowClass = "";
			}
			relateItems[0].RowClass = "active";
			selectedOrgs = relateItems;
			if (queryUser?.InvokeQueryAsync != null)
			{
				await queryUser.InvokeQueryAsync();
			}
		}
	}

	private async Task InitQueryUser(NovaAdminQueryInfo e)
	{
		queryUser = e;
		e.Filters = new NovaAdminFilterInfo[0];
		await Task.Yield();
	}

	private void OnQueryUser(NovaAdminQueryEventArgs<SysUser> e)
	{
		queryUserUsedSelectedOrgs = selectedOrgs?.Any() ?? false;
		ISelect<SysUser> obj = e.Select.IncludeMany<SysRole>((Expression<Func<SysUser, IEnumerable<SysRole>>>)((SysUser a) => a.Roles), (Action<ISelect<SysRole>>)null).Include<SysOrg>((Expression<Func<SysUser, SysOrg>>)((SysUser a) => a.Org)).WhereIf(queryUserUsedSelectedOrgs, (Expression<Func<SysUser, bool>>)((SysUser a) => selectedOrgs.Select((NovaAdminItem<SysOrg> org) => org.Value.Id).Contains(a.OrgId)))
			.WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysUser, bool>>)((SysUser a) => a.Username.Contains(e.SearchText) || a.Nickname.Contains(e.SearchText)));
		bool num = !e.Sort.IsNull();
		string obj2 = e.Sort?.Replace("@desc", "");
		string sort = e.Sort;
		((ISelect0<ISelect<SysUser>, SysUser>)(object)obj).OrderByPropertyNameIf(num, obj2, sort == null || !sort.Contains("@desc"));
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysUser>> e)
	{
		if (e.Argument.Any((SysUser a) => a.Id == admin.User.Id))
		{
			await MessageService.Error("不能删除当前登陆的账号!");
			e.Cancel = true;
		}
	}

	private async Task OnEdit(SysUser item)
	{
		if (item.Id == 0L && selectedOrgs != null)
		{
			item.Org = selectedOrgs[0].Value;
			item.OrgId = selectedOrgs[0].Value.Id;
		}
		await Task.Yield();
	}

	[NovaButton("alloc_roles")]
	private async Task BeginAllocRoles(SysUser item)
	{
		await _0024Rougamo_BeginAllocRoles(item);
	}

	[NovaButton("loginlog")]
	private async Task ShowUserLogin()
	{
		await _0024Rougamo_ShowUserLogin();
	}

	private async Task InitQueryUserLogin(NovaAdminQueryInfo e)
	{
		e.Filters = new NovaAdminFilterInfo[2]
		{
			new NovaAdminFilterInfo("日志", "Type", "登陆成功,登陆失败", "0,1"),
			new NovaAdminFilterInfo("设备", "Device", "PC,Mobile,Tablet", "0,1,2")
		};
		await Task.Yield();
	}

	private void OnQueryUserLogin(NovaAdminQueryEventArgs<SysUserLoginLog> e)
	{
		e.Select.WhereIf(e.Filters[0].HasValue, (Expression<Func<SysUserLoginLog, bool>>)((SysUserLoginLog a) => (int)a.Type == (int)e.Filters[0].Value<SysUserLoginLog.LogType>())).WhereIf(e.Filters[1].HasValue, (Expression<Func<SysUserLoginLog, bool>>)((SysUserLoginLog a) => (int)a.Device == (int)e.Filters[1].Value<WebClientDeviceType>())).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysUserLoginLog, bool>>)((SysUserLoginLog a) => a.Ip.Contains(e.SearchText) || a.Username.Contains(e.SearchText)))
			.OrderByDescending<long>((Expression<Func<SysUserLoginLog, long>>)((SysUserLoginLog a) => a.Id));
	}

	private async Task _0024Rougamo_BeginAllocRoles(SysUser item)
	{
		allocItemRoles = item;
		await Task.Yield();
	}

	private async Task _0024Rougamo_ShowUserLogin()
	{
		showUserLogin = true;
		await Task.Yield();
	}
}
