using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Menus;
using NeoAdmin.Blazor.Services;
using NeoAdmin.Blazor.Utils;
using NeoUI.Blazor;
using Rougamo;
using Rougamo.Context;

namespace NeoAdmin.Blazor.Attributes;

/// <summary>
/// 按钮权限拦截：方法执行前校验当前页面菜单与按钮权限。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NovaButtonAttribute : MoAttribute
{
    public string Name { get; }

    public NovaButtonAttribute(string name)
    {
        Name = name;
    }

    public override void OnEntry(MethodContext context)
    {
        if (context.ReturnValueReplaced)
        {
            return;
        }

        Type type = context.Target.GetType();
        if (MoAttributeHelper.GetPropertyOrFieldValue(type, context.Target, "ServiceProvider") is not IServiceProvider provider)
        {
            MoAttributeHelper.ReplaceReturnValue(this, context, null);
            throw new InvalidOperationException("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
        }

        MenuPermissionService permissionService = provider.GetRequiredService<MenuPermissionService>();
        IToastService toastService = provider.GetRequiredService<IToastService>();

        string? menuPath = ResolveMenuPath(type, context.Target);
        if (string.IsNullOrWhiteSpace(menuPath))
        {
            MoAttributeHelper.ReplaceReturnValue(this, context, null);
            throw new InvalidOperationException(
                $"NovaButton -> {Name} 未能获取 Menu 信息，请为 {type.Name} 配置 @page 路由或 MenuPath 属性。");
        }

        menuPath = MenuService.NormalizePath(menuPath);

        if (Name == "NovaAdminTable_look")
        {
            if (!permissionService.HasPageAsync(menuPath).GetAwaiter().GetResult())
            {
                MoAttributeHelper.ReplaceReturnValue(this, context, null);
                toastService.Error("没有访问权限", "当前账号无法访问此页面。");
            }

            return;
        }

        if (!permissionService.HasPageAsync(menuPath).GetAwaiter().GetResult()
            || !permissionService.HasButtonAsync(menuPath, Name).GetAwaiter().GetResult())
        {
            MoAttributeHelper.ReplaceReturnValue(this, context, null);
            toastService.Error("没有访问权限", "当前账号没有此按钮权限。");
        }
    }

    private static string? ResolveMenuPath(Type type, object target)
    {
        foreach (string propertyName in new[] { "MenuPath", "WorkflowAuditMenuPath" })
        {
            if (MoAttributeHelper.GetPropertyOrFieldValue(type, target, propertyName) is string path
                && !string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
        }

        RouteAttribute? route = type.GetCustomAttribute<RouteAttribute>();
        if (route?.Template is { Length: > 0 } template)
        {
            return template;
        }

        MemberInfo? tabInfoMember = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(member =>
                member.GetCustomAttribute<CascadingParameterAttribute>() is not null
                && member.Name.Contains("TabInfo", StringComparison.OrdinalIgnoreCase));

        if (tabInfoMember is not null
            && MoAttributeHelper.GetPropertyOrFieldValue(type, target, tabInfoMember.Name) is { } tabInfo)
        {
            PropertyInfo? menuProperty = tabInfo.GetType().GetProperty("Menu");
            if (menuProperty?.GetValue(tabInfo) is SysMenu menu && !string.IsNullOrWhiteSpace(menu.Path))
            {
                return menu.Path;
            }
        }

        return null;
    }
}
