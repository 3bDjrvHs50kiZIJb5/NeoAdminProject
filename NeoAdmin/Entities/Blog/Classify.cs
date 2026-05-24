using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Entities.Blog;

/// <summary>
/// 随笔专栏
/// </summary>
[Table(Name = "blog_classify")]
public class Classify : EntityCreated
{
    [Column(StringLength = 50)]
    public string ClassifyName { get; set; } = string.Empty;

    public int SortCode { get; set; }

    public int ArticleCount { get; set; }

    [Column(StringLength = 100)]
    public string Thumbnail { get; set; } = string.Empty;
}
