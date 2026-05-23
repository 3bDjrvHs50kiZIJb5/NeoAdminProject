using FreeSql;

namespace NeoAdmin.Blazor.Components;

public sealed class CrudGridRow<TItem>
{
    public int RowIndex { get; set; }

    public TItem Item { get; set; } = default!;
}

public sealed class CrudQueryEventArgs<TItem> where TItem : class
{
    public ISelect<TItem>? Select { get; }

    public IQueryable<TItem>? Queryable { get; set; }

    public string? SearchText { get; }

    public CrudFilterInfo[] Filters { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public bool IsInMemory => Select == null;

    public CrudQueryEventArgs(
        ISelect<TItem> select,
        string? searchText,
        CrudFilterInfo[] filters,
        int pageNumber,
        int pageSize)
    {
        Select = select;
        SearchText = searchText;
        Filters = filters;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public CrudQueryEventArgs(
        IQueryable<TItem> queryable,
        string? searchText,
        CrudFilterInfo[] filters,
        int pageNumber,
        int pageSize)
    {
        Queryable = queryable;
        SearchText = searchText;
        Filters = filters;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

public sealed class CrudConfirmEventArgs<TItem>
{
    public TItem Item { get; }

    public bool Cancel { get; set; }

    public CrudConfirmEventArgs(TItem item)
    {
        Item = item;
    }
}

public sealed class CrudBatchConfirmEventArgs<TItem>
{
    public IReadOnlyList<TItem> Items { get; }

    public bool Cancel { get; set; }

    public CrudBatchConfirmEventArgs(IReadOnlyList<TItem> items)
    {
        Items = items;
    }
}

public sealed class CrudQueriedEventArgs<TItem>
{
    public IReadOnlyList<TItem> Items { get; }

    public long Total { get; }

    public CrudQueriedEventArgs(IReadOnlyList<TItem> items, long total)
    {
        Items = items;
        Total = total;
    }
}
