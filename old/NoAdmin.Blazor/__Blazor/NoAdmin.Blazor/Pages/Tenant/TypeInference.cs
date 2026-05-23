using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Pages.Tenant;

internal static class TypeInference
{
	public static void CreateAvatarUpload_0<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, List<UploadFile> __arg0, int __seq1, Func<UploadFile, Task> __arg1, int __seq2, int __arg2, int __seq3, int __arg3, int __seq4, bool __arg4, int __seq5, string __arg5, int __seq6, string __arg6, int __seq7, TValue __arg7, int __seq8, EventCallback<TValue> __arg8, int __seq9, Expression<Func<TValue>> __arg9)
	{
		__builder.OpenComponent<AvatarUpload<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "DefaultFileList", __arg0);
		__builder.AddComponentParameter(__seq1, "OnChange", __arg1);
		__builder.AddComponentParameter(__seq2, "Width", __arg2);
		__builder.AddComponentParameter(__seq3, "Height", __arg3);
		__builder.AddComponentParameter(__seq4, "IsCircle", __arg4);
		__builder.AddComponentParameter(__seq5, "BorderRadius", __arg5);
		__builder.AddComponentParameter(__seq6, "Accept", __arg6);
		__builder.AddComponentParameter(__seq7, "Value", __arg7);
		__builder.AddComponentParameter(__seq8, "ValueChanged", __arg8);
		__builder.AddComponentParameter(__seq9, "ValueExpression", __arg9);
		__builder.CloseComponent();
	}

	public static void CreateAvatarUpload_1<TValue>(RenderTreeBuilder __builder, int seq, int __seq0, List<UploadFile> __arg0, int __seq1, Func<UploadFile, Task> __arg1, int __seq2, int __arg2, int __seq3, int __arg3, int __seq4, bool __arg4, int __seq5, string __arg5, int __seq6, string __arg6, int __seq7, TValue __arg7, int __seq8, EventCallback<TValue> __arg8, int __seq9, Expression<Func<TValue>> __arg9)
	{
		__builder.OpenComponent<AvatarUpload<TValue>>(seq);
		__builder.AddComponentParameter(__seq0, "DefaultFileList", __arg0);
		__builder.AddComponentParameter(__seq1, "OnChange", __arg1);
		__builder.AddComponentParameter(__seq2, "Width", __arg2);
		__builder.AddComponentParameter(__seq3, "Height", __arg3);
		__builder.AddComponentParameter(__seq4, "IsCircle", __arg4);
		__builder.AddComponentParameter(__seq5, "BorderRadius", __arg5);
		__builder.AddComponentParameter(__seq6, "Accept", __arg6);
		__builder.AddComponentParameter(__seq7, "Value", __arg7);
		__builder.AddComponentParameter(__seq8, "ValueChanged", __arg8);
		__builder.AddComponentParameter(__seq9, "ValueExpression", __arg9);
		__builder.CloseComponent();
	}
}
