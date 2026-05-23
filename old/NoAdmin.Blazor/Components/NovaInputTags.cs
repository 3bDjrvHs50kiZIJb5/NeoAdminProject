using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class NovaInputTags<TItem> : ComponentBase, IAsyncDisposable where TItem : class
{
	private ElementReference _container;

	private ElementReference _inputElement;

	private DotNetObjectReference<NovaInputTags<TItem>>? _dotNetHelper;

	private string _inputValue = "";

	private List<TItem> _searchResults = new List<TItem>();

	private bool _isDropdownVisible = false;

	private int _activeIndex = -1;

	private bool _selectionMade = false;

	private bool _isInternalAction = false;

	public string ClientId { get; set; } = "input_" + Guid.NewGuid().ToString("n");

	[Parameter]
	public ICollection<TItem> Value { get; set; }

	[Parameter]
	public EventCallback<ICollection<TItem>> ValueChanged { get; set; }

	[Parameter]
	[EditorRequired]
	public Func<TItem, string> DisplayText { get; set; }

	[Parameter]
	[EditorRequired]
	public Func<string, TItem> OnCreate { get; set; }

	[Parameter]
	[EditorRequired]
	public Func<string, Task<IEnumerable<TItem>>> OnSearch { get; set; }

	[Parameter]
	public string Placeholder { get; set; } = "Add...";

	[Parameter]
	public bool DropdownAsGrid { get; set; } = false;

	[Parameter]
	public int GridMinItemWidth { get; set; } = 150;

	private string _dropdownContainerClass => DropdownAsGrid ? "input-tags-dropdown-grid" : "input-tags-dropdown list-group";

	private string _dropdownContainerStyle => DropdownAsGrid ? $"--grid-item-min-width: {GridMinItemWidth}px;" : "";

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
		__builder.OpenElement(0, "div");
		__builder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)FocusInput));
		__builder.AddAttribute(2, "class", "input-tags-container form-control");
		__builder.AddAttribute(3, "tabindex", "-1");
		__builder.AddElementReferenceCapture(4, delegate(ElementReference __value)
		{
			_container = __value;
		});
		if (Value != null)
		{
			foreach (TItem item in Value)
			{
				__builder.OpenElement(5, "span");
				__builder.AddAttribute(6, "class", "input-tags-badge badge bg-primary me-1");
				__builder.AddContent(7, DisplayText(item));
				__builder.AddMarkupContent(8, "\r\n                ");
				__builder.OpenElement(9, "button");
				__builder.AddAttribute(10, "type", "button");
				__builder.AddAttribute(11, "class", "input-tags-btn-close");
				__builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RemoveItem(item)));
				__builder.AddAttribute(13, "onmousedown", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
				{
					_isInternalAction = true;
				}));
				__builder.CloseElement();
				__builder.CloseElement();
			}
		}
		__builder.OpenElement(14, "input");
		__builder.AddAttribute(15, "id", ClientId);
		__builder.AddAttribute(16, "type", "text");
		__builder.AddAttribute(17, "class", "input-tags-field");
		ICollection<TItem> value = Value;
		__builder.AddAttribute(18, "placeholder", (value != null && value.Any()) ? "" : Placeholder);
		__builder.AddAttribute(19, "onfocus", EventCallback.Factory.Create<FocusEventArgs>((object)this, (Func<Task>)ShowDropdown));
		__builder.AddElementReferenceCapture(20, delegate(ElementReference __value)
		{
			_inputElement = __value;
		});
		__builder.CloseElement();
		if (_isDropdownVisible && _searchResults.Any())
		{
			__builder.OpenElement(21, "div");
			__builder.AddAttribute(22, "class", _dropdownContainerClass);
			__builder.AddAttribute(23, "style", _dropdownContainerStyle);
			foreach (var item3 in _searchResults.Select((TItem item3, int item4) => (item: item3, index: item4)))
			{
				var (item2, index) = item3;
				__builder.OpenElement(24, "button");
				__builder.AddAttribute(25, "type", "button");
				__builder.AddAttribute(26, "class", _dropdownItemClass(index));
				__builder.AddAttribute(27, "onmousedown", EventCallback.Factory.Create<MouseEventArgs>(this, () => SelectItem(item2)));
				__builder.AddContent(28, DisplayText(item2));
				__builder.CloseElement();
			}
			__builder.CloseElement();
		}
		__builder.CloseElement();
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			_dotNetHelper = DotNetObjectReference.Create(this);
			await JS.InvokeVoidAsync("novaAdminJS.inputTags.initialize", _inputElement, _dotNetHelper);
		}
	}

	private string _dropdownItemClass(int index)
	{
		string text = ((index == _activeIndex) ? "active" : "");
		string text2 = (DropdownAsGrid ? "input-tags-dropdown-item" : "list-group-item list-group-item-action");
		return text2 + " " + text;
	}

	[JSInvokable]
	public async Task OnDebouncedInputAsync(string value)
	{
		_inputValue = value;
		_activeIndex = -1;
		_isDropdownVisible = true;
		await TriggerSearch();
	}

	private async Task TriggerSearch()
	{
		_searchResults = (await OnSearch(_inputValue ?? "")).Where((TItem sr) => Value == null || !Value.Any((TItem v) => DisplayText(v).Equals(DisplayText(sr), StringComparison.OrdinalIgnoreCase))).ToList();
		await InvokeAsync((Action)base.StateHasChanged);
	}

	private async Task AddItem(TItem item)
	{
		if (item != null)
		{
			if (Value == null)
			{
				Value = new List<TItem>();
			}
			if (!Value.Any((TItem v) => DisplayText(v).Equals(DisplayText(item), StringComparison.OrdinalIgnoreCase)))
			{
				Value.Add(item);
				await ValueChanged.InvokeAsync(Value);
			}
		}
	}

	private async Task RemoveItem(TItem item)
	{
		Value.Remove(item);
		await ValueChanged.InvokeAsync(Value);
		_isDropdownVisible = true;
		await TriggerSearch();
		await FocusInput();
	}

	private async Task CreateAndAddItem(string valueToCreate)
	{
		if (!string.IsNullOrWhiteSpace(valueToCreate))
		{
			TItem newItem = OnCreate(valueToCreate);
			await SelectItem(newItem);
		}
	}

	private async Task SelectItem(TItem item)
	{
		_selectionMade = true;
		await AddItem(item);
		_inputValue = "";
		_isDropdownVisible = false;
		await JS.InvokeVoidAsync("eval", "document.getElementById('" + ClientId + "').value = ''");
		await FocusInput();
		StateHasChanged();
	}

	private async Task CreateAndAddItem()
	{
		if (!string.IsNullOrWhiteSpace(_inputValue))
		{
			TItem newItem = OnCreate(_inputValue);
			await SelectItem(newItem);
		}
	}

	[JSInvokable]
	public async Task OnJsBlurAsync(string currentValue)
	{
		if (_isInternalAction)
		{
			_isInternalAction = false;
			return;
		}
		await InvokeAsync(async delegate
		{
			await Task.Delay(200);
			if (_selectionMade)
			{
				_selectionMade = false;
			}
			else
			{
				_isDropdownVisible = false;
				_activeIndex = -1;
				await CreateAndAddItem(currentValue);
				StateHasChanged();
			}
		});
	}

	[JSInvokable]
	public async Task OnJsInstantActionAsync(string key, string currentValue)
	{
		await InvokeAsync(async delegate
		{
			if (_activeIndex >= 0 && _activeIndex < _searchResults.Count)
			{
				await SelectItem(_searchResults[_activeIndex]);
			}
			else if (!string.IsNullOrWhiteSpace(currentValue))
			{
				TItem newItem = OnCreate(currentValue);
				await SelectItem(newItem);
			}
			StateHasChanged();
		});
	}

	[JSInvokable]
	public async Task OnJsStateActionAsync(string key)
	{
		await InvokeAsync(async delegate
		{
			switch (key)
			{
			case "ArrowDown":
				if (_searchResults.Any())
				{
					_activeIndex = (_activeIndex + 1) % _searchResults.Count;
				}
				break;
			case "ArrowUp":
				if (_searchResults.Any())
				{
					_activeIndex = (_activeIndex - 1 + _searchResults.Count) % _searchResults.Count;
				}
				break;
			case "Escape":
				_isDropdownVisible = false;
				break;
			case "Backspace":
				if (Value?.Any() ?? false)
				{
					await RemoveItem(Value.Last());
				}
				break;
			}
			StateHasChanged();
		});
	}

	private async Task ShowDropdown()
	{
		_isDropdownVisible = true;
		await TriggerSearch();
	}

	private async Task FocusInput()
	{
		try
		{
			await _inputElement.FocusAsync();
		}
		catch (JSException)
		{
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_dotNetHelper != null)
		{
			try
			{
				await JS.InvokeVoidAsync("novaAdminJS.inputTags.dispose", _inputElement);
			}
			catch (JSException)
			{
			}
		}
		_dotNetHelper?.Dispose();
	}
}
