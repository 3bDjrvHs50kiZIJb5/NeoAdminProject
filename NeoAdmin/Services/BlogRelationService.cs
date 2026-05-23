using FreeSql;
using NeoAdmin.Entities.Blog;

namespace NeoAdmin.Services;

/// <summary>
/// 博客多对多关联读写。
/// </summary>
public static class BlogRelationService
{
    /// 获取栏目已关联的标签 ID 列表。
    public static async Task<IReadOnlyCollection<long>> GetChannelTagIdsAsync(IFreeSql freeSql, long channelId) =>
        await freeSql.Select<Tag2.ChannelTag2>()
            .Where(a => a.ChannelId == channelId)
            .ToListAsync(a => a.TagId);

    /// 保存栏目与标签关联（先清空再按传入 ID 重建）。
    public static async Task SaveChannelTagsAsync(IFreeSql freeSql, long channelId, IReadOnlyCollection<long> tagIds)
    {
        await freeSql.Delete<Tag2.ChannelTag2>().Where(a => a.ChannelId == channelId).ExecuteAffrowsAsync();
        if (tagIds.Count == 0)
        {
            return;
        }

        await freeSql.Insert(tagIds.Select(tagId => new Tag2.ChannelTag2
        {
            ChannelId = channelId,
            TagId = tagId,
        })).ExecuteAffrowsAsync();
    }

    /// 获取合集已关联的文章 ID 列表。
    public static async Task<IReadOnlyCollection<long>> GetCollectionArticleIdsAsync(IFreeSql freeSql, long collectionId) =>
        await freeSql.Select<Article.ArticleCollection>()
            .Where(a => a.CollectionId == collectionId)
            .ToListAsync(a => a.ArticleId);

    /// 保存合集与文章关联（先清空再按传入 ID 重建，并记录创建人）。
    public static async Task SaveCollectionArticlesAsync(
        IFreeSql freeSql,
        long collectionId,
        IReadOnlyCollection<long> articleIds,
        long? userId,
        string userName)
    {
        await freeSql.Delete<Article.ArticleCollection>()
            .Where(a => a.CollectionId == collectionId)
            .ExecuteAffrowsAsync();

        if (articleIds.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.Now;
        await freeSql.Insert(articleIds.Select(articleId => new Article.ArticleCollection
        {
            ArticleId = articleId,
            CollectionId = collectionId,
            CreatedUserId = userId,
            CreatedUserName = userName,
            CreatedTime = now,
        })).ExecuteAffrowsAsync();
    }

    /// 获取文章已关联的标签 ID 列表。
    public static async Task<IReadOnlyCollection<long>> GetArticleTagIdsAsync(IFreeSql freeSql, long articleId) =>
        await freeSql.Select<Tag2.TagArticle>()
            .Where(a => a.ArticleId == articleId)
            .ToListAsync(a => a.TagId);

    /// 保存文章与标签关联（先清空再按传入 ID 重建）。
    public static async Task SaveArticleTagsAsync(IFreeSql freeSql, long articleId, IReadOnlyCollection<long> tagIds)
    {
        await freeSql.Delete<Tag2.TagArticle>().Where(a => a.ArticleId == articleId).ExecuteAffrowsAsync();
        if (tagIds.Count == 0)
        {
            return;
        }

        await freeSql.Insert(tagIds.Select(tagId => new Tag2.TagArticle
        {
            ArticleId = articleId,
            TagId = tagId,
        })).ExecuteAffrowsAsync();
    }

    /// 获取文章已关联的合集 ID 列表。
    public static async Task<IReadOnlyCollection<long>> GetArticleCollectionIdsAsync(IFreeSql freeSql, long articleId) =>
        await freeSql.Select<Article.ArticleCollection>()
            .Where(a => a.ArticleId == articleId)
            .ToListAsync(a => a.CollectionId);

    /// 保存文章与合集关联（先清空再按传入 ID 重建，并记录创建人）。
    public static async Task SaveArticleCollectionsAsync(
        IFreeSql freeSql,
        long articleId,
        IReadOnlyCollection<long> collectionIds,
        long? userId,
        string userName)
    {
        await freeSql.Delete<Article.ArticleCollection>()
            .Where(a => a.ArticleId == articleId)
            .ExecuteAffrowsAsync();

        if (collectionIds.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.Now;
        await freeSql.Insert(collectionIds.Select(collectionId => new Article.ArticleCollection
        {
            ArticleId = articleId,
            CollectionId = collectionId,
            CreatedUserId = userId,
            CreatedUserName = userName,
            CreatedTime = now,
        })).ExecuteAffrowsAsync();
    }

    /// 获取标签已关联的栏目 ID 列表。
    public static async Task<IReadOnlyCollection<long>> GetTagChannelIdsAsync(IFreeSql freeSql, long tagId) =>
        await freeSql.Select<Tag2.ChannelTag2>()
            .Where(a => a.TagId == tagId)
            .ToListAsync(a => a.ChannelId);

    /// 保存标签与栏目关联（先清空再按传入 ID 重建）。
    public static async Task SaveTagChannelsAsync(IFreeSql freeSql, long tagId, IReadOnlyCollection<long> channelIds)
    {
        await freeSql.Delete<Tag2.ChannelTag2>().Where(a => a.TagId == tagId).ExecuteAffrowsAsync();
        if (channelIds.Count == 0)
        {
            return;
        }

        await freeSql.Insert(channelIds.Select(channelId => new Tag2.ChannelTag2
        {
            ChannelId = channelId,
            TagId = tagId,
        })).ExecuteAffrowsAsync();
    }

    /// 按 ID 批量加载标签实体（空集合时返回空列表）。
    public static async Task<List<Tag2>> LoadTagsByIdsAsync(IFreeSql freeSql, IReadOnlyCollection<long> tagIds)
    {
        if (tagIds.Count == 0)
        {
            return [];
        }

        return await freeSql.Select<Tag2>().Where(t => tagIds.Contains(t.Id)).ToListAsync();
    }

    /// 按 ID 批量加载合集实体（空集合时返回空列表）。
    public static async Task<List<Collection>> LoadCollectionsByIdsAsync(IFreeSql freeSql, IReadOnlyCollection<long> collectionIds)
    {
        if (collectionIds.Count == 0)
        {
            return [];
        }

        return await freeSql.Select<Collection>().Where(c => collectionIds.Contains(c.Id)).ToListAsync();
    }

    /// 按 ID 批量加载栏目实体（空集合时返回空列表）。
    public static async Task<List<Channel>> LoadChannelsByIdsAsync(IFreeSql freeSql, IReadOnlyCollection<long> channelIds)
    {
        if (channelIds.Count == 0)
        {
            return [];
        }

        return await freeSql.Select<Channel>().Where(c => channelIds.Contains(c.Id)).ToListAsync();
    }
}
