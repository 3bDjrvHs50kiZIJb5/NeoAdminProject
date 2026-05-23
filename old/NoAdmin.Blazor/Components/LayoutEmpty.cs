using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Components;

public class LayoutEmpty : LayoutComponentBase
{
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
		__builder.OpenComponent<ErrorLogger>(0);
		__builder.AddComponentParameter(1, "OnErrorHandleAsync", new Func<ILogger, Exception, Task>(OnErrorHandleAsync));
		__builder.AddAttribute(2, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddContent(3, base.Body);
		});
		__builder.CloseComponent();
	}

	private Task OnErrorHandleAsync(ILogger logger, Exception ex)
	{
		logger.LogError(ex, ex.Message);
		if (ex.InnerException != null && ex != null)
		{
			ex = ex.InnerException;
		}
		return ToastServiceExtensions.Error(ToastService, "系统异常", ex.Message, false);
	}
}
