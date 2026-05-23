namespace NeoAdmin.Blazor.Models;

/// <summary>
/// 带层级与选中状态的列表项，用于下拉树形展示或分配选择。
/// </summary>
public sealed class NeoAdminItem<T>
{
    public T Value { get; set; }

    public bool Selected { get; set; }

    public int Level { get; set; } = 1;

    public NeoAdminItem(T value)
    {
        Value = value;
    }
}
