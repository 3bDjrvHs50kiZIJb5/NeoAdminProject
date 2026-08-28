namespace NeoAdmin.Blazor.Models;

/// <summary>ApiExplorer 扫描得到的单个接口。</summary>
public sealed class ApiExplorerEndpoint
{
    public required string Id { get; init; }

    public required string Group { get; init; }

    public required string Title { get; init; }

    /// <summary>来自 Action 的 XML &lt;summary&gt;。</summary>
    public string? Summary { get; init; }

    /// <summary>来自 Action 的 XML &lt;remarks&gt;。</summary>
    public string? Remarks { get; init; }

    public bool AllowAnonymous { get; init; }

    public required string HttpMethod { get; init; }

    public required string RelativePath { get; init; }

    public string? ControllerName { get; init; }

    public string? ActionName { get; init; }

    /// <summary>Controller 所在程序集文件名（如 NeoAdmin.dll）。</summary>
    public string? SourceAssembly { get; init; }

    public IReadOnlyList<ApiExplorerParameter> Parameters { get; init; } = [];

    public string? RequestBodyTypeName { get; init; }

    public string? RequestBodySampleJson { get; init; }

    public string? ResponseTypeName { get; init; }

    /// <summary>按返回类型生成的模拟响应 JSON，用于展示接口返回样式。</summary>
    public string? ResponseBodySampleJson { get; init; }

    public bool HasFormFile { get; init; }
}

public sealed class ApiExplorerParameter
{
    public required string Name { get; init; }

    public required string Source { get; init; }

    public required string Type { get; init; }

    public bool Required { get; init; }

    public string? Description { get; init; }

    public string? DefaultValue { get; init; }
}

public sealed class ApiExplorerInvokeRequest
{
    public required string HttpMethod { get; init; }

    public required string RelativePath { get; init; }

    /// <summary>路径模板中的路由参数，如 id → 1。</summary>
    public IReadOnlyDictionary<string, string>? RouteValues { get; init; }

    /// <summary>Query 参数名 → 值。</summary>
    public IReadOnlyDictionary<string, string>? QueryValues { get; init; }

    public string? JsonBody { get; init; }
}

public sealed record ApiExplorerInvokeResult
{
    public required int StatusCode { get; init; }

    public required string Summary { get; init; }

    public required string Body { get; init; }

    public long DurationMs { get; init; }

    public bool Succeeded { get; init; }

    public bool Truncated { get; init; }
}
