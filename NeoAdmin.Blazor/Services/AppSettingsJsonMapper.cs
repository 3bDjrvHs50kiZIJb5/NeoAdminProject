using System.Text.Json;
using System.Text.Json.Nodes;
using NeoAdmin.Blazor.Models;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 在 appsettings JSON 与固定字段表单之间读写；保留未建模的未知键。
/// </summary>
public static class AppSettingsJsonMapper
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static AppSettingsFormModel ToForm(JsonNode? root)
    {
        JsonObject obj = AsObject(root) ?? new JsonObject();
        return new AppSettingsFormModel
        {
            DataType = GetString(obj, "NeoAdmin", "DataType"),
            ConnectionString = GetString(obj, "NeoAdmin", "ConnectionString"),
            AutoSyncStructure = GetBool(obj, "NeoAdmin", "AutoSyncStructure"),
            EnableSeedData = GetBool(obj, "NeoAdmin", "EnableSeedData"),
            HomePath = GetString(obj, "NeoAdmin", "HomePath"),
            MonitorCommand = GetBool(obj, "NeoAdmin", "MonitorCommand"),
            SeedAdminUserName = GetString(obj, "NeoAdmin", "SeedAdminUserName"),
            SeedAdminPassword = GetString(obj, "NeoAdmin", "SeedAdminPassword"),
            WorkId = GetInt(obj, "NeoAdmin", "WorkId"),
            EnableIpWhitelist = GetBool(obj, "NeoAdmin", "EnableIpWhitelist"),
            IsSwagger = GetBool(obj, "NeoAdmin", "IsSwagger"),
            SwaggerHides = GetStringArray(obj, "NeoAdmin", "SwaggerHides"),
            SchedulerAutoLoad = GetBool(obj, "NeoAdmin", "SchedulerAutoLoad"),
            LogDirectory = GetString(obj, "NeoAdmin", "LogDirectory"),
            LogFilePrefix = GetString(obj, "NeoAdmin", "LogFilePrefix"),

            FileUploadDirectory = GetString(obj, "NeoAdmin", "FileUpload", "Directory"),
            FileUploadDateTimeDirectory = GetString(obj, "NeoAdmin", "FileUpload", "DateTimeDirectory"),
            FileUploadMd5 = GetBool(obj, "NeoAdmin", "FileUpload", "Md5"),
            FileUploadMaxSize = GetLong(obj, "NeoAdmin", "FileUpload", "MaxSize"),
            FileUploadIncludeExtension = GetStringArray(obj, "NeoAdmin", "FileUpload", "IncludeExtension"),
            FileUploadExcludeExtension = GetStringArray(obj, "NeoAdmin", "FileUpload", "ExcludeExtension"),

            OssEndpoint = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "Endpoint"),
            OssAccessKeyId = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "AccessKeyId"),
            OssAccessKeySecret = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "AccessKeySecret"),
            OssBucketName = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "BucketName"),
            OssCustomDomain = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "CustomDomain"),
            OssPrefix = GetString(obj, "NeoAdmin", "FileUpload", "Oss", "Prefix"),

            DashScopeApiKey = GetString(obj, "DashScope", "ApiKey"),
            DashScopeRealtimeModel = GetString(obj, "DashScope", "RealtimeModel"),
            DashScopeSampleRate = GetInt(obj, "DashScope", "SampleRate"),

            LoggingDefault = GetString(obj, "Logging", "LogLevel", "Default"),
            LoggingAspNetCore = GetString(obj, "Logging", "LogLevel", "Microsoft.AspNetCore"),
        };
    }

    /// <summary>
    /// 将表单合并进现有 JSON：非 null 写入；null 移除对应键；保留未知键。
    /// </summary>
    public static string MergeAndSerialize(JsonNode? existingRoot, AppSettingsFormModel form)
    {
        JsonObject root = AsObject(existingRoot)?.DeepClone().AsObject() ?? new JsonObject();

        SetOrRemoveString(root, form.DataType, "NeoAdmin", "DataType");
        SetOrRemoveString(root, form.ConnectionString, "NeoAdmin", "ConnectionString");
        SetOrRemoveBool(root, form.AutoSyncStructure, "NeoAdmin", "AutoSyncStructure");
        SetOrRemoveBool(root, form.EnableSeedData, "NeoAdmin", "EnableSeedData");
        SetOrRemoveString(root, form.HomePath, "NeoAdmin", "HomePath");
        SetOrRemoveBool(root, form.MonitorCommand, "NeoAdmin", "MonitorCommand");
        SetOrRemoveString(root, form.SeedAdminUserName, "NeoAdmin", "SeedAdminUserName");
        SetOrRemoveString(root, form.SeedAdminPassword, "NeoAdmin", "SeedAdminPassword");
        SetOrRemoveInt(root, form.WorkId, "NeoAdmin", "WorkId");
        SetOrRemoveBool(root, form.EnableIpWhitelist, "NeoAdmin", "EnableIpWhitelist");
        SetOrRemoveBool(root, form.IsSwagger, "NeoAdmin", "IsSwagger");
        SetOrRemoveStringArray(root, form.SwaggerHides, "NeoAdmin", "SwaggerHides");
        SetOrRemoveBool(root, form.SchedulerAutoLoad, "NeoAdmin", "SchedulerAutoLoad");
        SetOrRemoveString(root, form.LogDirectory, "NeoAdmin", "LogDirectory");
        SetOrRemoveString(root, form.LogFilePrefix, "NeoAdmin", "LogFilePrefix");

        SetOrRemoveString(root, form.FileUploadDirectory, "NeoAdmin", "FileUpload", "Directory");
        SetOrRemoveString(root, form.FileUploadDateTimeDirectory, "NeoAdmin", "FileUpload", "DateTimeDirectory");
        SetOrRemoveBool(root, form.FileUploadMd5, "NeoAdmin", "FileUpload", "Md5");
        SetOrRemoveLong(root, form.FileUploadMaxSize, "NeoAdmin", "FileUpload", "MaxSize");
        SetOrRemoveStringArray(root, form.FileUploadIncludeExtension, "NeoAdmin", "FileUpload", "IncludeExtension");
        SetOrRemoveStringArray(root, form.FileUploadExcludeExtension, "NeoAdmin", "FileUpload", "ExcludeExtension");

        SetOrRemoveString(root, form.OssEndpoint, "NeoAdmin", "FileUpload", "Oss", "Endpoint");
        SetOrRemoveString(root, form.OssAccessKeyId, "NeoAdmin", "FileUpload", "Oss", "AccessKeyId");
        SetOrRemoveString(root, form.OssAccessKeySecret, "NeoAdmin", "FileUpload", "Oss", "AccessKeySecret");
        SetOrRemoveString(root, form.OssBucketName, "NeoAdmin", "FileUpload", "Oss", "BucketName");
        SetOrRemoveString(root, form.OssCustomDomain, "NeoAdmin", "FileUpload", "Oss", "CustomDomain");
        SetOrRemoveString(root, form.OssPrefix, "NeoAdmin", "FileUpload", "Oss", "Prefix");

        SetOrRemoveString(root, form.DashScopeApiKey, "DashScope", "ApiKey");
        SetOrRemoveString(root, form.DashScopeRealtimeModel, "DashScope", "RealtimeModel");
        SetOrRemoveInt(root, form.DashScopeSampleRate, "DashScope", "SampleRate");

        SetOrRemoveString(root, form.LoggingDefault, "Logging", "LogLevel", "Default");
        SetOrRemoveString(root, form.LoggingAspNetCore, "Logging", "LogLevel", "Microsoft.AspNetCore");

        PruneEmptyObjects(root);
        return root.ToJsonString(WriteOptions) + Environment.NewLine;
    }

    public static JsonNode? ParseOrEmpty(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new JsonObject();
        }

        return JsonNode.Parse(json) ?? new JsonObject();
    }

    private static JsonObject? AsObject(JsonNode? node) => node as JsonObject;

    private static string? GetString(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue(out string? text))
        {
            return text;
        }

        return node.ToJsonString().Trim('"');
    }

    private static bool? GetBool(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue(out bool b))
        {
            return b;
        }

        if (bool.TryParse(node.ToString(), out bool parsed))
        {
            return parsed;
        }

        return null;
    }

    private static int? GetInt(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue(out int i))
        {
            return i;
        }

        if (int.TryParse(node.ToString(), out int parsed))
        {
            return parsed;
        }

        return null;
    }

    private static long? GetLong(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue(out long l))
        {
            return l;
        }

        if (long.TryParse(node.ToString(), out long parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? GetStringArray(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is null)
        {
            return null;
        }

        if (node is not JsonArray array)
        {
            return node.ToString();
        }

        List<string> items = [];
        foreach (JsonNode? item in array)
        {
            if (item is null)
            {
                continue;
            }

            if (item is JsonValue value && value.TryGetValue(out string? text))
            {
                items.Add(text);
            }
            else
            {
                items.Add(item.ToJsonString().Trim('"'));
            }
        }

        return string.Join(", ", items);
    }

    private static void SetOrRemoveString(JsonObject root, string? value, params string[] path)
    {
        if (value is null)
        {
            RemovePath(root, path);
            return;
        }

        SetNode(root, JsonValue.Create(value), path);
    }

    private static void SetOrRemoveBool(JsonObject root, bool? value, params string[] path)
    {
        if (value is null)
        {
            RemovePath(root, path);
            return;
        }

        SetNode(root, JsonValue.Create(value.Value), path);
    }

    private static void SetOrRemoveInt(JsonObject root, int? value, params string[] path)
    {
        if (value is null)
        {
            RemovePath(root, path);
            return;
        }

        SetNode(root, JsonValue.Create(value.Value), path);
    }

    private static void SetOrRemoveLong(JsonObject root, long? value, params string[] path)
    {
        if (value is null)
        {
            RemovePath(root, path);
            return;
        }

        SetNode(root, JsonValue.Create(value.Value), path);
    }

    private static void SetOrRemoveStringArray(JsonObject root, string? csv, params string[] path)
    {
        if (csv is null)
        {
            RemovePath(root, path);
            return;
        }

        string[] parts = csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var array = new JsonArray();
        foreach (string part in parts)
        {
            array.Add(part);
        }

        SetNode(root, array, path);
    }

    private static JsonNode? GetNode(JsonObject root, params string[] path)
    {
        JsonNode? current = root;
        foreach (string segment in path)
        {
            if (current is not JsonObject obj || !obj.TryGetPropertyValue(segment, out JsonNode? next))
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private static void SetNode(JsonObject root, JsonNode value, params string[] path)
    {
        JsonObject current = root;
        for (int i = 0; i < path.Length - 1; i++)
        {
            string segment = path[i];
            if (current[segment] is not JsonObject child)
            {
                child = new JsonObject();
                current[segment] = child;
            }

            current = child;
        }

        current[path[^1]] = value;
    }

    private static void RemovePath(JsonObject root, params string[] path)
    {
        if (path.Length == 0)
        {
            return;
        }

        List<JsonObject> stack = [root];
        JsonObject current = root;
        for (int i = 0; i < path.Length - 1; i++)
        {
            if (current[path[i]] is not JsonObject child)
            {
                return;
            }

            stack.Add(child);
            current = child;
        }

        current.Remove(path[^1]);
    }

    /// <summary>移除我们可能留下的空对象（不影响含未知键的对象）。</summary>
    private static void PruneEmptyObjects(JsonObject root)
    {
        PruneKnown(root, "NeoAdmin", "FileUpload", "Oss");
        PruneKnown(root, "NeoAdmin", "FileUpload");
        PruneKnown(root, "NeoAdmin");
        PruneKnown(root, "DashScope");
        PruneKnown(root, "Logging", "LogLevel");
        PruneKnown(root, "Logging");
    }

    private static void PruneKnown(JsonObject root, params string[] path)
    {
        JsonNode? node = GetNode(root, path);
        if (node is JsonObject obj && obj.Count == 0)
        {
            RemovePath(root, path);
        }
    }
}
