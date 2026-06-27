namespace NeoAdmin.Blazor.Data;

/// <summary>
/// 本地上传配置。
/// </summary>
public sealed class FileUploadOptions
{
    /// <summary>
    /// 上传根目录（相对 wwwroot）。
    /// </summary>
    public string Directory { get; set; } = "uploads";

    /// <summary>
    /// 按日期分子目录的格式，留空则不分子目录。
    /// </summary>
    public string DateTimeDirectory { get; set; } = "yyyyMMdd";

    /// <summary>
    /// 是否按 MD5 去重。
    /// </summary>
    public bool Md5 { get; set; }

    /// <summary>
    /// 单文件最大字节数，默认 100MB。
    /// </summary>
    public long MaxSize { get; set; } = 104_857_600;

    public string[] IncludeExtension { get; set; } = [];

    public string[] ExcludeExtension { get; set; } = [".exe", ".dll", ".jar"];

    /// <summary>
    /// 阿里云 OSS；Endpoint、AccessKeyId、AccessKeySecret、BucketName 均填写后启用。
    /// </summary>
    public OssOptions Oss { get; set; } = new();
}
