using FreeScheduler;
using Microsoft.Extensions.DependencyInjection;
using NeoAdmin.Blazor.Services;
using NeoAdmin.Blazor.Utils;
using Rougamo;
using Rougamo.Context;

namespace NeoAdmin.Blazor.Attributes;

/// <summary>
/// 防重复触发：方法执行期间拦截同 key 的再次调用，结束后延迟解锁。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AntiConcurrencyAttribute : MoAttribute
{
    private readonly int _milliseconds;
    private NeoAdminScopeState? _scopeState;
    private Scheduler? _scheduler;
    private string? _concurrencyKey;

    public AntiConcurrencyAttribute(int milliseconds = 100)
    {
        _milliseconds = milliseconds <= 0 ? 100 : milliseconds;
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
            throw new InvalidOperationException("_Imports.razor 未使用 @inject IServiceProvider ServiceProvider");
        }

        _scopeState = provider.GetRequiredService<NeoAdminScopeState>();
        _scheduler = provider.GetRequiredService<Scheduler>();
        _concurrencyKey = context.Method.DeclaringType!.FullName + "." + context.Method.Name;

        if (_scopeState.Bags.ContainsKey(_concurrencyKey))
        {
            MoAttributeHelper.ReplaceReturnValue(this, context, null);
            return;
        }

        _scopeState.Bags[_concurrencyKey] = true;
    }

    public override void OnExit(MethodContext context)
    {
        if (typeof(Task).IsAssignableFrom(context.ReturnType) && context.ReturnValue is Task task)
        {
            _ = task.ContinueWith(_ => ReleaseLock(), TaskScheduler.Default);
            return;
        }

        ReleaseLock();
    }

    private void ReleaseLock()
    {
        if (_concurrencyKey is null || _scopeState is null || _scheduler is null)
        {
            return;
        }

        string key = _concurrencyKey;
        NeoAdminScopeState scopeState = _scopeState;
        _scheduler.AddTempTask(TimeSpan.FromMilliseconds(_milliseconds), () =>
        {
            scopeState.Bags.TryRemove(key, out _);
        });
    }
}
