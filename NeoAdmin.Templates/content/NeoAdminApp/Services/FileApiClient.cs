using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Api.Dto;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Services;

namespace NeoAdminApp.Services;

/// <summary>
/// 文件上传 API 客户端。
/// Blazor Server 内上传走 <see cref="FileService"/>（与 API 相同逻辑，避免 HttpClient 回调自身导致 502）；
/// 外部应用请直接请求 <c>/api/file/@Upload</c>（见演示页 curl / fetch 示例）。
/// </summary>
public sealed class FileApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FileService _fileService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private readonly long _maxUploadBytes;
    private readonly ILogger<FileApiClient> _logger;

    public FileApiClient(
        FileService fileService,
        IHttpClientFactory httpClientFactory,
        NavigationManager navigationManager,
        IJSRuntime jsRuntime,
        IOptions<NeoAdminOptions> options,
        ILogger<FileApiClient> logger)
    {
        _fileService = fileService;
        _httpClientFactory = httpClientFactory;
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
        _maxUploadBytes = options.Value.FileUpload.MaxSize;
        _logger = logger;
    }

    /// <summary>上传单个文件（Blazor 页面内推荐；与 API 共用 FileService，OSS 配置启用时自动走 OSS）。</summary>
    public async Task<ApiResult<FileUploadResponse>> UploadAsync(
        IBrowserFile file,
        string directory = "",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileApiClient 开始上传，FileName={FileName}，Directory={Directory}", file.Name, directory);

        try
        {
            SysFile uploaded = await _fileService.UploadFileAsync(
                file,
                directory,
                isRename: true,
                maxSizeLimit: _maxUploadBytes,
                cancellationToken);

            _logger.LogInformation("FileApiClient 上传成功，Id={Id}，Url={Url}", uploaded.Id, uploaded.LinkUrl);
            return ApiResult<FileUploadResponse>.Success(FileUploadResponse.From(uploaded));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FileApiClient 上传失败，FileName={FileName}", file.Name);
            return ApiResult<FileUploadResponse>.Error(ex.Message);
        }
    }

    /// <summary>通过 HTTP 上传（供控制台、移动端等外部调用方参考；勿在 Blazor Server 页面内使用）。</summary>
    public async Task<ApiResult<FileUploadResponse>> UploadViaHttpAsync(
        string fileName,
        Stream content,
        string? contentType,
        long length,
        string directory = "",
        CancellationToken cancellationToken = default)
    {
        HttpClient client = await CreateAuthorizedClientAsync(cancellationToken);
        using MultipartFormDataContent form = new();
        StreamContent fileContent = new(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        fileContent.Headers.ContentLength = length;
        form.Add(fileContent, "file", fileName);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            form.Add(new StringContent(directory), "directory");
        }

        using HttpResponseMessage response = await client.PostAsync("api/file/@Upload", form, cancellationToken);
        ApiResult<FileUploadResponse>? result = await ReadResultAsync<FileUploadResponse>(response, cancellationToken);
        return result ?? ApiResult<FileUploadResponse>.Error("无法解析上传响应");
    }

    /// <summary>删除文件（与 API 共用 FileService）。</summary>
    public async Task<ApiResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileApiClient 开始删除，Id={Id}", id);

        try
        {
            await _fileService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("FileApiClient 删除成功，Id={Id}", id);
            return ApiResult.Success("删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FileApiClient 删除失败，Id={Id}", id);
            return ApiResult.Error(ex.Message);
        }
    }

    /// <summary>通过 HTTP 删除（外部调用方参考）。</summary>
    public async Task<ApiResult> DeleteViaHttpAsync(long id, CancellationToken cancellationToken = default)
    {
        HttpClient client = await CreateAuthorizedClientAsync(cancellationToken);
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "api/file/@Delete",
            new DeleteFileRequest { Id = id },
            cancellationToken);

        return await ReadApiResultAsync(response, cancellationToken);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_navigationManager.BaseUri);

        string? token = await GetTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("neoAdminAuth.getToken", cancellationToken);
        }
        catch (JSException)
        {
            return null;
        }
    }

    private static async Task<ApiResult<T>?> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return ApiResult<T>.Error($"HTTP {(int)response.StatusCode}");
        }

        return JsonSerializer.Deserialize<ApiResult<T>>(json, JsonOptions);
    }

    private static async Task<ApiResult> ReadApiResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return ApiResult.Error($"HTTP {(int)response.StatusCode}");
        }

        return JsonSerializer.Deserialize<ApiResult>(json, JsonOptions) ?? ApiResult.Error("无法解析删除响应");
    }
}
