namespace NoAdmin.Blazor.Infrastructure.File;

/// <summary>
/// 本地上传配置
/// </summary>
public class OSSLocalUploadConfig
{
	/// <summary>
	/// 上传目录
	/// </summary>
	public string Directory { get; set; } = "upload";

	/// <summary>
	/// 日期目录
	/// </summary>
	public string DateTimeDirectory { get; set; } = "yyyy/MM/dd";

	/// <summary>
	/// 文件Md5码
	/// </summary>
	public bool Md5 { get; set; } = false;

	/// <summary>
	/// 文件大小
	/// </summary>
	public long MaxSize { get; set; } = 104857600L;

	/// <summary>
	/// 包含文件拓展名列表
	/// </summary>
	public string[] IncludeExtension { get; set; }

	/// <summary>
	/// 排除文件拓展名列表
	/// </summary>
	public string[] ExcludeExtension { get; set; }
}
