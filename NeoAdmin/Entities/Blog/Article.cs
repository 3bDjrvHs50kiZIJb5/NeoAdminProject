using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Entities.Blog;

partial class Article
{
    [Navigate(ManyToMany = typeof(ArticleCollection))]
    public List<Collection> Collections { get; set; } = [];

    [Table(Name = "blog_article_collection")]
    public class ArticleCollection
    {
        [Column(IsPrimary = true)]
        public long ArticleId { get; set; }

        [Column(IsPrimary = true)]
        public long CollectionId { get; set; }

        public Article Article { get; set; } = default!;
        public Collection Collection { get; set; } = default!;

        public long? CreatedUserId { get; set; }
        public string CreatedUserName { get; set; } = string.Empty;
        public DateTime? CreatedTime { get; set; }
    }
}

/// <summary>
/// 随笔文章
/// </summary>
[Table(Name = "blog_article")]
public partial class Article : EntityAudited
{
    [Navigate(ManyToMany = typeof(Tag2.TagArticle))]
    public List<Tag2> Tags { get; set; } = [];

    [Navigate(nameof(UserLike.SubjectId))]
    public List<UserLike> UserLikes { get; set; } = [];

    public long? ClassifyId { get; set; }
    public Classify? Classify { get; set; }

    public long ChannelId { get; set; }
    public Channel? Channel { get; set; }

    [Column(StringLength = 200)]
    public string Title { get; set; } = string.Empty;

    [Column(StringLength = 400)]
    public string Keywords { get; set; } = string.Empty;

    [Column(StringLength = 400)]
    public string Source { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string Excerpt { get; set; } = string.Empty;

    [Column(StringLength = -2)]
    public string Content { get; set; } = string.Empty;

    public int ViewHits { get; set; }
    public int CommentQuantity { get; set; }
    public int LikesQuantity { get; set; }
    public int CollectQuantity { get; set; }

    [Column(StringLength = 400)]
    public string Thumbnail { get; set; } = string.Empty;

    public bool IsAudit { get; set; }
    public bool Recommend { get; set; }
    public bool IsStickie { get; set; }

    [Column(MapType = typeof(int))]
    public ArticleType ArticleType { get; set; }

    public int WordNumber { get; set; }
    public int ReadingTime { get; set; }
    public bool Commentable { get; set; } = true;
}

public enum ArticleType
{
    原创,
    转载,
    翻译,
}
