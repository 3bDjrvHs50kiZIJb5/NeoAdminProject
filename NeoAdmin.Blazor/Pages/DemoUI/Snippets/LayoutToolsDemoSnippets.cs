namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class LayoutToolsDemoSnippets
{
    public const string Separator = """
        <div>
            <p class="text-sm">区块 A</p>
            <Separator Class="my-4" />
            <p class="text-sm">区块 B</p>
        </div>

        <div class="flex h-8 items-center gap-2">
            <span class="text-sm">左</span>
            <Separator Orientation="SeparatorOrientation.Vertical" Class="h-4" />
            <span class="text-sm">右</span>
        </div>
        """;

    public const string Resizable = """
        <ResizablePanelGroup Direction="ResizableDirection.Horizontal"
                             Class="h-48 max-w-2xl rounded-lg border">
            <ResizablePanel DefaultSize="35" MinSize="20">
                <div class="flex h-full items-center justify-center p-4 text-sm">侧栏</div>
            </ResizablePanel>
            <ResizableHandle WithHandle="true" />
            <ResizablePanel DefaultSize="65" MinSize="30">
                <div class="flex h-full items-center justify-center p-4 text-sm">主内容</div>
            </ResizablePanel>
        </ResizablePanelGroup>
        """;

    public const string Sortable = """
        <Sortable TItem="DemoSortItem"
                  Items="@sortItems"
                  GetItemId="@(item => item.Id)"
                  OnItemsReordered="OnSortItemsReordered">
            <SortableContent Class="max-w-md space-y-2">
                @foreach (DemoSortItem item in sortItems)
                {
                    <SortableItem Value="@item.Id" Class="rounded-lg border bg-card p-3">
                        <div class="flex items-center gap-2">
                            <SortableItemHandle />
                            <span>@item.Label</span>
                        </div>
                    </SortableItem>
                }
            </SortableContent>
        </Sortable>
        """;

    public const string ThemeSwitcher = """
        <ThemeSwitcher Strategy="PositioningStrategy.Fixed"
                       ShowStyles="true"
                       TriggerClass="size-9"
                       PopoverContentClass="!w-10 !h-16" />
        """;

    public const string Localization = """
        @inject ILocalizer Localizer

        <p>@Localizer["Combobox.Placeholder"]</p>
        <p>@Localizer["DataTable.Loading"]</p>
        <p>@Localizer["Sortable.DragHandle"]</p>
        """;
}
