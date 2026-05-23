namespace NeoAdmin.Blazor.Components;

public sealed class CrudQueryInfo
{
    public string? SearchText { get; set; }

    public string? Sort { get; set; }

    public CrudFilterInfo[] Filters { get; set; } = [];

    public Func<Task>? InvokeQueryAsync { get; set; }
}
