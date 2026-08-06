using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// ApiExplorer 试调前展开 <c>{{unix}}</c>、<c>{{random:8}}</c> 等占位符。
/// 配置文件与编辑区保留模板原文，仅实际请求时展开。
/// </summary>
public static partial class ApiExplorerVariableExpander
{
    private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    [GeneratedRegex(@"\{\{\s*([^}]+?)\s*\}\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderRegex();

    /// <summary>展开字符串中的全部占位符。</summary>
    public static string Expand(string? input) =>
        string.IsNullOrEmpty(input)
            ? input ?? string.Empty
            : PlaceholderRegex().Replace(input, match => ResolvePlaceholder(match.Groups[1].Value.Trim()));

    /// <summary>
    /// 展开 JSON 请求体：字符串值中的占位符会被替换；
    /// 若某字符串值恰好是单个占位符且展开结果为 number/bool，则输出对应 JSON 类型。
    /// </summary>
    public static string ExpandJsonBody(string? jsonBody)
    {
        if (string.IsNullOrWhiteSpace(jsonBody))
        {
            return jsonBody ?? string.Empty;
        }

        try
        {
            JsonNode? node = JsonNode.Parse(jsonBody);
            if (node is null)
            {
                return Expand(jsonBody);
            }

            JsonNode expanded = ExpandJsonNode(node);
            return expanded.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            return Expand(jsonBody);
        }
    }

    /// <summary>展开字典中每个值（Route / Query）。</summary>
    public static Dictionary<string, string> ExpandDictionary(IReadOnlyDictionary<string, string> values)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> pair in values)
        {
            result[pair.Key] = Expand(pair.Value);
        }

        return result;
    }

    private static JsonNode ExpandJsonNode(JsonNode node) =>
        node switch
        {
            JsonObject obj => ExpandJsonObject(obj),
            JsonArray array => ExpandJsonArray(array),
            JsonValue value => ExpandJsonValue(value),
            _ => node
        };

    private static JsonObject ExpandJsonObject(JsonObject obj)
    {
        var result = new JsonObject();
        foreach (KeyValuePair<string, JsonNode?> pair in obj)
        {
            if (pair.Value is null)
            {
                result[pair.Key] = null;
                continue;
            }

            result[pair.Key] = ExpandJsonNode(pair.Value);
        }

        return result;
    }

    private static JsonArray ExpandJsonArray(JsonArray array)
    {
        var result = new JsonArray();
        foreach (JsonNode? item in array)
        {
            result.Add(item is null ? null : ExpandJsonNode(item));
        }

        return result;
    }

    private static JsonNode ExpandJsonValue(JsonValue value)
    {
        if (value.GetValueKind() != JsonValueKind.String)
        {
            return value.DeepClone();
        }

        string? text = value.GetValue<string>();
        if (string.IsNullOrEmpty(text))
        {
            return value.DeepClone();
        }

        if (IsSinglePlaceholder(text, out string inner))
        {
            string expanded = ResolvePlaceholder(inner);
            if (TryParseJsonScalar(expanded, out JsonNode? scalar) && scalar is not null)
            {
                return scalar;
            }

            return JsonValue.Create(expanded);
        }

        return JsonValue.Create(Expand(text));
    }

    private static bool IsSinglePlaceholder(string text, out string inner)
    {
        Match match = PlaceholderRegex().Match(text);
        if (match.Success && match.Index == 0 && match.Length == text.Length)
        {
            inner = match.Groups[1].Value.Trim();
            return true;
        }

        inner = string.Empty;
        return false;
    }

    private static bool TryParseJsonScalar(string value, out JsonNode? node)
    {
        node = null;
        if (bool.TryParse(value, out bool flag))
        {
            node = JsonValue.Create(flag);
            return true;
        }

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
        {
            node = JsonValue.Create(integer);
            return true;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number))
        {
            node = JsonValue.Create(number);
            return true;
        }

        return false;
    }

    private static string ResolvePlaceholder(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return "{{}}";
        }

        string key = expression;
        string? arg = null;
        int colon = expression.IndexOf(':');
        if (colon > 0)
        {
            key = expression[..colon];
            arg = expression[(colon + 1)..];
        }

        key = key.Trim().ToLowerInvariant();

        return key switch
        {
            "unix" => ResolveUnixSeconds(expression, DateTimeOffset.UtcNow),
            "unixms" => ResolveUnixMilliseconds(expression, DateTimeOffset.UtcNow),
            "now" => FormatDateTime(DateTime.Now, arg, DefaultDateTimeFormat),
            "utcnow" => FormatDateTime(DateTime.UtcNow, arg, DefaultDateTimeFormat),
            "date" => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            "guid" or "uuid" => ResolveGuid(arg),
            "random" => GenerateRandomDigits(ParseLength(arg, 8)),
            "randomint" => ResolveRandomInt(arg),
            "alpha" => GenerateAlpha(ParseLength(arg, 8)),
            _ when key.StartsWith("unix", StringComparison.Ordinal) => ResolveUnixSeconds(expression, DateTimeOffset.UtcNow),
            _ => "{{" + expression + "}}"
        };
    }

    private static string ResolveUnixSeconds(string expression, DateTimeOffset anchor)
    {
        if (string.Equals(expression, "unix", StringComparison.OrdinalIgnoreCase))
        {
            return anchor.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        }

        if (!expression.StartsWith("unix", StringComparison.OrdinalIgnoreCase))
        {
            return "{{" + expression + "}}";
        }

        string offset = expression[4..].Trim();
        if (string.IsNullOrEmpty(offset))
        {
            return anchor.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        }

        return ApplyTimeOffset(anchor, offset).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
    }

    private static string ResolveUnixMilliseconds(string expression, DateTimeOffset anchor)
    {
        if (string.Equals(expression, "unixms", StringComparison.OrdinalIgnoreCase))
        {
            return anchor.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        }

        if (!expression.StartsWith("unixms", StringComparison.OrdinalIgnoreCase))
        {
            return "{{" + expression + "}}";
        }

        string offset = expression[6..].Trim();
        if (string.IsNullOrEmpty(offset))
        {
            return anchor.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        }

        return ApplyTimeOffset(anchor, offset).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ApplyTimeOffset(DateTimeOffset anchor, string offset)
    {
        if (string.IsNullOrWhiteSpace(offset))
        {
            return anchor;
        }

        char sign = offset[0];
        if (sign is not '+' and not '-')
        {
            return anchor;
        }

        string amountText = offset[1..];
        if (amountText.Length < 2)
        {
            return anchor;
        }

        char unit = char.ToLowerInvariant(amountText[^1]);
        if (!int.TryParse(amountText[..^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount))
        {
            return anchor;
        }

        TimeSpan delta = unit switch
        {
            's' => TimeSpan.FromSeconds(amount),
            'm' => TimeSpan.FromMinutes(amount),
            'h' => TimeSpan.FromHours(amount),
            'd' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };

        return sign == '-'
            ? anchor.Subtract(delta)
            : anchor.Add(delta);
    }

    private static string FormatDateTime(DateTime time, string? format, string defaultFormat)
    {
        string pattern = string.IsNullOrWhiteSpace(format) ? defaultFormat : format;
        try
        {
            return time.ToString(pattern, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return time.ToString(defaultFormat, CultureInfo.InvariantCulture);
        }
    }

    private static string ResolveGuid(string? format)
    {
        Guid guid = Guid.NewGuid();
        return string.IsNullOrWhiteSpace(format)
            ? guid.ToString()
            : format.Trim().ToLowerInvariant() switch
            {
                "n" => guid.ToString("N"),
                "d" => guid.ToString("D"),
                "b" => guid.ToString("B"),
                "p" => guid.ToString("P"),
                _ => guid.ToString()
            };
    }

    private static string ResolveRandomInt(string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return Random.Shared.Next(0, 100).ToString(CultureInfo.InvariantCulture);
        }

        string[] parts = arg.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int min)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int max))
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }

            return Random.Shared.Next(min, max + 1).ToString(CultureInfo.InvariantCulture);
        }

        return Random.Shared.Next(0, 100).ToString(CultureInfo.InvariantCulture);
    }

    private static int ParseLength(string? arg, int defaultLength)
    {
        if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int length) && length > 0)
        {
            return Math.Min(length, 32);
        }

        return defaultLength;
    }

    private static string GenerateRandomDigits(int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        Span<char> buffer = length <= 64 ? stackalloc char[length] : new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = (char)('0' + Random.Shared.Next(0, 10));
        }

        return new string(buffer);
    }

    private static string GenerateAlpha(int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        const string alphabet = "abcdefghijklmnopqrstuvwxyz";
        Span<char> buffer = length <= 64 ? stackalloc char[length] : new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = alphabet[Random.Shared.Next(alphabet.Length)];
        }

        return new string(buffer);
    }
}
