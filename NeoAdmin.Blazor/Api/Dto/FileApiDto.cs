using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Api.Dto;

public sealed class FileUploadResponse
{
    public long Id { get; set; }

    public string OriginFileName { get; set; } = string.Empty;

    public string LinkUrl { get; set; } = string.Empty;

    public string? Provider { get; set; }

    public long Size { get; set; }

    public string SizeFormat { get; set; } = string.Empty;

    public static FileUploadResponse From(SysFile file) => new()
    {
        Id = file.Id,
        OriginFileName = file.OriginFileName,
        LinkUrl = file.LinkUrl,
        Provider = file.Provider,
        Size = file.Size,
        SizeFormat = file.SizeFormat
    };
}

public sealed class FileUploadBatchResponse
{
    public int SuccessCount { get; set; }

    public List<FileUploadResponse> Files { get; set; } = [];
}

public sealed class DeleteFileRequest
{
    public long Id { get; set; }
}
