using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NeoAdmin.Blazor.Data;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoAdmin.Blazor.Swagger;

public sealed class HideControllerFilter : IDocumentFilter
{
    private readonly NeoAdminOptions options;

    public HideControllerFilter(IOptions<NeoAdminOptions> options)
    {
        this.options = options.Value;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        string[] swaggerHides = options.SwaggerHides;
        if (swaggerHides.Length == 0 || swaggerDoc.Paths is null)
        {
            return;
        }

        List<string> pathsToRemove = swaggerDoc.Paths
            .Where(path => swaggerHides.Any(hide => path.Key.Contains(hide, StringComparison.OrdinalIgnoreCase)))
            .Select(path => path.Key)
            .ToList();

        foreach (string path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}
