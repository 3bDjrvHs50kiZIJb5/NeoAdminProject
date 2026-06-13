namespace NeoAdmin.Blazor.Models;

public sealed class SerilogLogFileInfo
{
    public string FileName { get; init; } = string.Empty;

    public string FullPath { get; init; } = string.Empty;

    public long SizeBytes { get; init; }

    public DateTime LastWriteTime { get; init; }

    public string SizeText { get; init; } = string.Empty;
}

public sealed class SerilogLogEntry
{
    public int Index { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public string Level { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Exception { get; set; } = string.Empty;
}

public sealed class SerilogLogQuery
{
    public string? FileName { get; set; }

    public string? Level { get; set; }

    public string? SearchText { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}

public sealed class SerilogLogPageResult
{
    public IReadOnlyList<SerilogLogFileInfo> Files { get; init; } = [];

    public IReadOnlyList<SerilogLogEntry> Items { get; init; } = [];

    public int Total { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int MaxPageNumber { get; init; }

    public string? SelectedFile { get; init; }

    public string LogsDirectory { get; init; } = string.Empty;

    public string? ErrorMessage { get; init; }
}

public sealed class SerilogLogClearResult
{
    public bool Success { get; init; }

    public string? SelectedFile { get; init; }

    public int RemovedCount { get; init; }

    public string? ErrorMessage { get; init; }
}

public sealed class SerilogLogDeleteResult
{
    public bool Success { get; init; }

    public string? DeletedFile { get; init; }

    public string? ErrorMessage { get; init; }
}
