namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 当前 Blazor 电路/请求范围内的临时状态（如防并发标记）。
/// </summary>
public sealed class NeoAdminScopeState
{
    public System.Collections.Concurrent.ConcurrentDictionary<string, object> Bags { get; } = new(StringComparer.Ordinal);
}
