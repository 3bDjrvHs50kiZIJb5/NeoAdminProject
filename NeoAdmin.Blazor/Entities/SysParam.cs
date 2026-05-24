using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

public sealed class SysParam : Entity
{
    [Column(StringLength = 50)]
    public string Key { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Title { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Sort { get; set; }

    [Column(StringLength = 1024)]
    public string Value { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value2 { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value3 { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value4 { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value5 { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value6 { get; set; } = string.Empty;

    [Column(StringLength = 1024)]
    public string Value7 { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
