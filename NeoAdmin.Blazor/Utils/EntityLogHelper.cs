using System.Reflection;

namespace NeoAdmin.Blazor.Utils;

public static class EntityLogHelper
{
    public static string Describe(object? entity)
    {
        if (entity is null)
        {
            return "null";
        }

        Type type = entity.GetType();
        object? id = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity);
        string? name = GetDisplayName(entity, type);

        if (name is not null && id is not null)
        {
            return $"{type.Name}(Id={id}, {name})";
        }

        if (id is not null)
        {
            return $"{type.Name}(Id={id})";
        }

        return type.Name;
    }

    private static string? GetDisplayName(object entity, Type type)
    {
        foreach (string propertyName in new[] { "Name", "Label", "Username", "Title", "Topic" })
        {
            PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            string? value = property?.GetValue(entity)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
