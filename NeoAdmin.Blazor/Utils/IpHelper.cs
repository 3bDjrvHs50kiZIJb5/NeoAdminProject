using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NeoAdmin.Blazor.Utils;

public static class IpHelper
{
    public static string GetClientIpAddress(HttpContext httpContext, ILogger? logger = null)
    {
        try
        {
            if (httpContext is null)
            {
                logger?.LogWarning("HttpContext 为 null，无法获取客户端 IP");
                return "unknown";
            }

            string? cfConnectingIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(cfConnectingIp))
            {
                return NormalizeIp(cfConnectingIp);
            }

            string? xRealIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xRealIp))
            {
                return NormalizeIp(xRealIp);
            }

            string? xForwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xForwardedFor))
            {
                string? firstForwardedIp = xForwardedFor
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstForwardedIp))
                {
                    return NormalizeIp(firstForwardedIp);
                }
            }

            string remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return NormalizeIp(remoteIp);
        }
        catch (Exception ex)
        {
            logger?.LogInformation(ex, "获取客户端 IP 时发生异常");
            return "unknown";
        }
    }

    public static string NormalizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return "unknown";
        }

        string trimmed = ip.Trim();
        if (!IPAddress.TryParse(trimmed, out IPAddress? parsedIp))
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

        string normalizedIp = NormalizeIp(ip);
        return normalizedIp == "::1" ? "127.0.0.1" : normalizedIp;
    }
}
