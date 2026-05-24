using System.Net.Mime;
using System.Text.Encodings.Web;
using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Middlewares;

/// <summary>
/// 基于数据库 IP 白名单表的请求拦截中间件。
/// </summary>
public sealed class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipWhitelist(context.Request.Path))
        {
            await _next(context);
            return;
        }

        string clientIp = IpHelper.GetClientIpAddress(context, _logger);
        if (string.IsNullOrWhiteSpace(clientIp) || clientIp == "unknown")
        {
            _logger.LogWarning("IP 白名单校验失败：无法识别客户端 IP，路径：{Path}", context.Request.Path);
            await RejectAsync(context, "unknown");
            return;
        }

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IFreeSql fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();

        List<SysIpWhitelist> enabledEntries = await fsql.Select<SysIpWhitelist>()
            .Where(x => x.IsEnabled)
            .ToListAsync();

        if (enabledEntries.Count == 0)
        {
            await _next(context);
            return;
        }

        SysIpWhitelist? matchedEntry = enabledEntries
            .FirstOrDefault(x => IpHelper.NormalizeIp(x.IpAddress) == clientIp);

        if (matchedEntry is null)
        {
            _logger.LogWarning("IP 白名单拦截：IP={ClientIp}，路径={Path}", clientIp, context.Request.Path);
            await RejectAsync(context, IpHelper.ToIpv4Display(clientIp));
            return;
        }

        try
        {
            await fsql.Update<SysIpWhitelist>()
                .Set(a => a.LastAccessTime, DateTime.Now)
                .Set(a => a.AccessCount, matchedEntry.AccessCount + 1)
                .Where(a => a.Id == matchedEntry.Id)
                .ExecuteAffrowsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IP 白名单命中后更新访问统计失败：IP={ClientIp}", clientIp);
        }

        await _next(context);
    }

    private static bool ShouldSkipWhitelist(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/profile", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase);

    private static async Task RejectAsync(HttpContext context, string clientIpv4)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypeNames.Text.Html + "; charset=utf-8";

        string encodedIp = HtmlEncoder.Default.Encode(clientIpv4);
        await context.Response.WriteAsync($$"""
<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>403 Forbidden</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #f7f8fb;
      --panel: #ffffff;
      --text: #1f2937;
      --muted: #6b7280;
      --line: #e5e7eb;
      --accent: #2563eb;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 32px 16px;
      background: var(--bg);
      color: var(--text);
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
      line-height: 1.7;
    }
    main {
      width: min(680px, 100%);
      padding: 36px;
      border: 1px solid var(--line);
      border-radius: 12px;
      background: var(--panel);
      box-shadow: 0 18px 50px rgba(15, 23, 42, 0.08);
    }
    .code {
      margin: 0 0 18px;
      color: var(--accent);
      font-size: 15px;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }
    h1 {
      margin: 0 0 20px;
      font-size: clamp(26px, 4vw, 38px);
      line-height: 1.2;
    }
    p {
      margin: 0 0 14px;
      font-size: 17px;
    }
    .note {
      margin-top: 26px;
      padding-top: 18px;
      border-top: 1px solid var(--line);
      color: var(--muted);
      font-size: 14px;
    }
  </style>
</head>
<body>
  <main>
    <p class="code">403 Forbidden</p>
    <h1>你访问的页面暂时进不去</h1>
    <p>当前请求没有通过 IP 白名单校验。</p>
    <p class="note">当前客户端 IP：{{encodedIp}}</p>
  </main>
</body>
</html>
""");
    }
}
