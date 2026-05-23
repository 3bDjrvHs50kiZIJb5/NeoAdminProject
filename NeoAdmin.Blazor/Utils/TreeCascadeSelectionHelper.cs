namespace NeoAdmin.Blazor.Utils;

/// <summary>
/// 树形扁平列表的父子勾选联动（移植自 NovaAdminTable.CheckboxOnChange / NeoAllocTable）。
/// 要求 items 为深度优先扁平顺序。
/// </summary>
public static class TreeCascadeSelectionHelper
{
    public static void ApplyCascade<T>(
        IReadOnlyList<T> items,
        int index,
        bool selected,
        Func<T, int> levelSelector,
        Action<T, bool> setSelected,
        Func<T, bool> isSelected)
    {
        if (index < 0 || index >= items.Count)
        {
            return;
        }

        T current = items[index];
        int level = levelSelector(current);
        setSelected(current, selected);

        for (int childIndex = index + 1; childIndex < items.Count; childIndex++)
        {
            if (levelSelector(items[childIndex]) <= level)
            {
                break;
            }

            setSelected(items[childIndex], selected);
        }

        List<int> parentIndexes = new();
        for (int parentIndex = index - 1; parentIndex >= 0; parentIndex--)
        {
            int parentLevel = levelSelector(items[parentIndex]);
            if (parentLevel >= level)
            {
                continue;
            }

            if (parentIndexes.Any(existing => levelSelector(items[existing]) == parentLevel))
            {
                continue;
            }

            parentIndexes.Add(parentIndex);
            if (parentLevel == GetRootLevel(items, levelSelector))
            {
                break;
            }
        }

        foreach (int parentIndex in parentIndexes)
        {
            if (selected)
            {
                setSelected(items[parentIndex], true);
                continue;
            }

            int selectedDescendantCount = 0;
            int parentLevel = levelSelector(items[parentIndex]);
            int maxDescendantLevel = parentLevel;
            for (int descendantIndex = parentIndex + 1;
                 descendantIndex < items.Count && levelSelector(items[descendantIndex]) > parentLevel;
                 descendantIndex++)
            {
                int descendantLevel = levelSelector(items[descendantIndex]);
                maxDescendantLevel = descendantLevel;
                if (isSelected(items[descendantIndex]))
                {
                    selectedDescendantCount++;
                }
            }

            if (selectedDescendantCount > 0)
            {
                setSelected(items[parentIndex], true);
                continue;
            }

            if (maxDescendantLevel > parentLevel + 1)
            {
                setSelected(items[parentIndex], false);
            }
        }
    }

    public static int GetRootLevel<T>(IReadOnlyList<T> items, Func<T, int> levelSelector)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        return items.Min(levelSelector);
    }

    public static int FindIndex<T>(IReadOnlyList<T> items, T item, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        for (int index = 0; index < items.Count; index++)
        {
            if (comparer.Equals(items[index], item))
            {
                return index;
            }
        }

        return -1;
    }
}
