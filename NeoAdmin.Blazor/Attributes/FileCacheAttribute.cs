using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NeoAdmin.Blazor.Utils;
using Newtonsoft.Json;
using Rougamo;
using Rougamo.Context;

namespace NeoAdmin.Blazor.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class FileCacheAttribute : MoAttribute
{
    private const string CacheKeyState = "__admin_file_cache_key";
    private const string CacheHitState = "__admin_file_cache_hit";
    private const string CleanupStampFileName = ".filecache.cleanup.stamp";
    private const int CleanupIntervalSeconds = 600;

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> CleanupLocks = new();

    /// <summary>
    /// 缓存有效期，单位秒。
    /// </summary>
    public int Seconds { get; }

    /// <summary>
    /// 缓存目录，默认位于应用程序根目录下的 Cache/FileCache。
    /// </summary>
    public string CacheDirectory { get; set; } = "Cache/FileCache";

    /// <summary>
    /// 缓存 key 前缀，默认使用方法所属类型全名。
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// 是否缓存空值。
    /// </summary>
    public bool CacheNullValue { get; set; } = true;

    public FileCacheAttribute(int seconds = 300)
    {
        Seconds = seconds <= 0 ? 300 : seconds;
    }

    public override void OnEntry(MethodContext context)
    {
        if (context.ReturnValueReplaced || !CanCache(context))
        {
            return;
        }

        string cacheKey = BuildCacheKey(context);
        context.Datas[CacheKeyState] = cacheKey;
        TryCleanupExpiredFiles();

        if (TryReadCache(context, cacheKey, out object? value))
        {
            context.Datas[CacheHitState] = true;
            ReplaceReturnValue(context, value);
        }
    }

    public override void OnExit(MethodContext context)
    {
        if (!context.Datas.Contains(CacheKeyState) || context.Datas.Contains(CacheHitState))
        {
            return;
        }

        if (context.Exception != null)
        {
            return;
        }

        if (context.ReturnValue == null && !CacheNullValue)
        {
            return;
        }

        string cacheKey = (string)context.Datas[CacheKeyState];

        if (typeof(Task).IsAssignableFrom(context.ReturnType) && context.ReturnValue is Task task)
        {
            _ = task.ContinueWith(_ =>
            {
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    return;
                }

                if (!TryGetTaskResultType(context.ReturnType, out Type? taskResultType) || taskResultType is null)
                {
                    return;
                }

                object? result = ReadTaskResult(task, taskResultType);
                if (result == null && !CacheNullValue)
                {
                    return;
                }

                WriteCache(cacheKey, result);
            });
            return;
        }

        WriteCache(cacheKey, context.ReturnValue);
    }

    private static bool CanCache(MethodContext context) =>
        context.Method is not null
        && context.ReturnType != typeof(void)
        && context.ReturnType != typeof(Task);

    private string BuildCacheKey(MethodContext context)
    {
        var builder = new StringBuilder();
        builder.Append(string.IsNullOrEmpty(Prefix) ? context.Method.DeclaringType?.FullName : Prefix);
        builder.Append('|');
        builder.Append(context.Method);
        builder.Append('|');
        builder.Append(SerializeArguments(context));
        return builder.ToString();
    }

    private static string SerializeArguments(MethodContext context)
    {
        try
        {
            ParameterInfo[] parameters = context.Method.GetParameters();
            object?[] values = context.Arguments ?? [];
            var payload = parameters.Select((p, i) => new
            {
                Name = p.Name,
                Type = p.ParameterType.AssemblyQualifiedName,
                Value = i < values.Length ? values[i] : null
            }).ToArray();
            return JsonConvert.SerializeObject(payload, Formatting.None, NeoAdminJson.Settings);
        }
        catch
        {
            return string.Join("|", (context.Arguments ?? []).Select(a => a?.ToString() ?? "<null>"));
        }
    }

    private bool TryReadCache(MethodContext context, string cacheKey, out object? value)
    {
        value = null;
        string filePath = GetCacheFilePath(cacheKey);
        if (!File.Exists(filePath))
        {
            return false;
        }

        SemaphoreSlim semaphore = Locks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
        semaphore.Wait();
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            FileCacheEntry? entry = JsonConvert.DeserializeObject<FileCacheEntry>(json, NeoAdminJson.Settings);
            if (entry == null)
            {
                return false;
            }

            if (GetExpireAtUtc(entry) <= DateTimeOffset.UtcNow)
            {
                SafeDelete(filePath);
                return false;
            }

            value = DeserializeValue(GetCacheValueType(context.ReturnType), entry);
            return true;
        }
        catch
        {
            SafeDelete(filePath);
            return false;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private void WriteCache(string cacheKey, object? value)
    {
        string filePath = GetCacheFilePath(cacheKey);
        SemaphoreSlim semaphore = Locks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
        semaphore.Wait();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            DateTimeOffset expireAtUtc = DateTimeOffset.UtcNow.AddSeconds(Seconds);
            FileCacheEntry entry = new()
            {
                ExpireAt = expireAtUtc,
                ExpireAtUnixSeconds = expireAtUtc.ToUnixTimeSeconds(),
                TypeName = value?.GetType().AssemblyQualifiedName,
                Json = value == null ? null : JsonConvert.SerializeObject(value, Formatting.None, NeoAdminJson.Settings),
                HasValue = value != null
            };
            string tempFile = filePath + ".tmp";
            File.WriteAllText(tempFile, JsonConvert.SerializeObject(entry, Formatting.None, NeoAdminJson.Settings), Encoding.UTF8);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(tempFile, filePath);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private void TryCleanupExpiredFiles()
    {
        string root = GetCacheRoot();
        string stampPath = Path.Combine(root, CleanupStampFileName);
        SemaphoreSlim semaphore = CleanupLocks.GetOrAdd(root, _ => new SemaphoreSlim(1, 1));

        if (!ShouldCleanup(stampPath))
        {
            return;
        }

        semaphore.Wait();
        try
        {
            if (!ShouldCleanup(stampPath))
            {
                return;
            }

            CleanupExpiredFiles(root);
            Directory.CreateDirectory(root);
            File.WriteAllText(stampPath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), Encoding.UTF8);
        }
        catch
        {
            // 清理失败不影响主流程，下一次请求会再次尝试。
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static bool ShouldCleanup(string stampPath)
    {
        if (!File.Exists(stampPath))
        {
            return true;
        }

        try
        {
            string text = File.ReadAllText(stampPath, Encoding.UTF8);
            if (!long.TryParse(text, out long lastSeconds))
            {
                return true;
            }

            DateTimeOffset lastCleanup = DateTimeOffset.FromUnixTimeSeconds(lastSeconds);
            return DateTimeOffset.UtcNow - lastCleanup >= TimeSpan.FromSeconds(CleanupIntervalSeconds);
        }
        catch
        {
            return true;
        }
    }

    private static void CleanupExpiredFiles(string root)
    {
        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (string filePath in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            if (IsCleanupStamp(filePath))
            {
                continue;
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                FileCacheEntry? entry = JsonConvert.DeserializeObject<FileCacheEntry>(json, NeoAdminJson.Settings);
                if (entry == null || GetExpireAtUtc(entry) <= DateTimeOffset.UtcNow)
                {
                    SafeDelete(filePath);
                }
            }
            catch
            {
                SafeDelete(filePath);
            }
        }
    }

    private static bool IsCleanupStamp(string filePath) =>
        filePath.EndsWith(CleanupStampFileName, StringComparison.OrdinalIgnoreCase);

    private string GetCacheRoot() => Path.Combine(AppContext.BaseDirectory, CacheDirectory);

    private string GetCacheFilePath(string cacheKey)
    {
        string root = GetCacheRoot();
        Directory.CreateDirectory(root);
        return Path.Combine(root, Sha256Hex(cacheKey) + ".json");
    }

    private static string Sha256Hex(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool TryGetTaskResultType(Type returnType, out Type? resultType)
    {
        resultType = null;
        if (!returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(Task<>))
        {
            return false;
        }

        resultType = returnType.GetGenericArguments().FirstOrDefault();
        return resultType != null;
    }

    private static object? ReadTaskResult(Task task, Type taskResultType)
    {
        PropertyInfo? property = task.GetType().GetProperty("Result");
        if (property == null)
        {
            return null;
        }

        object? result = property.GetValue(task);
        if (result == null)
        {
            return null;
        }

        if (taskResultType.IsInstanceOfType(result))
        {
            return result;
        }

        string json = JsonConvert.SerializeObject(result, Formatting.None, NeoAdminJson.Settings);
        return JsonConvert.DeserializeObject(json, taskResultType, NeoAdminJson.Settings);
    }

    private static Type GetCacheValueType(Type returnType) =>
        TryGetTaskResultType(returnType, out Type? taskResultType) && taskResultType is not null
            ? taskResultType
            : returnType;

    private static object? DeserializeValue(Type returnType, FileCacheEntry entry)
    {
        if (!entry.HasValue)
        {
            return null;
        }

        Type? actualType = string.IsNullOrEmpty(entry.TypeName)
            ? null
            : Type.GetType(entry.TypeName, false);

        if (actualType != null && returnType.IsAssignableFrom(actualType))
        {
            return JsonConvert.DeserializeObject(entry.Json, actualType, NeoAdminJson.Settings);
        }

        return JsonConvert.DeserializeObject(entry.Json, returnType, NeoAdminJson.Settings);
    }

    private void ReplaceReturnValue(MethodContext context, object? value)
    {
        if (TryGetTaskResultType(context.ReturnType, out Type? taskResultType) && taskResultType is not null)
        {
            MethodInfo fromResult = typeof(Task).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Task.FromResult) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                .MakeGenericMethod(taskResultType);

            object? typedValue = value;
            if (typedValue != null && !taskResultType.IsInstanceOfType(typedValue))
            {
                typedValue = JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(value, Formatting.None, NeoAdminJson.Settings),
                    taskResultType,
                    NeoAdminJson.Settings);
            }

            context.ReplaceReturnValue(this, fromResult.Invoke(null, [typedValue]));
            return;
        }

        context.ReplaceReturnValue(this, value);
    }

    private static void SafeDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
        }
    }

    private static DateTimeOffset GetExpireAtUtc(FileCacheEntry entry) =>
        entry.ExpireAtUnixSeconds > 0
            ? DateTimeOffset.FromUnixTimeSeconds(entry.ExpireAtUnixSeconds)
            : entry.ExpireAt;

    private sealed class FileCacheEntry
    {
        public DateTimeOffset ExpireAt { get; set; }

        public long ExpireAtUnixSeconds { get; set; }

        public bool HasValue { get; set; }

        public string? TypeName { get; set; }

        public string? Json { get; set; }
    }
}
