using System;
using System.Linq.Expressions;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Pages.Menu;

internal static class TypeInference
{
	public static void CreateCheckbox_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, bool __arg0, int __seq1, TValue __arg1, int __seq2, EventCallback<TValue> __arg2, int __seq3, Expression<Func<TValue>> __arg3)
	{
		__builder.OpenComponent<Checkbox<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "IsDisabled", __arg0);
		__builder.AddComponentParameter(__seq1, "Value", __arg1);
		__builder.AddComponentParameter(__seq2, "ValueChanged", __arg2);
		__builder.AddComponentParameter(__seq3, "ValueExpression", __arg3);
		__builder.CloseComponent();
	}

	public static void CreateCheckbox_1<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, bool __arg0, int __seq1, string __arg1, int __seq2, object __arg2, int __seq3, TValue __arg3, int __seq4, EventCallback<TValue> __arg4, int __seq5, Expression<Func<TValue>> __arg5)
	{
		__builder.OpenComponent<Checkbox<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "ShowAfterLabel", __arg0);
		__builder.AddComponentParameter(__seq1, "DisplayText", __arg1);
		__builder.AddComponentParameter(__seq2, "style", __arg2);
		__builder.AddComponentParameter(__seq3, "Value", __arg3);
		__builder.AddComponentParameter(__seq4, "ValueChanged", __arg4);
		__builder.AddComponentParameter(__seq5, "ValueExpression", __arg5);
		__builder.CloseComponent();
	}

	public static void CreateCheckbox_2<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, bool __arg0, int __seq1, string __arg1, int __seq2, object __arg2, int __seq3, TValue __arg3, int __seq4, EventCallback<TValue> __arg4, int __seq5, Expression<Func<TValue>> __arg5)
	{
		__builder.OpenComponent<Checkbox<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "ShowAfterLabel", __arg0);
		__builder.AddComponentParameter(__seq1, "DisplayText", __arg1);
		__builder.AddComponentParameter(__seq2, "style", __arg2);
		__builder.AddComponentParameter(__seq3, "Value", __arg3);
		__builder.AddComponentParameter(__seq4, "ValueChanged", __arg4);
		__builder.AddComponentParameter(__seq5, "ValueExpression", __arg5);
		__builder.CloseComponent();
	}
}
