using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Entities.Blog;

namespace NeoAdmin.SeedData;

/// <summary>
/// 启动种子调度（业务表结构、菜单种子、演示数据）。
/// </summary>
public static class DataSetup
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        IFreeSql freeSql = serviceProvider.GetRequiredService<IFreeSql>();
        NeoAdminOptions options = serviceProvider.GetRequiredService<IOptions<NeoAdminOptions>>().Value;

        if (options.AutoSyncStructure)
        {
            SyncStructure(freeSql);
        }

        MenuSeedData.Ensure(freeSql);

        if (options.EnableSeedData)
        {
            EnsureSeedData(freeSql, options);
        }
    }

    public static void EnsureSeedData(IFreeSql freeSql, NeoAdminOptions options)
    {
        PageSearchTabSeedData.Ensure();
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
        freeSql.CodeFirst.SyncStructure<SysAuditLog>();
        freeSql.CodeFirst.SyncStructure<SysAuditEntityLog>();
    }
}
