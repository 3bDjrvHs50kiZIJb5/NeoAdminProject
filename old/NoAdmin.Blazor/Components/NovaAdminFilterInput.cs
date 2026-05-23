using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaAdminFilterInput : ComponentBase
{
	private string _searchTagText = "";

	[CascadingParameter]
	public NovaAdminQueryInfo q { get; set; }

	[Parameter]
	public NovaAdminFilterInfo Filter { get; set; }

	[Parameter]
	public string FilterKey { get; set; }

	[Inject]
	private NavigationManager Nav { get; set; } = null;

	[Inject]
	private ToastService ToastService { get; set; } = null;

	[Inject]
	private MessageService MessageService { get; set; } = null;

	[Inject]
	private IJSRuntime JS { get; set; } = null;

	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null;

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		if (Filter == null)
		{
			return;
		}
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "class", "admin-filter-input");
		if (Filter.Type == NovaAdminFilterType.DateRange && Filter.Options.Length == 2)
		{
			DateTime result;
			DateTime result2;
			DateTimeRangeValue value = new DateTimeRangeValue
			{
				NullStart = (DateTime.TryParse(Filter.Options[0].Value.Value, out result) ? new DateTime?(result) : ((DateTime?)null)),
				NullEnd = (DateTime.TryParse(Filter.Options[1].Value.Value, out result2) ? new DateTime?(result2) : ((DateTime?)null))
			};
			__builder.OpenComponent<DateTimeRange>(2);
			__builder.AddComponentParameter(3, "Value", RuntimeHelpers.TypeCheck<DateTimeRangeValue>(value));
			__builder.AddComponentParameter(4, "OnValueChanged", new Func<DateTimeRangeValue, Task>(DateRangeValueChanged));
			__builder.AddComponentParameter(5, "class", "compact-date-range");
			__builder.AddComponentParameter(6, "ShowSidebar", RuntimeHelpers.TypeCheck(value: false));
			__builder.CloseComponent();
		}
		else if (Filter.Type == NovaAdminFilterType.Text && Filter.Options.Length == 1)
		{
			__builder.OpenElement(7, "input");
			__builder.AddAttribute(8, "value", Filter.Options[0].Value.Value);
			__builder.AddAttribute(9, "onchange", EventCallback.Factory.Create(this, (ChangeEventArgs e) => TextValueChanged(e.Value?.ToString())));
			__builder.AddAttribute(10, "onkeyup", EventCallback.Factory.Create((object)this, (Func<KeyboardEventArgs, Task>)OnTextKeyUp));
			__builder.AddAttribute(11, "class", "form-control form-control-sm compact-input");
			__builder.AddAttribute(12, "placeholder", "");
			__builder.CloseElement();
		}
		else
		{
			__builder.OpenElement(13, "div");
			__builder.AddAttribute(14, "class", "btn-group btn-group-sm w-100");
			__builder.OpenElement(15, "button");
			__builder.AddAttribute(16, "type", "button");
			__builder.AddAttribute(17, "class", "btn btn-outline-secondary dropdown-toggle text-truncate compact-btn");
			__builder.AddAttribute(18, "data-bs-toggle", "dropdown");
			__builder.AddAttribute(19, "data-bs-popper", "fixed");
			__builder.AddAttribute(20, "aria-expanded", "false");
			__builder.AddContent(21, GetFilterLabel());
			__builder.CloseElement();
			__builder.AddMarkupContent(22, "\n                ");
			__builder.OpenElement(23, "div");
			__builder.AddAttribute(24, "class", "dropdown-menu p-1 shadow-sm");
			__builder.AddAttribute(25, "style", "min-width: 220px; max-height: 450px; overflow-y: hidden;");
			__builder.OpenElement(26, "div");
			__builder.AddAttribute(27, "class", "px-2 py-1 border-bottom bg-light");
			__builder.OpenElement(28, "input");
			__builder.AddAttribute(29, "class", "form-control form-control-sm mb-1 compact-input");
			__builder.AddAttribute(30, "placeholder", "");
			__builder.AddEventStopPropagationAttribute(31, "onclick", value: true);
			__builder.AddAttribute(32, "value", BindConverter.FormatValue(_searchTagText));
			__builder.AddAttribute(33, "oninput", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				_searchTagText = __value;
			}, _searchTagText));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(34, "\n\n                    ");
			__builder.OpenElement(35, "div");
			__builder.AddAttribute(36, "class", "p-2");
			__builder.AddAttribute(37, "style", "overflow-y: auto; flex: 1;");
			__builder.OpenElement(38, "span");
			__builder.AddAttribute(39, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => TagsButtonClick(-1)));
			__builder.AddAttribute(40, "class", "btn " + ((!Filter.HasValue) ? "btn-primary" : "btn-outline-secondary") + " btn-xs me-1 mb-1");
			__builder.AddMarkupContent(41, "默认");
			__builder.CloseElement();
			for (int num = 0; num < Filter.Options.Length; num++)
			{
				int localB = num;
				NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> adminItem = Filter.Options[num];
				if (string.IsNullOrEmpty(_searchTagText) || adminItem.Value.Label.Contains(_searchTagText, StringComparison.OrdinalIgnoreCase))
				{
					__builder.OpenElement(42, "span");
					__builder.AddAttribute(43, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => TagsButtonClick(localB)));
					__builder.AddAttribute(44, "class", "btn " + (adminItem.Selected ? "btn-primary" : "btn-outline-secondary") + " btn-xs me-1 mb-1");
					if (Filter.Type == NovaAdminFilterType.TagsMultiple && adminItem.Selected)
					{
						__builder.AddMarkupContent(45, "<i class=\"fa fa-check me-1\"></i>");
					}
					__builder.AddContent(46, adminItem.Value.Label);
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}
		__builder.CloseElement();
	}

	protected override void OnParametersSet()
	{
		if (Filter == null && !string.IsNullOrEmpty(FilterKey) && q?.Filters != null)
		{
			Filter = q.Filters.FirstOrDefault((NovaAdminFilterInfo f) => f.QueryStringName == FilterKey || f.Label == FilterKey);
		}
	}

	private string GetFilterLabel()
	{
		if (!Filter.HasValue)
		{
			return "默认";
		}
		List<string> list = (from o in Filter.Options
			where o.Selected
			select o.Value.Label).ToList();
		if (Filter.Type == NovaAdminFilterType.TagsMultiple && list.Count > 2)
		{
			return $"已选 {list.Count} 项";
		}
		return string.Join(",", list);
	}

	private async Task DateRangeValueChanged(DateTimeRangeValue val)
	{
		Filter.Options[0].Value.Value = val.NullStart?.ToString("yyyy-MM-dd") ?? "";
		Filter.Options[1].Value.Value = val.NullEnd?.ToString("yyyy-MM-dd") ?? "";
		Filter.Options[0].Selected = !string.IsNullOrEmpty(Filter.Options[0].Value.Value);
		Filter.Options[1].Selected = !string.IsNullOrEmpty(Filter.Options[1].Value.Value);
		await QueryAgain();
	}

	private async Task TextValueChanged(string val)
	{
		if (val != null)
		{
			val = val.Trim().Replace("\t", "");
		}
		Filter.Options[0].Value.Value = val ?? "";
		Filter.Options[0].Selected = !string.IsNullOrEmpty(Filter.Options[0].Value.Value);
	}

	private async Task OnTextKeyUp(KeyboardEventArgs e)
	{
		if (e.Key == "Enter")
		{
			await QueryAgain();
		}
	}

	private async Task TagsButtonClick(int b)
	{
		if (b < 0)
		{
			NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem>[] options = Filter.Options;
			foreach (NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> opt in options)
			{
				opt.Selected = false;
			}
		}
		else if (Filter.Type == NovaAdminFilterType.Tags)
		{
			bool state = !Filter.Options[b].Selected;
			NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem>[] options2 = Filter.Options;
			foreach (NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> opt2 in options2)
			{
				opt2.Selected = false;
			}
			Filter.Options[b].Selected = state;
		}
		else
		{
			Filter.Options[b].Selected = !Filter.Options[b].Selected;
		}
		await QueryAgain();
	}

	private async Task QueryAgain()
	{
		_searchTagText = "";
		if (q.IsQueryString)
		{
			Dictionary<string, object?> dict = new Dictionary<string, object>
			{
				[q.PageNumberQueryStringName] = null,
				[q.SearchTextQueryStringName] = q.SearchText,
				[q.SortQueryStringName] = q.Sort
			};
			NovaAdminFilterInfo[] filters = q.Filters;
			foreach (NovaAdminFilterInfo f in filters)
			{
				List<string> vals = new List<string>();
				if (f.Type == NovaAdminFilterType.DateRange)
				{
					if (f.Options[0].Selected || f.Options[1].Selected)
					{
						vals.Add(f.Options[0].Value.Value);
						vals.Add(f.Options[1].Value.Value);
					}
				}
				else if (f.Type == NovaAdminFilterType.Text)
				{
					if (f.Options[0].Selected)
					{
						vals.Add(f.Options[0].Value.Value);
					}
				}
				else
				{
					NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem>[] options = f.Options;
					foreach (NoAdmin.Blazor.Models.NovaAdminItem<NoAdmin.Blazor.Models.NovaAdminOptionsItem> opt in options)
					{
						if (opt.Selected)
						{
							vals.Add(opt.Value.Value);
						}
					}
				}
				dict[f.QueryStringName] = (vals.Any() ? vals.ToArray() : null);
			}
			Nav.NavigateTo(Nav.GetUriWithQueryParameters(dict), forceLoad: false);
		}
		q.PageNumber = 1;
		if (q.InvokeQueryAsync != null)
		{
			await q.InvokeQueryAsync();
		}
	}
}
