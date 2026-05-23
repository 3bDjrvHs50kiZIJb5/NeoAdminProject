using System;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace NoAdmin.Blazor.Components;

public class NovaParamText : ComponentBase
{
	[Parameter]
	public string ParamId { get; set; }

	[Parameter]
	public string Field { get; set; } = nameof(SysParam.Value);

	[Parameter]
	public string DefaultText { get; set; } = string.Empty;

	[Parameter]
	public string TagName { get; set; } = "span";

	[Parameter]
	public string Class { get; set; }

	[Parameter]
	public string Style { get; set; }

	[Parameter]
	public bool EnabledOnly { get; set; } = true;

	[Inject]
	private IFreeSql fsql { get; set; }

	private string _text = string.Empty;

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		__builder.OpenElement(0, string.IsNullOrWhiteSpace(TagName) ? "span" : TagName);
		if (!string.IsNullOrWhiteSpace(Class))
		{
			__builder.AddAttribute(1, "class", Class);
		}
		if (!string.IsNullOrWhiteSpace(Style))
		{
			__builder.AddAttribute(2, "style", Style);
		}
		__builder.AddContent(3, _text);
		__builder.CloseElement();
	}

	protected override async Task OnParametersSetAsync()
	{
		if (string.IsNullOrWhiteSpace(ParamId))
		{
			_text = DefaultText;
			return;
		}

		var query = fsql.Select<SysParam>()
			.Where(a => a.Id == ParamId);

		if (EnabledOnly)
		{
			query = query.Where(a => a.Enabled);
		}

		var param = await query.FirstAsync();
		_text = GetFieldValue(param) ?? DefaultText;
	}

	private string GetFieldValue(SysParam? param)
	{
		if (param == null)
		{
			return null;
		}

		return Field switch
		{
			nameof(SysParam.Title) => param.Title,
			nameof(SysParam.Value) => param.Value,
			nameof(SysParam.Value2) => param.Value2,
			nameof(SysParam.Value3) => param.Value3,
			nameof(SysParam.Value4) => param.Value4,
			nameof(SysParam.Value5) => param.Value5,
			nameof(SysParam.Value6) => param.Value6,
			nameof(SysParam.Value7) => param.Value7,
			nameof(SysParam.Description) => param.Description,
			_ => param.Value
		};
	}
}
