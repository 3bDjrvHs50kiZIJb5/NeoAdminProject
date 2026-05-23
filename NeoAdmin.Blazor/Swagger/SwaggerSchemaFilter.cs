using System.Text;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoAdmin.Blazor.Swagger;

public sealed class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        var description = new StringBuilder();
        foreach (string name in Enum.GetNames(context.Type))
        {
            description.AppendLine($"{name}={Convert.ToInt64(Enum.Parse(context.Type, name))}");
        }

        schema.Description = description.ToString();
    }
}
