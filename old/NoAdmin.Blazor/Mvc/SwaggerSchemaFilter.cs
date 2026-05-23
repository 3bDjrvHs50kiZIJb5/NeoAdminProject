using System;
using System.Linq;
using System.Text;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NoAdmin.Blazor.Mvc;

public class SwaggerSchemaFilter : ISchemaFilter
{
	public void Apply(IOpenApiSchema model, SchemaFilterContext context)
	{
		if (context.Type.IsEnum)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Enum.GetNames(context.Type).ToList().ForEach(delegate(string name)
			{
				Enum obj = (Enum)Enum.Parse(context.Type, name);
				string value = $"{name}={Convert.ToInt64(Enum.Parse(context.Type, name))}";
				stringBuilder.AppendLine(value);
			});
			((IOpenApiDescribedElement)model).Description = stringBuilder.ToString();
		}
	}
}
