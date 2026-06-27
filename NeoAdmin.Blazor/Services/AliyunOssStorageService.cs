using Aliyun.OSS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 阿里云 OSS 存储；配置未填写时不启用。
/// </summary>
public sealed class AliyunOssStorageService
{
    public const string ProviderName = "AliyunOss";

    private readonly OssOptions _options;
    private readonly ILogger<AliyunOssStorageService> _logger;
    private OssClient? _client;

    public AliyunOssStorageService(IOptions<NeoAdminOptions> options, ILogger<AliyunOssStorageService> logger)
    {
        _options = options.Value.FileUpload.Oss;
        _logger = logger;
    }

    public bool IsEnabled => _options.IsEnabled;

    public string BucketName => _options.BucketName;

    public async Task UploadAsync(
        string objectKey,
        byte[] fileBytes,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("阿里云 OSS 未配置，无法上传。");
        }

        ObjectMetadata metadata = new();
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            metadata.ContentType = contentType;
        }

        _logger.LogInformation("OSS 开始上传，Bucket={Bucket}，ObjectKey={ObjectKey}，Size={Size}",
            _options.BucketName, objectKey, fileBytes.Length);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using MemoryStream stream = new(fileBytes);
            GetClient().PutObject(_options.BucketName, objectKey, stream, metadata);
        }, cancellationToken);

        _logger.LogInformation("OSS 上传成功，ObjectKey={ObjectKey}", objectKey);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(objectKey))
        {
            return;
        }

        _logger.LogInformation("OSS 开始删除，Bucket={Bucket}，ObjectKey={ObjectKey}",
            _options.BucketName, objectKey);

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetClient().DeleteObject(_options.BucketName, objectKey);
        }, cancellationToken);

        _logger.LogInformation("OSS 删除成功，ObjectKey={ObjectKey}", objectKey);
    }

    public string BuildObjectKey(string relativeDirectory, string fileNameOnDisk)
    {
        string directory = relativeDirectory.Replace('\\', '/').Trim('/');
        string key = string.IsNullOrWhiteSpace(directory)
            ? fileNameOnDisk
            : $"{directory}/{fileNameOnDisk}";

        string prefix = _options.Prefix.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return key;
        }

        return $"{prefix}/{key}";
    }

    public string BuildPublicUrl(string objectKey)
    {
        string normalizedKey = objectKey.Replace('\\', '/').TrimStart('/');

        if (!string.IsNullOrWhiteSpace(_options.CustomDomain))
        {
            string domain = _options.CustomDomain.Trim().TrimEnd('/');
            if (!domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                domain = "https://" + domain;
            }

            return $"{domain}/{normalizedKey}";
        }

        string endpoint = NormalizeEndpoint(_options.Endpoint);
        return $"https://{_options.BucketName}.{endpoint}/{normalizedKey}";
    }

    public string BuildObjectKeyFromFile(SysFile file)
    {
        string fileNameOnDisk = file.SaveFileName + file.Extension;
        return BuildObjectKey(file.FileDirectory, fileNameOnDisk);
    }

    private OssClient GetClient()
    {
        if (_client is not null)
        {
            return _client;
        }

        string endpoint = NormalizeEndpoint(_options.Endpoint);
        _client = new OssClient(endpoint, _options.AccessKeyId, _options.AccessKeySecret);
        return _client;
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        string value = endpoint.Trim();
        if (value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = value[8..];
        }
        else if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            value = value[7..];
        }

        return value.TrimEnd('/');
    }
}
