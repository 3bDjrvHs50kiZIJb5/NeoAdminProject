namespace NeoAdmin.Blazor.Utils;

public static class FileSizeFormat
{
    public static string Format(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.##} KB";
        }

        if (bytes < 1024L * 1024 * 1024)
        {
            return $"{bytes / 1024.0 / 1024.0:0.##} MB";
        }

        return $"{bytes / 1024.0 / 1024.0 / 1024.0:0.##} GB";
    }
}
