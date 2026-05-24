using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace NeoAdmin.Blazor.Components;

public sealed class CrudFilterInfo
{
    public string Label { get; set; } = string.Empty;

    public string QueryStringName { get; set; } = string.Empty;

    public CrudFilterType Type { get; set; }

    public CrudFilterOption[] Options { get; set; } = [];

    public int Col { get; set; } = 12;

    public bool HasValue => Type switch
    {
        CrudFilterType.Tags or CrudFilterType.TagsMultiple =>
            Options.Any(option => option.Selected),
        CrudFilterType.DateRange =>
            Options.Any(option => !string.IsNullOrWhiteSpace(option.Value)),
        CrudFilterType.Text =>
            Options.Length > 0 && !string.IsNullOrWhiteSpace(Options[0].Value),
        _ => false
    };

    public T[] Values<T>()
    {
        return Options
            .Where(option => option.Selected)
            .Select(option => ConvertValue<T>(option.Value))
            .ToArray();
    }

    public T Value<T>()
    {
        return Values<T>().FirstOrDefault()!;
    }

    public bool TryGetDateRange(out DateTime? start, out DateTime? end)
    {
        start = null;
        end = null;
        if (Type != CrudFilterType.DateRange || Options.Length < 2)
        {
            return false;
        }

        if (DateTime.TryParse(Options[0].Value, out DateTime startDate))
        {
            start = startDate.Date;
        }

        if (DateTime.TryParse(Options[1].Value, out DateTime endDate))
        {
            end = endDate.Date;
        }

        return start.HasValue || end.HasValue;
    }

    public CrudFilterInfo(string label, string queryStringName, string texts, string values)
        : this(label, queryStringName, multiple: false, 12, texts, values)
    {
    }

    public CrudFilterInfo(string label, string queryStringName, bool multiple, int col, string texts, string values)
    {
        Label = label;
        QueryStringName = queryStringName;
        Type = multiple ? CrudFilterType.TagsMultiple : CrudFilterType.Tags;
        string[] labels = texts.Split(',');
        string[] vals = values.Split(',');
        if (labels.Length != vals.Length)
        {
            throw new Exception("texts.Split(',').Length != values.Split(',').Length");
        }

        Options = labels
            .Select((text, index) => new CrudFilterOption(text.Trim(), vals[index].Trim()))
            .Where(option => !string.IsNullOrWhiteSpace(option.Label))
            .ToArray();
        Col = col;
    }

    public CrudFilterInfo(string label, string queryStringName, Type enumType, bool multiple = false, int col = 12)
    {
        if (enumType == null || !enumType.IsEnum)
        {
            throw new ArgumentException("The provided type must be an Enum.", nameof(enumType));
        }

        List<string> labels = new();
        List<string> vals = new();
        foreach (string name in Enum.GetNames(enumType))
        {
            MemberInfo? member = enumType.GetMember(name).FirstOrDefault();
            if (member == null)
            {
                continue;
            }

            DisplayAttribute? display = member.GetCustomAttribute<DisplayAttribute>();
            DescriptionAttribute? description = member.GetCustomAttribute<DescriptionAttribute>();
            string text = display?.GetName() ?? description?.Description ?? name;
            object value = Enum.Parse(enumType, name);
            labels.Add(text);
            vals.Add(Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
        }

        Label = label;
        QueryStringName = queryStringName;
        Type = multiple ? CrudFilterType.TagsMultiple : CrudFilterType.Tags;
        Col = col;
        Options = labels
            .Select((text, index) => new CrudFilterOption(text.Trim(), vals[index].Trim()))
            .ToArray();
    }

    public CrudFilterInfo(string label, string queryStringName, CrudFilterType type, int col)
    {
        Label = label;
        QueryStringName = queryStringName;
        Type = type;
        Options = type switch
        {
            CrudFilterType.DateRange =>
            [
                new CrudFilterOption("Date1", ""),
                new CrudFilterOption("Date2", "")
            ],
            CrudFilterType.Text =>
            [
                new CrudFilterOption("Text", "")
            ],
            _ => []
        };
        Col = col;
    }

    private static T ConvertValue<T>(string value)
    {
        Type targetType = typeof(T);
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(bool))
        {
            if (bool.TryParse(value, out bool boolValue))
            {
                return (T)(object)boolValue;
            }

            if (value is "1" or "0")
            {
                return (T)(object)(value == "1");
            }
        }

        if (underlyingType.IsEnum)
        {
            return (T)Enum.ToObject(underlyingType, int.Parse(value, CultureInfo.InvariantCulture));
        }

        return (T)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
    }
}
