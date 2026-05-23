namespace NeoAdmin.Blazor.Data.Attributes;

/// <summary>
/// 标记主键在插入时使用雪花算法生成（Yitter.IdGenerator）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SnowflakeAttribute : Attribute
{
    public bool Enable { get; set; } = true;
}
