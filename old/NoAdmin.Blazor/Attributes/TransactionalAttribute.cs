using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Rougamo;
using Rougamo.Context;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class TransactionalAttribute : MoAttribute
{
	private IsolationLevel? m_IsolationLevel;

	internal static AsyncLocal<IServiceProvider> AdminOmniServiceProvider = new AsyncLocal<IServiceProvider>();

	private IUnitOfWork _uow;

	public Propagation Propagation { get; set; } = (Propagation)0;

	public IsolationLevel IsolationLevel
	{
		get
		{
			return m_IsolationLevel.Value;
		}
		set
		{
			m_IsolationLevel = value;
		}
	}

	public static void SetServiceProvider(IServiceProvider serviceProvider)
	{
		AdminOmniServiceProvider.Value = serviceProvider;
	}

	public TransactionalAttribute(Propagation propagation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Propagation = propagation;
	}

	public TransactionalAttribute(Propagation propagation, IsolationLevel isolationLevel)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Propagation = propagation;
		m_IsolationLevel = isolationLevel;
	}

	public override void OnEntry(MethodContext context)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!context.ReturnValueReplaced)
		{
			Type type = context.Target.GetType();
			IServiceProvider serviceProvider = AdminOmniServiceProvider.Value ?? (type.GetPropertyOrFieldValue(context.Target, "ServiceProvider") as IServiceProvider);
			if (serviceProvider == null)
			{
				context.ReplaceReturnValue((IMo)(object)this, context.HasReturnValue ? FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(context.ReturnType) : null);
				throw new Exception("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
			}
			UnitOfWorkManager service = serviceProvider.GetService<UnitOfWorkManager>();
			_uow = service.Begin(Propagation, m_IsolationLevel);
		}
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
			if (_uow == null)
			{
				return;
			}
			try
			{
				if (context.Exception == null)
				{
					_uow.Commit();
				}
				else
				{
					_uow.Rollback();
				}
			}
			finally
			{
				((IDisposable)_uow).Dispose();
			}
		}
	}
}