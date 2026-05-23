using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace NeoAdmin.Blazor.Middlewares;

public static class IpWhitelistApplicationBuilderExtensions
{
    public static WebApplication UseIpWhitelist(this WebApplication app)
    {
        app.UseWhen(
            context => ShouldApplyIpWhitelist(context.Request.Path),
            branch => branch.UseMiddleware<IpWhitelistMiddleware>());

        return app;
    }

    private static bool ShouldApplyIpWhitelist(PathString path)
    {
        if (!path.HasValue)
        {
            return true;
        }

        if (IsPublicAssetPath(path))
        {
            return false;
        }

        return !path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
               && !path.StartsWithSegments("/profile", StringComparison.OrdinalIgnoreCase)
               && !path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase)
               && !path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPublicAssetPath(PathString path)
    {
        if (path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/avatars", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/_content", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string value = path.Value ?? string.Empty;
        return value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".ico", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".map", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".woff", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)
               || value.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase);
    }
}
