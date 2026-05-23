namespace NeoAdmin.Api.Dto;

public sealed class ArticleListResponse
{
    public int TotalCount { get; init; }

    public List<ArticleListItemResponse> Items { get; init; } = [];
}

public sealed class ArticleListItemResponse
{
    public long Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Excerpt { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public long? ClassifyId { get; init; }

    public string? ClassifyName { get; init; }

    public long ChannelId { get; init; }

    public string? ChannelName { get; init; }

    public bool IsAudit { get; init; }

    public bool Recommend { get; init; }

    public bool IsStickie { get; init; }

    public string ArticleType { get; init; } = string.Empty;

    public int ViewHits { get; init; }

    public int CommentQuantity { get; init; }

    public int LikesQuantity { get; init; }

    public int CollectQuantity { get; init; }

    public string Thumbnail { get; init; } = string.Empty;

    public DateTime? CreatedTime { get; init; }

    public string CreatedUserName { get; init; } = string.Empty;
}
