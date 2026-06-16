using FreeSql.DataAnnotations;

namespace NeoAdmin.Blazor.Entities;

/// <summary>
/// 系统文件记录。
/// </summary>
[Table(Name = "sysfile")]
public sealed class SysFile : EntityCreated
{
    /// <summary>
    /// OSS 供应商标识（本地上传时为空）。
    /// </summary>
    [Column(StringLength = 50)]
    public string? Provider { get; set; }

    [Column(StringLength = 200)]
    public string BucketName { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string FileDirectory { get; set; } = string.Empty;

    public Guid FileGuid { get; set; }

    [Column(StringLength = 200)]
    public string SaveFileName { get; set; } = string.Empty;

    [Column(StringLength = 200)]
    public string OriginFileName { get; set; } = string.Empty;

    [Column(StringLength = 20)]
    public string Extension { get; set; } = string.Empty;

    public long Size { get; set; }

    [Column(StringLength = 50)]
    public string SizeFormat { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string LinkUrl { get; set; } = string.Empty;

    [Column(StringLength = 50)]
    public string Md5 { get; set; } = string.Empty;

    [Column(IsIgnore = true)]
    public bool IsSelect { get; set; }
}
