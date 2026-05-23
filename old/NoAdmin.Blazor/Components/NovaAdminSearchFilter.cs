using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using __Blazor.NoAdmin.Blazor.Components.NovaAdminSearchFilter;

namespace NoAdmin.Blazor.Components;

public class NovaAdminSearchFilter : ComponentBase
{
	[Parameter]
	public NovaAdminQueryInfo AdminQuery { get; set; }

	[CascadingParameter]
	public NovaAdminContext.TabInfo TabInfo { get; set; }

	private NovaAdminQueryInfo q => AdminQuery;

	private NovaAdminFilterInfo[] Filters => q.Filters;

	[Inject]
	private ToastService ToastService { get; set; } = null;

	[Inject]
	private MessageService MessageService { get; set; } = null;

	[Inject]
	private NavigationManager Nav { get; set; } = null;

	[Inject]
	private IJSRuntime JS { get; set; } = null;

	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null;

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "class", "row admin-search-filter");
		for (int i = 0; i < Filters?.Length; i++)
		{
			if (!Filters[i].Options.Any())
			{
				continue;
			}
			int localA = i;
			__builder.OpenElement(2, "div");
			__builder.AddAttribute(3, "class", "col-" + Filters[i].Col);
			__builder.AddAttribute(4, "style", "padding:0;margin:0;");
			__builder.OpenElement(5, "div");
			__builder.AddAttribute(6, "class", "mb-2");
			__builder.OpenElement(7, "div");
			__builder.AddAttribute(8, "style", "float:left;width:65px");
			__builder.AddAttribute(9, "class", "sm");
			__builder.AddContent(10, Filters[i].Label);
			__builder.CloseElement();
			if (Filters[i].Type == NovaAdminFilterType.DateRange)
			{
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "col-auto");
				__builder.AddAttribute(13, "style", "padding:0;margin:0;float:left;");
				if (Filters[i].Options.Length == 2)
				{
					DateTime result;
					DateTime result2;
					DateTimeRangeValue value = new DateTimeRangeValue
					{
						NullStart = (DateTime.TryParse(Filters[i].Options[0].Value.Value, out result) ? new DateTime?(result) : ((DateTime?)null)),
						NullEnd = (DateTime.TryParse(Filters[i].Options[1].Value.Value, out result2) ? new DateTime?(result2) : ((DateTime?)null))
					};
					__builder.OpenComponent<DateTimeRange>(14);
					__builder.AddComponentParameter(15, "Value", RuntimeHelpers.TypeCheck<DateTimeRangeValue>(value));
					__builder.AddComponentParameter(16, "OnValueChanged", (Func<DateTimeRangeValue, Task>)((DateTimeRangeValue v) => DateRangeValueChanged(localA, v)));
					__builder.AddComponentParameter(17, "ShowToday", RuntimeHelpers.TypeCheck(Filters[i].ExtraData.ContainsKey("ShowToday") && object.Equals(Filters[i].ExtraData["ShowToday"], true)));
					__builder.CloseComponent();
				}
				__builder.CloseElement();
			}
			else if (Filters[i].Type == NovaAdminFilterType.Text)
			{
				__builder.OpenElement(18, "div");
				__builder.AddAttribute(19, "class", "col-auto");
				__builder.AddAttribute(20, "style", "padding:0;margin:0;float:left;");
				if (Filters[i].Options.Length == 1)
				{
					TypeInference.CreateBootstrapInput_0(__builder, 21, 22, "请输入", 23, Filters[i].Options[0].Value.Value, 24, (string v) => TextValueChanged(localA, v), 25, (string v) => TextValueChanged(localA, v), 26, __arg4: true, 27, __arg5: true);
				}
				__builder.CloseElement();
			}
			else
			{
				__builder.OpenElement(28, "div");
				__builder.AddAttribute(29, "class", "col-auto");
				__builder.AddAttribute(30, "style", "padding:0;margin:0;");
				__builder.OpenElement(31, "span");
				__builder.AddAttribute(32, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => TagsButtonClick(localA, -1)));
				__builder.AddAttribute(33, "class", "btn " + ((!Filters[i].Options.Any((NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> b) => b.Selected)) ? "btn-outline-primary" : "btn-outline-secondary") + " btn-xs pl-1 pr-1");
				__builder.AddMarkupContent(34, "默认");
				__builder.CloseElement();
				for (int num = 0; num < Filters[i].Options.Length; num++)
				{
					int localB = num;
					__builder.OpenElement(35, "span");
					__builder.AddAttribute(36, "onclick", EventCallback.Factory.Create(this, (MouseEventArgs e) => TagsButtonClick(localA, localB)));
					__builder.AddAttribute(37, "class", "btn " + (Filters[i].Options[num].Selected ? "btn-outline-primary " : "btn-outline-secondary") + " btn-xs ml-1 pl-1 pr-1");
					__builder.AddContent(38, Filters[i].Options[num].Value.Label);
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}
		__builder.CloseElement();
	}

	protected override void OnInitialized()
	{
		if (!q.IsQueryString)
		{
			return;
		}
		NovaAdminFilterInfo[] filters = Filters;
		foreach (NovaAdminFilterInfo adminFilterInfo in filters)
		{
			string[] queryStringValues = Nav.GetQueryStringValues(adminFilterInfo.QueryStringName, TabInfo?.Url);
			if (adminFilterInfo.Type == NovaAdminFilterType.DateRange)
			{
				if (queryStringValues.Length != 0)
				{
					adminFilterInfo.Options[0].Value.Value = (DateTime.TryParse(queryStringValues[0], out var result) ? result.ToString("yyyy-MM-dd") : "");
				}
				if (queryStringValues.Length > 1)
				{
					adminFilterInfo.Options[1].Value.Value = (DateTime.TryParse(queryStringValues[1], out var result2) ? result2.ToString("yyyy-MM-dd") : "");
				}
				adminFilterInfo.Options[0].Selected = adminFilterInfo.Options[0].Value.Value != "";
				adminFilterInfo.Options[1].Selected = adminFilterInfo.Options[1].Value.Value != "";
				continue;
			}
			if (adminFilterInfo.Type == NovaAdminFilterType.Text)
			{
				if (queryStringValues.Length != 0)
				{
					adminFilterInfo.Options[0].Value.Value = queryStringValues[0];
				}
				continue;
			}
			string[] array = queryStringValues;
			foreach (string text in array)
			{
				for (int k = 0; k < adminFilterInfo.Options.Length; k++)
				{
					if (adminFilterInfo.Options[k].Value.Value == text)
					{
						adminFilterInfo.Options[k].Selected = true;
					}
				}
			}
		}
	}

	private async Task DateRangeValueChanged(int a, DateTimeRangeValue dateTimeRangeValue)
	{
		NovaAdminFilterInfo filter = Filters[a];
		filter.Options[0].Value.Value = dateTimeRangeValue.NullStart?.ToString("yyyy-MM-dd") ?? "";
		filter.Options[1].Value.Value = dateTimeRangeValue.NullEnd?.ToString("yyyy-MM-dd") ?? "";
		filter.Options[0].Selected = !string.IsNullOrEmpty(filter.Options[0].Value.Value);
		filter.Options[1].Selected = !string.IsNullOrEmpty(filter.Options[1].Value.Value);
		await QueryAgian();
	}

	private async Task TextValueChanged(int a, string textValue)
	{
		NovaAdminFilterInfo filter = Filters[a];
		filter.Options[0].Value.Value = textValue ?? "";
		filter.Options[0].Selected = !string.IsNullOrEmpty(filter.Options[0].Value.Value);
		await QueryAgian();
	}

	private async Task TagsButtonClick(int a, int b)
	{
		if (b < 0 || Filters[a].Type == NovaAdminFilterType.Tags)
		{
			NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem>[] options = Filters[a].Options;
			foreach (NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> btn in options)
			{
				if (b < 0 || Filters[a].Options[b] != btn)
				{
					btn.Selected = false;
				}
			}
		}
		if (b >= 0)
		{
			Filters[a].Options[b].Selected = !Filters[a].Options[b].Selected;
		}
		await QueryAgian();
	}

	private async Task QueryAgian()
	{
		if (q.IsQueryString)
		{
			Dictionary<string, object> dict = new Dictionary<string, object>
			{
				[q.PageNumberQueryStringName] = null,
				[q.SearchTextQueryStringName] = q.SearchText,
				[q.SortQueryStringName] = q.Sort
			};
			NovaAdminFilterInfo[] filters = Filters;
			foreach (NovaAdminFilterInfo filter in filters)
			{
				List<string> vals = new List<string>();
				if (filter.Type == NovaAdminFilterType.DateRange)
				{
					if (filter.Options[0].Value.Value != "" || filter.Options[0].Value.Value != "")
					{
						vals.Add(filter.Options[0].Value.Value);
						vals.Add(filter.Options[1].Value.Value);
					}
				}
				else if (filter.Type == NovaAdminFilterType.Text)
				{
					if (!string.IsNullOrEmpty(filter.Options[0].Value.Value))
					{
						vals.Add(filter.Options[0].Value.Value);
					}
				}
				else
				{
					for (int x = 0; x < filter.Options.Length; x++)
					{
						if (filter.Options[x].Selected)
						{
							vals.Add(filter.Options[x].Value.Value);
						}
					}
				}
				dict[filter.QueryStringName] = (vals.Any() ? vals.ToArray() : null);
			}
			string url = Nav.GetUriWithQueryParameters(dict);
			Nav.NavigateTo(url);
		}
		q.PageNumber = 1;
		if (q.InvokeQueryAsync != null)
		{
			await q.InvokeQueryAsync();
		}
	}
}
