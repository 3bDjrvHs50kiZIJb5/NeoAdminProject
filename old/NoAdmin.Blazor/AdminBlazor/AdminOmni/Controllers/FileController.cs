using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Services;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace NoAdmin.Blazor.AdminOmni.Controllers;

/// <summary>
/// 文件
/// </summary>
[Route("api/Admin/File")]
[Tags(new string[] { "系统" })]
public class FileController : ControllerBase
{
	private class BrowserFormFile : IBrowserFile
	{
		private IFormFile _file;

		public string Name => _file.Name;

		public DateTimeOffset LastModified
		{
			get
			{
				StringValues value;
				DateTime result;
				return DateTime.TryParse(_file.Headers.TryGetValue("Last-Modified", out value) ? ((string?)value) : "", out result) ? result : DateTime.Parse("1970-1-1");
			}
		}

		public long Size => _file.Length;

		public string ContentType => _file.ContentType;

		public BrowserFormFile(IFormFile file)
		{
			_file = file;
		}

		public Stream OpenReadStream(long maxAllowedSize = 512000L, CancellationToken cancellationToken = default(CancellationToken))
		{
			return _file.OpenReadStream();
		}
	}

	private readonly FileService fileService;

	public FileController(NovaAdminContext admin, FileService fileService)
	{
		this.fileService = fileService;
	}

	[HttpPost("@upload")]
	public async Task<ApiResult> UploadFiles(List<IFormFile> files)
	{
		try
		{
			if (files == null || files.Count == 0)
			{
				return ApiResult.Error.SetMessage("没有选择文件。");
			}
			List<string> uploadedFiles = new List<string>();
			foreach (IFormFile file in files)
			{
				UploadFile val = new UploadFile
					{
						File = new BrowserFormFile(file),
						FileName = file.FileName,
						Code = 0,
						FileCount = 1
					};
					val.Size = file.Length;
				UploadFile upfile = val;
				typeof(UploadFile).SetPropertyOrFieldValue(upfile, "OriginFileName", file.FileName);
				uploadedFiles.Add((await fileService.UploadFileAsync(upfile, "", isRename: true, 0L)).LinkUrl);
			}
			return ApiResult.Success.SetData(new
			{
				files = uploadedFiles
			});
		}
		catch (Exception)
		{
			return ApiResult.Error.SetMessage("上传文件时发生错误。");
		}
	}
}
