using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Components.NovaAdminTable;

internal static class TypeInference
{
	public static void CreateCascadingValue_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, RenderFragment __arg1)
	{
		__builder.OpenComponent<CascadingValue<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "Value", __arg0);
		__builder.AddComponentParameter(__seq1, "ChildContent", __arg1);
		__builder.CloseComponent();
	}

	public static void CreateCascadingValue_1<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, bool __arg1, int __seq2, RenderFragment __arg2)
	{
		__builder.OpenComponent<CascadingValue<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "Value", __arg0);
		__builder.AddComponentParameter(__seq1, "IsFixed", __arg1);
		__builder.AddComponentParameter(__seq2, "ChildContent", __arg2);
		__builder.CloseComponent();
	}
}
