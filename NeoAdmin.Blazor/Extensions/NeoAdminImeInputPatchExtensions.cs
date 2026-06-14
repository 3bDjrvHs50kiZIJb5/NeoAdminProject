using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace NeoAdmin.Blazor.Extensions;

/// <summary>
/// 在运行时替换 NeoUI <c>input.js</c>，避免 IME 组合输入期间向 Blazor Server 同步中间拼音。
/// </summary>
public static class NeoAdminImeInputPatchExtensions
{
    private const string NeoUIInputRequestPath = "/_content/NeoUI.Blazor/js/input.js";

    private static readonly EmbeddedFileProvider PatchFileProvider = new(
        typeof(NeoAdminImeInputPatchExtensions).Assembly,
        "NeoAdmin.Blazor.wwwroot.patches");

    /// <summary>
    /// 在 UseStaticFiles / MapStaticAssets 之前调用。
    /// </summary>
    public static WebApplication UseNeoAdminImeInputPatch(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        IFileInfo patchFile = PatchFileProvider.GetFileInfo("neoui-input.js");
        if (!patchFile.Exists)
        {
            ILogger logger = app.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(NeoAdminImeInputPatchExtensions));
            logger.LogWarning("未找到 NeoUI input.js IME 补丁文件");
            return app;
        }

        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method)
                || !context.Request.Path.Equals(NeoUIInputRequestPath, StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            context.Response.ContentType = "text/javascript; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache";
            await using Stream stream = patchFile.CreateReadStream();
            await stream.CopyToAsync(context.Response.Body);
        });

        return app;
    }
}
