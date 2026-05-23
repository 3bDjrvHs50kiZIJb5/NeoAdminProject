using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Data.Entities.Blog;

/// <summary>
/// 评论
/// </summary>
[Table(Name = "blog_comment")]
public class Comment : EntityModified
{
    [Navigate(nameof(UserLike.SubjectId))]
    public List<UserLike> UserLikes { get; set; } = [];

    public long ArticleId { get; set; }
    public Article? Article { get; set; }

    [Column(StringLength = 500)]
    public string Text { get; set; } = string.Empty;

    public int LikesQuantity { get; set; }
    public bool IsAudit { get; set; } = true;
}
