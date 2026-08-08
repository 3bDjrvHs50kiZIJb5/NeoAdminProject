using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// IP 白名单申请页图形验证码（内存缓存，短期有效）。
/// </summary>
public sealed class IpWhitelistCaptchaService
{
    private const int CodeLength = 4;
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);
    private static readonly char[] Charset = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();

    private readonly IMemoryCache _cache;

    public IpWhitelistCaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IpWhitelistCaptchaIssue Issue()
    {
        string code = GenerateCode();
        string id = Guid.NewGuid().ToString("N");
        _cache.Set(BuildCacheKey(id), code, Expiration);
        return new IpWhitelistCaptchaIssue(id, BuildSvg(code));
    }

    public bool Validate(string? captchaId, string? captchaCode)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(captchaCode))
        {
            return false;
        }

        string key = BuildCacheKey(captchaId.Trim());
        if (!_cache.TryGetValue(key, out string? expected) || string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }

        _cache.Remove(key);
        return string.Equals(expected, captchaCode.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildCacheKey(string id) => $"ip-whitelist:captcha:{id}";

    private static string GenerateCode()
    {
        Span<char> buffer = stackalloc char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            buffer[i] = Charset[RandomNumberGenerator.GetInt32(Charset.Length)];
        }

        return new string(buffer);
    }

    private static string BuildSvg(string code)
    {
        StringBuilder svg = new();
        svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"32\" viewBox=\"0 0 100 32\">");
        svg.Append("<rect width=\"100\" height=\"32\" fill=\"#f3f4f6\" rx=\"6\"/>");

        for (int i = 0; i < 5; i++)
        {
            int x1 = RandomNumberGenerator.GetInt32(0, 100);
            int y1 = RandomNumberGenerator.GetInt32(0, 32);
            int x2 = RandomNumberGenerator.GetInt32(0, 100);
            int y2 = RandomNumberGenerator.GetInt32(0, 32);
            svg.Append($"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"#cbd5e1\" stroke-width=\"1\"/>");
        }

        for (int i = 0; i < code.Length; i++)
        {
            int x = 12 + i * 20;
            int y = 21 + RandomNumberGenerator.GetInt32(-3, 4);
            int rotate = RandomNumberGenerator.GetInt32(-18, 19);
            char ch = code[i];
            svg.Append(
                $"<text x=\"{x}\" y=\"{y}\" fill=\"#1f2937\" font-size=\"18\" font-family=\"Arial,sans-serif\" font-weight=\"700\" transform=\"rotate({rotate} {x} {y})\">{ch}</text>");
        }

        svg.Append("</svg>");
        return svg.ToString();
    }
}

public sealed record IpWhitelistCaptchaIssue(string Id, string Svg);
