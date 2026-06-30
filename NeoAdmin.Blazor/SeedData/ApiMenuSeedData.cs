using System.Reflection;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NeoAdmin.Blazor.Entities;
using MvcRouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace NeoAdmin.Blazor.SeedData;

/// <summary>
/// 根据 <c>AddNeoAdminApi</c> 注册的程序集反射生成 Api 权限菜单（含框架与宿主控制器）。
/// 每次启动增量补齐：已存在则不更新，缺失则插入。
/// </summary>
public static class ApiMenuSeedData
{
    public static void Ensure(IFreeSql freeSql, IEnumerable<Assembly> apiAssemblies)
    {
        long apiRootId = EnsureApiRoot(freeSql);

        List<DiscoveredApiGroup> groups = Discover(apiAssemblies)
            .OrderBy(group => group.GroupPath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            DiscoveredApiGroup group = groups[groupIndex];
            int groupSort = (groupIndex + 1) * 100;
            long groupId = EnsureApiGroup(freeSql, apiRootId, group, groupSort);

            for (int actionIndex = 0; actionIndex < group.Actions.Count; actionIndex++)
            {
                DiscoveredApiAction action = group.Actions[actionIndex];
                EnsureApiAction(freeSql, groupId, action, groupSort + actionIndex + 1);
            }
        }
    }

    private static long EnsureApiRoot(IFreeSql freeSql)
    {
        SysMenu? existing = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == 0 && menu.Label == "Api" && menu.Type == SysMenuType.接口)
            .First();
        if (existing is not null)
        {
            return existing.Id;
        }

        var root = new SysMenu
        {
            ParentId = 0,
            Label = "Api",
            Icon = string.Empty,
            Path = string.Empty,
            Sort = 0,
            Type = SysMenuType.接口,
            SidebarStyle = SysMenuSidebarStyle.收起,
            IsHidden = true,
            IsSystem = false,
        };
        freeSql.Insert(root).ExecuteAffrows();
        return root.Id;
    }

    private static long EnsureApiGroup(IFreeSql freeSql, long apiRootId, DiscoveredApiGroup group, int sort)
    {
        SysMenu? existing = freeSql.Select<SysMenu>()
            .Where(menu => menu.ParentId == apiRootId
                           && menu.Path == group.GroupPath
                           && menu.Type == SysMenuType.接口)
            .First();
        if (existing is not null)
        {
            return existing.Id;
        }

        var menu = new SysMenu
        {
            ParentId = apiRootId,
            Label = group.Label,
            Icon = string.Empty,
            Path = group.GroupPath,
            Sort = sort,
            Type = SysMenuType.接口,
            SidebarStyle = SysMenuSidebarStyle.收起,
            IsSystem = false,
        };
        freeSql.Insert(menu).ExecuteAffrows();
        return menu.Id;
    }

    private static void EnsureApiAction(IFreeSql freeSql, long groupId, DiscoveredApiAction action, int sort)
    {
        bool exists = freeSql.Select<SysMenu>()
            .Any(menu => menu.ParentId == groupId
                         && menu.Path == action.Path
                         && menu.Type == SysMenuType.接口);
        if (exists)
        {
            return;
        }

        freeSql.Insert(new SysMenu
        {
            ParentId = groupId,
            Label = action.Label,
            Icon = string.Empty,
            Path = action.Path,
            Sort = sort,
            Type = SysMenuType.接口,
            IsSystem = false,
        }).ExecuteAffrows();
    }

    private static IEnumerable<DiscoveredApiGroup> Discover(IEnumerable<Assembly> apiAssemblies)
    {
        foreach (Assembly assembly in apiAssemblies.Where(assembly => assembly is not null))
        {
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.IsAbstract || !typeof(ControllerBase).IsAssignableFrom(type))
                {
                    continue;
                }

                MvcRouteAttribute? route = type.GetCustomAttribute<MvcRouteAttribute>();
                if (route is null || !route.Template.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string groupPath = ExtractGroupPath(route.Template, type);
                if (ShouldSkipGroup(groupPath))
                {
                    continue;
                }

                List<DiscoveredApiAction> actions = DiscoverActions(type)
                    .OrderBy(action => action.Path, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (actions.Count == 0)
                {
                    continue;
                }

                yield return new DiscoveredApiGroup(
                    GetControllerLabel(type),
                    groupPath,
                    actions);
            }
        }
    }

    private static List<DiscoveredApiAction> DiscoverActions(Type controllerType)
    {
        List<DiscoveredApiAction> actions = [];

        foreach (MethodInfo method in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            HttpMethodAttribute? httpAttribute = method
                .GetCustomAttributes(inherit: true)
                .OfType<HttpMethodAttribute>()
                .FirstOrDefault();
            if (httpAttribute is null)
            {
                continue;
            }

            string actionPath = ExtractActionPath(httpAttribute.Template, method.Name);
            if (string.IsNullOrWhiteSpace(actionPath))
            {
                continue;
            }

            actions.Add(new DiscoveredApiAction(actionPath, actionPath));
        }

        return actions;
    }

    private static string ExtractGroupPath(string routeTemplate, Type controllerType)
    {
        string template = routeTemplate.Trim().TrimStart('/');
        if (template.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            template = template[4..];
        }

        template = template.Replace(
            "[controller]",
            GetControllerName(controllerType),
            StringComparison.OrdinalIgnoreCase);

        return template.Trim('/').ToLowerInvariant();
    }

    private static string ExtractActionPath(string? template, string methodName)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return methodName;
        }

        template = template.Trim();
        if (template.StartsWith('@'))
        {
            return template[1..].Trim('/');
        }

        if (template.Contains('/'))
        {
            return template.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];
        }

        return template;
    }

    private static bool ShouldSkipGroup(string groupPath) =>
        groupPath.Contains("e2e", StringComparison.OrdinalIgnoreCase);

    private static string GetControllerName(Type controllerType)
    {
        string name = controllerType.Name;
        return name.EndsWith("Controller", StringComparison.Ordinal)
            ? name[..^"Controller".Length]
            : name;
    }

    private static string GetControllerLabel(Type controllerType) => GetControllerName(controllerType);

    private sealed record DiscoveredApiGroup(
        string Label,
        string GroupPath,
        IReadOnlyList<DiscoveredApiAction> Actions);

    private sealed record DiscoveredApiAction(string Label, string Path);
}
