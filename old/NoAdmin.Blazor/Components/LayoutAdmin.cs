using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Rougamo;
using Rougamo.Context;
using __Blazor.NoAdmin.Blazor.Components.LayoutAdmin;

namespace NoAdmin.Blazor.Components;

public class LayoutAdmin : LayoutComponentBase
{
	private bool isInitialized = false;

	private List<SysMenu> menus;

	private string currentPath;

	private string currentQuery;

	private long activePrimaryMenuId;

	private HashSet<long> activeSecondaryMenuIds = new HashSet<long>();

	private SysMenu activeItem;

	private DateTime lastRefreshTime;

	private EventHandler<LocationChangedEventArgs> locationChangedEvent;

	[CascadingParameter]
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
		//IL_1938: Unknown result type (might be due to invalid IL or missing references)
		//IL_19f0: Unknown result type (might be due to invalid IL or missing references)
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(2, admin.Tenant?.Title);
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenElement(4, "div");
		__builder.AddAttribute(5, "class", "app-wrapper");
		if (!isInitialized)
		{
			__builder.AddMarkupContent(6, "<div class=\"c1-auth-mask\"><div class=\"c1-auth-mask-content\"><div class=\"c1-auth-spinner\"></div>\r\n                <div class=\"c1-auth-text\">安全环境检测中...</div></div></div>");
		}
		__builder.OpenElement(7, "aside");
		__builder.AddAttribute(8, "class", "app-sidebar bg-body-secondary shadow " + (NovaAdminExtensions.Options.IsSampleLayout ? "sidebar-sample-layout" : ""));
		__builder.AddAttribute(9, "data-bs-theme", "dark");
		__builder.OpenElement(10, "div");
		__builder.AddAttribute(11, "class", "sidebar-brand");
		__builder.OpenElement(12, "a");
		__builder.AddAttribute(13, "href", "/");
		__builder.AddAttribute(14, "class", "brand-link");
		__builder.OpenElement(15, "img");
		__builder.AddAttribute(16, "src", admin.Tenant?.Logo ?? "logo.png");
		__builder.AddAttribute(17, "class", "brand-image opacity-75 shadow");
		__builder.CloseElement();
		__builder.AddMarkupContent(18, "\r\n                ");
		__builder.OpenElement(19, "span");
		__builder.AddAttribute(20, "class", "brand-text fw-light");
		__builder.AddContent(21, admin.Tenant?.Title);
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
		if (NovaAdminExtensions.Options.IsSampleLayout)
		{
			__builder.OpenElement(22, "div");
			__builder.AddAttribute(23, "class", "c1-nav-sample-container scroll");
			__builder.OpenElement(24, "nav");
			__builder.AddAttribute(25, "class", "c1-nav-sample-menu");
			__builder.OpenElement(26, "ul");
			__builder.AddAttribute(27, "class", "c1-nav-list");
			if (menus != null)
			{
				foreach (SysMenu menu1 in menus.Where((SysMenu a) => a.ParentId == 0L && !a.IsHidden && a.Type != SysMenuType.按钮))
				{
					List<SysMenu> list = ((menu1.Type != SysMenuType.外部连接) ? menus.Where((SysMenu m) => m.ParentId == menu1.Id && !m.IsHidden && m.Type != SysMenuType.按钮).ToList() : new List<SysMenu>());
					string text = ((menu1.SidebarStyle == SysMenuSidebarStyle.展开 || menu1.Id == activePrimaryMenuId) ? "menu-open" : "");
					__builder.OpenElement(28, "li");
					__builder.AddAttribute(29, "class", "c1-nav-item " + text);
					__builder.OpenElement(30, "a");
					__builder.AddAttribute(31, "href", list.Any() ? "javascript:void(0)" : menu1.Path);
					__builder.AddAttribute(32, "class", "c1-nav-link " + ((!list.Any() && activePrimaryMenuId == menu1.Id) ? "active" : ""));
					__builder.AddAttribute(33, "onclick", list.Any() ? "novaAdminJS.toggleSidebarMenuItem(this)" : null);
					__builder.AddAttribute(34, "target", (menu1.Type == SysMenuType.外部连接) ? "_blank" : null);
					__builder.OpenElement(35, "i");
					__builder.AddAttribute(36, "class", "nav-icon fas " + menu1.Icon.IsNull("fa-bars"));
					__builder.CloseElement();
					__builder.AddMarkupContent(37, "\r\n                                        ");
					__builder.OpenElement(38, "p");
					__builder.AddContent(39, menu1.Label);
					__builder.CloseElement();
					if (list.Any())
					{
						__builder.AddMarkupContent(40, "<i class=\"nav-arrow fas fa-angle-left\"></i>");
					}
					__builder.CloseElement();
					if (list.Any())
					{
						__builder.OpenElement(41, "ul");
						__builder.AddAttribute(42, "class", "c1-nav-submenu");
						foreach (SysMenu menu2 in list)
						{
							List<SysMenu> list2 = ((menu2.Type != SysMenuType.外部连接) ? menus.Where((SysMenu m) => m.ParentId == menu2.Id && !m.IsHidden && m.Type != SysMenuType.按钮).ToList() : new List<SysMenu>());
							string text2 = ((menu2.SidebarStyle == SysMenuSidebarStyle.展开) ? "menu-open" : "");
							__builder.OpenElement(43, "li");
							__builder.AddAttribute(44, "class", "c1-nav-item " + text2);
							__builder.OpenElement(45, "a");
							__builder.AddAttribute(46, "href", list2.Any() ? "javascript:void(0)" : menu2.Path);
							__builder.AddAttribute(47, "class", "c1-nav-link " + ((!list2.Any() && activeItem?.Id == menu2.Id) ? "active" : ""));
							__builder.AddAttribute(48, "onclick", list2.Any() ? "novaAdminJS.toggleSidebarMenuItem(this)" : null);
							__builder.AddAttribute(49, "target", (menu2.Type == SysMenuType.外部连接) ? "_blank" : null);
							__builder.OpenElement(50, "i");
							__builder.AddAttribute(51, "class", "fas " + menu2.Icon.IsNull("fa-file-alt") + " nav-icon");
							__builder.CloseElement();
							__builder.AddMarkupContent(52, "\r\n                                                        ");
							__builder.OpenElement(53, "p");
							__builder.AddContent(54, menu2.Label);
							__builder.CloseElement();
							if (list2.Any())
							{
								__builder.AddMarkupContent(55, "<i class=\"nav-arrow fas fa-angle-left\"></i>");
							}
							__builder.CloseElement();
							if (list2.Any())
							{
								__builder.OpenElement(56, "ul");
								__builder.AddAttribute(57, "class", "c1-nav-submenu");
								foreach (SysMenu item in list2)
								{
									__builder.OpenElement(58, "li");
									__builder.AddAttribute(59, "class", "c1-nav-item");
									__builder.OpenElement(60, "a");
									__builder.AddAttribute(61, "class", "c1-nav-link " + ((activeItem?.Id == item.Id) ? "active" : ""));
									__builder.AddAttribute(62, "href", item.Path);
									__builder.AddAttribute(63, "target", (item.Type == SysMenuType.外部连接) ? "_blank" : null);
									__builder.OpenElement(64, "i");
									__builder.AddAttribute(65, "class", "fas " + item.Icon.IsNull("fa-file-lines") + " nav-icon");
									__builder.CloseElement();
									__builder.AddMarkupContent(66, "\r\n                                                                        ");
									__builder.OpenElement(67, "p");
									__builder.AddContent(68, item.Label);
									__builder.CloseElement();
									__builder.CloseElement();
									__builder.CloseElement();
								}
								__builder.CloseElement();
							}
							__builder.CloseElement();
						}
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}
		else
		{
			__builder.OpenElement(69, "div");
			__builder.AddAttribute(70, "class", "c1-nav-container");
			__builder.OpenElement(71, "div");
			__builder.AddAttribute(72, "class", "c1-nav-primary");
			__builder.OpenElement(73, "nav");
			__builder.AddAttribute(74, "class", "c1-nav-primary-menu");
			__builder.OpenElement(75, "ul");
			__builder.AddAttribute(76, "class", "c1-nav-list");
			if (menus != null)
			{
				foreach (SysMenu item2 in menus.Where((SysMenu a) => a.ParentId == 0L && !a.IsHidden && a.Type != SysMenuType.按钮))
				{
					string text3 = ((item2.Id == activePrimaryMenuId) ? "active" : "");
					__builder.OpenElement(77, "li");
					__builder.AddAttribute(78, "class", "c1-nav-item " + text3);
					if (item2.Type == SysMenuType.外部连接 || !item2.Path.IsNull())
					{
						__builder.OpenElement(79, "a");
						__builder.AddAttribute(80, "class", "c1-nav-link");
						__builder.AddAttribute(81, "href", item2.Path);
						__builder.AddAttribute(82, "target", (item2.Type == SysMenuType.外部连接) ? "_blank" : null);
						__builder.OpenElement(83, "i");
						__builder.AddAttribute(84, "class", "nav-icon fas " + item2.Icon.IsNull("fa-bars"));
						__builder.CloseElement();
						__builder.AddMarkupContent(85, "\r\n                                                ");
						__builder.OpenElement(86, "p");
						__builder.AddContent(87, item2.Label);
						__builder.CloseElement();
						__builder.CloseElement();
					}
					else
					{
						__builder.OpenElement(88, "a");
						__builder.AddAttribute(89, "href", "javascript:void(0)");
						__builder.AddAttribute(90, "class", "c1-nav-link");
						__builder.AddAttribute(91, "onmouseenter", "novaAdminJS.cancelHideMegaMenuTimer(); novaAdminJS.showMegaMenu(this, '#menu-group-" + item2.Id + "')");
						__builder.AddAttribute(92, "onmouseleave", "novaAdminJS.startHideMegaMenuTimer()");
						__builder.OpenElement(93, "i");
						__builder.AddAttribute(94, "class", "nav-icon fas " + item2.Icon.IsNull("fa-bars"));
						__builder.CloseElement();
						__builder.AddMarkupContent(95, "\r\n                                                ");
						__builder.OpenElement(96, "p");
						__builder.AddContent(97, item2.Label);
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(98, "\r\n                ");
			__builder.OpenElement(99, "div");
			__builder.AddAttribute(100, "class", "c1-nav-secondary");
			if (menus != null)
			{
				foreach (SysMenu menu3 in menus.Where((SysMenu a) => a.ParentId == 0L && !a.IsHidden && a.Type != SysMenuType.按钮))
				{
					List<SysMenu> list3 = ((menu3.Type != SysMenuType.外部连接) ? menus.Where((SysMenu m) => m.ParentId == menu3.Id && !m.IsHidden && m.Type != SysMenuType.按钮).ToList() : new List<SysMenu>());
					if (!list3.Any())
					{
						continue;
					}
					string text4 = ((menu3.SidebarStyle == SysMenuSidebarStyle.网格) ? "layout-grid" : "layout-accordion");
					__builder.OpenElement(101, "div");
					__builder.AddAttribute(102, "id", "menu-group-" + menu3.Id);
					__builder.AddAttribute(103, "class", "c1-nav-secondary-group " + text4);
					__builder.AddAttribute(104, "onmouseenter", "novaAdminJS.cancelHideMegaMenuTimer()");
					__builder.AddAttribute(105, "onmouseleave", "novaAdminJS.startHideMegaMenuTimer()");
					__builder.OpenElement(106, "ul");
					__builder.AddAttribute(107, "class", "c1-nav-list c1-nav-secondary-menu");
					foreach (SysMenu menu4 in list3)
					{
						bool flag = activeSecondaryMenuIds.Contains(menu4.Id);
						List<SysMenu> list4 = ((menu4.Type != SysMenuType.外部连接) ? menus.Where((SysMenu a) => a.ParentId == menu4.Id && !a.IsHidden && a.Type != SysMenuType.按钮).ToList() : new List<SysMenu>());
						if (list4.Any())
						{
							string text5 = ((menu4.SidebarStyle == SysMenuSidebarStyle.展开) ? "menu-open" : "");
							__builder.OpenElement(108, "li");
							__builder.AddAttribute(109, "class", "c1-nav-item " + text5);
							__builder.OpenElement(110, "a");
							__builder.AddAttribute(111, "href", "javascript:void(0)");
							__builder.AddAttribute(112, "class", "c1-nav-link c1-nav-toggle " + (flag ? "active" : ""));
							__builder.AddAttribute(113, "onclick", "novaAdminJS.toggleSidebarMenuItem(this)");
							__builder.OpenElement(114, "i");
							__builder.AddAttribute(115, "class", "fas " + menu4.Icon.IsNull("fa-file-alt") + " nav-icon");
							__builder.CloseElement();
							__builder.AddMarkupContent(116, "\r\n                                                        ");
							__builder.OpenElement(117, "p");
							__builder.AddContent(118, menu4.Label);
							__builder.CloseElement();
							__builder.AddMarkupContent(119, "\r\n                                                        <i class=\"nav-arrow fas fa-angle-left\"></i>");
							__builder.CloseElement();
							__builder.AddMarkupContent(120, "\r\n                                                    ");
							__builder.OpenElement(121, "ul");
							__builder.AddAttribute(122, "class", "c1-nav-submenu");
							foreach (SysMenu item3 in list4)
							{
								__builder.OpenElement(123, "li");
								__builder.AddAttribute(124, "class", "c1-nav-item");
								__builder.OpenElement(125, "a");
								__builder.AddAttribute(126, "class", "c1-nav-link " + ((activeItem?.Id == item3.Id) ? "active" : ""));
								__builder.AddAttribute(127, "href", item3.Path);
								__builder.AddAttribute(128, "target", (menu4.Type == SysMenuType.外部连接) ? "_blank" : null);
								__builder.OpenElement(129, "i");
								__builder.AddAttribute(130, "class", "fas " + item3.Icon.IsNull("fa-file-lines") + " nav-icon");
								__builder.CloseElement();
								__builder.AddMarkupContent(131, "\r\n                                                                    ");
								__builder.OpenElement(132, "p");
								__builder.AddContent(133, item3.Label);
								__builder.CloseElement();
								__builder.CloseElement();
								__builder.CloseElement();
							}
							__builder.CloseElement();
							__builder.CloseElement();
						}
						else
						{
							__builder.OpenElement(134, "li");
							__builder.AddAttribute(135, "class", "c1-nav-item");
							__builder.OpenElement(136, "a");
							__builder.AddAttribute(137, "class", "c1-nav-link " + (flag ? "active" : ""));
							__builder.AddAttribute(138, "href", menu4.Path);
							__builder.AddAttribute(139, "target", (menu4.Type == SysMenuType.外部连接) ? "_blank" : null);
							__builder.OpenElement(140, "i");
							__builder.AddAttribute(141, "class", "fas " + menu4.Icon.IsNull("fa-file-alt") + " nav-icon");
							__builder.CloseElement();
							__builder.AddMarkupContent(142, "\r\n                                                        ");
							__builder.OpenElement(143, "p");
							__builder.AddContent(144, menu4.Label);
							__builder.CloseElement();
							__builder.CloseElement();
							__builder.CloseElement();
						}
					}
					__builder.CloseElement();
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}
		__builder.CloseElement();
		__builder.AddMarkupContent(145, "\r\n\r\n    ");
		__builder.OpenElement(146, "main");
		__builder.AddAttribute(147, "class", "app-main");
		if (admin?.User != null)
		{
				__builder.OpenElement(148, "div");
				__builder.AddAttribute(149, "class", "app-main-top-toolbar");
				__builder.OpenElement(150, "div");
				if (!admin.Modals.Any())
				{
					__builder.OpenElement(201, "span");
						__builder.AddAttribute(202, "class", "message-top message-refresh");
					__builder.AddAttribute(203, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)RefreshTab));
					__builder.AddMarkupContent(204, "<i class=\"fas fa-rotate\"></i>");
					__builder.CloseElement();
				}
				else
				{
						__builder.AddMarkupContent(211, "<span class=\"message-top message-refresh\"><i class=\"fas fa-rotate\"></i></span>");
				}
				__builder.OpenComponent<Logout>(152);
				__builder.AddComponentParameter(153, "ImageUrl", RuntimeHelpers.TypeCheck(admin.Tenant?.Logo ?? "logo.png"));
				__builder.AddComponentParameter(154, "DisplayName", RuntimeHelpers.TypeCheck(admin.User.Username));
				__builder.AddComponentParameter(155, "UserName", RuntimeHelpers.TypeCheck(admin.Roles.FirstOrDefault()?.Name));
			__builder.AddAttribute(156, "HeaderTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.OpenElement(157, "div");
				renderTreeBuilder.AddAttribute(158, "class", "d-flex flex-fill align-items-center");
				renderTreeBuilder.OpenElement(159, "img");
				renderTreeBuilder.AddAttribute(160, "alt", "avatar");
				renderTreeBuilder.AddAttribute(161, "src", admin.Tenant?.Logo ?? "logo.png");
				renderTreeBuilder.AddAttribute(162, "style", "border-radius:50%;");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(163, "\r\n                                ");
				renderTreeBuilder.OpenElement(164, "div");
				renderTreeBuilder.AddAttribute(165, "class", "flex-fill");
				renderTreeBuilder.OpenElement(166, "div");
				renderTreeBuilder.AddAttribute(167, "class", "logout-dn");
				renderTreeBuilder.AddContent(168, admin.User.Username);
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.AddMarkupContent(169, "\r\n                                    ");
				renderTreeBuilder.OpenElement(170, "div");
				renderTreeBuilder.AddAttribute(171, "class", "logout-un");
				renderTreeBuilder.AddContent(172, admin.Roles.FirstOrDefault()?.Description);
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			});
			__builder.AddAttribute(173, "LinkTemplate", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.AddMarkupContent(174, "<div><a href=\"/Admin/UserProfile\"><i class=\"fa-solid fa-key\"></i><span>修改密码</span></a></div>\r\n                            ");
				renderTreeBuilder.OpenElement(175, "div");
				renderTreeBuilder.OpenElement(176, "a");
				renderTreeBuilder.AddAttribute(177, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)LogoutClick));
				renderTreeBuilder.AddAttribute(178, "href", "/");
				renderTreeBuilder.AddMarkupContent(179, "<i class=\"fa-solid fa-power-off\"></i>");
				renderTreeBuilder.AddMarkupContent(180, "<span>注销</span>");
				renderTreeBuilder.CloseElement();
				renderTreeBuilder.CloseElement();
			});
			__builder.CloseComponent();
			__builder.CloseElement();
			if (!admin.Modals.Any())
			{
				if (admin.Messages.Any())
				{
					__builder.OpenElement(181, "span");
					__builder.AddAttribute(182, "class", "message-top");
					__builder.AddAttribute(183, "data-bs-toggle", "dropdown");
					__builder.AddAttribute(184, "aria-expanded", "false");
					__builder.AddMarkupContent(185, "<i class=\"fas fa-bell\"></i>");
					__builder.OpenElement(186, "span");
					__builder.AddAttribute(187, "class", "badge");
					__builder.AddContent(188, admin.Messages.Count);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(189, "\r\n                        ");
					__builder.OpenElement(190, "div");
					__builder.AddAttribute(191, "class", "dropdown-menu shadow scroll");
					foreach (NovaAdminMessageInfo message in admin.Messages)
					{
						__builder.OpenElement(192, "div");
						__builder.AddAttribute(193, "class", "dropdown-item");
						__builder.AddContent(194, message.SendTime.ToString("dd HH:mm:ss"));
						__builder.AddContent(195, " ");
						__builder.AddContent(196, message.Content);
						__builder.CloseElement();
					}
					__builder.OpenElement(197, "div");
					__builder.AddAttribute(198, "class", "dropdown-item text-primary text-center");
					__builder.AddAttribute(199, "onclick", EventCallback.Factory.Create((object)this, (Action<MouseEventArgs>)delegate
					{
						admin.Messages.Clear();
					}));
					__builder.AddMarkupContent(200, "<i class=\"fas fa-trash-can\"></i> 清除");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				}
				else
				{
					if (admin.Messages.Any())
					{
					__builder.OpenElement(205, "span");
					__builder.AddAttribute(206, "class", "message-top");
					__builder.AddMarkupContent(207, "<i class=\"fas fa-bell\"></i>");
					__builder.OpenElement(208, "span");
					__builder.AddAttribute(209, "class", "badge");
					__builder.AddContent(210, admin.Messages.Count);
						__builder.CloseElement();
						__builder.CloseElement();
					}
				}
				__builder.CloseElement();
			}
		__builder.AddMarkupContent(212, "<div id=\"app-dockview-tab-contextmenu\" class=\"dropdown-menu\" style=\"position:fixed;z-index:1054;text-align:left;min-width:108px;line-height:18px;font-size:12px;\"><div class=\"dropdown-item\" style=\"padding:3px 12px\">关闭</div>\r\n            <div class=\"dropdown-item\" style=\"padding:3px 12px\">关闭所有</div>\r\n            <div class=\"dropdown-item\" style=\"padding:3px 12px\">关闭其他</div>\r\n            <div class=\"dropdown-item\" style=\"padding:3px 12px\">关闭所有(右侧)</div>\r\n            <div class=\"dropdown-item\" style=\"padding:3px 12px\">关闭所有(左侧)</div></div>\r\n        <div id=\"app-dockview\" class=\"app-content dockview-theme-light\" style=\"overflow:hidden\"></div>");
		__builder.CloseElement();
		__builder.CloseElement();
		foreach (NovaAdminContext.TabInfo tab in admin.Tabs)
		{
			__builder.OpenElement(213, "div");
			__builder.AddAttribute(214, "id", "dockview-panel-" + tab.Key);
			__builder.AddAttribute(215, "style", "height:100%;");
			__builder.SetKey(tab.Key);
			if (tab.Exception != null)
			{
				__builder.OpenComponent<Alert>(216);
				__builder.AddComponentParameter(217, "ShowBar", RuntimeHelpers.TypeCheck(value: true));
				__builder.AddComponentParameter(218, "ShowBorder", RuntimeHelpers.TypeCheck(value: true));
				__builder.AddComponentParameter(219, "Color", RuntimeHelpers.TypeCheck<Color>((Color)5));
				__builder.AddAttribute(220, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
				{
					renderTreeBuilder.OpenElement(221, "div");
					renderTreeBuilder.AddContent(222, tab.Exception);
					renderTreeBuilder.CloseElement();
				});
				__builder.CloseComponent();
			}
			if (tab.IsLoad)
			{
				if (tab.PageType == null)
				{
					__builder.OpenComponent<Alert>(223);
					__builder.AddComponentParameter(224, "ShowBar", RuntimeHelpers.TypeCheck(value: true));
					__builder.AddComponentParameter(225, "ShowBorder", RuntimeHelpers.TypeCheck(value: true));
					__builder.AddComponentParameter(226, "Color", RuntimeHelpers.TypeCheck<Color>((Color)5));
					__builder.AddAttribute(227, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
					{
						renderTreeBuilder.OpenElement(228, "div");
						renderTreeBuilder.AddContent(229, tab.Url);
						renderTreeBuilder.AddMarkupContent(230, " 路由页面不存在！");
						renderTreeBuilder.CloseElement();
					});
					__builder.CloseComponent();
				}
				else
				{
					TypeInference.CreateCascadingValue_0(__builder, 231, 232, tab, 233, delegate(RenderTreeBuilder renderTreeBuilder)
					{
						renderTreeBuilder.OpenComponent<ErrorLogger>(234);
						renderTreeBuilder.AddComponentParameter(235, "OnErrorHandleAsync", new Func<ILogger, Exception, Task>(OnErrorHandleAsync));
						renderTreeBuilder.AddAttribute(236, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
						{
							renderTreeBuilder2.OpenComponent<DynamicComponent>(237);
							renderTreeBuilder2.AddComponentParameter(238, "Type", RuntimeHelpers.TypeCheck(tab.PageType));
							renderTreeBuilder2.SetKey($"{tab.Key},{tab.ComponentKey}");
							renderTreeBuilder2.CloseComponent();
						});
						renderTreeBuilder.CloseComponent();
					});
				}
			}
			__builder.CloseElement();
		}
		foreach (NovaModal modal in admin.Modals)
		{
			__builder.OpenElement(239, "div");
			__builder.AddAttribute(240, "class", "modal fade " + (modal.IsDrawer ? "" : ("modal-fx-" + modal.Animation.ToString().ToLower())));
			__builder.AddAttribute(241, "id", modal.ClientId);
			__builder.AddAttribute(242, "data-bs-backdrop", modal.IsBackdropStatic ? "static" : "true");
			__builder.AddAttribute(243, "data-bs-keyboard", modal.IsKeyboard ? "true" : "false");
			__builder.AddAttribute(244, "tabindex", "-1");
			__builder.AddAttribute(245, "aria-hidden", "false");
			__builder.OpenElement(246, "div");
			__builder.AddAttribute(247, "class", "modal-dialog " + ((modal.IsDraggable && !modal.IsDrawer) ? "draggable" : "") + " " + modal.GetDialogClass() + " " + modal.GetDrawerClass());
			__builder.AddAttribute(248, "style", modal.GetDrawerStyle());
			__builder.OpenElement(249, "div");
			__builder.AddAttribute(250, "class", "modal-content");
			__builder.OpenElement(251, "div");
			__builder.AddAttribute(252, "class", "admintable2-loader table-loader");
			__builder.AddAttribute(253, "style", modal.IsShowLoader ? "" : "display:none");
			__builder.AddMarkupContent(254, "<div class=\"spinner spinner-border text-primary\"><span class=\"visually-hidden\">Loading...</span></div>");
			__builder.CloseElement();
			__builder.AddMarkupContent(255, "\r\n                ");
			__builder.OpenElement(256, "div");
			__builder.AddAttribute(257, "class", "modal-header");
			__builder.OpenElement(258, "div");
			__builder.AddAttribute(259, "class", "modal-title");
			__builder.AddContent(260, modal.Title);
			__builder.CloseElement();
			__builder.AddMarkupContent(261, "\r\n                    ");
			__builder.OpenElement(262, "button");
			__builder.AddAttribute(263, "type", "button");
			__builder.AddAttribute(264, "class", "close");
			__builder.AddAttribute(265, "aria-label", "Close");
			__builder.AddAttribute(266, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)modal.Hide));
			__builder.AddMarkupContent(267, "<span aria-hidden=\"false\">&times;</span>");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(268, "\r\n                ");
			__builder.OpenElement(269, "div");
			__builder.AddAttribute(270, "class", "modal-body scroll");
			if (modal.Visible)
			{
				TypeInference.CreateCascadingValue_1(__builder, 271, 272, modal.TabInfo, 273, delegate(RenderTreeBuilder renderTreeBuilder)
				{
					renderTreeBuilder.OpenComponent<ErrorLogger>(274);
					renderTreeBuilder.AddComponentParameter(275, "OnErrorHandleAsync", new Func<ILogger, Exception, Task>(OnErrorHandleAsync));
					renderTreeBuilder.AddAttribute(276, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
					{
						if (modal.Body != null)
						{
							renderTreeBuilder2.AddContent(277, modal.Body);
						}
						else
						{
							renderTreeBuilder2.AddContent(278, modal.ChildContent);
						}
					});
					renderTreeBuilder.CloseComponent();
				});
				Task.Delay(50).ContinueWith((Task _) => modal._visibleNotifyChanged = false);
			}
			__builder.CloseElement();
			if (modal.IsFooter)
			{
				__builder.OpenElement(279, "div");
				__builder.AddAttribute(280, "class", "modal-footer");
				if (modal.Footer != null)
				{
					__builder.AddContent(281, modal.Footer);
				}
				else
				{
					__builder.OpenElement(282, "button");
					__builder.AddAttribute(283, "type", "button");
					__builder.AddAttribute(284, "class", "btn btn-secondary");
					__builder.AddAttribute(285, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)modal.Hide));
					__builder.AddMarkupContent(286, "<i class=\"fa fa-close\"></i> ");
					__builder.AddContent(287, modal.CloseButton);
					__builder.CloseElement();
					if (modal.OnYes.HasDelegate)
					{
						__builder.OpenElement(288, "button");
						__builder.AddAttribute(289, "type", "button");
						__builder.AddAttribute(290, "class", "btn btn-primary pl-4 pr-4");
						__builder.AddAttribute(291, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)modal.YesClick));
						__builder.AddMarkupContent(292, "<i class=\"fa fa-save\"></i> ");
						__builder.AddContent(293, modal.YesButton);
						__builder.CloseElement();
					}
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}
	}

	/// <summary>
	/// 根据当前URL计算并设置所有激活和展开状态
	/// </summary>
	private void SetMenuStatesByCurrentPath()
	{
		activeItem = menus?.FirstOrDefault((SysMenu m) => !m.Path.IsNull() && string.Compare(currentPath, Nav.ToAbsoluteUri(m.Path).AbsolutePath, ignoreCase: true) == 0);
		activeSecondaryMenuIds.Clear();
		if (activeItem != null)
		{
			SysMenu current = activeItem;
			while (current != null)
			{
				if (current.ParentId == 0)
				{
					activePrimaryMenuId = current.Id;
					break;
				}
				if (menus.Any((SysMenu m) => m.Id == current.ParentId && m.ParentId == 0))
				{
					activeSecondaryMenuIds.Add(current.Id);
				}
				SysMenu sysMenu = menus.FirstOrDefault((SysMenu m) => m.Id == current.ParentId);
				if (sysMenu != null)
				{
					current = sysMenu;
				}
				else
				{
					current = null;
				}
			}
		}
		else if (menus != null && menus.Any())
		{
			activePrimaryMenuId = menus.FirstOrDefault((SysMenu m) => m.ParentId == 0)?.Id ?? 0;
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await admin.Init();
			await admin.InitTabRoute();
			if (admin.Tenant == null)
			{
				ToastServiceExtensions.Error(ToastService, "Error", "租户信息错误.", true);
			}
			menus = (from a in admin.RoleMenus
				where new SysMenuType[3]
				{
					SysMenuType.菜单,
					SysMenuType.增删改查,
					SysMenuType.外部连接
				}.Contains(a.Type)
				orderby a.Sort
				select a).ToList();
			Uri uri = new Uri(Nav.Uri);
			currentPath = uri.AbsolutePath;
			currentQuery = uri.Query;
			SetMenuStatesByCurrentPath();
			isInitialized = true;
			StateHasChanged();
			locationChangedEvent = delegate(object? s, LocationChangedEventArgs e)
			{
				Uri uri2 = Nav.ToAbsoluteUri(e.Location);
				currentPath = uri2.AbsolutePath;
				currentQuery = uri2.Query;
				SetMenuStatesByCurrentPath();
				StateHasChanged();
			};
			Nav.LocationChanged += locationChangedEvent;
			await Task.Yield();
		}
	}

	private async Task LogoutClick()
	{
		await admin.SignOut();
		admin.RedirectLogin();
	}

	[AntiConcurrency(100)]
	[StackTraceHidden]
	[DebuggerStepThrough]
	private void RefreshTab()
	{
		_0024Rougamo_RefreshTab();
	}

	public void Dispose()
	{
		if (locationChangedEvent != null)
		{
			Nav.LocationChanged -= locationChangedEvent;
		}
	}

	private Task OnErrorHandleAsync(ILogger logger, Exception ex)
	{
		logger.LogError(ex, ex.Message);
		if (ex.InnerException != null && ex != null)
		{
			ex = ex.InnerException;
		}
		return ToastServiceExtensions.Error(ToastService, "系统异常", ex.Message, false);
	}

	private void _0024Rougamo_RefreshTab()
	{
		if (!(DateTime.Now.Subtract(lastRefreshTime).TotalSeconds <= 1.0))
		{
			lastRefreshTime = DateTime.Now;
			NovaAdminContext.TabInfo tabInfo = admin.Tabs.FirstOrDefault((NovaAdminContext.TabInfo a) => a.IsActive);
			if (tabInfo != null)
			{
				tabInfo.Url = Nav.ToAbsoluteUri(tabInfo.Url).AbsolutePath;
				tabInfo.ComponentKey++;
				Nav.NavigateTo(tabInfo.Url, forceLoad: false);
			}
		}
	}
}
