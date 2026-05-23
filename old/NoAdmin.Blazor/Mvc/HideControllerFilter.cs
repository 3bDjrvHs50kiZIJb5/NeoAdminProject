using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NoAdmin.Blazor.Mvc;

public class HideControllerFilter : IDocumentFilter
{
	public string[] SwaggerHides { get; }

	public HideControllerFilter(string[] swaggerHides)
	{
		SwaggerHides = swaggerHides;
	}

	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		string[] swaggerHides = SwaggerHides;
		if (swaggerHides == null || !swaggerHides.Any())
		{
			return;
		}
		List<KeyValuePair<string, IOpenApiPathItem>> list = ((IEnumerable<KeyValuePair<string, IOpenApiPathItem>>)swaggerDoc.Paths).Where((KeyValuePair<string, IOpenApiPathItem> path) => SwaggerHides.Any((string b) => path.Key.Contains(b, StringComparison.OrdinalIgnoreCase))).ToList();
		foreach (KeyValuePair<string, IOpenApiPathItem> item in list)
		{
			((Dictionary<string, IOpenApiPathItem>)(object)swaggerDoc.Paths).Remove(item.Key);
		}
	}
}
