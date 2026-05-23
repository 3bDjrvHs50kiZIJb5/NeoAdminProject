using FreeScheduler;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Attributes;
using NeoAdmin.Entities.Blog;
using BlogArticle = NeoAdmin.Entities.Blog.Article;

namespace NeoAdmin.Jobs;

/// <summary>
/// 博客定时任务（在方法上标记 <see cref="SchedulerAttribute"/> 即可）。
/// </summary>
public static class BlogJobs
{
    /// <summary>
    /// 每天 03:00 同步专栏文章数量。
    /// </summary>
    [Scheduler("blog.sync-article-stats", "0 0 3 * * *")]
    public static async Task SyncArticleStats(IServiceProvider serviceProvider, TaskInfo task)
    {
        IFreeSql freeSql = serviceProvider.GetRequiredService<IFreeSql>();
        ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(BlogJobs));

        logger.LogInformation("SyncArticleStats 开始，TaskId={TaskId}", task.Id);

        List<Classify> classifies = await freeSql.Select<Classify>().ToListAsync();
        foreach (Classify classify in classifies)
        {
            long count = await freeSql.Select<BlogArticle>()
                .Where(a => a.ClassifyId == classify.Id)
                .CountAsync();

            await freeSql.Update<Classify>()
                .Where(a => a.Id == classify.Id)
                .Set(a => a.ArticleCount, (int)count)
                .ExecuteAffrowsAsync();
        }

        logger.LogInformation("SyncArticleStats 完成，专栏数={Count}", classifies.Count);
    }
}
