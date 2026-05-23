using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using FreeSql.Internal;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NoAdmin.Blazor.Mvc;

public class SwaggerDocumentFilter : IDocumentFilter
{
	private ConcurrentDictionary<Type, Dictionary<string, string>> _controllerSummarys = new ConcurrentDictionary<Type, Dictionary<string, string>>();

	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		if (context.ApiDescriptions != null)
		{
			MethodInfo methodInfo = default(MethodInfo);
			foreach (ApiDescription apiDescription in context.ApiDescriptions)
			{
				if (!Swashbuckle.AspNetCore.SwaggerGen.ApiDescriptionExtensions.TryGetMethodInfo(apiDescription, out methodInfo) || !(methodInfo.DeclaringType != null) || !methodInfo.DeclaringType.IsGenericType || !((Dictionary<string, IOpenApiPathItem>)(object)swaggerDoc.Paths).TryGetValue("/" + apiDescription.RelativePath.TrimStart('/'), out IOpenApiPathItem value))
				{
					continue;
				}
				foreach (KeyValuePair<HttpMethod, OpenApiOperation> operation in value.Operations)
				{
					if (!operation.Value.Summary.IsNull())
					{
						Dictionary<string, string> orAdd = _controllerSummarys.GetOrAdd(methodInfo.ReflectedType, (Func<Type, Dictionary<string, string>>)CommonUtils.GetProperyCommentBySummary);
						string value2;
						string text = ((orAdd != null && orAdd.TryGetValue("", out value2)) ? value2 : methodInfo.ReflectedType.Name);
						operation.Value.Summary = text + " - " + operation.Value.Summary;
					}
				}
			}
		}
		if (swaggerDoc.Tags == null)
		{
			return;
		}
		Dictionary<string, int> tagCount = new Dictionary<string, int>();
		foreach (KeyValuePair<string, IOpenApiPathItem> item in (Dictionary<string, IOpenApiPathItem>)(object)swaggerDoc.Paths)
		{
			IEnumerable<OpenApiOperation> source = item.Value.Operations.Select((KeyValuePair<HttpMethod, OpenApiOperation> x) => x.Value);
			source.Where((OpenApiOperation o) => ((o != null) ? o.Tags : null) != null).ToList().ForEach(delegate(OpenApiOperation x)
			{
				foreach (OpenApiTagReference tag in x.Tags)
				{
					if (!tagCount.ContainsKey(tag.Name))
					{
						tagCount.Add(tag.Name, 1);
					}
					else
					{
						tagCount[tag.Name]++;
					}
				}
			});
		}
		foreach (string key in tagCount.Keys)
		{
			foreach (OpenApiTag tag2 in swaggerDoc.Tags)
			{
				if (tag2.Name == key)
				{
					tag2.Description = $"{tag2.Description}({tagCount[key]})";
				}
			}
		}
	}
}
