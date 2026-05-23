using FreeSql.Internal.Model;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Extensions;

public static class FreeSqlTreeExtensions
{
    /// <summary>
    /// 将平铺列表转为带 Level 的树形展示项（用于下拉、分配列表缩进）。
    /// </summary>
    public static List<NeoAdminItem<TItem>> ToNeoAdminItemList<TItem>(
        this List<TItem> list,
        IFreeSql freeSql)
    {
        TableInfo meta = freeSql.CodeFirst.GetTableByEntity(typeof(TItem));
        KeyValuePair<string, TableRef> treeNav = meta.GetAllTableRef()
            .FirstOrDefault(pair =>
                pair.Value.Exception == null
                && (int)pair.Value.RefType == 2
                && pair.Value.RefEntityType == typeof(TItem)
                && pair.Value.Columns.Count == 1
                && pair.Value.Columns[0].Attribute.IsPrimary
                && pair.Value.RefColumns.Count == 1);

        if (string.IsNullOrEmpty(treeNav.Key))
        {
            return list.Select(item => new NeoAdminItem<TItem>(item)).ToList();
        }

        object rootId = Activator.CreateInstance(treeNav.Value.Columns[0].CsType)!;
        List<TItem> working = list;
        List<NeoAdminItem<TItem>> result = EachListParentId(working, rootId, 1);
        list.Clear();
        return result;

        object GetItemPrimaryValue(TItem item) => meta.Primarys[0].GetValue(item)!;

        List<NeoAdminItem<TItem>> EachListParentId(List<TItem> source, object parentId, int level)
        {
            List<NeoAdminItem<TItem>> levelItems = new();
            for (int index = source.Count - 1; index >= 0; index--)
            {
                object? parentValue = treeNav.Value.RefColumns[0].GetValue(source[index]);
                if (Equals(parentValue, parentId) || (level == 1 && parentValue is null))
                {
                    levelItems.Insert(0, new NeoAdminItem<TItem>(source[index]) { Level = level });
                    source.RemoveAt(index);
                }
            }

            if (!source.Any())
            {
                return levelItems;
            }

            int childGroupIndex = 0;
            int insertIndex = 0;
            while (childGroupIndex < levelItems.Count)
            {
                List<NeoAdminItem<TItem>> children = EachListParentId(
                    source,
                    GetItemPrimaryValue(levelItems[childGroupIndex].Value),
                    level + 1);
                if (children.Count > 0)
                {
                    levelItems.InsertRange(insertIndex + 1, children);
                    insertIndex += children.Count;
                }

                childGroupIndex++;
                insertIndex++;
            }

            return levelItems;
        }
    }
}
