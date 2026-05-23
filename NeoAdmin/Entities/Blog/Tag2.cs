using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Entities.Blog;

partial class Tag2
{
    [Navigate(ManyToMany = typeof(TagArticle))]
    public List<Article> Articles { get; set; } = [];

    [Table(Name = "blog_tag_article")]
    public class TagArticle
    {
        public long TagId { get; set; }
        public long ArticleId { get; set; }

        public Tag2 Tag { get; set; } = default!;
        public Article Article { get; set; } = default!;
    }
}

/// <summary>
/// 标签
/// </summary>
[Table(Name = "blog_tag")]
public partial class Tag2 : EntityCreated
{
    [Column(StringLength = 50)]
    public string TagName { get; set; } = string.Empty;

    [Column(StringLength = 200)]
    public string Alias { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string Thumbnail { get; set; } = string.Empty;

    public bool Status { get; set; }

    public int ArticleCount { get; set; }

    public int ViewHits { get; set; }

    [Column(StringLength = 500)]
    public string Remark { get; set; } = string.Empty;
}
