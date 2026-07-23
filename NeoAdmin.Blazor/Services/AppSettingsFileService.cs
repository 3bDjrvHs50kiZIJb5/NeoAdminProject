using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 读写宿主 ContentRoot 下白名单 appsettings 文件，并与固定字段表单互转。
/// </summary>
public sealed class AppSettingsFileService
{
    public static IReadOnlyList<AppSettingsFileInfo> Files { get; } =
    [
        new("appsettings.json", "基础 (json)", Optional: false),
        new("appsettings.Development.json", "开发 (Development)", Optional: true),
        new("appsettings.Production.json", "生产 (Production)", Optional: true),
    ];

    private static readonly HashSet<string> AllowedFileNames = new(
        Files.Select(f => f.FileName),
        StringComparer.OrdinalIgnoreCase);

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AppSettingsFileService> _logger;

    public AppSettingsFileService(
        IWebHostEnvironment environment,
        ILogger<AppSettingsFileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public bool FileExists(string fileName)
    {
        string path = ResolvePath(fileName);
        return File.Exists(path);
    }

    public async Task<AppSettingsFormModel> LoadFormAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        string path = ResolvePath(fileName);
        if (!File.Exists(path))
        {
            AppSettingsFileInfo info = Files.First(f =>
                string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
            if (!info.Optional)
            {
                throw new FileNotFoundException($"配置文件不存在：{fileName}", path);
            }

            _logger.LogInformation("配置文件不存在，返回空表单，FileName={FileName}", fileName);
            return new AppSettingsFormModel();
        }

        string json = await File.ReadAllTextAsync(path, cancellationToken);
        JsonNode? root = AppSettingsJsonMapper.ParseOrEmpty(json);
        _logger.LogInformation("已加载配置文件，FileName={FileName}", fileName);
        return AppSettingsJsonMapper.ToForm(root);
    }

    public async Task SaveFormAsync(
        string fileName,
        AppSettingsFormModel form,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(form);

        if (form.WorkId is < 0 or > 63)
        {
            throw new InvalidOperationException("WorkId 必须在 0–63 之间。");
        }

        NormalizeForSave(form);

        string path = ResolvePath(fileName);
        JsonNode? existing = null;
        if (File.Exists(path))
        {
            string current = await File.ReadAllTextAsync(path, cancellationToken);
            existing = AppSettingsJsonMapper.ParseOrEmpty(current);
        }

        string output = AppSettingsJsonMapper.MergeAndSerialize(existing, form);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, output, cancellationToken);
        _logger.LogInformation("已保存配置文件，FileName={FileName}", fileName);
    }

    /// <summary>
    /// 非密钥字符串空白视为未配置（不写入）；密钥字段保留空串以便清除。
    /// </summary>
    private static void NormalizeForSave(AppSettingsFormModel form)
    {
        form.DataType = NullIfWhiteSpace(form.DataType);
        form.ConnectionString = NullIfWhiteSpace(form.ConnectionString);
        form.HomePath = NullIfWhiteSpace(form.HomePath);
        form.SeedAdminUserName = NullIfWhiteSpace(form.SeedAdminUserName);
        form.SwaggerHides = NullIfWhiteSpace(form.SwaggerHides);
        form.LogDirectory = NullIfWhiteSpace(form.LogDirectory);
        form.LogFilePrefix = NullIfWhiteSpace(form.LogFilePrefix);
        form.FileUploadDirectory = NullIfWhiteSpace(form.FileUploadDirectory);
        form.FileUploadDateTimeDirectory = NullIfWhiteSpace(form.FileUploadDateTimeDirectory);
        form.FileUploadIncludeExtension = NullIfWhiteSpace(form.FileUploadIncludeExtension);
        form.FileUploadExcludeExtension = NullIfWhiteSpace(form.FileUploadExcludeExtension);
        form.OssEndpoint = NullIfWhiteSpace(form.OssEndpoint);
        form.OssBucketName = NullIfWhiteSpace(form.OssBucketName);
        form.OssCustomDomain = NullIfWhiteSpace(form.OssCustomDomain);
        form.OssPrefix = NullIfWhiteSpace(form.OssPrefix);
        form.DashScopeRealtimeModel = NullIfWhiteSpace(form.DashScopeRealtimeModel);
        form.LoggingDefault = NullIfWhiteSpace(form.LoggingDefault);
        form.LoggingAspNetCore = NullIfWhiteSpace(form.LoggingAspNetCore);

        // 密钥：null 仍表示未配置；非 null（含 ""）按原样写入以便清除
        if (form.OssAccessKeyId is not null)
        {
            form.OssAccessKeyId = form.OssAccessKeyId.Trim();
        }
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string ResolvePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0
            || fileName.Contains("..", StringComparison.Ordinal)
            || !AllowedFileNames.Contains(fileName))
        {
            throw new InvalidOperationException($"不允许访问的配置文件：{fileName}");
        }

        string contentRoot = Path.GetFullPath(_environment.ContentRootPath);
        string fullPath = Path.GetFullPath(Path.Combine(contentRoot, fileName));
        if (!fullPath.StartsWith(contentRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"不允许访问的配置路径：{fileName}");
        }

        return fullPath;
    }
}
