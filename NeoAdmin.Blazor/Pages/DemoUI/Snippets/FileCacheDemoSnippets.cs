namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class FileCacheDemoSnippets
{
    public const string Usage = """
        [FileCache(60, CacheDirectory = "Cache/FileCache.Tests")]
        private string BuildCacheResult(string a)
        {
            var currentBuildCount = Interlocked.Increment(ref buildCount);
            return $"input={a};buildCount={currentBuildCount};cacheKey=page:{a}";
        }
        """;

    public const string CleanupRules = """
        最直白地说：

        “过期了还被访问” -> 立刻删
        “过期了但没人访问” -> 下次有人进缓存逻辑时，按 10 分钟节流统一扫掉
        “文件坏了” -> 也删
        """;

    public const string Behavior = """
        同一个参数：第一次会执行方法体并生成缓存
        同一个参数：第二次会直接命中缓存
        参数变化：会生成新的缓存文件
        """;
}
