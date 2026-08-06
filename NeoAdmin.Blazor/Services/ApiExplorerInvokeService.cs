using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// API 试调：进程内走 MVC IActionInvoker，避免 Blazor Server HttpClient 回调自身 502。
/// </summary>
public sealed class ApiExplorerInvokeService
{
    private const int MaxBodyChars = 200_000;
    private const int MaxArrayPreviewItems = 10;
    private static readonly Regex RouteParamRegex = new(@"\{([^}:?]+)[^}]*\}", RegexOptions.Compiled);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptions;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ApiExplorerInvokeService> _logger;

    public ApiExplorerInvokeService(
        IServiceScopeFactory scopeFactory,
        IApiDescriptionGroupCollectionProvider apiDescriptions,
        IJSRuntime jsRuntime,
        ILogger<ApiExplorerInvokeService> logger)
    {
        _scopeFactory = scopeFactory;
        _apiDescriptions = apiDescriptions;
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<ApiExplorerInvokeResult> InvokeAsync(
        ApiExplorerInvokeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string path = BuildPath(request.RelativePath, request.RouteValues, request.QueryValues);
        string httpMethod = request.HttpMethod.ToUpperInvariant();
        string? token = await GetTokenAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            ApiDescription? description = FindDescription(httpMethod, request.RelativePath);
            if (description?.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
            {
                return Fail(0, "未找到对应 Controller Action，无法进程内试调", stopwatch);
            }

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            DefaultHttpContext httpContext = CreateHttpContext(
                scope.ServiceProvider,
                httpMethod,
                path,
                token,
                request.JsonBody,
                request.RouteValues);

            Microsoft.AspNetCore.Routing.RouteData routeData = BuildRouteData(actionDescriptor, description, request.RouteValues);
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

            IActionInvokerFactory invokerFactory =
                scope.ServiceProvider.GetRequiredService<IActionInvokerFactory>();
            IActionInvoker? invoker = invokerFactory.CreateInvoker(actionContext);
            if (invoker is null)
            {
                return Fail(0, "无法创建 ActionInvoker", stopwatch);
            }

            await invoker.InvokeAsync();
            stopwatch.Stop();

            httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8, leaveOpen: true);
            string raw = await reader.ReadToEndAsync(cancellationToken);
            (string body, bool truncated) = FormatResponsePreview(raw);
            if (body.Length > MaxBodyChars)
            {
                body = body[..MaxBodyChars] + "\n…(truncated)";
                truncated = true;
            }

            int statusCode = httpContext.Response.StatusCode;
            if (statusCode == 0)
            {
                statusCode = StatusCodes.Status200OK;
            }

            bool succeeded = statusCode >= 200 && statusCode < 300;
            _logger.LogInformation(
                "ApiExplorer 进程内试调完成，Method={Method}，Path={Path}，Status={Status}，DurationMs={DurationMs}",
                httpMethod,
                path,
                statusCode,
                stopwatch.ElapsedMilliseconds);

            return new ApiExplorerInvokeResult
            {
                StatusCode = statusCode,
                Summary = $"{statusCode}",
                Body = body,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Succeeded = succeeded,
                Truncated = truncated
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "ApiExplorer 试调失败，Method={Method}，Path={Path}", httpMethod, path);
            return Fail(0, ex.Message, stopwatch);
        }
    }

    private ApiDescription? FindDescription(string httpMethod, string relativePath)
    {
        string normalized = NormalizePath(relativePath);
        foreach (ApiDescriptionGroup group in _apiDescriptions.ApiDescriptionGroups.Items)
        {
            foreach (ApiDescription description in group.Items)
            {
                if (!string.Equals(description.HttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(NormalizePath(description.RelativePath), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return description;
                }
            }
        }

        return null;
    }

    private static DefaultHttpContext CreateHttpContext(
        IServiceProvider requestServices,
        string httpMethod,
        string pathAndQuery,
        string? token,
        string? jsonBody,
        IReadOnlyDictionary<string, string>? routeValues)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = requestServices
        };

        string path = pathAndQuery;
        string query = string.Empty;
        int q = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
        if (q >= 0)
        {
            path = pathAndQuery[..q];
            query = pathAndQuery[q..];
        }

        if (routeValues is not null)
        {
            path = RouteParamRegex.Replace(path, match =>
            {
                string key = match.Groups[1].Value;
                return routeValues.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
                    ? value
                    : match.Value;
            });
        }

        httpContext.Request.Method = httpMethod;
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/" + path.TrimStart('/');
        httpContext.Request.QueryString = string.IsNullOrEmpty(query)
            ? QueryString.Empty
            : new QueryString(query.StartsWith('?') ? query : "?" + query);
        httpContext.Request.Headers.Accept = "application/json";
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpContext.Request.Headers.Authorization = "Bearer " + token;
        }

        bool allowBody = !HttpMethods.IsGet(httpMethod) && !HttpMethods.IsHead(httpMethod);
        if (allowBody && !string.IsNullOrWhiteSpace(jsonBody))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonBody);
            httpContext.Request.Body = new MemoryStream(bytes);
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.ContentLength = bytes.Length;
        }

        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static Microsoft.AspNetCore.Routing.RouteData BuildRouteData(
        ControllerActionDescriptor actionDescriptor,
        ApiDescription description,
        IReadOnlyDictionary<string, string>? routeValues)
    {
        var routeData = new Microsoft.AspNetCore.Routing.RouteData();
        routeData.Values["controller"] = actionDescriptor.ControllerName;
        routeData.Values["action"] = actionDescriptor.ActionName;

        if (routeValues is not null)
        {
            foreach ((string key, string value) in routeValues)
            {
                routeData.Values[key] = value;
            }
        }

        foreach (ApiParameterDescription parameter in description.ParameterDescriptions)
        {
            if (parameter.Source?.Id == "Path"
                && routeValues is not null
                && routeValues.TryGetValue(parameter.Name, out string? value))
            {
                routeData.Values[parameter.Name] = value;
            }
        }

        return routeData;
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
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static ApiExplorerInvokeResult Fail(int status, string message, Stopwatch stopwatch) =>
        new()
        {
            StatusCode = status,
            Summary = status == 0 ? "请求失败" : $"{status}",
            Body = message,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Succeeded = false,
            Truncated = false
        };

    internal static string BuildPath(
        string relativePath,
        IReadOnlyDictionary<string, string>? routeValues,
        IReadOnlyDictionary<string, string>? queryValues)
    {
        string path = relativePath.Trim().TrimStart('/');
        path = RouteParamRegex.Replace(path, match =>
        {
            string key = match.Groups[1].Value;
            if (routeValues is not null
                && routeValues.TryGetValue(key, out string? value)
                && !string.IsNullOrWhiteSpace(value))
            {
                return Uri.EscapeDataString(value);
            }

            return match.Value;
        });

        if (queryValues is { Count: > 0 })
        {
            string query = string.Join(
                "&",
                queryValues
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
                    .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
            if (!string.IsNullOrEmpty(query))
            {
                path += (path.Contains('?', StringComparison.Ordinal) ? "&" : "?") + query;
            }
        }

        return path;
    }

    private static string NormalizePath(string? relativePath) =>
        (relativePath ?? string.Empty).Trim().TrimStart('/');

    /// <summary>
    /// 所有 JSON 响应统一预览：任意数组最多保留 <see cref="MaxArrayPreviewItems"/> 条。
    /// </summary>
    private static (string Json, bool Truncated) FormatResponsePreview(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (string.Empty, false);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(raw);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
            {
                bool truncated = WritePreview(writer, document.RootElement, MaxArrayPreviewItems);
                writer.Flush();
                string json = Encoding.UTF8.GetString(stream.ToArray());
                return (json, truncated);
            }
        }
        catch (JsonException)
        {
            if (raw.Length > MaxBodyChars)
            {
                return (raw[..MaxBodyChars] + "\n…(truncated)", true);
            }

            return (raw, false);
        }
    }

    private static bool WritePreview(Utf8JsonWriter writer, JsonElement element, int maxArrayItems)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                writer.WriteStartObject();
                bool truncated = false;
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    truncated |= WritePreview(writer, property.Value, maxArrayItems);
                }

                writer.WriteEndObject();
                return truncated;
            }
            case JsonValueKind.Array:
            {
                writer.WriteStartArray();
                bool truncated = false;
                int index = 0;
                int total = element.GetArrayLength();
                foreach (JsonElement item in element.EnumerateArray())
                {
                    if (index >= maxArrayItems)
                    {
                        truncated = true;
                        break;
                    }

                    truncated |= WritePreview(writer, item, maxArrayItems);
                    index++;
                }

                if (truncated)
                {
                    writer.WriteStartObject();
                    writer.WriteString("_preview", $"… 已截取，原数组共 {total} 条，仅展示前 {maxArrayItems} 条样例");
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                return truncated;
            }
            default:
                element.WriteTo(writer);
                return false;
        }
    }
}
