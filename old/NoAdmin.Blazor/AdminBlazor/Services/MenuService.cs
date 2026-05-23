using System;
using System.Linq;
using System.Linq.Expressions;
using NoAdmin.Blazor.Components;
using FreeSql;

namespace NoAdmin.Blazor.Services;

/// <summary>
/// 菜单数据初始化服务
/// </summary>
public class MenuService
{
	public static void InitMenuData(IFreeSql fsql, SysMenu desiredMenu)
	{
		SysMenu sysMenu = ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>().Include<SysMenu>((Expression<Func<SysMenu, SysMenu>>)((SysMenu a) => a.Parent)).WhereIf(desiredMenu.Id > 0, (Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Id == desiredMenu.Id))
			.WhereIf(!desiredMenu.Path.IsNull(), (Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Path == desiredMenu.Path))
			.WhereIf(!desiredMenu.Label.IsNull(), (Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Label == desiredMenu.Label))).Cancel((Func<bool>)(() => desiredMenu.Id == 0L && desiredMenu.Path.IsNull() && desiredMenu.Label.IsNull()))).First();
		SysMenu sysMenu2 = null;
		if (sysMenu != null && sysMenu.Parent != null)
		{
			sysMenu2 = sysMenu.Parent;
			desiredMenu.Type = sysMenu.Type;
		}
		else
		{
			if (!(desiredMenu.Path == "/api"))
			{
				CreateMenuIfNotExistsRecursive(fsql, desiredMenu, 0L);
				return;
			}
			sysMenu2 = ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>().Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.Label == "系统管理"))).First();
			if (sysMenu2 == null)
			{
				sysMenu2 = ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>().Where((Expression<Func<SysMenu, bool>>)((SysMenu a) => a.ParentId == 0))).First();
			}
		}
		if (sysMenu2 == null)
		{
			System.Console.WriteLine("错误：未找到任何可用的根菜单节点。");
		}
		else
		{
			CreateMenuIfNotExistsRecursive(fsql, desiredMenu, sysMenu2.Id);
		}
	}

	private static void CreateMenuIfNotExistsRecursive(IFreeSql fsql, SysMenu menuNode, long parentId)
	{
		SysMenu sysMenu = ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>().Where((Expression<Func<SysMenu, bool>>)((SysMenu m) => m.ParentId == parentId && (int)m.Type == (int)menuNode.Type)).WhereIf(!menuNode.Path.IsNull(), (Expression<Func<SysMenu, bool>>)((SysMenu m) => m.Path == menuNode.Path))
			.WhereIf(!menuNode.Label.IsNull(), (Expression<Func<SysMenu, bool>>)((SysMenu m) => m.Label == menuNode.Label))).Cancel((Func<bool>)(() => menuNode.Path.IsNull() && menuNode.Label.IsNull()))).First();
		long id;
		if (sysMenu == null)
		{
			menuNode.ParentId = parentId;
			fsql.Insert<SysMenu>(menuNode).ExecuteAffrows();
			id = menuNode.Id;
			System.Console.WriteLine("创建了新的菜单项: " + menuNode.Label + ", Path: " + menuNode.Path);
		}
		else
		{
			id = sysMenu.Id;
		}
		if (menuNode.Children == null || !menuNode.Children.Any())
		{
			return;
		}
		foreach (SysMenu child in menuNode.Children)
		{
			CreateMenuIfNotExistsRecursive(fsql, child, id);
		}
	}
}
