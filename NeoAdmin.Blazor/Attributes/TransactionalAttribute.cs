using System.Data;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using NeoAdmin.Blazor.Utils;
using Rougamo;
using Rougamo.Context;

namespace NeoAdmin.Blazor.Attributes;

/// <summary>
/// 声明式数据库事务：方法执行前开启 FreeSql 工作单元，正常退出 Commit，异常 Rollback。
/// Blazor 需注入 <c>IServiceProvider</c>；API 请求由中间件调用 <see cref="SetServiceProvider"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TransactionalAttribute : MoAttribute
{
    internal static AsyncLocal<IServiceProvider?> AdminOmniServiceProvider = new();

    private IUnitOfWork? _uow;
    private IsolationLevel? _isolationLevel;

    public Propagation Propagation { get; set; } = Propagation.Required;

    public IsolationLevel IsolationLevel
    {
        set => _isolationLevel = value;
    }

    public TransactionalAttribute()
    {
    }

    public TransactionalAttribute(Propagation propagation)
    {
        Propagation = propagation;
    }

    public TransactionalAttribute(Propagation propagation, IsolationLevel isolationLevel)
    {
        Propagation = propagation;
        _isolationLevel = isolationLevel;
    }

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        AdminOmniServiceProvider.Value = serviceProvider;
    }

    public override void OnEntry(MethodContext context)
    {
        if (context.ReturnValueReplaced)
        {
            return;
        }

        Type type = context.Target.GetType();
        IServiceProvider? serviceProvider = AdminOmniServiceProvider.Value
            ?? MoAttributeHelper.GetPropertyOrFieldValue(type, context.Target, "ServiceProvider") as IServiceProvider;

        if (serviceProvider is null)
        {
            MoAttributeHelper.ReplaceReturnValue(this, context, null);
            throw new InvalidOperationException("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
        }

        UnitOfWorkManager uowManager = serviceProvider.GetRequiredService<UnitOfWorkManager>();
        _uow = uowManager.Begin(Propagation, _isolationLevel);
    }

    public override void OnExit(MethodContext context)
    {
        if (typeof(Task).IsAssignableFrom(context.ReturnType) && context.ReturnValue is Task task)
        {
            _ = task.ContinueWith(completedTask => Finish(context, completedTask), TaskScheduler.Default);
            return;
        }

        Finish(context);
    }

    private void Finish(MethodContext context, Task? task = null)
    {
        if (_uow is null)
        {
            return;
        }

        try
        {
            bool success = context.Exception is null
                && (task is null || task.IsCompletedSuccessfully);

            if (success)
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
            _uow.Dispose();
        }
    }
}
