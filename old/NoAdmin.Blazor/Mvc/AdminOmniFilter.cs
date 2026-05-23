using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NoAdmin.Blazor.Mvc;

public class AdminOmniFilter : Attribute, IExceptionFilter, IFilterMetadata, IAsyncActionFilter
{
	private ILogger _logger;

	private NovaAdminContext _admin;

	public AdminOmniFilter(ILogger<AdminOmniFilter> logger, NovaAdminContext admin)
	{
		_logger = logger;
		_admin = admin;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		AllowAnonymousAttribute allowAnonymous = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() ?? context.Controller.GetType().GetCustomAttribute<AllowAnonymousAttribute>();
		if (allowAnonymous != null)
		{
			await next();
		}
		else if (_admin.User != null)
		{
			string path = _admin.HttpContext.Request.Path.Value;
			if (path.StartsWith("/api/community/"))
			{
				await next();
				return;
			}
			if (path.StartsWith("/api/"))
			{
				path = path.Substring(4);
			}
			int buttonIndex = path.IndexOf("@");
			string button = "";
			if (buttonIndex > 0)
			{
				button = path.Substring(buttonIndex + 1);
				path = path.Substring(0, buttonIndex);
				button = button.Trim('/');
			}
			path = path.Trim('/');
				List<long> userRoleIds = await _admin.Orm.Select<SysRoleUser>().Where((Expression<Func<SysRoleUser, bool>>)((SysRoleUser a) => a.UserId == _admin.User.Id)).ToListAsync<long>((Expression<Func<SysRoleUser, long>>)((SysRoleUser a) => a.RoleId), default(CancellationToken));
				List<SysRole> roles = await ((ISelect0<ISelect<SysRole>, SysRole>)(object)_admin.Orm.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => userRoleIds.Contains(a.Id)))).ToListAsync(default(CancellationToken));
			if (roles.Any((SysRole a) => a.IsAdministrator))
			{
				await next();
				return;
			}
			if (button.IsNull())
			{
				if (await _admin.Orm.Select<SysRoleMenu>().AnyAsync((Expression<Func<SysRoleMenu, bool>>)((SysRoleMenu a) => a.Menu.Path == path && roles.Select((SysRole b) => b.Id).Contains(a.RoleId)), default(CancellationToken)))
				{
					await next();
					return;
				}
			}
			else if (await _admin.Orm.Select<SysRoleMenu>().AnyAsync((Expression<Func<SysRoleMenu, bool>>)((SysRoleMenu a) => a.Menu.Parent.Path == path && a.Menu.Path == button && roles.Select((SysRole b) => b.Id).Contains(a.RoleId)), default(CancellationToken)))
			{
				await next();
				return;
			}
			context.Result = new JsonResult(ApiResult.NoPermission)
			{
				StatusCode = 401
			};
		}
		else
		{
			context.Result = new JsonResult(ApiResult.RequireLogin)
			{
				StatusCode = 401
			};
		}
	}

	public void OnException(ExceptionContext context)
	{
		context.Result = new JsonResult(ApiResult.Error.SetMessage(context.Exception.Message))
		{
			StatusCode = 500
		};
		string value = ((context.Exception.InnerException != null) ? (" \r\n" + context.Exception.InnerException.Message + " \r\n" + context.Exception.InnerException.StackTrace) : "");
		_logger.LogError($"=============错误：{context.Exception.Message} \r\n{context.Exception.StackTrace}{value}\r\n");
		context.ExceptionHandled = true;
	}
}
