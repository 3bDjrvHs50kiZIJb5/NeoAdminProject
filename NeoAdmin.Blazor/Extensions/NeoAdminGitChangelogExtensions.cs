using Microsoft.Extensions.DependencyInjection;
using NeoAdmin.Blazor.Services;

namespace NeoAdmin.Blazor.Extensions;

public static class NeoAdminGitChangelogExtensions
{
    /// <summary>
    /// 注册 Git 更新日志服务：开发环境读 Git 提交，生产环境可回退到 <c>Data/git-changelog.json</c>。
    /// </summary>
    public static IServiceCollection AddNeoAdminGitChangelog(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<INeoGitChangelogService, NeoGitChangelogService>();
        return services;
    }
}
