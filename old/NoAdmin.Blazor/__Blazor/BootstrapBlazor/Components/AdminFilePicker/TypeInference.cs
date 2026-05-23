using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace __Blazor.NoAdmin.Blazor.Components.NovaAdminFilePicker;

internal static class TypeInference
{
	public static void CreateTreeView_0<TItem>(RenderTreeBuilder __builder, int seq, int __seq0, List<TreeViewItem<TItem>> __arg0, int __seq1, Func<TreeViewItem<TItem>, Task> __arg1)
	{
		__builder.OpenComponent<TreeView<TItem>>(seq);
		__builder.AddComponentParameter(__seq0, "Items", __arg0);
		__builder.AddComponentParameter(__seq1, "OnTreeItemClick", __arg1);
		__builder.CloseComponent();
	}

	public static void CreateTreeView_0_CaptureParameters<TItem>(List<TreeViewItem<TItem>> __arg0, out List<TreeViewItem<TItem>> __arg0_out, Func<TreeViewItem<TItem>, Task> __arg1, out Func<TreeViewItem<TItem>, Task> __arg1_out)
	{
		__arg0_out = __arg0;
		__arg1_out = __arg1;
	}
}
