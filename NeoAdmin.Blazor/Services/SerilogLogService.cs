using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Models;
using NeoAdmin.Blazor.Utils;

namespace NeoAdmin.Blazor.Services;

public sealed class SerilogLogService
{
    private const int MaxLinesToRead = 8000;

    private static readonly Regex EntryRegex = new(
        @"^\[(?<timestamp>.+?) (?<level>INF|WRN|ERR|FTL|DBG|VRB)\] (?<content>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ILogger<SerilogLogService> _logger;
    private readonly string _logsDirectory;
    private readonly string _filePrefix;

    public SerilogLogService(ILogger<SerilogLogService> logger, IOptions<NeoAdminOptions> options)
    {
        _logger = logger;
        NeoAdminOptions adminOptions = options.Value;
        _logsDirectory = Path.IsPathRooted(adminOptions.LogDirectory)
            ? adminOptions.LogDirectory
            : Path.Combine(AppContext.BaseDirectory, adminOptions.LogDirectory);
        _filePrefix = adminOptions.LogFilePrefix;
    }

    public string LogsDirectory => _logsDirectory;

    public Task<SerilogLogPageResult> QueryAsync(SerilogLogQuery query, CancellationToken cancellationToken = default) =>
        Task.Run(() => QueryCore(query), cancellationToken);

    public Task<SerilogLogClearResult> ClearInfEntriesAsync(string? fileName, CancellationToken cancellationToken = default) =>
        Task.Run(() => ClearInfEntriesCore(fileName), cancellationToken);

    public Task<SerilogLogDeleteResult> DeleteFileAsync(string? fileName, CancellationToken cancellationToken = default) =>
        Task.Run(() => DeleteFileCore(fileName), cancellationToken);

    private SerilogLogPageResult QueryCore(SerilogLogQuery query)
    {
        Directory.CreateDirectory(_logsDirectory);

        List<SerilogLogFileInfo> files = ListLogFiles();
        if (files.Count == 0)
        {
            return new SerilogLogPageResult
            {
                Files = files,
                LogsDirectory = _logsDirectory,
                ErrorMessage = "暂无日志文件，请先运行应用并产生日志。"
            };
        }

        string selectedFile = string.IsNullOrWhiteSpace(query.FileName)
            ? files[0].FileName
            : query.FileName;

        SerilogLogFileInfo? fileInfo = files.FirstOrDefault(a => a.FileName == selectedFile)
            ?? files[0];
        selectedFile = fileInfo.FileName;

        List<SerilogLogEntry> entries;
        try
        {
            List<string> lines = ReadTailLines(fileInfo.FullPath, MaxLinesToRead);
            entries = ParseEntries(lines)
                .Where(entry => !IsHiddenNoiseEntry(entry))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取日志文件失败：{FileName}", selectedFile);
            return new SerilogLogPageResult
            {
                Files = files,
                SelectedFile = selectedFile,
                LogsDirectory = _logsDirectory,
                ErrorMessage = $"读取日志失败：{ex.Message}"
            };
        }

        IEnumerable<SerilogLogEntry> filtered = entries;
        if (!string.IsNullOrWhiteSpace(query.Level))
        {
            string normalizedLevel = NormalizeLevel(query.Level);
            filtered = filtered.Where(a => NormalizeLevel(a.Level) == normalizedLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            string keyword = query.SearchText.Trim();
            filtered = filtered.Where(a =>
                a.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || a.Exception.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        List<SerilogLogEntry> items = filtered.ToList();
        int pageSize = query.PageSize <= 0 ? 50 : query.PageSize;
        int pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
        int maxPageNumber = Math.Max(1, (int)Math.Ceiling(items.Count / (double)pageSize));
        if (pageNumber > maxPageNumber)
        {
            pageNumber = maxPageNumber;
        }

        List<SerilogLogEntry> pageItems = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation(
            "查询系统日志：文件={FileName}，级别={Level}，关键词={SearchText}，第 {PageNumber} 页，共 {Total} 条",
            selectedFile,
            query.Level ?? "全部",
            query.SearchText ?? string.Empty,
            pageNumber,
            items.Count);

        return new SerilogLogPageResult
        {
            Files = files,
            Items = pageItems,
            Total = items.Count,
            PageNumber = pageNumber,
            PageSize = pageSize,
            MaxPageNumber = maxPageNumber,
            SelectedFile = selectedFile,
            LogsDirectory = _logsDirectory
        };
    }

    private List<SerilogLogFileInfo> ListLogFiles()
    {
        if (!Directory.Exists(_logsDirectory))
        {
            return [];
        }

        return Directory.EnumerateFiles(_logsDirectory, $"{_filePrefix}*.log")
            .Select(path => new FileInfo(path))
            .OrderByDescending(a => a.LastWriteTimeUtc)
            .Select(file => new SerilogLogFileInfo
            {
                FileName = file.Name,
                FullPath = file.FullName,
                SizeBytes = file.Length,
                LastWriteTime = file.LastWriteTime,
                SizeText = FileSizeFormat.Format(file.Length)
            })
            .ToList();
    }

    private SerilogLogClearResult ClearInfEntriesCore(string? fileName)
    {
        Directory.CreateDirectory(_logsDirectory);

        List<SerilogLogFileInfo> files = ListLogFiles();
        if (files.Count == 0)
        {
            return new SerilogLogClearResult
            {
                ErrorMessage = "暂无日志文件。"
            };
        }

        SerilogLogFileInfo fileInfo = files.FirstOrDefault(a => a.FileName == fileName) ?? files[0];
        string selectedFile = fileInfo.FileName;

        List<string> lines;
        try
        {
            lines = ReadAllLines(fileInfo.FullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取日志文件失败：{FileName}", selectedFile);
            return new SerilogLogClearResult
            {
                SelectedFile = selectedFile,
                ErrorMessage = $"读取日志失败：{ex.Message}"
            };
        }

        List<string> kept = [];
        bool skipInfBlock = false;
        int removedCount = 0;

        foreach (string line in lines)
        {
            if (TryParseEntryLevel(line, out string level))
            {
                if (string.Equals(level, "INF", StringComparison.Ordinal))
                {
                    skipInfBlock = true;
                    removedCount++;
                    continue;
                }

                skipInfBlock = false;
                kept.Add(line);
                continue;
            }

            if (skipInfBlock)
            {
                continue;
            }

            kept.Add(line);
        }

        if (removedCount == 0)
        {
            return new SerilogLogClearResult
            {
                Success = true,
                SelectedFile = selectedFile,
                RemovedCount = 0
            };
        }

        try
        {
            WriteAllLines(fileInfo.FullPath, kept);
            _logger.LogInformation(
                "清空日志 INF 记录完成：文件={FileName}，删除 {RemovedCount} 条",
                selectedFile,
                removedCount);
            return new SerilogLogClearResult
            {
                Success = true,
                SelectedFile = selectedFile,
                RemovedCount = removedCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入日志文件失败：{FileName}", selectedFile);
            return new SerilogLogClearResult
            {
                SelectedFile = selectedFile,
                ErrorMessage = $"写入日志失败：{ex.Message}"
            };
        }
    }

    private SerilogLogDeleteResult DeleteFileCore(string? fileName)
    {
        Directory.CreateDirectory(_logsDirectory);

        List<SerilogLogFileInfo> files = ListLogFiles();
        if (files.Count == 0)
        {
            return new SerilogLogDeleteResult
            {
                ErrorMessage = "暂无日志文件。"
            };
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new SerilogLogDeleteResult
            {
                ErrorMessage = "请先选择要删除的日志文件。"
            };
        }

        SerilogLogFileInfo? fileInfo = files.FirstOrDefault(a => a.FileName == fileName);
        if (fileInfo is null)
        {
            return new SerilogLogDeleteResult
            {
                ErrorMessage = $"日志文件不存在：{fileName}"
            };
        }

        if (!IsManagedLogFile(fileInfo.FullPath))
        {
            _logger.LogWarning("拒绝删除日志文件，路径不在允许范围内：{FilePath}", fileInfo.FullPath);
            return new SerilogLogDeleteResult
            {
                DeletedFile = fileInfo.FileName,
                ErrorMessage = "只能删除日志目录内的滚动日志文件。"
            };
        }

        try
        {
            File.Delete(fileInfo.FullPath);
            _logger.LogInformation("删除日志文件成功：{FileName}", fileInfo.FileName);
            return new SerilogLogDeleteResult
            {
                Success = true,
                DeletedFile = fileInfo.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除日志文件失败：{FileName}", fileInfo.FileName);
            return new SerilogLogDeleteResult
            {
                DeletedFile = fileInfo.FileName,
                ErrorMessage = $"删除失败：{ex.Message}"
            };
        }
    }

    private bool IsManagedLogFile(string fullPath)
    {
        string logsDirectory = Path.GetFullPath(_logsDirectory);
        string normalizedPath = Path.GetFullPath(fullPath);
        if (!normalizedPath.StartsWith(logsDirectory + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !string.Equals(normalizedPath, logsDirectory, StringComparison.Ordinal))
        {
            return false;
        }

        string fileName = Path.GetFileName(normalizedPath);
        return fileName.StartsWith(_filePrefix, StringComparison.Ordinal)
            && fileName.EndsWith(".log", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseEntryLevel(string line, out string level)
    {
        Match match = EntryRegex.Match(line);
        if (!match.Success)
        {
            level = string.Empty;
            return false;
        }

        level = match.Groups["level"].Value;
        return true;
    }

    private static List<string> ReadAllLines(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (stream.Length == 0)
        {
            return [];
        }

        List<string> lines = [];
        using StreamReader reader = new(stream);
        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }

    private static void WriteAllLines(string path, IReadOnlyList<string> lines)
    {
        string tempPath = path + ".tmp";
        File.WriteAllLines(tempPath, lines);
        File.Move(tempPath, path, overwrite: true);
    }

    private static List<string> ReadTailLines(string path, int maxLines)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (stream.Length == 0)
        {
            return [];
        }

        List<string> allLines = [];
        using StreamReader reader = new(stream);
        while (reader.ReadLine() is { } line)
        {
            allLines.Add(line);
        }

        if (allLines.Count <= maxLines)
        {
            return allLines;
        }

        return allLines.Skip(allLines.Count - maxLines).ToList();
    }

    internal static List<SerilogLogEntry> ParseEntries(IReadOnlyList<string> lines)
    {
        List<SerilogLogEntry> entries = [];
        SerilogLogEntry? current = null;

        foreach (string line in lines)
        {
            Match match = EntryRegex.Match(line);
            if (match.Success)
            {
                if (current is not null)
                {
                    entries.Add(current);
                }

                current = new SerilogLogEntry
                {
                    Index = entries.Count + 1,
                    Timestamp = TryParseTimestamp(match.Groups["timestamp"].Value),
                    Level = match.Groups["level"].Value,
                    Content = match.Groups["content"].Value
                };
                continue;
            }

            if (current is null)
            {
                continue;
            }

            current.Exception = string.IsNullOrEmpty(current.Exception)
                ? line
                : current.Exception + Environment.NewLine + line;
        }

        if (current is not null)
        {
            entries.Add(current);
        }

        entries.Reverse();
        for (int index = 0; index < entries.Count; index++)
        {
            entries[index].Index = index + 1;
        }

        return entries;
    }

    private static DateTimeOffset? TryParseTimestamp(string value)
    {
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset parsed))
        {
            return parsed;
        }

        return null;
    }

    /// <summary>Blazor Server 断开连接时的无害日志，系统日志页不展示。</summary>
    private static bool IsHiddenNoiseEntry(SerilogLogEntry entry)
    {
        if (entry.Content.Contains("CircuitHost", StringComparison.OrdinalIgnoreCase)
            && entry.Content.Contains("Unhandled exception in circuit", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string combined = entry.Content + Environment.NewLine + entry.Exception;
        return combined.Contains("JSDisconnectedException", StringComparison.OrdinalIgnoreCase)
            && combined.Contains("circuit has disconnected", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeLevel(string level) => level.Trim().ToUpperInvariant() switch
    {
        "INF" or "INFORMATION" or "信息" => "INF",
        "WRN" or "WARNING" or "警告" => "WRN",
        "ERR" or "ERROR" or "错误" => "ERR",
        "FTL" or "FATAL" or "致命" => "FTL",
        "DBG" or "DEBUG" or "调试" => "DBG",
        "VRB" or "VERBOSE" or "详细" => "VRB",
        _ => level.Trim().ToUpperInvariant()
    };
}
