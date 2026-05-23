namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class OverlaysFloatingDemoSnippets
{
    public const string DropdownMenu = """
        <DropdownMenu>
            <DropdownMenuTrigger AsChild>
                <Button Variant="ButtonVariant.Outline">打开菜单</Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent Class="w-56">
                <DropdownMenuLabel>我的账户</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuItem>个人资料</DropdownMenuItem>
                    <DropdownMenuItem>账单</DropdownMenuItem>
                    <DropdownMenuItem>设置</DropdownMenuItem>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuItem>
                    退出登录
                    <DropdownMenuShortcut>⌘Q</DropdownMenuShortcut>
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
        """;

    public const string ContextMenu = """
        <ContextMenu>
            <ContextMenuTrigger>
                <div class="flex h-32 w-full max-w-md items-center justify-center rounded-md border border-dashed text-sm text-muted-foreground">
                    在此区域右键
                </div>
            </ContextMenuTrigger>
            <ContextMenuContent Class="w-48">
                <ContextMenuItem>复制</ContextMenuItem>
                <ContextMenuItem>粘贴</ContextMenuItem>
                <ContextMenuSeparator />
                <ContextMenuItem>刷新</ContextMenuItem>
            </ContextMenuContent>
        </ContextMenu>
        """;

    public const string Popover = """
        <Popover>
            <PopoverTrigger AsChild>
                <Button Variant="ButtonVariant.Outline">打开 Popover</Button>
            </PopoverTrigger>
            <PopoverContent Class="w-80">
                <div class="grid gap-2">
                    <h4 class="font-medium leading-none">尺寸</h4>
                    <p class="text-sm text-muted-foreground">为当前行设置列宽。</p>
                </div>
            </PopoverContent>
        </Popover>
        """;

    public const string Tooltip = """
        <TooltipProvider DelayDuration="300">
            <Tooltip>
                <TooltipTrigger AsChild>
                    <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Icon">
                        <LucideIcon Name="info" Size="16" />
                    </Button>
                </TooltipTrigger>
                <TooltipContent>
                    <p>这是一条简短提示</p>
                </TooltipContent>
            </Tooltip>
        </TooltipProvider>
        """;

    public const string HoverCard = """
        <HoverCard OpenDelay="400" CloseDelay="200">
            <HoverCardTrigger AsChild>
                <Button Variant="ButtonVariant.Link" Class="px-0">@hoverCardUser</Button>
            </HoverCardTrigger>
            <HoverCardContent Class="w-80">
                <div class="flex gap-4">
                    <Avatar>
                        <AvatarFallback>NA</AvatarFallback>
                    </Avatar>
                    <div class="space-y-1">
                        <h4 class="text-sm font-semibold">@hoverCardUser</h4>
                        <p class="text-sm text-muted-foreground">NeoUI 组件库贡献者</p>
                    </div>
                </div>
            </HoverCardContent>
        </HoverCard>
        """;
}
