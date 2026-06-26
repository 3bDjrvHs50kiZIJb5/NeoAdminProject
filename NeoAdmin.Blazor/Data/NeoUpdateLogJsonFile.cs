using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;

namespace NeoAdmin.Blazor.Data;

/// <summary>
/// 从 <c>Data/git-changelog.json</c> 读取更新日志（构建时由 generate_git_changelog.py 生成）。
/// </summary>
public static class NeoUpdateLogJsonFile
{
    public const string DefaultRelativePath = "Data/git-changelog.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static IReadOnlyList<NeoUpdateLogEntry> LoadFromEnvironment(
        IWebHostEnvironment environment,
        int maxCount = 15)
    {
        string? path = ResolvePath(environment);
        return path is null ? [] : Load(path, maxCount);
    }

    public static IReadOnlyList<NeoUpdateLogEntry> Load(string path, int maxCount = 15)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            string json = File.ReadAllText(path);
            List<GitChangelogEntryDto>? entries = JsonSerializer.Deserialize<List<GitChangelogEntryDto>>(json, JsonOptions);
            if (entries is null || entries.Count == 0)
            {
                return [];
            }

            int count = Math.Clamp(maxCount, 1, 50);
            return entries
                .Take(count)
                .Select(entry => NeoUpdateLogEntry.FromBody(
                    entry.Subject,
                    entry.CommittedAt,
                    entry.Body,
                    entry.ShortHash))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string? ResolvePath(IWebHostEnvironment environment)
    {
        string[] candidates =
        [
            Path.Combine(environment.ContentRootPath, DefaultRelativePath),
            Path.Combine(AppContext.BaseDirectory, DefaultRelativePath),
        ];

        return candidates.FirstOrDefault(File.Exists);
    }

    private sealed class GitChangelogEntryDto
    {
        [JsonPropertyName("shortHash")]
        public string ShortHash { get; set; } = string.Empty;

        [JsonPropertyName("committedAt")]
        public DateTimeOffset CommittedAt { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
