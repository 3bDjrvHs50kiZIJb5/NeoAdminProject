using Microsoft.AspNetCore.Components;

namespace NeoAdmin.Blazor.Services;

/// <summary>
/// 在 Layout 层渲染选择器弹窗，避免 CrudTable 编辑 Dialog 内嵌 NeoInputTable Dialog 无法显示。
/// </summary>
public sealed class NeoPickerOverlayService
{
    public event Action? Changed;

    public bool IsOpen { get; private set; }

    public string Title { get; private set; } = "选择..";

    public string DialogClassName { get; private set; } = string.Empty;

    public RenderFragment? Body { get; private set; }

    public void Open(string title, string dialogClassName, RenderFragment body)
    {
        Title = title;
        DialogClassName = dialogClassName;
        Body = body;
        IsOpen = true;
        Changed?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        Body = null;
        Changed?.Invoke();
    }

    public void Refresh() => Changed?.Invoke();
}
