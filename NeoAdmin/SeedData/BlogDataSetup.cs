using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Entities.Blog;

namespace NeoAdmin.SeedData;

/// <summary>
/// 博客表结构同步与种子数据（宿主项目专用）。
/// </summary>
public static class BlogDataSetup
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        IFreeSql freeSql = serviceProvider.GetRequiredService<IFreeSql>();
        NeoAdminOptions options = serviceProvider.GetRequiredService<IOptions<NeoAdminOptions>>().Value;

        if (options.AutoSyncStructure)
        {
            SyncStructure(freeSql);
        }

        BlogMenuSeedData.Ensure(freeSql);
        BlogSeedData.Ensure(freeSql, options);
    }

    public static void SyncStructure(IFreeSql freeSql)
    {
        freeSql.CodeFirst.SyncStructure<Classify>();
        freeSql.CodeFirst.SyncStructure<Channel>();
        freeSql.CodeFirst.SyncStructure<Tag2>();
        freeSql.CodeFirst.SyncStructure<Collection>();
        freeSql.CodeFirst.SyncStructure<Article>();
        freeSql.CodeFirst.SyncStructure<Comment>();
        freeSql.CodeFirst.SyncStructure<UserLike>();
        freeSql.CodeFirst.SyncStructure<Tag2.TagArticle>();
        freeSql.CodeFirst.SyncStructure<Tag2.ChannelTag2>();
        freeSql.CodeFirst.SyncStructure<Article.ArticleCollection>();
    }
}
