using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

public sealed class SysDict : Entity
{
    public long ParentId { get; set; }

    [Navigate(nameof(ParentId))]
    public SysDict? Parent { get; set; }

    [Navigate(nameof(ParentId))]
    public List<SysDict> Children { get; set; } = new();

    [Column(StringLength = 50)]
    public string Name { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Value { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Value2 { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Value3 { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Value4 { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Value5 { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sort { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
