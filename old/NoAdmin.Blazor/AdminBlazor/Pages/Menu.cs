using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using __Blazor.NoAdmin.Blazor.Pages.Menu;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/Menu")]
public class Menu : ComponentBase
{
	private NovaAdminQueryInfo q;

	private SysMenu item;

	private List<NovaAdminItem<SysMenu>> actionButtons;

	private SysMenu menuAdd;

	[Inject]
	private IAggregateRootRepository<SysMenu> repo { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "菜单");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenComponent<NovaAdminTable<SysMenu>>(4);
		__builder.AddComponentParameter(5, "PageSize", RuntimeHelpers.TypeCheck(-1));
		__builder.AddComponentParameter(6, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
		__builder.AddComponentParameter(7, "TableTd1Width", RuntimeHelpers.TypeCheck(500));
		__builder.AddComponentParameter(8, "FixedLeftColumns", RuntimeHelpers.TypeCheck(2));
		__builder.AddComponentParameter(9, "FixedRightColumns", RuntimeHelpers.TypeCheck(1));
		__builder.AddComponentParameter(10, "InitQuery", new Func<NovaAdminQueryInfo, Task>(InitQuery));
		__builder.AddComponentParameter(11, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<NovaAdminQueryEventArgs<SysMenu>>)OnQuery)));
		__builder.AddComponentParameter(12, "OnEdit", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysMenu, Task>)OnEdit)));
		__builder.AddComponentParameter(13, "OnRemoving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<List<SysMenu>>, Task>)OnRemoving)));
		__builder.AddComponentParameter(14, "OnRemoved", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<List<SysMenu>, Task>)OnRemoved)));
		__builder.AddComponentParameter(15, "OnSaving", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<NovaAdminConfirmEventArgs<SysMenu>, Task>)OnSaving)));
		__builder.AddComponentParameter(16, "OnSaved", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<SysMenu, Task>)OnSaved)));
		__builder.AddAttribute(17, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(18, "<th width=\"80\">类型</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(19, "<th>路径</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(20, "<th width=\"220\">Icon</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(21, "<th width=\"55\">样式</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(22, "<th width=\"36\">隐藏</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(23, "<th width=\"55\">排序</th>\r\n        ");
			renderTreeBuilder.AddMarkupContent(24, "<th width=\"165\">创建时间</th>");
		});
		__builder.AddAttribute(25, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(26, "菜单名称");
		});
		__builder.AddAttribute(27, "TableTd1", (RenderFragment<SysMenu>)((SysMenu item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(28, item.Label);
			if (item.Type == SysMenuType.菜单)
			{
				renderTreeBuilder.OpenElement(29, "button");
				renderTreeBuilder.AddAttribute(30, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => BeginAdd(item)));
				renderTreeBuilder.AddAttribute(31, "type", "button");
				renderTreeBuilder.AddAttribute(32, "class", "ml-2 btn btn-light btn-xs float-end");
				renderTreeBuilder.AddMarkupContent(33, "<i class=\"fa fa-plus\"></i>");
				renderTreeBuilder.CloseElement();
			}
		}));
		__builder.AddAttribute(39, "TableRow", (RenderFragment<SysMenu>)((SysMenu item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(40, "td");
			renderTreeBuilder.AddContent(41, item.Type);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(42, "\r\n        ");
			renderTreeBuilder.OpenElement(43, "td");
			renderTreeBuilder.AddContent(44, item.Path);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(45, "\r\n        ");
			renderTreeBuilder.OpenElement(46, "td");
			if (!item.Icon.IsNull())
			{
				renderTreeBuilder.OpenElement(47, "i");
				renderTreeBuilder.AddAttribute(48, "class", "fa " + item.Icon);
				renderTreeBuilder.AddAttribute(49, "style", "padding-right:3px;");
				renderTreeBuilder.CloseElement();
			}
			renderTreeBuilder.AddContent(50, item.Icon);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(51, "\r\n        ");
			renderTreeBuilder.OpenElement(52, "td");
			renderTreeBuilder.AddContent(53, item.SidebarStyle);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(54, "\r\n        ");
			renderTreeBuilder.OpenElement(55, "td");
			renderTreeBuilder.AddContent(56, item.IsHidden ? "是" : "-");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(57, "\r\n        ");
			renderTreeBuilder.OpenElement(58, "td");
			renderTreeBuilder.AddContent(59, item.Sort);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(60, "\r\n        ");
			renderTreeBuilder.OpenElement(61, "td");
			renderTreeBuilder.AddContent(62, item.CreatedTime?.ToString("yyyy-MM-dd HH:mm:ss"));
			renderTreeBuilder.CloseElement();
		}));
		__builder.AddAttribute(63, "EditTemplate", (RenderFragment<SysMenu>)((SysMenu item) => delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.OpenElement(64, "div");
			renderTreeBuilder.AddAttribute(65, "class", "row");
			renderTreeBuilder.OpenElement(66, "div");
			renderTreeBuilder.AddAttribute(67, "class", "form-group col-12");
			renderTreeBuilder.AddMarkupContent(68, "<label class=\"form-label\">父菜单</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaInputTable<SysMenu, long>>(69);
			renderTreeBuilder.AddComponentParameter(70, "DisplayText", (Func<SysMenu, string>)((SysMenu a) => $"[{a.Id}]{a.Label}"));
			renderTreeBuilder.AddComponentParameter(71, "IsSearchText", RuntimeHelpers.TypeCheck(value: false));
			renderTreeBuilder.AddComponentParameter(72, "OnQuery", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(NovaAdminQueryEventArgs<SysMenu> e)
			{
				e.Select.Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => (int)a.Type == 0 || (int)a.Type == 3)).Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Path == null || a.Path == string.Empty)).OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort));
			})));
			renderTreeBuilder.AddComponentParameter(73, "OnItemChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<SysMenu>)ParentChanged)));
			renderTreeBuilder.AddComponentParameter(74, "Value", RuntimeHelpers.TypeCheck(item.ParentId));
			renderTreeBuilder.AddComponentParameter(75, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(long __value)
			{
				item.ParentId = __value;
			}, item.ParentId))));
			renderTreeBuilder.AddComponentParameter(76, "Item", RuntimeHelpers.TypeCheck(item.Parent));
			renderTreeBuilder.AddComponentParameter(77, "ItemChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysMenu __value)
			{
				item.Parent = __value;
			}, item.Parent))));
			renderTreeBuilder.AddAttribute(78, "TableTh1", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(79, "菜单名称");
			});
			renderTreeBuilder.AddAttribute(80, "TableTd1", (RenderFragment<SysMenu>)((SysMenu sysMenu) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddContent(81, sysMenu.Label);
			}));
			renderTreeBuilder.AddAttribute(82, "TableHeader", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(83, "<th width=\"180\">Icon</th>\r\n                        ");
				renderTreeBuilder2.AddMarkupContent(84, "<th width=\"36\">隐藏</th>\r\n                        ");
				renderTreeBuilder2.AddMarkupContent(85, "<th width=\"55\">排序</th>");
			});
			renderTreeBuilder.AddAttribute(86, "TableRow", (RenderFragment<SysMenu>)((SysMenu sysMenu) => delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.OpenElement(87, "td");
				if (!sysMenu.Icon.IsNull())
				{
					renderTreeBuilder2.OpenElement(88, "i");
					renderTreeBuilder2.AddAttribute(89, "class", "fa " + sysMenu.Icon);
					renderTreeBuilder2.AddAttribute(90, "style", "padding-right:3px;");
					renderTreeBuilder2.CloseElement();
				}
				renderTreeBuilder2.AddContent(91, sysMenu.Icon);
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(92, "\r\n                        ");
				renderTreeBuilder2.OpenElement(93, "td");
				renderTreeBuilder2.AddContent(94, sysMenu.IsHidden ? "是" : "-");
				renderTreeBuilder2.CloseElement();
				renderTreeBuilder2.AddMarkupContent(95, "\r\n                        ");
				renderTreeBuilder2.OpenElement(96, "td");
				renderTreeBuilder2.AddContent(97, sysMenu.Sort);
				renderTreeBuilder2.CloseElement();
			}));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(98, "\r\n        ");
			renderTreeBuilder.OpenElement(99, "div");
			renderTreeBuilder.AddAttribute(100, "class", "row");
			renderTreeBuilder.OpenElement(101, "div");
			renderTreeBuilder.AddAttribute(102, "class", "form-group col-9");
			renderTreeBuilder.AddMarkupContent(103, "<label class=\"form-label\">名称</label>\r\n                ");
			renderTreeBuilder.OpenElement(104, "input");
			renderTreeBuilder.AddAttribute(105, "type", "text");
			renderTreeBuilder.AddAttribute(106, "class", "form-control");
			renderTreeBuilder.AddAttribute(107, "placeholder", "");
			renderTreeBuilder.AddAttribute(108, "maxlength", "50");
			renderTreeBuilder.AddAttribute(109, "data-valid", "true");
			renderTreeBuilder.AddAttribute(110, "disabled", item.Label == "系统管理");
			renderTreeBuilder.AddAttribute(111, "value", BindConverter.FormatValue(item.Label));
			renderTreeBuilder.AddAttribute(112, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Label = __value;
			}, item.Label));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(113, "\r\n            ");
			renderTreeBuilder.OpenElement(114, "div");
			renderTreeBuilder.AddAttribute(115, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(116, "<label class=\"form-label\">类型</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaSelectEnum<SysMenuType>>(117);
			renderTreeBuilder.AddComponentParameter(118, "OnValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Action<SysMenuType>)MenuEntityTypeChanged)));
			renderTreeBuilder.AddComponentParameter(119, "Value", RuntimeHelpers.TypeCheck(item.Type));
			renderTreeBuilder.AddComponentParameter(120, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysMenuType __value)
			{
				item.Type = __value;
			}, item.Type))));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(121, "\r\n        ");
			renderTreeBuilder.OpenElement(122, "div");
			renderTreeBuilder.AddAttribute(123, "class", "row");
			renderTreeBuilder.OpenElement(124, "div");
			renderTreeBuilder.AddAttribute(125, "class", "form-group col-9");
			renderTreeBuilder.AddMarkupContent(126, "<label class=\"form-label\">路径</label>\r\n                ");
			renderTreeBuilder.OpenElement(127, "input");
			renderTreeBuilder.AddAttribute(128, "type", "text");
			renderTreeBuilder.AddAttribute(129, "class", "form-control");
			renderTreeBuilder.AddAttribute(130, "placeholder", "Admin/Menu");
			renderTreeBuilder.AddAttribute(131, "maxlength", "50");
			renderTreeBuilder.AddAttribute(132, "data-valid", "true");
			renderTreeBuilder.AddAttribute(133, "value", BindConverter.FormatValue(item.Path));
			renderTreeBuilder.AddAttribute(134, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Path = __value;
			}, item.Path));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(135, "\r\n            ");
			renderTreeBuilder.OpenElement(136, "div");
			renderTreeBuilder.AddAttribute(137, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(138, "<label class=\"form-label\">样式</label>\r\n                ");
			renderTreeBuilder.OpenComponent<NovaSelectEnum<SysMenuSidebarStyle>>(139);
			renderTreeBuilder.AddComponentParameter(140, "Disabled", RuntimeHelpers.TypeCheck(item.Type == SysMenuType.按钮));
			renderTreeBuilder.AddComponentParameter(141, "Value", RuntimeHelpers.TypeCheck(item.SidebarStyle));
			renderTreeBuilder.AddComponentParameter(142, "ValueChanged", RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(SysMenuSidebarStyle __value)
			{
				item.SidebarStyle = __value;
			}, item.SidebarStyle))));
			renderTreeBuilder.CloseComponent();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(143, "\r\n        ");
			renderTreeBuilder.OpenElement(144, "div");
			renderTreeBuilder.AddAttribute(145, "class", "row");
			renderTreeBuilder.OpenElement(146, "div");
			renderTreeBuilder.AddAttribute(147, "class", "form-group col-6");
			renderTreeBuilder.AddMarkupContent(148, "<label class=\"form-label\">Icon</label>\r\n                ");
			renderTreeBuilder.OpenElement(149, "input");
			renderTreeBuilder.AddAttribute(150, "type", "text");
			renderTreeBuilder.AddAttribute(151, "class", "form-control");
			renderTreeBuilder.AddAttribute(152, "placeholder", "fa-circle");
			renderTreeBuilder.AddAttribute(153, "maxlength", "50");
			renderTreeBuilder.AddAttribute(154, "data-valid", "true");
			renderTreeBuilder.AddAttribute(155, "value", BindConverter.FormatValue(item.Icon));
			renderTreeBuilder.AddAttribute(156, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				item.Icon = __value;
			}, item.Icon));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(157, "\r\n            ");
			renderTreeBuilder.OpenElement(158, "div");
			renderTreeBuilder.AddAttribute(159, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(160, "<label class=\"form-label\">排序</label>\r\n                ");
			renderTreeBuilder.OpenElement(161, "input");
			renderTreeBuilder.AddAttribute(162, "type", "number");
			renderTreeBuilder.AddAttribute(163, "class", "form-control");
			renderTreeBuilder.AddAttribute(164, "data-valid", "true");
			renderTreeBuilder.AddAttribute(165, "value", BindConverter.FormatValue(item.Sort, CultureInfo.InvariantCulture));
			renderTreeBuilder.AddAttribute(166, "onchange", EventCallback.Factory.CreateBinder(this, delegate(int __value)
			{
				item.Sort = __value;
			}, item.Sort, CultureInfo.InvariantCulture));
			renderTreeBuilder.SetUpdatesAttributeName("value");
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.AddMarkupContent(167, "\r\n            ");
			renderTreeBuilder.OpenElement(168, "div");
			renderTreeBuilder.AddAttribute(169, "class", "form-group col-3");
			renderTreeBuilder.AddMarkupContent(170, "<label class=\"form-label\">隐藏</label>\r\n                ");
			TypeInference.CreateCheckbox_0(renderTreeBuilder, 171, 172, item.Type == SysMenuType.按钮, 173, item.IsHidden, 174, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
			{
				item.IsHidden = __value;
			}, item.IsHidden)), 175, () => item.IsHidden);
			renderTreeBuilder.CloseElement();
			renderTreeBuilder.CloseElement();
			List<NovaAdminItem<SysMenu>> list = actionButtons;
			if (list != null && list.Any())
			{
				IEnumerable<NovaAdminItem<SysMenu>> enumerable = actionButtons.Where((NovaAdminItem<SysMenu> a) => a.Value.Sort >= 301 && a.Value.Sort <= 303);
				if (enumerable.Any())
				{
					renderTreeBuilder.OpenElement(176, "div");
					renderTreeBuilder.AddAttribute(177, "class", "row");
					renderTreeBuilder.OpenElement(178, "div");
					renderTreeBuilder.AddAttribute(179, "class", "form-group col-12");
					foreach (NovaAdminItem<SysMenu> btn in enumerable)
					{
						TypeInference.CreateCheckbox_1(renderTreeBuilder, 180, 181, __arg0: true, 182, btn.Value.Label, 183, "padding-right:12px", 184, btn.Selected, 185, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
						{
							btn.Selected = __value;
						}, btn.Selected)), 186, () => btn.Selected);
					}
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
				}
				enumerable = actionButtons.Where((NovaAdminItem<SysMenu> a) => a.Value.Sort >= 400 && a.Value.Sort <= 432);
				if (enumerable.Any())
				{
					renderTreeBuilder.OpenElement(187, "div");
					renderTreeBuilder.AddAttribute(188, "class", "row");
					renderTreeBuilder.OpenElement(189, "div");
					renderTreeBuilder.AddAttribute(190, "class", "form-group col-12");
					foreach (NovaAdminItem<SysMenu> btn2 in enumerable)
					{
						TypeInference.CreateCheckbox_2(renderTreeBuilder, 191, 192, __arg0: true, 193, btn2.Value.Label, 194, "padding-right:12px", 195, btn2.Selected, 196, EventCallback.Factory.Create(this, RuntimeHelpers.CreateInferredEventCallback(this, delegate(bool __value)
						{
							btn2.Selected = __value;
						}, btn2.Selected)), 197, () => btn2.Selected);
					}
					renderTreeBuilder.CloseElement();
					renderTreeBuilder.CloseElement();
				}
			}
		}));
		__builder.CloseComponent();
	}

	private async Task InitQuery(NovaAdminQueryInfo e)
	{
		q = e;
		e.Filters = new NovaAdminFilterInfo[1]
		{
			new NovaAdminFilterInfo("类型", "Type", multiple: true, 12, string.Join(",", Enum.GetNames(typeof(SysMenuType))), string.Join(",", from a in Enum.GetNames(typeof(SysMenuType))
				select (int)Enum.Parse<SysMenuType>(a)))
		};
		await Task.Yield();
	}

	private void OnQuery(NovaAdminQueryEventArgs<SysMenu> e)
	{
		e.Select.WhereIf(!e.Filters[0].HasValue, (Expression<Func<SysMenu, bool>>)((SysMenu a) => (int)a.Type != 1)).WhereIf(e.Filters[0].HasValue, (Expression<Func<SysMenu, bool>>)((SysMenu a) => e.Filters[0].Values<SysMenuType>().Contains(a.Type))).WhereIf(!e.SearchText.IsNull(), (Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Label.Contains(e.SearchText) || a.Path.Contains(e.SearchText)))
			.OrderBy<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort));
	}

	private async Task OnEdit(SysMenu menu)
	{
		actionButtons = null;
		item = menu;
		if (menuAdd != null)
		{
			item.ParentId = menuAdd.Id;
			item.Parent = menuAdd;
			SysMenu sysMenu = item;
			sysMenu.Sort = await ((IBaseRepository<SysMenu>)(object)repo).Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.ParentId == menuAdd.Id)).MaxAsync<int>((Expression<Func<SysMenu, int>>)((SysMenu a) => a.Sort), default(CancellationToken)) + 1;
		}
		if (menu.Parent == null && menu.ParentId > 0)
		{
			SysMenu sysMenu2 = menu;
			sysMenu2.Parent = await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((IBaseRepository<SysMenu>)(object)repo).Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Id == menu.ParentId))).FirstAsync(default(CancellationToken));
		}
		if (item.Children == null)
		{
			if (item.Id != 0)
			{
				await FreeSqlGlobalExtensions.IncludeManyAsync<SysMenu, SysMenu>(new List<SysMenu> { item }, ((IBaseRepository)repo).Orm, (Expression<Func<SysMenu, IEnumerable<SysMenu>>>)((SysMenu a) => a.Children), (Action<ISelect<SysMenu>>)null, default(CancellationToken));
			}
			else
			{
				item.Children = new List<SysMenu>();
			}
			((IBaseRepository<SysMenu>)(object)repo).Attach(item);
		}
		MenuEntityTypeChanged(menu.Type);
	}

	private async Task OnSaving(NovaAdminConfirmEventArgs<SysMenu> e)
	{
		if (actionButtons?.Any() ?? false)
		{
			if (item.Children == null)
			{
				item.Children = new List<SysMenu>();
			}
			foreach (NovaAdminItem<SysMenu> btn in actionButtons)
			{
				int index = item.Children.FindIndex((SysMenu a) => a.Path == btn.Value.Path);
				if (btn.Selected && index == -1)
				{
					item.Children.Add(btn.Value);
				}
				else if (!btn.Selected && index != -1)
				{
					item.Children.RemoveAt(index);
				}
			}
			if (item.Children.Any((SysMenu a) => Regex.IsMatch(a.Path, "audit_\\d\\d")) && !item.Children.Any((SysMenu a) => a.Path == "audit_00"))
			{
				await MessageService.Error("审核功能必须选择【提交】，提交=锁定，审核期间无法修改/删除数据！");
				e.Cancel = true;
			}
		}
		await Task.Yield();
	}

	private async Task OnSaved(SysMenu e)
	{
		List<SysMenu> menus = await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((IBaseRepository)repo).Orm.Select<SysMenu>()).ToListAsync(default(CancellationToken));
		NovaAdminContext._TenantMenusDict.AddOrUpdate(admin.Tenant.Id, (string k1) => menus, (string k1, List<SysMenu> k2) => menus);
	}

	private void ParentChanged(SysMenu parent)
	{
		if (item.Id == 0L && parent != null && !parent.Path.IsNull())
		{
			item.Path = parent.Path + "/xx";
		}
	}

	private void MenuEntityTypeChanged(SysMenuType type)
	{
		if (type == SysMenuType.按钮)
		{
			item.SidebarStyle = SysMenuSidebarStyle.收起;
			item.IsHidden = false;
		}
		List<SysMenu> source = new List<SysMenu>
		{
			new SysMenu
			{
				Label = "添加",
				Path = "add",
				Sort = 301,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "编辑",
				Path = "edit",
				Sort = 302,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "删除",
				Path = "remove",
				Sort = 303,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "历史版本",
				Path = "edit_version",
				Sort = 304,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "提交",
				Path = "audit_00",
				Sort = 400,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "一审",
				Path = "audit_01",
				Sort = 401,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "二审",
				Path = "audit_02",
				Sort = 402,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "三审",
				Path = "audit_03",
				Sort = 403,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "四审",
				Path = "audit_04",
				Sort = 404,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "五审",
				Path = "audit_05",
				Sort = 405,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "拒绝",
				Path = "audit_98",
				Sort = 431,
				Type = SysMenuType.按钮,
				IsSystem = true
			},
			new SysMenu
			{
				Label = "反审",
				Path = "audit_99",
				Sort = 432,
				Type = SysMenuType.按钮,
				IsSystem = true
			}
		};
		if (item.Id == 0)
		{
			if (type == SysMenuType.增删改查)
			{
				actionButtons = source.Select((SysMenu a) => new NovaAdminItem<SysMenu>(a)
				{
					Selected = (a.Sort < 400)
				}).ToList();
			}
			else
			{
				actionButtons?.Clear();
			}
			return;
		}
		List<SysMenu> children = item.Children;
		if (children == null || !children.All((SysMenu a) => a.Type == SysMenuType.按钮))
		{
			return;
		}
		if (type == SysMenuType.增删改查 || type == SysMenuType.菜单)
		{
			actionButtons = source.Select((SysMenu a) => new NovaAdminItem<SysMenu>(a)
			{
				Selected = (item.Children?.Any((SysMenu b) => b.Path == a.Path) ?? false)
			}).ToList();
		}
		else
		{
			actionButtons?.Clear();
		}
	}

	private async Task OnRemoving(NovaAdminConfirmEventArgs<List<SysMenu>> e)
	{
		if (e.Argument.Any((SysMenu a) => a.IsSystem))
		{
			await MessageService.Error("不能删除系统菜单！");
			e.Cancel = true;
		}
		await FreeSqlGlobalExtensions.IncludeManyAsync<SysMenu, SysMenu>(e.Argument, ((IBaseRepository)repo).Orm, (Expression<Func<SysMenu, IEnumerable<SysMenu>>>)((SysMenu a) => a.Children), (Action<ISelect<SysMenu>>)null, default(CancellationToken));
	}

	private async Task OnRemoved(List<SysMenu> e)
	{
		List<SysMenu> menus = await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((IBaseRepository)repo).Orm.Select<SysMenu>()).ToListAsync(default(CancellationToken));
		NovaAdminContext._TenantMenusDict.AddOrUpdate(admin.Tenant.Id, (string k1) => menus, (string k1, List<SysMenu> k2) => menus);
	}

	private async Task BeginAdd(SysMenu menu)
	{
		menuAdd = menu;
		await q.InvokeAddAsync();
		menuAdd = null;
	}
}
