using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace NeoAdmin.Blazor.Extensions;

/// <summary>
/// 在运行时替换 NeoUI <c>input.js</c>，避免 IME 组合输入期间向 Blazor Server 同步中间拼音。
/// 同时拦截 MapStaticAssets 生成的指纹文件名（如 <c>input.g3or5i3j54.js</c>），否则 import map
/// 会绕过补丁直接加载未补丁的原始文件。
/// </summary>
public static class NeoAdminImeInputPatchExtensions
{
    // 匹配 /_content/NeoUI.Blazor/js/input.js、input.js.gz 以及带指纹的 input.<hash>.js(.gz)
    private static readonly Regex NeoUIInputPathRegex = new(
        @"^/_content/NeoUI\.Blazor/js/input(\.[\w-]+)?\.js(?<gz>\.gz)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly EmbeddedFileProvider PatchFileProvider = new(
        typeof(NeoAdminImeInputPatchExtensions).Assembly,
        "NeoAdmin.Blazor.wwwroot.patches");

    private static byte[]? _patchBytes;
    private static byte[]? _patchGzipBytes;

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

        _patchBytes = ReadAllBytes(patchFile);
        _patchGzipBytes = Gzip(_patchBytes);

        ILogger startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(NeoAdminImeInputPatchExtensions));
        startupLogger.LogInformation(
            "已加载 NeoUI input.js IME 补丁，Length={Length}",
            _patchBytes.Length);

        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                await next();
                return;
            }

            string path = context.Request.Path.Value ?? string.Empty;
            Match match = NeoUIInputPathRegex.Match(path);
            if (match.Success)
            {
                bool gzip = match.Groups["gz"].Success;
                await WritePatchAsync(context, gzip);
                return;
            }

            await next();
        });

        return app;
    }

    private static async Task WritePatchAsync(HttpContext context, bool gzip)
    {
        byte[] body = gzip ? _patchGzipBytes! : _patchBytes!;

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
        context.Response.Headers["X-NeoAdmin-Ime-Patch"] = "1";
        context.Response.ContentLength = body.Length;

        if (gzip)
        {
            context.Response.ContentType = "application/x-gzip";
        }
        else
        {
            context.Response.ContentType = "text/javascript; charset=utf-8";
        }

        await context.Response.Body.WriteAsync(body);
    }

    private static byte[] ReadAllBytes(IFileInfo file)
    {
        using MemoryStream ms = new();
        using Stream stream = file.CreateReadStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static byte[] Gzip(byte[] input)
    {
        using MemoryStream output = new();
        using (GZipStream gzip = new(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(input, 0, input.Length);
        }

        return output.ToArray();
    }
}
