using System;
using Microsoft.Extensions.Caching.Memory;
using OnceMi.AspNetCore.OSS;

internal sealed class AdminMemoryCacheProvider : ICacheProvider
{
	private readonly IMemoryCache _cache;

	public AdminMemoryCacheProvider(IMemoryCache cache)
	{
		_cache = cache;
	}

	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	public T Get<T>(string key) where T : class
	{
		return _cache.Get<T>(key);
	}

	public void Set<T>(string key, T value, TimeSpan ts) where T : class
	{
		_cache.Set(key, value, ts);
	}
}
