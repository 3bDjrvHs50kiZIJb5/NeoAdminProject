using System;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Components.NovaAdminSearchFilter;

internal static class TypeInference
{
	public static void CreateBootstrapInput_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, string __arg0, int __seq1, TValue __arg1, int __seq2, Func<TValue, Task> __arg2, int __seq3, Func<TValue, Task> __arg3, int __seq4, bool __arg4, int __seq5, bool __arg5)
	{
		__builder.OpenComponent<BootstrapInput<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "PlaceHolder", __arg0);
		__builder.AddComponentParameter(__seq1, "Value", __arg1);
		__builder.AddComponentParameter(__seq2, "OnValueChanged", __arg2);
		__builder.AddComponentParameter(__seq3, "OnEnterAsync", __arg3);
		__builder.AddComponentParameter(__seq4, "IsTrim", __arg4);
		__builder.AddComponentParameter(__seq5, "IsClearable", __arg5);
		__builder.CloseComponent();
	}
}
