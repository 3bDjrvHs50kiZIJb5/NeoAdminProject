using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Entities.Blog;

/// <summary>
/// 收藏集
/// </summary>
[Table(Name = "blog_collection")]
public class Collection : EntityAudited
{
    public List<Article> Articles { get; set; } = [];

    [Column(StringLength = 50)]
    public string Name { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Remark { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(MapType = typeof(int))]
    public PrivacyType PrivacyType { get; set; }
}

public enum PrivacyType
{
    公开可见 = 0,
    仅自己可见 = 1,
}
