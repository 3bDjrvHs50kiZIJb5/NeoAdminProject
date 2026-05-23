using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NoAdmin.Blazor.Utils;

/// <summary>
/// IP地址帮助类
/// </summary>
public static class IpHelper
{
	public static string GetClientIpAddress(HttpContext httpContext, ILogger? logger = null)
	{
		try
		{
			if (httpContext == null)
			{
				logger?.LogWarning("HttpContext为null，无法获取白名单校验IP");
				return "unknown";
			}


			var cfConnectingIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(cfConnectingIp))
			{
				return NormalizeIp(cfConnectingIp);
			}

			var xRealIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(xRealIp))
			{
				return NormalizeIp(xRealIp);
			}


			var xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(xForwardedFor))
			{
				var firstForwardedIp = xForwardedFor
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(firstForwardedIp))
				{
					return NormalizeIp(firstForwardedIp);
				}
			}

			var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			return NormalizeIp(remoteIp);
		}
		catch (Exception ex)
		{
			logger?.LogInformation(ex, "获取白名单校验IP时发生异常");
			return "unknown";
		}
	}

	public static string NormalizeIp(string? ip)
	{
		if (string.IsNullOrWhiteSpace(ip))
		{
			return "unknown";
		}

		var trimmed = ip.Trim();
		if (!IPAddress.TryParse(trimmed, out var parsedIp))
		{
			return trimmed;
		}

		if (parsedIp.IsIPv4MappedToIPv6)
		{
			parsedIp = parsedIp.MapToIPv4();
		}

		return parsedIp.ToString();
	}

	public static string ToIpv4Display(string? ip)
	{
		if (string.IsNullOrWhiteSpace(ip))
		{
			return "unknown";
		}

		var normalizedIp = NormalizeIp(ip);
		if (normalizedIp == "::1")
		{
			return "127.0.0.1";
		}

		return normalizedIp;
	}
}
