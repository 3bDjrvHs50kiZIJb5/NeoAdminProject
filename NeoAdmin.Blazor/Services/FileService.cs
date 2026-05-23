using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Data.Entities;
using NeoAdmin.Blazor.Utils;
using FreeSql;

namespace NeoAdmin.Blazor.Services;

public sealed class FileService
{
    private readonly IFreeSql _freeSql;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly FileUploadOptions _uploadOptions;

    public FileService(
        IFreeSql freeSql,
        IWebHostEnvironment webHostEnvironment,
        IOptions<NeoAdminOptions> options)
    {
        _freeSql = freeSql;
        _webHostEnvironment = webHostEnvironment;
        _uploadOptions = options.Value.FileUpload;
    }

    public async Task<SysFile> UploadFileAsync(
        IBrowserFile file,
        string fileDirectory = "",
        bool isRename = true,
        long maxSizeLimit = 0,
        CancellationToken cancellationToken = default)
    {
        if (file.Size <= 0)
        {
            throw new InvalidOperationException("文件内容不能为空。");
        }

        string extension = Path.GetExtension(file.Name).ToLowerInvariant();
        ValidateExtension(extension);

        long limit = maxSizeLimit > 0 ? maxSizeLimit : _uploadOptions.MaxSize;
        if (limit > 0 && file.Size > limit)
        {
            throw new InvalidOperationException($"文件大小不能超过 {FileSizeFormat.Format(limit)}");
        }

        await using Stream stream = file.OpenReadStream(limit, cancellationToken);
        string md5 = string.Empty;
        byte[] fileBytes;

        if (_uploadOptions.Md5)
        {
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer, cancellationToken);
            fileBytes = buffer.ToArray();
            md5 = FileMd5.GetHash(new MemoryStream(fileBytes));

            SysFile? existing = await _freeSql.Select<SysFile>()
                .Where(a => a.Md5 == md5)
                .FirstAsync(cancellationToken);

            if (existing is not null)
            {
                SysFile sameFile = new()
                {
                    FileGuid = Guid.NewGuid(),
                    SaveFileName = existing.SaveFileName,
                    Extension = extension,
                    OriginFileName = file.Name,
                    FileDirectory = existing.FileDirectory,
                    Size = existing.Size,
                    SizeFormat = existing.SizeFormat,
                    LinkUrl = existing.LinkUrl,
                    Md5 = md5,
                    CreatedTime = DateTime.Now
                };
                await _freeSql.Insert(sameFile).ExecuteAffrowsAsync(cancellationToken);
                return sameFile;
            }
        }
        else
        {
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer, cancellationToken);
            fileBytes = buffer.ToArray();
        }

        string relativeDirectory = BuildRelativeDirectory(fileDirectory);
        Guid fileGuid = Guid.NewGuid();
        string saveFileName = isRename ? fileGuid.ToString("N") : Path.GetFileNameWithoutExtension(file.Name);
        string fileNameOnDisk = saveFileName + extension;
        string relativePath = Path.Combine(relativeDirectory, fileNameOnDisk).Replace('\\', '/');

        string physicalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(physicalDirectory);

        string physicalPath = Path.Combine(physicalDirectory, fileNameOnDisk);
        await File.WriteAllBytesAsync(physicalPath, fileBytes, cancellationToken);

        SysFile fileEntity = new()
        {
            FileGuid = fileGuid,
            OriginFileName = file.Name,
            Extension = extension,
            FileDirectory = relativeDirectory.Replace('\\', '/'),
            Size = fileBytes.LongLength,
            SizeFormat = FileSizeFormat.Format(fileBytes.LongLength),
            Md5 = md5,
            SaveFileName = saveFileName,
            LinkUrl = "/" + relativePath,
            CreatedTime = DateTime.Now
        };

        if (await _freeSql.Select<SysFile>().AnyAsync(a => a.OriginFileName == fileEntity.OriginFileName, cancellationToken))
        {
            fileEntity.OriginFileName = fileEntity.OriginFileName.Replace(
                ".",
                "_" + Random.Shared.Next(1000, 9999) + ".",
                StringComparison.Ordinal);
        }

        await _freeSql.Insert(fileEntity).ExecuteAffrowsAsync(cancellationToken);
        return fileEntity;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SysFile? file = await _freeSql.Select<SysFile>().Where(a => a.Id == id).FirstAsync(cancellationToken);
        if (file is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(file.Provider))
        {
            string physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                file.FileDirectory,
                file.SaveFileName + file.Extension);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        await _freeSql.Delete<SysFile>().Where(a => a.Id == id).ExecuteAffrowsAsync(cancellationToken);
    }

    private string BuildRelativeDirectory(string fileDirectory)
    {
        string directory = string.IsNullOrWhiteSpace(fileDirectory) || fileDirectory == "所有文件"
            ? _uploadOptions.Directory
            : Path.Combine(_uploadOptions.Directory, fileDirectory.Replace('\\', '/')).Replace('\\', '/');

        if (!string.IsNullOrWhiteSpace(_uploadOptions.DateTimeDirectory)
            && string.IsNullOrWhiteSpace(fileDirectory))
        {
            directory = Path.Combine(directory, DateTime.Now.ToString(_uploadOptions.DateTimeDirectory))
                .Replace('\\', '/');
        }

        return directory;
    }

    private void ValidateExtension(string extension)
    {
        string[] include = _uploadOptions.IncludeExtension;
        if (include.Length > 0 && !include.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"不允许上传 {extension} 文件格式");
        }

        string[] exclude = _uploadOptions.ExcludeExtension;
        if (exclude.Length > 0 && exclude.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"不允许上传 {extension} 文件格式");
        }
    }
}
