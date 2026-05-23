using System;
using System.Threading.Tasks;
using FreeScheduler;
using Microsoft.Extensions.DependencyInjection;
using Rougamo;
using Rougamo.Context;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class AntiConcurrencyAttribute : MoAttribute
{
	private int _milliseconds;

	private NovaAdminContext admin;

	private Scheduler scheduler;

	private string concurrencyKey;

	public AntiConcurrencyAttribute(int milliseconds = 100)
	{
		_milliseconds = milliseconds;
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
			throw new Exception("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
		}
		admin = provider.GetService<NovaAdminContext>();
		scheduler = provider.GetService<Scheduler>();
		concurrencyKey = context.Method.DeclaringType.FullName + "." + context.Method.Name;
		if (admin.Bags.ContainsKey(concurrencyKey))
		{
			System.Console.WriteLine("防抖动，拦截");
			context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
			return;
		}
		admin.Bags.AddOrUpdate(concurrencyKey, (string key) => true, (string key, object value) => true);
	}

	public override void OnExit(MethodContext context)
	{
		if (typeof(Task).IsAssignableFrom(context.ReturnType) && context.ReturnValue != null)
		{
			((Task)context.ReturnValue).ContinueWith(delegate
			{
				_OnExit();
			});
		}
		else
		{
			_OnExit();
		}
		void _OnExit()
		{
			if (concurrencyKey != null)
			{
				scheduler.AddTempTask(TimeSpan.FromMicroseconds(_milliseconds), (Action)delegate
				{
					admin.Bags.TryRemove(concurrencyKey, out object _);
				});
			}
		}
	}
}