using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Utils;
using FreeSql;

namespace NeoAdmin.Blazor.Services;

public sealed class FileService
{
    private readonly IFreeSql _freeSql;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly FileUploadOptions _uploadOptions;
    private readonly NeoAdminAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IFreeSql freeSql,
        IWebHostEnvironment webHostEnvironment,
        IOptions<NeoAdminOptions> options,
        NeoAdminAuthService authService,
        IHttpContextAccessor httpContextAccessor,
        IJSRuntime jsRuntime,
        ILogger<FileService> logger)
    {
        _freeSql = freeSql;
        _webHostEnvironment = webHostEnvironment;
        _uploadOptions = options.Value.FileUpload;
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _logger = logger;
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
            _logger.LogWarning("上传文件失败：文件内容为空，文件名={FileName}", file.Name);
            throw new InvalidOperationException("文件内容不能为空。");
        }

        long limit = maxSizeLimit > 0 ? maxSizeLimit : _uploadOptions.MaxSize;
        if (limit > 0 && file.Size > limit)
        {
            throw new InvalidOperationException($"文件大小不能超过 {FileSizeFormat.Format(limit)}");
        }

        await using Stream stream = file.OpenReadStream(limit, cancellationToken);
        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer, cancellationToken);
        return await UploadBytesAsync(
            file.Name,
            buffer.ToArray(),
            fileDirectory,
            isRename,
            cancellationToken);
    }

    public async Task<SysFile> UploadBytesAsync(
        string fileName,
        byte[] fileBytes,
        string fileDirectory = "",
        bool isRename = true,
        CancellationToken cancellationToken = default)
    {
        if (fileBytes.Length <= 0)
        {
            _logger.LogWarning("上传文件失败：文件内容为空，文件名={FileName}", fileName);
            throw new InvalidOperationException("文件内容不能为空。");
        }

        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        ValidateExtension(extension);

        string md5 = string.Empty;
        if (_uploadOptions.Md5)
        {
            md5 = FileMd5.GetHash(new MemoryStream(fileBytes));

            SysFile? existing = await _freeSql.Select<SysFile>()
                .Where(a => a.Md5 == md5)
                .FirstAsync(cancellationToken);

            if (existing is not null)
            {
                _logger.LogInformation("上传文件命中 MD5 去重：{FileName} -> {EntityDesc}", fileName, EntityLogHelper.Describe(existing));
                SysFile sameFile = new()
                {
                    FileGuid = Guid.NewGuid(),
                    SaveFileName = existing.SaveFileName,
                    Extension = extension,
                    OriginFileName = fileName,
                    FileDirectory = existing.FileDirectory,
                    Size = existing.Size,
                    SizeFormat = existing.SizeFormat,
                    LinkUrl = existing.LinkUrl,
                    Md5 = md5,
                    CreatedTime = DateTime.Now
                };
                await FillCreatedUserAsync(sameFile, cancellationToken);
                await _freeSql.Insert(sameFile).ExecuteAffrowsAsync(cancellationToken);
                return sameFile;
            }
        }

        string relativeDirectory = BuildRelativeDirectory(fileDirectory);
        Guid fileGuid = Guid.NewGuid();
        string saveFileName = isRename ? fileGuid.ToString("N") : Path.GetFileNameWithoutExtension(fileName);
        string fileNameOnDisk = saveFileName + extension;
        string relativePath = Path.Combine(relativeDirectory, fileNameOnDisk).Replace('\\', '/');

        string physicalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(physicalDirectory);

        string physicalPath = Path.Combine(physicalDirectory, fileNameOnDisk);
        await File.WriteAllBytesAsync(physicalPath, fileBytes, cancellationToken);

        SysFile fileEntity = new()
        {
            FileGuid = fileGuid,
            OriginFileName = fileName,
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

        await FillCreatedUserAsync(fileEntity, cancellationToken);
        await _freeSql.Insert(fileEntity).ExecuteAffrowsAsync(cancellationToken);
        _logger.LogInformation("上传文件成功：{EntityDesc}，原始文件名={OriginFileName}", EntityLogHelper.Describe(fileEntity), fileName);
        return fileEntity;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SysFile? file = await _freeSql.Select<SysFile>().Where(a => a.Id == id).FirstAsync(cancellationToken);
        if (file is null)
        {
            _logger.LogWarning("删除文件失败：文件不存在，Id={FileId}", id);
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
        _logger.LogInformation("删除文件成功：{EntityDesc}", EntityLogHelper.Describe(file));
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

    private async Task FillCreatedUserAsync(SysFile file, CancellationToken cancellationToken)
    {
        UserSummaryResponse? user = await GetCurrentUserAsync(cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("上传文件时未获取到当前用户，CreatedUserName 将为空，FileName={FileName}", file.OriginFileName);
            return;
        }

        file.CreatedUserId = user.Id;
        file.CreatedUserName = !string.IsNullOrWhiteSpace(user.Nickname) ? user.Nickname : user.Username;
    }

    private async Task<UserSummaryResponse?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        string? token = GetBearerTokenFromHttpContext();
        if (string.IsNullOrWhiteSpace(token))
        {
            try
            {
                token = await _jsRuntime.InvokeAsync<string?>("neoAdminAuth.getToken", cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // 非 Blazor 交互上下文时忽略。
            }
            catch (JSException)
            {
                // JS 未就绪时忽略。
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        ApiResult<UserSummaryResponse> result = await _authService.CheckAsync(token);
        return result.Succeeded ? result.Data : null;
    }

    private string? GetBearerTokenFromHttpContext()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        string? authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return null;
        }

        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization[7..].Trim()
            : authorization.Trim();
    }
}
