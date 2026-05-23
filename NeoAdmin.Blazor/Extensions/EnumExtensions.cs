using System.ComponentModel;
using System.Reflection;

namespace NeoAdmin.Blazor.Extensions;

public static class EnumExtensions
{
    public static string ToDescription(this Enum item)
    {
        string name = item.ToString();
        FieldInfo? field = item.GetType().GetField(name);
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>(inherit: false);
        return attribute?.Description ?? name;
    }
}
