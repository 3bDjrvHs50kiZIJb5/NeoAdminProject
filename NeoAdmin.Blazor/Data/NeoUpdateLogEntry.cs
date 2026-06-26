namespace NeoAdmin.Blazor.Data;

/// <summary>
/// 单条更新日志。可直接在数组中声明，例如：
/// <code>
/// private static readonly NeoUpdateLogEntry[] Updates =
/// [
///     new("发布 v1.0.0", new DateTimeOffset(2026, 6, 26, 0, 0, 0, TimeSpan.FromHours(8)), "新增功能 A", "修复问题 B"),
/// ];
/// </code>
/// </summary>
public sealed record NeoUpdateLogEntry
{
    public string Title { get; init; } = string.Empty;

    public DateTimeOffset Date { get; init; }

    public IReadOnlyList<string>? Details { get; init; }

    public string? Hash { get; init; }

    public NeoUpdateLogEntry()
    {
    }

    public NeoUpdateLogEntry(string title, DateTimeOffset date, params string[] details)
    {
        Title = title;
        Date = date;
        Details = details.Length > 0 ? details : null;
    }

    public static NeoUpdateLogEntry FromBody(
        string title,
        DateTimeOffset date,
        string? body,
        string? hash = null)
    {
        IReadOnlyList<string> lines = BuildDetailLines(body);
        return new()
        {
            Title = title,
            Date = date,
            Details = lines.Count > 0 ? lines : null,
            Hash = hash,
        };
    }

    public static IReadOnlyList<string> BuildDetailLines(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        return body
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public IReadOnlyList<string> GetDetailLines() =>
        Details is { Count: > 0 } details
            ? details
            : [];
}
