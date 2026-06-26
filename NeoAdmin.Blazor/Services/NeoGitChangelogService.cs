using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Data;

namespace NeoAdmin.Blazor.Services;

public interface INeoGitChangelogService
{
    Task<IReadOnlyList<NeoUpdateLogEntry>> GetRecentEntriesAsync(
        int maxCount = 15,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 优先从本地 Git 仓库读取最近提交；发布后无 Git 时回退到构建时生成的 JSON 文件。
/// </summary>
public sealed class NeoGitChangelogService(
    IWebHostEnvironment environment,
    ILogger<NeoGitChangelogService> logger) : INeoGitChangelogService
{
    public const string DefaultBundledRelativePath = NeoUpdateLogJsonFile.DefaultRelativePath;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object CacheLock = new();
    private IReadOnlyList<NeoUpdateLogEntry>? _cache;
    private DateTime _cacheExpiresAtUtc;

    public Task<IReadOnlyList<NeoUpdateLogEntry>> GetRecentEntriesAsync(
        int maxCount = 15,
        CancellationToken cancellationToken = default)
    {
        lock (CacheLock)
        {
            if (_cache is not null && DateTime.UtcNow < _cacheExpiresAtUtc)
            {
                return Task.FromResult(_cache);
            }
        }

        return LoadAndCacheAsync(maxCount, cancellationToken);
    }

    private async Task<IReadOnlyList<NeoUpdateLogEntry>> LoadAndCacheAsync(
        int maxCount,
        CancellationToken cancellationToken)
    {
        string? repoRoot = FindGitRepositoryRoot(environment.ContentRootPath);
        if (repoRoot is not null)
        {
            IReadOnlyList<NeoUpdateLogEntry> gitEntries = await Task.Run(
                () => ReadGitLog(repoRoot, maxCount),
                cancellationToken);

            if (gitEntries.Count > 0)
            {
                logger.LogInformation("从 Git 读取更新日志，Count={Count}", gitEntries.Count);
                return CacheEntries(gitEntries);
            }
        }
        else
        {
            logger.LogDebug(
                "未找到 Git 仓库，尝试读取构建时生成的更新日志。ContentRoot={ContentRoot}",
                environment.ContentRootPath);
        }

        IReadOnlyList<NeoUpdateLogEntry> bundled = ReadBundledChangelog(maxCount);
        logger.LogInformation("读取构建时更新日志，Count={Count}", bundled.Count);
        return CacheEntries(bundled);
    }

    private IReadOnlyList<NeoUpdateLogEntry> CacheEntries(IReadOnlyList<NeoUpdateLogEntry> entries)
    {
        lock (CacheLock)
        {
            _cache = entries;
            _cacheExpiresAtUtc = DateTime.UtcNow.Add(CacheDuration);
            return _cache;
        }
    }

    private static string? FindGitRepositoryRoot(string startPath)
    {
        DirectoryInfo? directory = new(startPath);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private IReadOnlyList<NeoUpdateLogEntry> ReadGitLog(string repoRoot, int maxCount)
    {
        int count = Math.Clamp(maxCount, 1, 50);

        ProcessStartInfo startInfo = new()
        {
            FileName = "git",
            Arguments = $"-C \"{repoRoot}\" log -{count} --format=%H%x00%ai%x00%s%x00%b%x00",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("无法启动 git 进程。");

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                logger.LogWarning(
                    "读取 Git 更新日志失败。ExitCode={ExitCode}, Error={Error}",
                    process.ExitCode,
                    error.Trim());
                return [];
            }

            return ParseGitLog(output);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            logger.LogWarning(ex, "执行 git 命令失败，更新日志不可用。");
            return [];
        }
    }

    private IReadOnlyList<NeoUpdateLogEntry> ReadBundledChangelog(int maxCount)
    {
        string? path = ResolveBundledChangelogPath();
        if (path is null)
        {
            logger.LogDebug("未找到构建时生成的更新日志文件。");
            return [];
        }

        try
        {
            IReadOnlyList<NeoUpdateLogEntry> entries = NeoUpdateLogJsonFile.Load(path, maxCount);
            return entries;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "读取构建时更新日志文件失败。Path={Path}", path);
            return [];
        }
    }

    private string? ResolveBundledChangelogPath() =>
        NeoUpdateLogJsonFile.ResolvePath(environment);

    private static List<NeoUpdateLogEntry> ParseGitLog(string output)
    {
        List<NeoUpdateLogEntry> entries = [];
        if (string.IsNullOrWhiteSpace(output))
        {
            return entries;
        }

        string[] records = output.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i + 3 < records.Length; i += 4)
        {
            string hash = records[i].Trim();
            if (hash.Length == 0)
            {
                continue;
            }

            if (!DateTimeOffset.TryParse(records[i + 1], out DateTimeOffset committedAt))
            {
                continue;
            }

            string subject = records[i + 2].Trim();
            string body = records[i + 3].Trim();
            if (string.IsNullOrWhiteSpace(subject))
            {
                continue;
            }

            entries.Add(NeoUpdateLogEntry.FromBody(
                subject,
                committedAt,
                body,
                hash.Length > 7 ? hash[..7] : hash));
        }

        return entries;
    }
}
