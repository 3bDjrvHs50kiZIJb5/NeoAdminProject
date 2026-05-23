using System;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor;

public class _Imports
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

	protected void Execute()
	{
	}
}
