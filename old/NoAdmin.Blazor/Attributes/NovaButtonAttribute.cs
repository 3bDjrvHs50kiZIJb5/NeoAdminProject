using System;
using System.Linq;
using System.Reflection;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Rougamo;
using Rougamo.Context;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class NovaButtonAttribute : MoAttribute
{
	public string Name { get; set; }

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
		if (!(type.GetPropertyOrFieldValue(context.Target, "ServiceProvider") is IServiceProvider provider))
		{
			context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
			throw new Exception("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
		}
		NovaAdminContext service = provider.GetService<NovaAdminContext>();
		MessageService service2 = provider.GetService<MessageService>();
		SysMenu sysMenu = null;
		if (typeof(ComponentBase).IsAssignableFrom(type))
		{
			RouteAttribute customAttribute = type.GetCustomAttribute<RouteAttribute>();
			if (customAttribute != null && customAttribute.Template != null)
			{
				string path = customAttribute.Template;
				if (path != "/")
				{
					path = path?.ToLower().Trim('/');
				}
				sysMenu = service.RoleMenus.Where((SysMenu a) => string.Compare(a.Path, path, ignoreCase: true) == 0).FirstOrDefault();
			}
		}
		if (sysMenu == null)
		{
			MemberInfo memberInfo = (from a in type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where a.GetPropertyOrFieldType() == typeof(NovaAdminContext.TabInfo) && a.GetCustomAttribute<CascadingParameterAttribute>() != null
				select a).FirstOrDefault();
			if (memberInfo != null)
			{
				sysMenu = ((type.GetPropertyOrFieldValue(context.Target, memberInfo.Name) is NovaAdminContext.TabInfo tabInfo) ? tabInfo.Menu : null);
			}
		}
		if (sysMenu == null)
		{
			context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
			throw new Exception($"NovaButton -> {Name} 未能获取 Menu 信息，解决方法：{FreeSqlGlobalExtensions.DisplayCsharp(context.TargetType, true)} 定义 [CascadingParameter] NovaAdminContext.TabInfo tabInfo");
		}
		if (Name == "NovaAdminTable_look")
		{
			if (!service.AuthPath(sysMenu.Path))
			{
				context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
				service2.Error("没有访问权限.");
			}
		}
		else if (!service.AuthPath(sysMenu.Path) || !service.AuthButton(sysMenu, Name))
		{
			context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
			service2.Error("没有访问权限.");
		}
	}
}