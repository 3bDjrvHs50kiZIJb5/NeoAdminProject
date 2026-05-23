using System.Collections.Generic;
using OnceMi.AspNetCore.OSS;

namespace NoAdmin.Blazor.Infrastructure.File;

/// <summary>
/// OSS配置
/// </summary>
public class OSSConfig
{
	/// <summary>
	/// 本地上传配置
	/// </summary>
	public OSSLocalUploadConfig LocalUploadConfig { get; set; }

	/// <summary>
	/// 文件存储供应商
	/// </summary>
	public OSSProvider Provider { get; set; } = OSSProvider.Minio;

	/// <summary>
	/// OSS配置列表
	/// </summary>
	public List<OSSOptions> OSSConfigs { get; set; }
}
