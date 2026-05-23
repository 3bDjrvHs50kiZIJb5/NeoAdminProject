using System;
using System.Linq.Expressions;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Pages.Org;

internal static class TypeInference
{
	public static void CreateCheckbox_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, TValue __arg0, int __seq1, EventCallback<TValue> __arg1, int __seq2, Expression<Func<TValue>> __arg2)
	{
		__builder.OpenComponent<Checkbox<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "Value", __arg0);
		__builder.AddComponentParameter(__seq1, "ValueChanged", __arg1);
		__builder.AddComponentParameter(__seq2, "ValueExpression", __arg2);
		__builder.CloseComponent();
	}
}
