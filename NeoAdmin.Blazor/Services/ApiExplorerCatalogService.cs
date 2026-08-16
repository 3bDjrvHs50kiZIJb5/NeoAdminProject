using System.Collections;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 从 <see cref="IApiDescriptionGroupCollectionProvider"/> 构建 API 目录（含宿主经 AddNeoAdminApi 注册的程序集）。
/// 请求默认值直接读取 ContentRoot 下 <c>appsettings.ApiExplorer.json</c>，不做账号覆盖。
/// </summary>
public sealed class ApiExplorerCatalogService
{
    public const string DefaultsFileName = "appsettings.ApiExplorer.json";

    private static readonly JsonSerializerOptions SampleJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptions;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ApiExplorerCatalogService> _logger;
    private IReadOnlyList<ApiExplorerEndpoint>? _cachedEndpoints;
    private Dictionary<string, JsonNode>? _configuredDefaults;
    private Dictionary<string, JsonNode>? _configuredResponseDefaults;

    public ApiExplorerCatalogService(
        IApiDescriptionGroupCollectionProvider apiDescriptions,
        IHostEnvironment environment,
        ILogger<ApiExplorerCatalogService> logger)
    {
        _apiDescriptions = apiDescriptions;
        _environment = environment;
        _logger = logger;
    }

    public IReadOnlyList<ApiExplorerEndpoint> GetEndpoints()
    {
        if (_cachedEndpoints is not null)
        {
            return _cachedEndpoints;
        }

        List<ApiExplorerEndpoint> endpoints = [];

        foreach (ApiDescriptionGroup group in _apiDescriptions.ApiDescriptionGroups.Items)
        {
            foreach (ApiDescription description in group.Items)
            {
                string relativePath = NormalizePath(description.RelativePath);
                if (!relativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase)
                    && !relativePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string groupName = ResolveGroupName(description, group.GroupName);
                if (ShouldSkip(groupName, relativePath))
                {
                    continue;
                }

                string httpMethod = (description.HttpMethod ?? "GET").ToUpperInvariant();
                ControllerActionDescriptor? actionDescriptor = description.ActionDescriptor as ControllerActionDescriptor;
                string controllerName = actionDescriptor?.ControllerName ?? groupName;
                string? actionName = actionDescriptor?.ActionName;
                (string? summary, string? remarks, bool allowAnonymous) = ApiExplorerDocumentation.GetActionDocs(actionDescriptor);

                List<ApiExplorerParameter> parameters = [];
                Type? bodyType = null;
                bool hasFormFile = false;

                foreach (ApiParameterDescription parameter in description.ParameterDescriptions)
                {
                    if (parameter.Source == BindingSource.Services
                        || parameter.Source == BindingSource.Special
                        || parameter.Source == BindingSource.ModelBinding && parameter.Name == "cancellationToken")
                    {
                        continue;
                    }

                    if (typeof(CancellationToken).IsAssignableFrom(parameter.Type)
                        || typeof(IFormFile).IsAssignableFrom(parameter.Type)
                        || typeof(IEnumerable<IFormFile>).IsAssignableFrom(parameter.Type))
                    {
                        if (typeof(IFormFile).IsAssignableFrom(parameter.Type)
                            || typeof(IEnumerable<IFormFile>).IsAssignableFrom(parameter.Type))
                        {
                            hasFormFile = true;
                        }

                        if (typeof(CancellationToken).IsAssignableFrom(parameter.Type))
                        {
                            continue;
                        }
                    }

                    string source = ResolveSource(parameter.Source);
                    if (source == "Body" && parameter.Type is not null && bodyType is null)
                    {
                        bodyType = UnwrapNullable(parameter.Type);
                    }

                    parameters.Add(new ApiExplorerParameter
                    {
                        Name = parameter.Name ?? "(unnamed)",
                        Source = source,
                        Type = FormatTypeName(parameter.Type),
                        Required = parameter.IsRequired,
                        Description = GetDescription(parameter),
                        DefaultValue = ResolveParameterDefault(parameter)
                    });
                }

                string id = $"{httpMethod}:{relativePath}";
                string title = actionName ?? relativePath;
                string? sampleJson = bodyType is null ? null : BuildSampleJson(bodyType);
                JsonNode? configured = FindConfiguredDefaults(id, relativePath);
                if (configured is not null)
                {
                    sampleJson = configured is JsonObject or JsonArray
                        ? configured.ToJsonString(SampleJsonOptions)
                        : configured.ToJsonString();
                    if (configured is JsonObject configuredObject)
                    {
                        ApplyConfiguredParameterDefaults(parameters, configuredObject);
                    }
                }
                else if (parameters.Count > 0
                    && parameters.Any(p => p.Source is "Query" or "Path" or "Form"))
                {
                    // 无 Body 模型时，用 Query/Path 默认值拼一份可编辑 JSON，便于在线调试
                    sampleJson = BuildParameterBagJson(parameters);
                }

                Type? responseType = ResolveResponseType(description);
                JsonNode? configuredResponse = FindConfiguredResponseDefaults(id, relativePath);
                endpoints.Add(new ApiExplorerEndpoint
                {
                    Id = id,
                    Group = groupName,
                    Title = title,
                    Summary = summary,
                    Remarks = remarks,
                    AllowAnonymous = allowAnonymous,
                    HttpMethod = httpMethod,
                    RelativePath = relativePath.TrimStart('/'),
                    ControllerName = controllerName,
                    ActionName = actionName,
                    Parameters = parameters,
                    RequestBodyTypeName = bodyType is null ? null : FormatTypeName(bodyType),
                    RequestBodySampleJson = sampleJson,
                    ResponseTypeName = responseType is null ? null : FormatTypeName(responseType),
                    ResponseBodySampleJson = configuredResponse is not null
                        ? FormatResponseSampleJson(configuredResponse)
                        : BuildResponseSampleJson(responseType),
                    HasFormFile = hasFormFile
                });
            }
        }

        List<ApiExplorerEndpoint> ordered = endpoints
            .GroupBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(e => e.Group, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.HttpMethod, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _cachedEndpoints = ordered;
        _logger.LogInformation(
            "ApiExplorer 扫描完成，Count={Count}，ConfiguredDefaults={ConfiguredCount}",
            ordered.Count,
            LoadConfiguredDefaults().Count);
        return ordered;
    }

    private Dictionary<string, JsonNode> LoadConfiguredDefaults()
    {
        if (_configuredDefaults is not null)
        {
            return _configuredDefaults;
        }

        var map = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);

        // 必须直接读 JSON 文件：键名含 POST:api/... 中的冒号，走 IConfiguration 会被拆成层级导致读不到。
        string path = Path.Combine(_environment.ContentRootPath, DefaultsFileName);
        if (File.Exists(path))
        {
            try
            {
                JsonNode? root = JsonNode.Parse(File.ReadAllText(path));
                if (root?["ApiExplorer"]?["RequestDefaults"] is JsonObject requestDefaults)
                {
                    foreach (KeyValuePair<string, JsonNode?> pair in requestDefaults)
                    {
                        if (pair.Value is not null)
                        {
                            map[pair.Key] = pair.Value.DeepClone()!;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 ApiExplorer 请求默认配置失败，Path={Path}", path);
            }
        }

        _configuredDefaults = map;
        return map;
    }

    private JsonNode? FindConfiguredDefaults(string endpointId, string relativePath)
    {
        Dictionary<string, JsonNode> map = LoadConfiguredDefaults();
        if (map.TryGetValue(endpointId, out JsonNode? byId))
        {
            return byId;
        }

        string path = relativePath.TrimStart('/');
        if (map.TryGetValue(path, out JsonNode? byPath))
        {
            return byPath;
        }

        // 兼容不带 METHOD 前缀的键，如 api/login/@Login
        foreach (KeyValuePair<string, JsonNode> pair in map)
        {
            if (pair.Key.EndsWith(":" + path, StringComparison.OrdinalIgnoreCase)
                || pair.Key.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private Dictionary<string, JsonNode> LoadConfiguredResponseDefaults()
    {
        if (_configuredResponseDefaults is not null)
        {
            return _configuredResponseDefaults;
        }

        var map = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
        string path = Path.Combine(_environment.ContentRootPath, DefaultsFileName);
        if (File.Exists(path))
        {
            try
            {
                JsonNode? root = JsonNode.Parse(File.ReadAllText(path));
                if (root?["ApiExplorer"]?["ResponseDefaults"] is JsonObject responseDefaults)
                {
                    foreach (KeyValuePair<string, JsonNode?> pair in responseDefaults)
                    {
                        if (pair.Value is not null)
                        {
                            map[pair.Key] = pair.Value.DeepClone()!;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 ApiExplorer 响应样例配置失败，Path={Path}", path);
            }
        }

        _configuredResponseDefaults = map;
        return map;
    }

    private JsonNode? FindConfiguredResponseDefaults(string endpointId, string relativePath)
    {
        Dictionary<string, JsonNode> map = LoadConfiguredResponseDefaults();
        if (map.TryGetValue(endpointId, out JsonNode? byId))
        {
            return byId;
        }

        string path = relativePath.TrimStart('/');
        if (map.TryGetValue(path, out JsonNode? byPath))
        {
            return byPath;
        }

        foreach (KeyValuePair<string, JsonNode> pair in map)
        {
            if (pair.Key.EndsWith(":" + path, StringComparison.OrdinalIgnoreCase)
                || pair.Key.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private static string FormatJsonNode(JsonNode node)
    {
        try
        {
            // 重新序列化以应用缩进与中文不转义
            using JsonDocument document = JsonDocument.Parse(node.ToJsonString());
            return JsonSerializer.Serialize(document.RootElement, SampleJsonOptions);
        }
        catch
        {
            return node.ToJsonString(SampleJsonOptions);
        }
    }

    /// <summary>
    /// 响应样例展示：与试调预览一致，展开二次序列化的 JSON 字符串字段，并截取过长数组。
    /// </summary>
    private static string FormatResponseSampleJson(JsonNode node)
    {
        (string json, _) = ApiExplorerInvokeService.FormatResponsePreview(node.ToJsonString());
        return string.IsNullOrEmpty(json) ? FormatJsonNode(node) : json;
    }

    private static void ApplyConfiguredParameterDefaults(
        List<ApiExplorerParameter> parameters,
        JsonObject configured)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            ApiExplorerParameter parameter = parameters[i];
            if (parameter.Source is not ("Query" or "Path" or "Form" or "Header"))
            {
                continue;
            }

            JsonNode? value = FindPropertyIgnoreCase(configured, parameter.Name);
            if (value is null)
            {
                continue;
            }

            parameters[i] = new ApiExplorerParameter
            {
                Name = parameter.Name,
                Source = parameter.Source,
                Type = parameter.Type,
                Required = parameter.Required,
                Description = parameter.Description,
                DefaultValue = value is JsonValue jsonValue
                    ? jsonValue.ToJsonString().Trim('"')
                    : value.ToJsonString()
            };
        }
    }

    private static JsonNode? FindPropertyIgnoreCase(JsonObject obj, string name)
    {
        foreach (KeyValuePair<string, JsonNode?> property in obj)
        {
            if (string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }

    private static string BuildParameterBagJson(IReadOnlyList<ApiExplorerParameter> parameters)
    {
        var bag = new JsonObject();
        foreach (ApiExplorerParameter parameter in parameters.Where(p => p.Source is "Query" or "Path" or "Form"))
        {
            string name = ToCamelCase(parameter.Name);
            if (string.IsNullOrWhiteSpace(parameter.DefaultValue))
            {
                bag[name] = parameter.Type switch
                {
                    "int" or "long" or "number" => 0,
                    "bool" => false,
                    _ => ""
                };
                continue;
            }

            if (parameter.Type is "int" or "long" && long.TryParse(parameter.DefaultValue, out long number))
            {
                bag[name] = number;
            }
            else if (parameter.Type is "number" && decimal.TryParse(parameter.DefaultValue, out decimal dec))
            {
                bag[name] = dec;
            }
            else if (parameter.Type is "bool" && bool.TryParse(parameter.DefaultValue, out bool flag))
            {
                bag[name] = flag;
            }
            else
            {
                bag[name] = parameter.DefaultValue;
            }
        }

        return bag.ToJsonString(SampleJsonOptions);
    }

    public ApiExplorerEndpoint? Find(string id) =>
        GetEndpoints().FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>读取已配置的响应样例 JSON（无则返回 null）。</summary>
    public string? GetConfiguredResponseSampleJson(string endpointId)
    {
        if (string.IsNullOrWhiteSpace(endpointId))
        {
            return null;
        }

        string path = endpointId;
        int colon = endpointId.IndexOf(':');
        if (colon >= 0 && colon < endpointId.Length - 1)
        {
            path = endpointId[(colon + 1)..];
        }

        JsonNode? node = FindConfiguredResponseDefaults(endpointId, path);
        return node is null ? null : FormatResponseSampleJson(node);
    }

    /// <summary>读取已配置的请求默认 JSON（无则返回 null）。</summary>
    public string? GetConfiguredRequestSampleJson(string endpointId)
    {
        if (string.IsNullOrWhiteSpace(endpointId))
        {
            return null;
        }

        string path = endpointId;
        int colon = endpointId.IndexOf(':');
        if (colon >= 0 && colon < endpointId.Length - 1)
        {
            path = endpointId[(colon + 1)..];
        }

        JsonNode? node = FindConfiguredDefaults(endpointId, path);
        return node is null ? null : FormatJsonNode(node);
    }

    private static readonly object DefaultsFileLock = new();

    /// <summary>
    /// 将试调成功的请求 Body 写入 <c>appsettings.ApiExplorer.json</c> 的 <c>RequestDefaults</c>（覆盖同键）。
    /// </summary>
    /// <returns>是否成功写入文件。</returns>
    public bool SaveRequestDefault(string endpointId, string requestBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointId);
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return false;
        }

        JsonNode? requestNode;
        try
        {
            requestNode = JsonNode.Parse(requestBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "ApiExplorer 请求默认配置不是合法 JSON，跳过写入，Endpoint={EndpointId}", endpointId);
            return false;
        }

        if (requestNode is null)
        {
            return false;
        }

        return SaveDefaultsSection(
            sectionName: "RequestDefaults",
            endpointId: endpointId,
            node: requestNode,
            onSaved: cloned =>
            {
                _configuredDefaults ??= new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
                _configuredDefaults[endpointId] = cloned;
                _cachedEndpoints = null;
            },
            logLabel: "请求默认配置");
    }

    /// <summary>
    /// 将试调成功的实际响应写入 <c>appsettings.ApiExplorer.json</c> 的 <c>ResponseDefaults</c>（覆盖同键）。
    /// </summary>
    /// <returns>是否成功写入文件。</returns>
    public bool SaveResponseDefault(string endpointId, string responseBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointId);
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return false;
        }

        JsonNode? responseNode;
        try
        {
            responseNode = JsonNode.Parse(responseBody);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "ApiExplorer 响应样例不是合法 JSON，跳过写入，Endpoint={EndpointId}", endpointId);
            return false;
        }

        if (responseNode is null)
        {
            return false;
        }

        return SaveDefaultsSection(
            sectionName: "ResponseDefaults",
            endpointId: endpointId,
            node: responseNode,
            onSaved: cloned =>
            {
                _configuredResponseDefaults ??= new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
                _configuredResponseDefaults[endpointId] = cloned;
                _cachedEndpoints = null;
            },
            logLabel: "响应样例");
    }

    private bool SaveDefaultsSection(
        string sectionName,
        string endpointId,
        JsonNode node,
        Action<JsonNode> onSaved,
        string logLabel)
    {
        string path = Path.Combine(_environment.ContentRootPath, DefaultsFileName);

        try
        {
            lock (DefaultsFileLock)
            {
                JsonObject root;
                if (File.Exists(path))
                {
                    JsonNode? parsed = JsonNode.Parse(File.ReadAllText(path));
                    root = parsed as JsonObject ?? new JsonObject();
                }
                else
                {
                    root = new JsonObject();
                }

                if (root["ApiExplorer"] is not JsonObject apiExplorer)
                {
                    apiExplorer = new JsonObject();
                    root["ApiExplorer"] = apiExplorer;
                }

                if (apiExplorer[sectionName] is not JsonObject sectionDefaults)
                {
                    sectionDefaults = new JsonObject();
                    apiExplorer[sectionName] = sectionDefaults;
                }

                RemoveMatchingEndpointKeys(sectionDefaults, endpointId);
                JsonNode cloned = node.DeepClone();
                sectionDefaults[endpointId] = cloned;

                string json = root.ToJsonString(SampleJsonOptions) + Environment.NewLine;
                File.WriteAllText(path, json);

                onSaved(cloned);
            }

            _logger.LogInformation(
                "已写入 ApiExplorer {LogLabel}，Endpoint={EndpointId}，Path={Path}",
                logLabel,
                endpointId,
                path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "写入 ApiExplorer {LogLabel} 失败，Endpoint={EndpointId}，Path={Path}",
                logLabel,
                endpointId,
                path);
            return false;
        }
    }

    private static void RemoveMatchingEndpointKeys(JsonObject sectionDefaults, string endpointId)
    {
        string? pathPart = endpointId;
        int colon = endpointId.IndexOf(':');
        if (colon >= 0 && colon < endpointId.Length - 1)
        {
            pathPart = endpointId[(colon + 1)..];
        }

        List<string> toRemove = [];
        foreach (KeyValuePair<string, JsonNode?> pair in sectionDefaults)
        {
            if (string.Equals(pair.Key, endpointId, StringComparison.OrdinalIgnoreCase))
            {
                toRemove.Add(pair.Key);
                continue;
            }

            if (pathPart is not null
                && (pair.Key.Equals(pathPart, StringComparison.OrdinalIgnoreCase)
                    || pair.Key.EndsWith(":" + pathPart, StringComparison.OrdinalIgnoreCase)))
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (string key in toRemove)
        {
            sectionDefaults.Remove(key);
        }
    }
    /// <summary>
    /// 启动时扫描接口：若 <c>appsettings.ApiExplorer.json</c> 不存在则创建；
    /// 若已存在则补全缺失的接口默认值（不覆盖已有项）。
    /// </summary>
    /// <returns>是否写入了文件（新建或补全）。</returns>
    public bool EnsureRequestDefaultsConfigFile(string contentRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        string path = Path.Combine(contentRootPath, DefaultsFileName);

        try
        {
            JsonObject root;
            JsonObject requestDefaults;
            bool fileExisted = File.Exists(path);

            lock (DefaultsFileLock)
            {
                if (fileExisted)
                {
                    JsonNode? parsed = JsonNode.Parse(File.ReadAllText(path));
                    root = parsed as JsonObject ?? new JsonObject();
                    if (root["ApiExplorer"] is not JsonObject apiExplorer)
                    {
                        apiExplorer = new JsonObject();
                        root["ApiExplorer"] = apiExplorer;
                    }

                    if (apiExplorer["RequestDefaults"] is not JsonObject existingDefaults)
                    {
                        requestDefaults = new JsonObject();
                        apiExplorer["RequestDefaults"] = requestDefaults;
                    }
                    else
                    {
                        requestDefaults = existingDefaults;
                    }
                }
                else
                {
                    requestDefaults = new JsonObject();
                    root = new JsonObject
                    {
                        ["ApiExplorer"] = new JsonObject
                        {
                            ["RequestDefaults"] = requestDefaults
                        }
                    };
                }

                // 用「无配置覆盖」的扫描结果补全缺失键
                _configuredDefaults = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
                _configuredResponseDefaults = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
                _cachedEndpoints = null;

                IReadOnlyList<ApiExplorerEndpoint> endpoints = GetEndpoints();
                int added = 0;

                foreach (ApiExplorerEndpoint endpoint in endpoints)
                {
                    if (string.IsNullOrWhiteSpace(endpoint.RequestBodySampleJson))
                    {
                        continue;
                    }

                    if (HasConfiguredKey(requestDefaults, endpoint.Id))
                    {
                        continue;
                    }

                    try
                    {
                        JsonNode? node = JsonNode.Parse(endpoint.RequestBodySampleJson);
                        if (node is null)
                        {
                            continue;
                        }

                        requestDefaults[endpoint.Id] = node;
                        added++;
                    }
                    catch (JsonException)
                    {
                        // 跳过无法解析的样例
                    }
                }

                // 恢复并合并最终配置供本进程使用
                _configuredDefaults = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, JsonNode?> pair in requestDefaults)
                {
                    if (pair.Value is not null)
                    {
                        _configuredDefaults[pair.Key] = pair.Value;
                    }
                }

                // 响应样例仍从文件读取，避免 Ensure 过程中的空缓存残留
                _configuredResponseDefaults = null;
                _cachedEndpoints = null;

                if (!fileExisted || added > 0)
                {
                    string json = root.ToJsonString(SampleJsonOptions) + Environment.NewLine;
                    File.WriteAllText(path, json);

                    _logger.LogInformation(
                        fileExisted
                            ? "已补全 ApiExplorer 请求默认配置，Path={Path}，Added={Added}，Total={Total}"
                            : "已自动创建 ApiExplorer 请求默认配置，Path={Path}，Count={Total}",
                        path,
                        fileExisted ? added : requestDefaults.Count,
                        requestDefaults.Count);
                    return true;
                }

                _logger.LogDebug(
                    "ApiExplorer 请求默认配置已完整，无需补全，Path={Path}，Count={Count}",
                    path,
                    requestDefaults.Count);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "自动创建/补全 ApiExplorer 请求默认配置失败，Path={Path}", path);
            return false;
        }
    }

    private static bool HasConfiguredKey(JsonObject requestDefaults, string endpointId)
    {
        foreach (KeyValuePair<string, JsonNode?> pair in requestDefaults)
        {
            if (string.Equals(pair.Key, endpointId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ResolveParameterDefault(ApiParameterDescription parameter)
    {
        if (parameter.DefaultValue is not null)
        {
            return parameter.DefaultValue.ToString();
        }

        object? sample = CreateSampleValue(parameter.Type ?? typeof(string), depth: 0, parameter.Name ?? string.Empty);
        return sample?.ToString();
    }

    private static bool ShouldSkip(string groupName, string relativePath) =>
        groupName.Contains("e2e", StringComparison.OrdinalIgnoreCase)
        || relativePath.Contains("e2e", StringComparison.OrdinalIgnoreCase);

    private static string ResolveGroupName(ApiDescription description, string? apiGroupName)
    {
        if (description.ActionDescriptor is ControllerActionDescriptor cad
            && !string.IsNullOrWhiteSpace(cad.ControllerName))
        {
            return cad.ControllerName;
        }

        if (!string.IsNullOrWhiteSpace(apiGroupName))
        {
            return apiGroupName;
        }

        string path = NormalizePath(description.RelativePath).TrimStart('/');
        if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            path = path[4..];
        }

        string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "api";
    }

    private static string NormalizePath(string? relativePath) =>
        (relativePath ?? string.Empty).Trim().TrimStart('/');

    private static string ResolveSource(BindingSource? source)
    {
        if (source is null)
        {
            return "Unknown";
        }

        if (source == BindingSource.Body)
        {
            return "Body";
        }

        if (source == BindingSource.Path)
        {
            return "Path";
        }

        if (source == BindingSource.Query)
        {
            return "Query";
        }

        if (source == BindingSource.Header)
        {
            return "Header";
        }

        if (source == BindingSource.Form || source == BindingSource.FormFile)
        {
            return "Form";
        }

        return source.DisplayName ?? source.Id ?? "Unknown";
    }

    private static string? GetDescription(ApiParameterDescription parameter)
    {
        ModelMetadata? metadata = parameter.ModelMetadata;
        if (metadata is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(metadata.Description))
        {
            return metadata.Description;
        }

        return null;
    }

    private static string FormatTypeName(Type? type)
    {
        if (type is null)
        {
            return "object";
        }

        type = UnwrapNullable(type);
        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(long))
        {
            return "long";
        }

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return "number";
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
        {
            return "datetime";
        }

        if (type == typeof(Guid))
        {
            return "guid";
        }

        if (type.IsEnum)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return FormatTypeName(type.GetElementType()) + "[]";
        }

        if (type.IsGenericType)
        {
            string name = type.Name;
            int tick = name.IndexOf('`');
            if (tick > 0)
            {
                name = name[..tick];
            }

            string args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{name}<{args}>";
        }

        return type.Name;
    }

    private static Type UnwrapNullable(Type type) =>
        Nullable.GetUnderlyingType(type) ?? type;

    private static Type? ResolveResponseType(ApiDescription description)
    {
        foreach (ApiResponseType response in description.SupportedResponseTypes
                     .OrderBy(r => r.StatusCode is >= 200 and < 300 ? 0 : 1)
                     .ThenBy(r => r.StatusCode))
        {
            Type? type = response.Type is null ? null : UnwrapNullable(response.Type);
            if (type is null || type == typeof(void) || type == typeof(Task))
            {
                continue;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                type = UnwrapNullable(type.GetGenericArguments()[0]);
            }

            if (type is null || type == typeof(void))
            {
                continue;
            }

            return type;
        }

        return null;
    }

    private static string BuildResponseSampleJson(Type? responseType)
    {
        try
        {
            object? sample = responseType is null
                ? CreateApiResultSample(typeof(object), depth: 0)
                : CreateSampleValue(responseType, depth: 0, propertyName: null);
            return JsonSerializer.Serialize(sample ?? CreateApiResultSample(typeof(object), depth: 0), SampleJsonOptions);
        }
        catch
        {
            return """
                   {
                     "code": 0,
                     "message": "成功",
                     "data": null,
                     "succeeded": true
                   }
                   """;
        }
    }

    private static string? BuildSampleJson(Type type)
    {
        try
        {
            object? sample = CreateSampleValue(type, depth: 0, propertyName: null);
            return JsonSerializer.Serialize(sample, SampleJsonOptions);
        }
        catch
        {
            return "{\n  \n}";
        }
    }

    private static Dictionary<string, object?> CreateApiResultSample(Type? dataType, int depth)
    {
        object? data = dataType is null || dataType == typeof(object) || dataType == typeof(void)
            ? new Dictionary<string, object?> { ["sample"] = "value" }
            : CreateSampleValue(dataType, depth + 1, "data");

        return new Dictionary<string, object?>
        {
            ["code"] = 0,
            ["message"] = "成功",
            ["data"] = data,
            ["succeeded"] = true
        };
    }

    private static object? CreateSampleValue(
        Type type,
        int depth,
        string? propertyName = null)
    {
        if (depth > 2)
        {
            return null;
        }

        type = UnwrapNullable(type);

        if (type == typeof(ApiResult))
        {
            return new Dictionary<string, object?>
            {
                ["code"] = 0,
                ["message"] = "成功",
                ["succeeded"] = true
            };
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResult<>))
        {
            return CreateApiResultSample(type.GetGenericArguments()[0], depth);
        }

        if (type == typeof(object))
        {
            return new Dictionary<string, object?> { ["sample"] = "value" };
        }

        if (type == typeof(string))
        {
            return ResolveStringSample(propertyName);
        }

        if (type == typeof(bool))
        {
            return false;
        }

        if (type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long))
        {
            return ResolveIntegerSample(propertyName);
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return ResolveNumberSample(propertyName);
        }

        if (type == typeof(Guid))
        {
            return Guid.Empty;
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return DateTime.UtcNow.ToString("O");
        }

        if (type == typeof(DateOnly))
        {
            return DateOnly.FromDateTime(DateTime.UtcNow).ToString("O");
        }

        if (type.IsEnum)
        {
            Array values = Enum.GetValues(type);
            return values.Length > 0 ? values.GetValue(0) : 0;
        }

        if (type == typeof(IFormFile) || typeof(IFormFile).IsAssignableFrom(type))
        {
            return null;
        }

        if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            if (elementType is null)
            {
                return Array.Empty<object>();
            }

            object? element = CreateSampleValue(elementType, depth + 1, propertyName);
            Array array = Array.CreateInstance(elementType, 1);
            if (element is not null && elementType.IsInstanceOfType(element))
            {
                array.SetValue(element, 0);
            }

            return array;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            Type elementType = type.IsGenericType
                ? type.GetGenericArguments()[0]
                : typeof(object);
            object? element = CreateSampleValue(elementType, depth + 1, propertyName);
            return element is null ? Array.Empty<object>() : new[] { element };
        }

        if (type.IsInterface || type.IsAbstract)
        {
            return new Dictionary<string, object?>();
        }

        try
        {
            object? instance = Activator.CreateInstance(type);
            if (instance is null)
            {
                return new Dictionary<string, object?>();
            }

            var bag = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                {
                    continue;
                }

                string name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                    ?? ToCamelCase(property.Name);
                try
                {
                    bag[name] = CreateSampleValue(property.PropertyType, depth + 1, property.Name);
                }
                catch
                {
                    bag[name] = null;
                }
            }

            return bag.Count > 0 ? bag : instance;
        }
        catch
        {
            return new Dictionary<string, object?>();
        }
    }

    private static object ResolveStringSample(string? propertyName)
    {
        string name = propertyName ?? string.Empty;

        if (IsMerchantAccountField(name))
        {
            return "demo_merchant";
        }

        if (IsMerchantApiKeyField(name))
        {
            return "demo_api_key";
        }

        if (IsUsernameField(name))
        {
            return "sample";
        }

        if (IsExistingPasswordField(name))
        {
            return "password";
        }

        if (IsNewPasswordField(name))
        {
            return "NewPass123";
        }

        return name.ToLowerInvariant() switch
        {
            "email" => "user@example.com",
            "phone" or "mobile" or "tel" => "13800138000",
            "nickname" => "测试昵称",
            "browserfingerprint" => "api-explorer-fingerprint-001",
            "invitecode" => "",
            "agentname" => "",
            "title" or "name" or "realname" => "示例名称",
            "description" or "remark" or "message" => "示例说明",
            "url" or "returnurl" or "homeurl" or "callbackurl" => "https://example.com",
            "ip" or "clientip" or "remoteip" => "127.0.0.1",
            "currency" => "CNY",
            "lang" or "language" => "CN",
            "apicode" or "code" or "gamecode" => "PG",
            "method" => "betTime",
            "rtp" => "96.5",
            "transferno" => $"{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            "avatar" or "base64" => "",
            "token" or "authorization" => "",
            _ => name.EndsWith("Url", StringComparison.OrdinalIgnoreCase)
                ? "https://example.com"
                : name.EndsWith("Name", StringComparison.OrdinalIgnoreCase)
                    ? "示例名称"
                    : "sample"
        };
    }

    private static object ResolveIntegerSample(string? propertyName)
    {
        string name = (propertyName ?? string.Empty).ToLowerInvariant();
        return name switch
        {
            "page" or "currentpage" => 1,
            "pagesize" or "perpage" or "size" or "limit" or "take" => 20,
            "amount" or "money" or "credit" => 100,
            "id" or "userid" or "memberid" or "agentid" => 1,
            "gametype" => 1,
            "ismobile" or "status" or "level" => 0,
            "startat" => DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds(),
            "endat" => DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            _ => name.EndsWith("id", StringComparison.OrdinalIgnoreCase) ? 1 : 0
        };
    }

    private static object ResolveNumberSample(string? propertyName)
    {
        string name = (propertyName ?? string.Empty).ToLowerInvariant();
        return name is "amount" or "money" or "credit" or "balance" ? 100m : 0m;
    }

    private static bool IsUsernameField(string name) =>
        name.Equals("username", StringComparison.OrdinalIgnoreCase)
        || name.Equals("userName", StringComparison.OrdinalIgnoreCase)
        || name.Equals("loginName", StringComparison.OrdinalIgnoreCase)
        || name.Equals("login", StringComparison.OrdinalIgnoreCase);

    private static bool IsMerchantAccountField(string name) =>
        name.Equals("account", StringComparison.OrdinalIgnoreCase);

    private static bool IsMerchantApiKeyField(string name) =>
        name.Equals("api_key", StringComparison.OrdinalIgnoreCase)
        || name.Equals("apiKey", StringComparison.OrdinalIgnoreCase);

    private static bool IsExistingPasswordField(string name) =>
        name.Equals("password", StringComparison.OrdinalIgnoreCase)
        || name.Equals("pwd", StringComparison.OrdinalIgnoreCase)
        || name.Equals("oldPassword", StringComparison.OrdinalIgnoreCase)
        || name.Equals("loginPassword", StringComparison.OrdinalIgnoreCase)
        || name.Equals("currentPassword", StringComparison.OrdinalIgnoreCase);

    private static bool IsNewPasswordField(string name) =>
        name.Equals("newPassword", StringComparison.OrdinalIgnoreCase)
        || name.Equals("confirmPassword", StringComparison.OrdinalIgnoreCase)
        || name.Equals("confirmPwd", StringComparison.OrdinalIgnoreCase);

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

