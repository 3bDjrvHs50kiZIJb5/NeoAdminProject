using FreeSql.DataAnnotations;

using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Entities.Blog;

/// <summary>
/// 用户点赞
/// </summary>
[Table(Name = "blog_user_like")]
public class UserLike : EntityCreated
{
    public long SubjectId { get; set; }

    [Navigate(nameof(SubjectId))]
    public Comment? Comment { get; set; }

    [Navigate(nameof(SubjectId))]
    public Article? Article { get; set; }

    [Column(MapType = typeof(int))]
    public UserLikeSubjectType SubjectType { get; set; }
}

public enum UserLikeSubjectType
{
    点赞随笔 = 0,
    点赞评论 = 1,
}
