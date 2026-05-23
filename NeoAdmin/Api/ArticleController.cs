using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoAdmin.Api.Dto;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Entities.Blog;
using BlogArticle = NeoAdmin.Entities.Blog.Article;

namespace NeoAdmin.Api;

/// <summary>
/// 博客文章 API（参照 NovaAdmin ArticleService）。
/// </summary>
[ApiController]
[Route("api/article")]
public sealed class ArticleController : ControllerBase
{
    private readonly IFreeSql freeSql;
    private readonly ILogger<ArticleController> logger;

    public ArticleController(IFreeSql freeSql, ILogger<ArticleController> logger)
    {
        this.freeSql = freeSql;
        this.logger = logger;
    }

    /// <summary>
    /// 获取文章列表。
    /// </summary>
    [HttpGet($"@{nameof(GetAll)}")]
    [AllowAnonymous]
    public async Task<ApiResult<ArticleListResponse>> GetAll()
    {
        logger.LogInformation("Article list request started.");

        List<BlogArticle> articles = await freeSql.Select<BlogArticle>()
            .Include(a => a.Classify)
            .Include(a => a.Channel)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync();

        logger.LogInformation("Article query completed. Count={Count}", articles.Count);

        List<ArticleListItemResponse> items = articles.Select(article => new ArticleListItemResponse
        {
            Id = article.Id,
            Title = article.Title,
            Excerpt = article.Excerpt,
            Content = article.Content,
            ClassifyId = article.ClassifyId,
            ClassifyName = article.Classify?.ClassifyName,
            ChannelId = article.ChannelId,
            ChannelName = article.Channel?.ChannelName,
            IsAudit = article.IsAudit,
            Recommend = article.Recommend,
            IsStickie = article.IsStickie,
            ArticleType = article.ArticleType.ToString(),
            ViewHits = article.ViewHits,
            CommentQuantity = article.CommentQuantity,
            LikesQuantity = article.LikesQuantity,
            CollectQuantity = article.CollectQuantity,
            Thumbnail = article.Thumbnail,
            CreatedTime = article.CreatedTime,
            CreatedUserName = article.CreatedUserName
        }).ToList();

        if (items.Count == 0)
        {
            logger.LogWarning("Article list is empty.");
        }

        return ApiResult<ArticleListResponse>.Success(new ArticleListResponse
        {
            TotalCount = items.Count,
            Items = items
        });
    }
}
