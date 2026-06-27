using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Api.Dto;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Services;

namespace NeoAdmin.Blazor.Api;

/// <summary>
/// 文件上传 API，统一走 <see cref="FileService"/>（OSS 配置启用时自动上传到阿里云）。
/// </summary>
[Microsoft.AspNetCore.Mvc.Route("api/file")]
[Tags("文件接口")]
public sealed class FileController : BaseApiController
{
    private readonly FileService _fileService;
    private readonly long _maxUploadBytes;

    public FileController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        FileService fileService,
        IOptions<NeoAdminOptions> options,
        ILogger<FileController> logger)
        : base(freeSql, auth, logger)
    {
        _fileService = fileService;
        _maxUploadBytes = options.Value.FileUpload.MaxSize;
    }

    /// <summary>
    /// 上传单个文件（multipart/form-data，字段名 file）。
    /// </summary>
    [HttpPost($"@{nameof(Upload)}")]
    [RequestSizeLimit(104_857_600)]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    public async Task<ApiResult<FileUploadResponse>> Upload(
        IFormFile? file,
        [FromForm] string? directory = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length <= 0)
        {
            return ApiResult<FileUploadResponse>.Error("请选择要上传的文件");
        }

        Logger.LogInformation("API 上传文件开始，FileName={FileName}，Directory={Directory}", file.FileName, directory);

        try
        {
            SysFile uploaded = await _fileService.UploadFormFileAsync(
                file,
                directory ?? string.Empty,
                isRename: true,
                maxSizeLimit: _maxUploadBytes,
                cancellationToken);

            Logger.LogInformation("API 上传文件成功，Id={Id}，Url={Url}", uploaded.Id, uploaded.LinkUrl);
            return ApiResult<FileUploadResponse>.Success(FileUploadResponse.From(uploaded));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "API 上传文件失败，FileName={FileName}", file.FileName);
            return ApiResult<FileUploadResponse>.Error(ex.Message);
        }
    }

    /// <summary>
    /// 批量上传文件（multipart/form-data，字段名 files）。
    /// </summary>
    [HttpPost($"@{nameof(UploadMultiple)}")]
    [RequestSizeLimit(104_857_600)]
    [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
    public async Task<ApiResult<FileUploadBatchResponse>> UploadMultiple(
        List<IFormFile>? files,
        [FromForm] string? directory = null,
        CancellationToken cancellationToken = default)
    {
        if (files is null || files.Count == 0)
        {
            return ApiResult<FileUploadBatchResponse>.Error("请选择要上传的文件");
        }

        Logger.LogInformation("API 批量上传开始，Count={Count}，Directory={Directory}", files.Count, directory);

        List<FileUploadResponse> uploadedFiles = [];
        int success = 0;
        foreach (IFormFile file in files)
        {
            if (file.Length <= 0)
            {
                continue;
            }

            try
            {
                SysFile uploaded = await _fileService.UploadFormFileAsync(
                    file,
                    directory ?? string.Empty,
                    isRename: true,
                    maxSizeLimit: _maxUploadBytes,
                    cancellationToken);
                success++;
                uploadedFiles.Add(FileUploadResponse.From(uploaded));
                Logger.LogInformation("API 批量上传成功，FileName={FileName}", file.FileName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "API 批量上传失败，FileName={FileName}", file.FileName);
            }
        }

        if (success == 0)
        {
            return ApiResult<FileUploadBatchResponse>.Error("所有文件上传失败");
        }

        return ApiResult<FileUploadBatchResponse>.Success(new FileUploadBatchResponse
        {
            SuccessCount = success,
            Files = uploadedFiles
        });
    }

    /// <summary>
    /// 删除文件记录及存储（本地或 OSS）。
    /// </summary>
    [HttpPost($"@{nameof(Delete)}")]
    public async Task<ApiResult> Delete([FromBody] DeleteFileRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Id <= 0)
        {
            return ApiResult.Error("文件 Id 无效");
        }

        Logger.LogInformation("API 删除文件开始，Id={Id}", request.Id);

        try
        {
            await _fileService.DeleteAsync(request.Id, cancellationToken);
            Logger.LogInformation("API 删除文件成功，Id={Id}", request.Id);
            return ApiResult.Success("删除成功");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "API 删除文件失败，Id={Id}", request.Id);
            return ApiResult.Error(ex.Message);
        }
    }
}
