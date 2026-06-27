namespace NeoAdmin.Blazor.Data;

/// <summary>
/// 阿里云 OSS 配置。Endpoint、AccessKeyId、AccessKeySecret、BucketName 均填写时启用。
/// </summary>
public sealed class OssOptions
{
    /// <summary>
    /// OSS 访问域名，如 oss-cn-hangzhou.aliyuncs.com。
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string AccessKeySecret { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// 自定义访问域名（CDN 等），留空则使用 Bucket 默认域名。
    /// </summary>
    public string CustomDomain { get; set; } = string.Empty;

    /// <summary>
    /// 对象键前缀，如 neoadmin。
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// 四项必填配置均已填写时视为启用 OSS。
    /// </summary>
    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Endpoint)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(AccessKeySecret)
        && !string.IsNullOrWhiteSpace(BucketName);
}
