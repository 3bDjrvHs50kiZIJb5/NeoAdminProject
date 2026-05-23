using FreeSql.Internal.Model;

namespace NeoAdmin.Blazor.Utils;

public static class NeoEntitySelectHelper
{
    public static TableInfo GetTableInfo(IFreeSql freeSql, Type entityType) =>
        freeSql.CodeFirst.GetTableByEntity(entityType);

    public static void EnsureSinglePrimaryKey<TItem, TKey>(IFreeSql freeSql)
    {
        TableInfo meta = GetTableInfo(freeSql, typeof(TItem));
        if (meta.Primarys.Length != 1)
        {
            throw new ArgumentException($"{typeof(TItem).Name} 必须使用单一主键。");
        }

        Type primaryType = Nullable.GetUnderlyingType(meta.Primarys[0].CsType) ?? meta.Primarys[0].CsType;
        Type keyType = Nullable.GetUnderlyingType(typeof(TKey)) ?? typeof(TKey);
        if (primaryType != keyType)
        {
            throw new ArgumentException($"{typeof(TItem).Name} 的主键类型必须与 {typeof(TKey).Name} 一致。");
        }
    }

    public static TKey GetPrimaryKey<TItem, TKey>(IFreeSql freeSql, TItem? item)
    {
        if (item is null)
        {
            return default!;
        }

        TableInfo meta = GetTableInfo(freeSql, typeof(TItem));
        object? value = meta.Primarys[0].GetValue(item);
        if (value is null)
        {
            return default!;
        }

        return (TKey)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(TKey)) ?? typeof(TKey));
    }

    public static string GetPrimaryKeyString<TItem>(IFreeSql freeSql, TItem? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        TableInfo meta = GetTableInfo(freeSql, typeof(TItem));
        return meta.Primarys[0].GetValue(item)?.ToString() ?? string.Empty;
    }

    public static ColumnInfo? GetFirstStringColumn(IFreeSql freeSql, Type entityType) =>
        GetTableInfo(freeSql, entityType)
            .ColumnsByPosition
            .FirstOrDefault(column => column.CsType == typeof(string));
}
