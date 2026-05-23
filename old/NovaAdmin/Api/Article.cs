using BootstrapBlazor.Components;
using FreeScheduler;
using FreeSql;
using LinCms.Entities.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NovaAdmin.API;

[ApiController]
[Route("api/article")]
[Tags("文章接口")]
public class ArticleService : BaseService
{
    /// <summary>
    /// 初始化服务
    /// </summary>
    public ArticleService(
        FreeSqlCloud freeSqlCloud,
        Scheduler scheduler,
        ILogger<ArticleService> logger,
        NovaAdminContext adminContext,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
        : base(freeSqlCloud, scheduler, logger, adminContext, configuration, webHostEnvironment)
    {
    }

    /// <summary>
    /// 获取文章列表
    /// </summary>
    [HttpGet($"@{nameof(GetAll)}")]
    [AllowAnonymous]
    public async Task<ApiResult> GetAll()
    {
        _logger.LogInformation("Article list request started.");

        var articles = await CurrentOrm.Select<Article>()
            .Include(a => a.Classify)
            .Include(a => a.Channel)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync();

        _logger.LogInformation("Article query completed. Count={Count}", articles.Count);

        var items = articles.Select(article => new ArticleListItemResponse
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
            })
            .ToList();

        if (items.Count == 0)
        {
            _logger.LogWarning("Article list is empty.");
        }
        else
        {
            _logger.LogInformation("Article list response prepared. FirstArticleId={ArticleId}", items[0].Id);
        }

        return ApiResult.Success.SetData(new ArticleListResponse
        {
            TotalCount = items.Count,
            Items = items
        });
    }
}

public sealed class ArticleListResponse
{
    public int TotalCount { get; set; }

    public List<ArticleListItemResponse> Items { get; set; } = new();
}

public sealed class ArticleListItemResponse
{
    public long Id { get; set; }

    public string? Title { get; set; }

    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    public long? ClassifyId { get; set; }

    public string? ClassifyName { get; set; }

    public long ChannelId { get; set; }

    public string? ChannelName { get; set; }

    public bool IsAudit { get; set; }

    public bool Recommend { get; set; }

    public bool IsStickie { get; set; }

    public string? ArticleType { get; set; }

    public int ViewHits { get; set; }

    public int CommentQuantity { get; set; }

    public int LikesQuantity { get; set; }

    public int CollectQuantity { get; set; }

    public string? Thumbnail { get; set; }

    public DateTime? CreatedTime { get; set; }

    public string? CreatedUserName { get; set; }
}
