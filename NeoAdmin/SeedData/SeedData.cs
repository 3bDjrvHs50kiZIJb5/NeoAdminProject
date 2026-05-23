using FreeSql;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Data.Entities;
using NeoAdmin.Entities.Blog;

namespace NeoAdmin.SeedData;

/// <summary>
/// 博客示例种子数据。
/// </summary>
public static class SeedData
{
    private const long ArticleIdBase = 510359705468997;
    private const long CommentIdBase = 510365667639365;
    private const long UserLikeIdBase = 510365571252293;

    public static void Ensure(IFreeSql freeSql, NeoAdminOptions options)
    {
        SysUser? admin = freeSql.Select<SysUser>()
            .Where(a => a.Username == options.SeedAdminUserName)
            .First();

        if (admin is null)
        {
            return;
        }

        Initialize(freeSql, admin.Id, admin.Username);
    }

    public static void Initialize(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        if (!freeSql.Select<Classify>().Any())
        {
            InsertClassifies(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Channel>().Any())
        {
            InsertChannels(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Tag2>().Any())
        {
            InsertTags(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Collection>().Any())
        {
            InsertCollections(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Article>().Any())
        {
            InsertArticles(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Tag2.ChannelTag2>().Any())
        {
            InsertChannelTags(freeSql);
        }

        if (!freeSql.Select<Article.ArticleCollection>().Any())
        {
            InsertArticleCollections(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<Tag2.TagArticle>().Any())
        {
            InsertArticleTags(freeSql);
        }

        if (!freeSql.Select<Comment>().Any())
        {
            InsertComments(freeSql, adminUserId, adminUsername);
        }

        if (!freeSql.Select<UserLike>().Any())
        {
            InsertUserLikes(freeSql, adminUserId, adminUsername);
        }
    }

    private static void InsertClassifies(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        freeSql.Insert(new[]
        {
            new Classify { Id = 510337284071493, ClassifyName = "FreeSql", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337332621381, ClassifyName = "FreeRedis", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337373491269, ClassifyName = "FreeScheduler", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337418735685, ClassifyName = "CSRedis", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337460719685, ClassifyName = "NeoAdmin", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337512158469, ClassifyName = "Blazor", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337568214021, ClassifyName = "ASP.NET Core", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337623914517, ClassifyName = "前端工程化", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertChannels(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        freeSql.Insert(new[]
        {
            new Channel { Id = 510338108866629, ChannelName = ".NET", ChannelCode = "net", Remark = ".NET技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338191179845, ChannelName = "前端", ChannelCode = "html", Remark = "前端技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338291052613, ChannelName = "数据库", ChannelCode = "db", Remark = "数据库技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338365388837, ChannelName = "架构", ChannelCode = "arch", Remark = "架构设计频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338442917381, ChannelName = "运维", ChannelCode = "ops", Remark = "运维与部署频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertTags(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        freeSql.Insert(new[]
        {
            new Tag2 { Id = 510340412510277, TagName = "orm", Remark = "orm 文章内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340482543685, TagName = "js", Remark = "js 有关内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340574564421, TagName = "vue", Remark = "vue 有关内容", Status = false, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340626989125, TagName = "react", Remark = "react 技术", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340691176837, TagName = "blazor", Remark = "blazor 相关内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340752112933, TagName = "api", Remark = "接口与服务端内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340813771621, TagName = "sql", Remark = "数据库与 SQL 内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Tag2 { Id = 510340875394437, TagName = "deploy", Remark = "部署与发布内容", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertChannelTags(IFreeSql freeSql)
    {
        freeSql.Insert(new[]
        {
            new Tag2.ChannelTag2 { ChannelId = 510338108866629, TagId = 510340412510277 },
            new Tag2.ChannelTag2 { ChannelId = 510338108866629, TagId = 510340752112933 },
            new Tag2.ChannelTag2 { ChannelId = 510338291052613, TagId = 510340412510277 },
            new Tag2.ChannelTag2 { ChannelId = 510338291052613, TagId = 510340813771621 },
            new Tag2.ChannelTag2 { ChannelId = 510338191179845, TagId = 510340482543685 },
            new Tag2.ChannelTag2 { ChannelId = 510338191179845, TagId = 510340574564421 },
            new Tag2.ChannelTag2 { ChannelId = 510338191179845, TagId = 510340626989125 },
            new Tag2.ChannelTag2 { ChannelId = 510338191179845, TagId = 510340691176837 },
            new Tag2.ChannelTag2 { ChannelId = 510338365388837, TagId = 510340752112933 },
            new Tag2.ChannelTag2 { ChannelId = 510338365388837, TagId = 510340875394437 },
            new Tag2.ChannelTag2 { ChannelId = 510338442917381, TagId = 510340875394437 },
            new Tag2.ChannelTag2 { ChannelId = 510338442917381, TagId = 510340813771621 },
        }).ExecuteAffrows();
    }

    private static void InsertCollections(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        freeSql.Insert(new[]
        {
            new Collection { Id = 510343691022405, Name = "年度最佳", Remark = "年度精华内容", PrivacyType = PrivacyType.公开可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343769964613, Name = "月度最佳", Remark = "每月精华内容", PrivacyType = PrivacyType.仅自己可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343845170181, Name = "技术笔记", Remark = "技术实践记录", PrivacyType = PrivacyType.公开可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343912843733, Name = "草稿精选", Remark = "值得整理的草稿", PrivacyType = PrivacyType.仅自己可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertArticles(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        string[] topics =
        [
            "FreeSql 入门实战",
            "Repository 模式整理",
            "Blazor 页面布局技巧",
            "前端组件复用经验",
            "ASP.NET Core 接口设计",
            "SQL 调优小技巧",
            "发布与部署流程",
            "缓存与性能优化",
            "后台管理系统设计",
            "领域模型拆分思路",
        ];

        long[] classifyIds =
        [
            510337284071493L,
            510337332621381L,
            510337373491269L,
            510337418735685L,
            510337460719685L,
            510337512158469L,
            510337568214021L,
            510337623914517L,
        ];

        long[] channelIds =
        [
            510338108866629L,
            510338191179845L,
            510338291052613L,
            510338365388837L,
            510338442917381L,
        ];

        Article[] articles = Enumerable.Range(1, 50)
            .Select(index =>
            {
                string topic = topics[(index - 1) % topics.Length];
                long classifyId = classifyIds[(index - 1) % classifyIds.Length];
                long channelId = channelIds[(index - 1) % channelIds.Length];
                long articleId = ArticleIdBase + index;

                return new Article
                {
                    Id = articleId,
                    ClassifyId = classifyId,
                    ChannelId = channelId,
                    Title = $"模拟文章 {index:00}：{topic}",
                    Excerpt = $"这是第 {index:00} 篇示例文章，主题是 {topic}。",
                    Content = $"这是第 {index:00} 篇博客模拟文章，用于演示文章列表、分类筛选、标签关联、收藏和评论等功能。主题：{topic}。\n\n这里可以放更长的正文内容，模拟真实的博客文章长度。",
                    IsAudit = index % 4 != 0,
                    CreatedUserId = adminUserId,
                    CreatedUserName = adminUsername,
                };
            })
            .ToArray();

        freeSql.Insert(articles).ExecuteAffrows();
    }

    private static void InsertArticleCollections(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        Article.ArticleCollection[] items = Enumerable.Range(1, 50)
            .Select(index => new
            {
                ArticleId = ArticleIdBase + index,
                CollectionId = index <= 20 ? 510343691022405L : index <= 35 ? 510343845170181L : 510343769964613L,
            })
            .Select(x => new Article.ArticleCollection
            {
                ArticleId = x.ArticleId,
                CollectionId = x.CollectionId,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername,
            })
            .ToArray();

        freeSql.Insert(items).ExecuteAffrows();
    }

    private static void InsertArticleTags(IFreeSql freeSql)
    {
        long[] tagIds =
        [
            510340412510277L,
            510340482543685L,
            510340574564421L,
            510340626989125L,
            510340691176837L,
            510340752112933L,
            510340813771621L,
            510340875394437L,
        ];

        Tag2.TagArticle[] items = Enumerable.Range(1, 50)
            .SelectMany(index => new[]
            {
                new Tag2.TagArticle
                {
                    ArticleId = ArticleIdBase + index,
                    TagId = tagIds[(index - 1) % tagIds.Length],
                },
                new Tag2.TagArticle
                {
                    ArticleId = ArticleIdBase + index,
                    TagId = tagIds[index % tagIds.Length],
                },
            })
            .ToArray();

        freeSql.Insert(items).ExecuteAffrows();
    }

    private static void InsertComments(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        Comment[] comments = Enumerable.Range(1, 20)
            .Select(index => new Comment
            {
                Id = CommentIdBase + index,
                ArticleId = ArticleIdBase + index,
                Text = $"这是一条第 {index:00} 篇文章的示例评论，适合演示评论列表和审核流程。",
                IsAudit = index % 3 != 0,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername,
            })
            .ToArray();

        freeSql.Insert(comments).ExecuteAffrows();
    }

    private static void InsertUserLikes(IFreeSql freeSql, long adminUserId, string adminUsername)
    {
        IEnumerable<UserLike> articleLikes = Enumerable.Range(1, 20)
            .Select(index => new UserLike
            {
                Id = UserLikeIdBase + index,
                SubjectId = ArticleIdBase + index,
                SubjectType = UserLikeSubjectType.点赞随笔,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername,
            });

        IEnumerable<UserLike> commentLikes = Enumerable.Range(1, 10)
            .Select(index => new UserLike
            {
                Id = UserLikeIdBase + 100 + index,
                SubjectId = CommentIdBase + index,
                SubjectType = UserLikeSubjectType.点赞评论,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername,
            });

        freeSql.Insert(articleLikes.Concat(commentLikes).ToArray()).ExecuteAffrows();
    }
}
