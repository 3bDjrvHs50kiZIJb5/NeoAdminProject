using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace NeoAdmin.Blazor.Utils;

/// <summary>
/// 与旧版 NovaAdmin 一致的 Newtonsoft 序列化配置，供 FileCache 等场景复用。
/// </summary>
public static class NeoAdminJson
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters =
        {
            new StringEnumConverter(),
            new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }
        }
    };
}
