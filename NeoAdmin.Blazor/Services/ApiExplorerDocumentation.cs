using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// API 调试「简介」：读取 XML 注释，并生成精简调用示例。
/// </summary>
public static class ApiExplorerDocumentation
{
    private static readonly JsonSerializerOptions CompactJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly object Sync = new();
    private static Dictionary<string, (string? Summary, string? Remarks)>? docs;

    public static (string? Summary, string? Remarks, bool AllowAnonymous) GetActionDocs(
        ControllerActionDescriptor? actionDescriptor)
    {
        if (actionDescriptor is null)
        {
            return (null, null, false);
        }

        MethodInfo method = actionDescriptor.MethodInfo;
        bool allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() is not null
            || actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
        (string? summary, string? remarks) = GetComments(method);
        return (summary, remarks, allowAnonymous);
    }

    public static string BuildExample(ApiExplorerEndpoint endpoint)
    {
        StringBuilder text = new();
        text.Append(endpoint.HttpMethod);
        text.Append(' ');
        text.Append('/');
        text.Append(endpoint.RelativePath.TrimStart('/'));

        string request = CompactSampleJson(endpoint.RequestBodySampleJson);
        if (!IsEmptyJson(request))
        {
            text.AppendLine();
            text.AppendLine();
            text.Append(request);
        }

        string response = CompactSampleJson(endpoint.ResponseBodySampleJson);
        if (!string.IsNullOrWhiteSpace(response))
        {
            text.AppendLine();
            text.AppendLine();
            text.AppendLine("返回：");
            text.Append(response);
        }

        return text.ToString().TrimEnd();
    }

    private static (string? Summary, string? Remarks) GetComments(MethodInfo method)
    {
        EnsureLoaded();
        if (docs is null)
        {
            return (null, null);
        }

        return docs.TryGetValue(GetMemberName(method), out (string? Summary, string? Remarks) value)
            ? value
            : (null, null);
    }

    private static void EnsureLoaded()
    {
        if (docs is not null)
        {
            return;
        }

        lock (Sync)
        {
            if (docs is not null)
            {
                return;
            }

            Dictionary<string, (string? Summary, string? Remarks)> map = new(StringComparer.Ordinal);
            string baseDir = AppContext.BaseDirectory;
            if (!Directory.Exists(baseDir))
            {
                docs = map;
                return;
            }

            foreach (string path in Directory.EnumerateFiles(baseDir, "*.xml"))
            {
                try
                {
                    LoadFile(path, map);
                }
                catch (Exception)
                {
                    // 个别 XML 损坏不影响简介页
                }
            }

            docs = map;
        }
    }

    private static void LoadFile(string path, Dictionary<string, (string? Summary, string? Remarks)> map)
    {
        XDocument document = XDocument.Load(path);
        foreach (XElement member in document.Descendants("member"))
        {
            string? name = (string?)member.Attribute("name");
            if (string.IsNullOrWhiteSpace(name) || !name.StartsWith("M:", StringComparison.Ordinal))
            {
                continue;
            }

            string summary = ReadDocNode(member.Element("summary"));
            string remarks = ReadDocNode(member.Element("remarks"));
            map[name] = (
                string.IsNullOrWhiteSpace(summary) ? null : summary,
                string.IsNullOrWhiteSpace(remarks) ? null : remarks);
        }
    }

    private static string ReadDocNode(XElement? element)
    {
        if (element is null)
        {
            return string.Empty;
        }

        StringBuilder text = new();
        foreach (XNode node in element.Nodes())
        {
            switch (node)
            {
                case XText textNode:
                    text.Append(textNode.Value);
                    break;
                case XElement child when child.Name.LocalName == "see":
                    string? cref = (string?)child.Attribute("cref");
                    text.Append(FormatCref(cref) ?? child.Value);
                    break;
                case XElement child:
                    text.Append(child.Value);
                    break;
            }
        }

        string[] lines = text.ToString()
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
        return string.Join(" ", lines);
    }

    private static string? FormatCref(string? cref)
    {
        if (string.IsNullOrWhiteSpace(cref))
        {
            return null;
        }

        string value = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
        int lastDot = value.LastIndexOf('.');
        return lastDot >= 0 ? value[(lastDot + 1)..] : value;
    }

    private static string GetMemberName(MethodInfo method)
    {
        StringBuilder name = new("M:");
        string typeName = (method.DeclaringType?.FullName ?? method.DeclaringType?.Name ?? "Unknown")
            .Replace('+', '.');
        name.Append(typeName);
        name.Append('.');
        name.Append(method.Name);

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return name.ToString();
        }

        name.Append('(');
        name.Append(string.Join(',', parameters.Select(p => GetXmlTypeName(p.ParameterType))));
        name.Append(')');
        return name.ToString();
    }

    private static string GetXmlTypeName(Type type)
    {
        if (type.IsByRef)
        {
            type = type.GetElementType()!;
        }

        if (type.IsGenericType)
        {
            Type definition = type.GetGenericTypeDefinition();
            string root = definition.FullName ?? definition.Name;
            int tick = root.IndexOf('`');
            if (tick >= 0)
            {
                root = root[..tick];
            }

            string args = string.Join(',', type.GetGenericArguments().Select(GetXmlTypeName));
            return $"{root.Replace('+', '.')}{{{args}}}";
        }

        if (type.IsArray)
        {
            return GetXmlTypeName(type.GetElementType()!) + "[]";
        }

        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static string CompactSampleJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            JsonNode? node = JsonNode.Parse(json);
            if (node is null)
            {
                return string.Empty;
            }

            CompactNode(node);
            return node.ToJsonString(CompactJsonOptions);
        }
        catch (JsonException)
        {
            return json.Trim();
        }
    }

    private static void CompactNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            foreach (KeyValuePair<string, JsonNode?> pair in obj.ToList())
            {
                if (pair.Value is JsonValue value && value.TryGetValue(out string? text) && text is not null)
                {
                    string compact = CompactString(text);
                    if (!string.Equals(compact, text, StringComparison.Ordinal))
                    {
                        obj[pair.Key] = compact;
                    }
                }
                else if (pair.Value is not null)
                {
                    CompactNode(pair.Value);
                }
            }

            return;
        }

        if (node is JsonArray array)
        {
            foreach (JsonNode? item in array)
            {
                if (item is not null)
                {
                    CompactNode(item);
                }
            }
        }
    }

    private static string CompactString(string text)
    {
        if (text.Contains("<svg", StringComparison.OrdinalIgnoreCase))
        {
            return "<svg>…</svg>";
        }

        return text.Length > 48 ? text[..40] + "…" : text;
    }

    private static bool IsEmptyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        string compact = json.Replace(" ", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\t", string.Empty);
        return compact is "{}" or "[]" or "null";
    }
}
