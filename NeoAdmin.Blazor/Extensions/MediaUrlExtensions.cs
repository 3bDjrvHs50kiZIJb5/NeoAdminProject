using Microsoft.AspNetCore.Components;

namespace NeoAdmin.Blazor.Extensions;

public static class MediaUrlExtensions
{
    public static string? ToMediaUrl(this NavigationManager navigation, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        return navigation.ToAbsoluteUri(url).ToString();
    }
}
