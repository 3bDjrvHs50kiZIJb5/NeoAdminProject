using System.Security.Cryptography;
using System.Text;

namespace NeoAdmin.Blazor.Utils;

public static class FileMd5
{
    public static string GetHash(Stream stream)
    {
        StringBuilder builder = new();
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(stream);
        foreach (byte b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
