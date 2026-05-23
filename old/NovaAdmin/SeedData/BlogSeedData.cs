using BootstrapBlazor.Components;
using FreeSql;
using LinCms.Entities.Blog;

namespace NovaAdmin.SeedData;

/// <summary>
/// 博客示例种子数据 - 用于初始化博客演示数据
/// </summary>
public static class BlogSeedData
{
    private const long ArticleIdBase = 510359705468997;
    private const long CommentIdBase = 510365667639365;
    private const long UserLikeIdBase = 510365571252293;

    /// <summary>
    /// 初始化博客示例数据
    /// </summary>
    public static void Initialize(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        // 逐表判断，避免因为菜单已存在就跳过博客数据写入。
        if (!fsql.Select<Classify>().Any())
        {
            InsertClassifies(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Channel>().Any())
        {
            InsertChannels(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Tag2>().Any())
        {
            InsertTags(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Collection>().Any())
        {
            InsertCollections(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Article>().Any())
        {
            InsertArticles(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Tag2.ChannelTag2>().Any())
        {
            InsertChannelTags(fsql);
        }

        if (!fsql.Select<Article.ArticleCollection>().Any())
        {
            InsertArticleCollections(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<Tag2.TagArticle>().Any())
        {
            InsertArticleTags(fsql);
        }

        if (!fsql.Select<Comment>().Any())
        {
            InsertComments(fsql, adminUserId, adminUsername);
        }

        if (!fsql.Select<UserLike>().Any())
        {
            InsertUserLikes(fsql, adminUserId, adminUsername);
        }
    }

    private static void InsertClassifies(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        fsql.Insert(new[]
        {
            new Classify { Id = 510337284071493, ClassifyName = "FreeSql", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337332621381, ClassifyName = "FreeRedis", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337373491269, ClassifyName = "FreeScheduler", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337418735685, ClassifyName = "CSRedis", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337460719685, ClassifyName = "NovaAdmin", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337512158469, ClassifyName = "Blazor", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337568214021, ClassifyName = "ASP.NET Core", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Classify { Id = 510337623914517, ClassifyName = "前端工程化", CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertChannels(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        fsql.Insert(new[]
        {
            new Channel { Id = 510338108866629, ChannelName = ".NET", ChannelCode = "net", Remark = ".NET技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338191179845, ChannelName = "前端", ChannelCode = "html", Remark = "前端技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338291052613, ChannelName = "数据库", ChannelCode = "db", Remark = "数据库技术频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338365388837, ChannelName = "架构", ChannelCode = "arch", Remark = "架构设计频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Channel { Id = 510338442917381, ChannelName = "运维", ChannelCode = "ops", Remark = "运维与部署频道", Status = true, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertTags(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        fsql.Insert(new[]
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

    private static void InsertChannelTags(FreeSqlCloud fsql)
    {
        fsql.Insert(new[]
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

    private static void InsertCollections(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        fsql.Insert(new[]
        {
            new Collection { Id = 510343691022405, Name = "年度最佳", Remark = "年度精华内容", PrivacyType = PrivacyType.公开可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343769964613, Name = "月度最佳", Remark = "每月精华内容", PrivacyType = PrivacyType.仅自己可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343845170181, Name = "技术笔记", Remark = "技术实践记录", PrivacyType = PrivacyType.公开可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
            new Collection { Id = 510343912843733, Name = "草稿精选", Remark = "值得整理的草稿", PrivacyType = PrivacyType.仅自己可见, CreatedUserId = adminUserId, CreatedUserName = adminUsername },
        }).ExecuteAffrows();
    }

    private static void InsertArticles(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var topics = new[]
        {
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
        };

        var classifyIds = new[]
        {
            510337284071493L,
            510337332621381L,
            510337373491269L,
            510337418735685L,
            510337460719685L,
            510337512158469L,
            510337568214021L,
            510337623914517L,
        };

        var channelIds = new[]
        {
            510338108866629L,
            510338191179845L,
            510338291052613L,
            510338365388837L,
            510338442917381L,
        };

        var articles = Enumerable.Range(1, 50)
            .Select(index =>
            {
                var topic = topics[(index - 1) % topics.Length];
                var classifyId = classifyIds[(index - 1) % classifyIds.Length];
                var channelId = channelIds[(index - 1) % channelIds.Length];
                var articleId = ArticleIdBase + index;

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
                    CreatedUserName = adminUsername
                };
            })
            .ToArray();

        fsql.Insert(articles).ExecuteAffrows();
    }

    private static void InsertArticleCollections(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var items = Enumerable.Range(1, 50)
            .Select(index => new
            {
                ArticleId = ArticleIdBase + index,
                CollectionId = index <= 20 ? 510343691022405L : index <= 35 ? 510343845170181L : 510343769964613L
            })
            .Select(x => new Article.ArticleCollection
            {
                ArticleId = x.ArticleId,
                CollectionId = x.CollectionId,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            })
            .ToArray();

        fsql.Insert(items).ExecuteAffrows();
    }

    private static void InsertArticleTags(FreeSqlCloud fsql)
    {
        var tagIds = new[]
        {
            510340412510277L,
            510340482543685L,
            510340574564421L,
            510340626989125L,
            510340691176837L,
            510340752112933L,
            510340813771621L,
            510340875394437L,
        };

        var items = Enumerable.Range(1, 50)
            .SelectMany(index => new[]
            {
                new Tag2.TagArticle
                {
                    ArticleId = ArticleIdBase + index,
                    TagId = tagIds[(index - 1) % tagIds.Length]
                },
                new Tag2.TagArticle
                {
                    ArticleId = ArticleIdBase + index,
                    TagId = tagIds[index % tagIds.Length]
                }
            })
            .ToArray();

        fsql.Insert(items).ExecuteAffrows();
    }

    private static void InsertComments(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var comments = Enumerable.Range(1, 20)
            .Select(index => new Comment
            {
                Id = CommentIdBase + index,
                ArticleId = ArticleIdBase + index,
                Text = $"这是一条第 {index:00} 篇文章的示例评论，适合演示评论列表和审核流程。",
                IsAudit = index % 3 != 0,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            })
            .ToArray();

        fsql.Insert(comments).ExecuteAffrows();
    }

    private static void InsertUserLikes(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var articleLikes = Enumerable.Range(1, 20)
            .Select(index => new UserLike
            {
                Id = UserLikeIdBase + index,
                SubjectId = ArticleIdBase + index,
                SubjectType = UserLikeSubjectType.点赞随笔,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            });

        var commentLikes = Enumerable.Range(1, 10)
            .Select(index => new UserLike
            {
                Id = UserLikeIdBase + 100 + index,
                SubjectId = CommentIdBase + index,
                SubjectType = UserLikeSubjectType.点赞评论,
                CreatedUserId = adminUserId,
                CreatedUserName = adminUsername
            });

        fsql.Insert(articleLikes.Concat(commentLikes).ToArray()).ExecuteAffrows();
    }
}
