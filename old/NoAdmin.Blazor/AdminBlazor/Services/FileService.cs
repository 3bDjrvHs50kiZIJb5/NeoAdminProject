using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Infrastructure.Encrypt;
using NoAdmin.Blazor.Infrastructure.File;
using NoAdmin.Blazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OnceMi.AspNetCore.OSS;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace NoAdmin.Blazor.Services;

public class FileService
{
	private class ByteArrayBrowserFile : IBrowserFile
	{
		private readonly byte[] _fileBytes;

		public string Name { get; }

		public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

		public long Size => _fileBytes.Length;

		public string ContentType { get; }

		public ByteArrayBrowserFile(byte[] fileBytes, string fileName)
		{
			_fileBytes = fileBytes ?? throw new ArgumentNullException("fileBytes");
			Name = fileName ?? throw new ArgumentNullException("fileName");
			ContentType = GetContentType(fileName);
		}

		public Stream OpenReadStream(long maxAllowedSize = 512000000L, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Size > maxAllowedSize)
			{
				throw new IOException($"File size {Size} exceeds the allowed limit of {maxAllowedSize} bytes.");
			}
			return new MemoryStream(_fileBytes);
		}

		/// <summary>
		/// 一个简单的辅助方法，根据文件名推断 MIME 类型。
		/// </summary>
		private static string GetContentType(string fileName)
		{
			string text = Path.GetExtension(fileName).ToLowerInvariant();
			if (1 == 0)
			{
			}
			string result = text switch
			{
				".jpg" => "image/jpeg", 
				".jpeg" => "image/jpeg", 
				".png" => "image/png", 
				".gif" => "image/gif", 
				".bmp" => "image/bmp", 
				".webp" => "image/webp", 
				".svg" => "image/svg+xml", 
				".txt" => "text/plain", 
				".pdf" => "application/pdf", 
				_ => "application/octet-stream", 
			};
			if (1 == 0)
			{
			}
			return result;
		}
	}

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

	private readonly IBaseRepository<SysFile, long> _fileRepository;

	private readonly OSSConfig _oSSConfig;

	private readonly IWebHostEnvironment _webHostEnvironment;

	private readonly NovaAdminContext _admin;

	public FileService(IBaseRepository<SysFile, long> fileRepository, IOptions<OSSConfig> oSSConfig, IWebHostEnvironment webHostEnvironment, NovaAdminContext admin)
	{
		_fileRepository = fileRepository;
		_oSSConfig = oSSConfig.Value;
		_webHostEnvironment = webHostEnvironment;
		_admin = admin;
	}

	/// <summary>
	/// 获取上传目录的所有子目录
	/// </summary>
	/// <returns></returns>
	public List<TreeViewItem<string>> GetGroups()
	{
		OSSLocalUploadConfig localUploadConfig = _oSSConfig.LocalUploadConfig;
		IWebHostEnvironment webHostEnvironment = _webHostEnvironment;
		string path = Path.Combine(webHostEnvironment.WebRootPath, localUploadConfig.Directory).ToPath();
		List<TreeViewItem<string>> list = new List<TreeViewItem<string>>
		{
			new TreeViewItem<string>("所有文件")
			{
				Text = "所有文件"
			}
		};
		string[] directories = Directory.GetDirectories(path);
		foreach (string text in directories)
		{
			string text2 = text.Replace(Path.Combine(webHostEnvironment.WebRootPath, localUploadConfig.Directory).ToPath(), "");
			list.Add(new TreeViewItem<string>(text2)
			{
				Text = text2
			});
		}
		return list;
	}

	/// <summary>
	/// 添加分组
	/// </summary>
	/// <param name="groupName"></param>
	public void AddGroup(string groupName)
	{
		IWebHostEnvironment webHostEnvironment = _webHostEnvironment;
		OSSLocalUploadConfig localUploadConfig = _oSSConfig.LocalUploadConfig;
		string directory = localUploadConfig.Directory;
		directory = Path.Combine(webHostEnvironment.WebRootPath, directory, groupName).ToPath();
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}
	}

	public async Task<SysFile> UploadFileAsync(byte[] fileBytes, string fileName, string fileDirectory = "", bool isRename = true, long maxSizeLimit = 0L)
	{
		if (fileBytes == null || fileBytes.Length == 0)
		{
			throw new ArgumentException("文件内容不能为空。", "fileBytes");
		}
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentException("文件名不能为空。", "fileName");
		}
		ByteArrayBrowserFile browserFile = new ByteArrayBrowserFile(fileBytes, fileName);
			UploadFile val = new UploadFile
			{
				File = browserFile,
				FileName = browserFile.Name,
				Code = 0,
				FileCount = 1
			};
			val.Size = browserFile.Size;
		UploadFile upfile = val;
		typeof(UploadFile).SetPropertyOrFieldValue(upfile, "OriginFileName", browserFile.Name);
		return await UploadFileAsync(upfile, fileDirectory, isRename, maxSizeLimit);
	}

	public async Task<SysFile> UploadFileAsync(IFormFile file, string fileDirectory = "", bool isRename = true, long maxSizeLimit = 0L)
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
		return await UploadFileAsync(upfile, fileDirectory, isRename, maxSizeLimit);
	}

	/// <summary>
	/// 上传文件
	/// </summary>
	/// <param name="file">文件</param>
	/// <param name="fileDirectory">文件目录</param>
	/// <param name="isRename">文件重命名</param>
	/// <returns></returns>
	public async Task<SysFile> UploadFileAsync(UploadFile file, string fileDirectory = "", bool isRename = true, long maxSizeLimit = 0L)
	{
		if (fileDirectory == "所有文件")
		{
			fileDirectory = "";
		}
		OSSLocalUploadConfig localUploadConfig = _oSSConfig.LocalUploadConfig;
		string extention = file.GetExtension()?.ToLower();
		string[] includeExtension = localUploadConfig.IncludeExtension;
		if (includeExtension != null && includeExtension.Length != 0 && !localUploadConfig.IncludeExtension.Contains(extention))
		{
			throw new Exception("不允许上传" + extention + "文件格式");
		}
		string[] excludeExtension = localUploadConfig.ExcludeExtension;
		if (excludeExtension != null && excludeExtension.Length != 0 && localUploadConfig.ExcludeExtension.Contains(extention))
		{
			throw new Exception("不允许上传" + extention + "文件格式");
		}
		if (maxSizeLimit == 0)
		{
			maxSizeLimit = localUploadConfig.MaxSize;
		}
		long fileLenth = file.Size;
		if (maxSizeLimit > 0 && fileLenth > maxSizeLimit)
		{
			throw new Exception($"文件大小不能超过{new FileSize(maxSizeLimit)}");
		}
		if (isRename && file.GetFileName().IsImage())
		{
			extention = ".jpeg";
		}
		NoAdmin.Blazor.Infrastructure.File.OSSOptions oSSOptions = _oSSConfig.OSSConfigs.Where((NoAdmin.Blazor.Infrastructure.File.OSSOptions a) => a.Enable && a.Provider == _oSSConfig.Provider).FirstOrDefault();
		bool enableOss = oSSOptions != null && oSSOptions.Enable;
		bool enableMd5 = (enableOss ? oSSOptions.Md5 : localUploadConfig.Md5);
		string md5 = string.Empty;
		if (enableMd5)
		{
			md5 = Md5Encrypt.GetHash(file.File.OpenReadStream(512000L));
			SysFile md5FileEntity = await ((ISelect0<ISelect<SysFile>, SysFile>)(object)((IBaseRepository<SysFile>)(object)_fileRepository).WhereIf(enableOss, (Expression<Func<SysFile, bool>>)((SysFile a) => (int?)a.Provider == (int?)oSSOptions.Provider)).Where((Expression<Func<SysFile, bool>>)((SysFile a) => a.Md5 == md5))).FirstAsync(default(CancellationToken));
			if (md5FileEntity != null)
			{
				SysFile sameFileEntity = new SysFile
				{
					Provider = md5FileEntity.Provider,
					BucketName = md5FileEntity.BucketName,
					FileGuid = FreeUtil.NewMongodbId(),
					SaveFileName = md5FileEntity.SaveFileName,
					Extension = extention,
					OriginFileName = file.OriginFileName,
					FileDirectory = md5FileEntity.FileDirectory,
					Size = md5FileEntity.Size,
					SizeFormat = md5FileEntity.SizeFormat,
					LinkUrl = md5FileEntity.LinkUrl,
					Md5 = md5
				};
				return await ((IBaseRepository<SysFile>)(object)_fileRepository).InsertAsync(sameFileEntity, default(CancellationToken));
			}
		}
		if (fileDirectory.IsNull())
		{
			fileDirectory = localUploadConfig.Directory;
			if (localUploadConfig.DateTimeDirectory.NotNull())
			{
				fileDirectory = Path.Combine(fileDirectory, DateTime.Now.ToString(localUploadConfig.DateTimeDirectory)).ToPath();
			}
		}
		else
		{
			fileDirectory = Path.Combine(localUploadConfig.Directory, fileDirectory.Replace("\\", "/")).ToPath();
		}
		FileSize fileSize = new FileSize(fileLenth);
		SysFile fileEntity = new SysFile
		{
			Provider = oSSOptions?.Provider,
			BucketName = oSSOptions?.BucketName,
			FileGuid = FreeUtil.NewMongodbId(),
			OriginFileName = ((!string.IsNullOrEmpty(file.FileName)) ? file.FileName : file.OriginFileName),
			Extension = extention,
			FileDirectory = fileDirectory,
			Size = fileSize.Size,
			SizeFormat = fileSize.ToString(),
			Md5 = md5
		};
		fileEntity.SaveFileName = (isRename ? fileEntity.FileGuid.ToString() : fileEntity.OriginFileName);
		string filePath = fileEntity.SaveFileName;
		if (!filePath.EndsWith(fileEntity.Extension))
		{
			filePath += fileEntity.Extension;
		}
		filePath = Path.Combine(fileDirectory, filePath).ToPath();
		_ = string.Empty;
		if (enableOss)
		{
			string url = oSSOptions.Url;
			if (url.IsNull())
			{
				OSSProvider provider = oSSOptions.Provider;
				if (1 == 0)
				{
				}
				string text = provider switch
				{
					OSSProvider.Minio => oSSOptions.Endpoint + "/" + oSSOptions.BucketName, 
					OSSProvider.Aliyun => oSSOptions.BucketName + "." + oSSOptions.Endpoint, 
					OSSProvider.QCloud => $"{oSSOptions.BucketName}-{oSSOptions.Endpoint}.cos.{oSSOptions.Region}.myqcloud.com", 
					OSSProvider.Qiniu => oSSOptions.BucketName + "." + oSSOptions.Region + ".qiniucs.com", 
					OSSProvider.HuaweiCloud => oSSOptions.BucketName + "." + oSSOptions.Endpoint, 
					_ => "", 
				};
				if (1 == 0)
				{
				}
				url = text;
				if (url.IsNull())
				{
					throw new Exception($"请配置{oSSOptions.Provider}的Url参数");
				}
				string urlProtocol = (oSSOptions.IsEnableHttps ? "https" : "http");
				fileEntity.LinkUrl = $"{urlProtocol}://{url}/{filePath}";
			}
			else
			{
				fileEntity.LinkUrl = url + "/" + filePath;
			}
		}
		else
		{
			fileEntity.LinkUrl = "/" + filePath;
		}
		if (enableOss)
		{
			string tempFilePath = Path.GetTempFileName();
			try
			{
				await UploadFileExtensions.SaveToFileAsync(file, tempFilePath, maxSizeLimit, 65536, default(CancellationToken));
				IOSSServiceFactory _oSSServiceFactory = _admin.Service.GetRequiredService<IOSSServiceFactory>();
				IOSSService oSSService = _oSSServiceFactory.Create(_oSSConfig.Provider.ToString());
				string objectName = filePath;
				await oSSService.PutObjectAsync(oSSOptions.BucketName, objectName, tempFilePath);
			}
			finally
			{
				if (File.Exists(tempFilePath))
				{
					try
					{
						File.Delete(tempFilePath);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		else
		{
			IWebHostEnvironment env = _webHostEnvironment;
			fileDirectory = Path.Combine(env.WebRootPath, fileDirectory).ToPath();
			if (!Directory.Exists(fileDirectory))
			{
				Directory.CreateDirectory(fileDirectory);
			}
			filePath = Path.Combine(env.WebRootPath, filePath).ToPath();
			await UploadFileExtensions.SaveToFileAsync(file, filePath, maxSizeLimit, 65536, default(CancellationToken));
			if (file.GetFileName().IsImage())
			{
				Image image = await Image.LoadAsync(filePath, default(CancellationToken));
				try
				{
					int w = image.Width;
					_ = image.Height;
					if (w > 1600)
					{
						ProcessingExtensions.Mutate(image, (Action<IImageProcessingContext>)delegate(IImageProcessingContext x)
						{
							ResizeExtensions.Resize(x, 1600, 0, true);
						});
							JpegEncoder encoder = new JpegEncoder
							{
								Quality = 90
							};
						await ImageExtensions.SaveAsJpegAsync(image, filePath, encoder, default(CancellationToken));
					}
				}
				finally
				{
					((IDisposable)image)?.Dispose();
				}
			}
		}
		if (await ((IBaseRepository<SysFile>)(object)_fileRepository).Select.AnyAsync((Expression<Func<SysFile, bool>>)((SysFile x) => x.OriginFileName == fileEntity.OriginFileName), default(CancellationToken)))
		{
			fileEntity.OriginFileName = fileEntity.OriginFileName.Replace(".", "_" + new Random().Next(1000, 9999) + ".");
		}
		fileEntity = await ((IBaseRepository<SysFile>)(object)_fileRepository).InsertAsync(fileEntity, default(CancellationToken));
		return fileEntity;
	}

	/// <summary>
	/// 上传多文件
	/// </summary>
	/// <param name="files">文件列表</param>
	/// <param name="fileDirectory">文件目录</param>
	/// <param name="fileReName">文件重命名</param>
	/// <returns></returns>
	public async Task<List<SysFile>> UploadFilesAsync(List<UploadFile> files, string fileDirectory = "", bool fileReName = true)
	{
		List<SysFile> fileList = new List<SysFile>();
		foreach (UploadFile file in files)
		{
			List<SysFile> list = fileList;
			list.Add(await UploadFileAsync(file, fileDirectory, fileReName, 0L));
		}
		return fileList;
	}

	/// <summary>
	/// 删除
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public async Task DeleteAsync(long id)
	{
		SysFile file = await _fileRepository.GetAsync(id, default(CancellationToken));
		if (file == null)
		{
			return;
		}
		if (file.Provider.HasValue)
		{
			IOSSServiceFactory _oSSServiceFactory = _admin.Service.GetRequiredService<IOSSServiceFactory>();
			IOSSService oSSService = _oSSServiceFactory.Create(file.Provider.ToString());
			if (_oSSConfig.OSSConfigs.Where((NoAdmin.Blazor.Infrastructure.File.OSSOptions a) => a.Enable && a.Provider == file.Provider).FirstOrDefault()?.Enable ?? false)
			{
				await oSSService.RemoveObjectAsync(objectName: Path.Combine(file.FileDirectory, file.SaveFileName + file.Extension).ToPath(), bucketName: file.BucketName);
			}
		}
		else
		{
			IWebHostEnvironment env = _webHostEnvironment;
			string filePath = Path.Combine(env.WebRootPath, file.FileDirectory, file.SaveFileName.Replace(file.Extension, "") + file.Extension).ToPath();
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		await _fileRepository.DeleteAsync(file.Id, default(CancellationToken));
	}

	/// <summary>
	/// 为指定的图像文件创建缩略图
	/// </summary>
	/// <param name="originalLinkUrl">原始文件的 Web 相对路径 (e.g., /uploads/image.jpg)</param>
	/// <param name="width">缩略图宽度</param>
	/// <param name="height">缩略图高度</param>
	/// <param name="suffix">缩略图文件名后缀</param>
	/// <returns>缩略图的 Web 访问相对路径</returns>
	public async Task<string> CreateThumbnailAsync(string originalLinkUrl, int width, int height, string suffix = "_thumb")
	{
		if (string.IsNullOrEmpty(originalLinkUrl))
		{
			return null;
		}
		string relativePath = originalLinkUrl.TrimStart('/');
		string originalPhysicalPath = Path.Combine(path2: relativePath.Replace('/', Path.DirectorySeparatorChar), path1: _webHostEnvironment.WebRootPath);
		if (!File.Exists(originalPhysicalPath))
		{
			return null;
		}
		FileInfo fileInfo = new FileInfo(originalPhysicalPath);
		_ = fileInfo.Extension;
		string thumbPhysicalPath = Path.Combine(path2: Path.GetFileNameWithoutExtension(originalPhysicalPath) + suffix + ".jpg", path1: fileInfo.DirectoryName);
		Image image = await Image.LoadAsync(originalPhysicalPath, default(CancellationToken));
		try
		{
			ProcessingExtensions.Mutate(image, (Action<IImageProcessingContext>)delegate(IImageProcessingContext x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Expected O, but got Unknown
				ResizeExtensions.Resize(x, new ResizeOptions
				{
						Size = new SixLabors.ImageSharp.Size(width, height),
					Mode = (ResizeMode)0
				});
			});
			await ImageExtensions.SaveAsJpegAsync(image, thumbPhysicalPath);
			string thumbUrl = thumbPhysicalPath.Replace(_webHostEnvironment.WebRootPath, "").Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
			return "/" + thumbUrl;
		}
		finally
		{
			((IDisposable)image)?.Dispose();
		}
	}
}
