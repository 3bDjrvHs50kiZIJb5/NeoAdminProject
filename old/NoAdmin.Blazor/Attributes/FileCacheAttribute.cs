using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rougamo;
using Rougamo.Context;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class FileCacheAttribute : MoAttribute
{
	private const string CacheKeyState = "__admin_file_cache_key";

	private const string CacheHitState = "__admin_file_cache_hit";

	private const string CacheValueState = "__admin_file_cache_value";

	private const string CleanupStampFileName = ".filecache.cleanup.stamp";

	private const int CleanupIntervalSeconds = 600;

	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cleanupLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

	/// <summary>
	/// 缓存有效期，单位秒。
	/// </summary>
	public int Seconds { get; }

	/// <summary>
	/// 缓存目录，默认位于应用程序根目录下的 Cache/FileCache。
	/// </summary>
	public string CacheDirectory { get; set; } = "Cache/FileCache";

	/// <summary>
	/// 缓存 key 前缀，默认使用方法签名。
	/// </summary>
	public string Prefix { get; set; }

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
		if (context.ReturnValueReplaced)
		{
			return;
		}
		if (!CanCache(context))
		{
			return;
		}
		string cacheKey = BuildCacheKey(context);
		context.Datas[CacheKeyState] = cacheKey;
		TryCleanupExpiredFiles(context);
		if (TryReadCache(context, cacheKey, out object value))
		{
			context.Datas[CacheHitState] = true;
			context.Datas[CacheValueState] = value;
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
			Task continuation = task.ContinueWith(delegate
			{
				if (task.Status != TaskStatus.RanToCompletion)
				{
					return;
				}
				if (!TryGetTaskResultType(context.ReturnType, out Type taskResultType))
				{
					return;
				}
				object result = ReadTaskResult(task, taskResultType);
				if (result == null && !CacheNullValue)
				{
					return;
				}
				WriteCache(context, cacheKey, result);
			});
			return;
		}
		WriteCache(context, cacheKey, context.ReturnValue);
	}

	private bool CanCache(MethodContext context)
	{
		if (context == null || context.Method == null)
		{
			return false;
		}
		if (context.ReturnType == typeof(void))
		{
			return false;
		}
		if (context.ReturnType == typeof(Task))
		{
			return false;
		}
		return true;
	}

	private string BuildCacheKey(MethodContext context)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Prefix.IsNull() ? context.Method.DeclaringType?.FullName : Prefix);
		stringBuilder.Append("|");
		stringBuilder.Append(context.Method.ToString());
		stringBuilder.Append("|");
		stringBuilder.Append(SerializeArguments(context));
		return stringBuilder.ToString();
	}

	private string SerializeArguments(MethodContext context)
	{
		try
		{
			ParameterInfo[] parameters = context.Method.GetParameters();
			object[] values = context.Arguments ?? Array.Empty<object>();
			var payload = parameters.Select((ParameterInfo p, int i) => new
			{
				Name = p.Name,
				Type = p.ParameterType.AssemblyQualifiedName,
				Value = i < values.Length ? values[i] : null
			}).ToArray();
			return JsonConvert.SerializeObject(payload, Formatting.None, NovaAdminExtensions.JsonSerializerSettings);
		}
		catch
		{
			return string.Join("|", (context.Arguments ?? Array.Empty<object>()).Select((object a) => a == null ? "<null>" : a.ToString()));
		}
	}

	private bool TryReadCache(MethodContext context, string cacheKey, out object value)
	{
		value = null;
		string filePath = GetCacheFilePath(cacheKey);
		if (!File.Exists(filePath))
		{
			return false;
		}
		SemaphoreSlim semaphore = _locks.GetOrAdd(filePath, delegate
		{
			return new SemaphoreSlim(1, 1);
		});
		semaphore.Wait();
		try
		{
			if (!File.Exists(filePath))
			{
				return false;
			}
			string json = File.ReadAllText(filePath, Encoding.UTF8);
			FileCacheEntry entry = JsonConvert.DeserializeObject<FileCacheEntry>(json, NovaAdminExtensions.JsonSerializerSettings);
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

	private void WriteCache(MethodContext context, string cacheKey, object value)
	{
		string filePath = GetCacheFilePath(cacheKey);
		SemaphoreSlim semaphore = _locks.GetOrAdd(filePath, delegate
		{
			return new SemaphoreSlim(1, 1);
		});
		semaphore.Wait();
		try
		{
			System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
			DateTimeOffset expireAtUtc = DateTimeOffset.UtcNow.AddSeconds(Seconds);
			FileCacheEntry entry = new FileCacheEntry
			{
				ExpireAt = expireAtUtc,
				ExpireAtUnixSeconds = expireAtUtc.ToUnixTimeSeconds(),
				TypeName = value?.GetType().AssemblyQualifiedName,
				Json = value == null ? null : JsonConvert.SerializeObject(value, Formatting.None, NovaAdminExtensions.JsonSerializerSettings),
				HasValue = value != null
			};
			string tempFile = filePath + ".tmp";
			File.WriteAllText(tempFile, JsonConvert.SerializeObject(entry, Formatting.None, NovaAdminExtensions.JsonSerializerSettings), Encoding.UTF8);
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

	private void TryCleanupExpiredFiles(MethodContext context)
	{
		string root = GetCacheRoot();
		string stampPath = System.IO.Path.Combine(root, CleanupStampFileName).ToPath();
		SemaphoreSlim semaphore = _cleanupLocks.GetOrAdd(root, _ => new SemaphoreSlim(1, 1));

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

			var lastCleanup = DateTimeOffset.FromUnixTimeSeconds(lastSeconds);
			return DateTimeOffset.UtcNow - lastCleanup >= TimeSpan.FromSeconds(CleanupIntervalSeconds);
		}
		catch
		{
			return true;
		}
	}

	private void CleanupExpiredFiles(string root)
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
				FileCacheEntry entry = JsonConvert.DeserializeObject<FileCacheEntry>(json, NovaAdminExtensions.JsonSerializerSettings);
				if (entry == null)
				{
					SafeDelete(filePath);
					continue;
				}

				if (GetExpireAtUtc(entry) <= DateTimeOffset.UtcNow)
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

	private string GetCacheRoot()
	{
		return System.IO.Path.Combine(AppContext.BaseDirectory, CacheDirectory).ToPath();
	}

	private string GetCacheFilePath(string cacheKey)
	{
		string root = System.IO.Path.Combine(AppContext.BaseDirectory, CacheDirectory).ToPath();
		System.IO.Directory.CreateDirectory(root);
		string fileName = Sha256Hex(cacheKey) + ".json";
		return System.IO.Path.Combine(root, fileName).ToPath();
	}

	private static string Sha256Hex(string text)
	{
		using SHA256 sha256 = SHA256.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
		return Convert.ToHexString(sha256.ComputeHash(bytes)).ToLowerInvariant();
	}

	private static bool TryGetTaskResultType(Type returnType, out Type resultType)
	{
		resultType = null;
		if (!returnType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = returnType.GetGenericTypeDefinition();
		if (genericTypeDefinition != typeof(Task<>))
		{
			return false;
		}
		resultType = returnType.GetGenericArguments().FirstOrDefault();
		return resultType != null;
	}

	private static object ReadTaskResult(Task task, Type taskResultType)
	{
		PropertyInfo property = task.GetType().GetProperty("Result");
		if (property == null)
		{
			return null;
		}
		object result = property.GetValue(task);
		if (result == null)
		{
			return null;
		}
		if (taskResultType.IsInstanceOfType(result))
		{
			return result;
		}
		string json = JsonConvert.SerializeObject(result, Formatting.None, NovaAdminExtensions.JsonSerializerSettings);
		return JsonConvert.DeserializeObject(json, taskResultType, NovaAdminExtensions.JsonSerializerSettings);
	}

	private static Type GetCacheValueType(Type returnType)
	{
		if (TryGetTaskResultType(returnType, out Type taskResultType))
		{
			return taskResultType;
		}
		return returnType;
	}

	private static object DeserializeValue(Type returnType, FileCacheEntry entry)
	{
		if (!entry.HasValue)
		{
			return null;
		}
		Type actualType = null;
		if (entry.TypeName.NotNull())
		{
			actualType = Type.GetType(entry.TypeName, false);
		}
		if (actualType != null && returnType.IsAssignableFrom(actualType))
		{
			return JsonConvert.DeserializeObject(entry.Json, actualType, NovaAdminExtensions.JsonSerializerSettings);
		}
		return JsonConvert.DeserializeObject(entry.Json, returnType, NovaAdminExtensions.JsonSerializerSettings);
	}

	private void ReplaceReturnValue(MethodContext context, object value)
	{
		if (TryGetTaskResultType(context.ReturnType, out Type taskResultType))
		{
			MethodInfo fromResult = typeof(Task).GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First((MethodInfo m) => m.Name == "FromResult" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
				.MakeGenericMethod(taskResultType);
			object typedValue = value;
			if (typedValue != null && !taskResultType.IsInstanceOfType(typedValue))
			{
				typedValue = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.None, NovaAdminExtensions.JsonSerializerSettings), taskResultType, NovaAdminExtensions.JsonSerializerSettings);
			}
			context.ReplaceReturnValue((IMo)(object)this, fromResult.Invoke(null, new object[1] { typedValue }));
			return;
		}
		context.ReplaceReturnValue((IMo)(object)this, value);
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

	private static DateTimeOffset GetExpireAtUtc(FileCacheEntry entry)
	{
		if (entry.ExpireAtUnixSeconds > 0)
		{
			return DateTimeOffset.FromUnixTimeSeconds(entry.ExpireAtUnixSeconds);
		}
		return entry.ExpireAt;
	}

	private sealed class FileCacheEntry
	{
		public DateTimeOffset ExpireAt { get; set; }

		public long ExpireAtUnixSeconds { get; set; }

		public bool HasValue { get; set; }

		public string TypeName { get; set; }

		public string Json { get; set; }
	}

}