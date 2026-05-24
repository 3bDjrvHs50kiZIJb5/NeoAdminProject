using System.Reflection;
using Rougamo;
using Rougamo.Context;

namespace NeoAdmin.Blazor.Utils;

internal static class MoAttributeHelper
{
    public static object? GetPropertyOrFieldValue(Type type, object target, string name)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (Type? current = type; current is not null; current = current.BaseType)
        {
            PropertyInfo? property = current.GetProperty(name, flags);
            if (property is not null)
            {
                return property.GetValue(target);
            }

            FieldInfo? field = current.GetField(name, flags);
            if (field is not null)
            {
                return field.GetValue(target);
            }
        }

        return null;
    }

    public static object? CreateDefaultReturnValue(Type returnType)
    {
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            Type resultType = returnType.GetGenericArguments()[0];
            object? defaultValue = CreateDefaultReturnValue(resultType);
            MethodInfo fromResult = typeof(Task).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(method => method.Name == nameof(Task.FromResult)
                                 && method.IsGenericMethodDefinition
                                 && method.GetParameters().Length == 1)
                .MakeGenericMethod(resultType);
            return fromResult.Invoke(null, [defaultValue]);
        }

        if (returnType.IsValueType)
        {
            return Activator.CreateInstance(returnType);
        }

        return null;
    }

    public static void ReplaceReturnValue(IMo mo, MethodContext context, object? value)
    {
        if (context.HasReturnValue)
        {
            context.ReplaceReturnValue(mo, value ?? CreateDefaultReturnValue(context.ReturnType));
            return;
        }

        context.ReplaceReturnValue(mo, null);
    }
}
