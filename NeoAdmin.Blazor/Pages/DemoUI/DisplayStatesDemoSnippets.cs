namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class DisplayStatesDemoSnippets
{
    public const string Empty = """
        <Empty>
            <EmptyIcon>
                <LucideIcon Name="inbox" Size="40" Class="text-muted-foreground" />
            </EmptyIcon>
            <EmptyTitle>暂无数据</EmptyTitle>
            <EmptyDescription>当前列表为空，可点击下方按钮添加。</EmptyDescription>
            <EmptyActions>
                <Button Size="ButtonSize.Small">新建</Button>
            </EmptyActions>
        </Empty>
        """;

    public const string Skeleton = """
        <div class="flex items-center gap-4">
            <Skeleton Shape="SkeletonShape.Circular" Class="h-12 w-12" />
            <div class="space-y-2">
                <Skeleton Class="h-4 w-[200px]" />
                <Skeleton Class="h-4 w-[160px]" />
            </div>
        </div>

        <Skeleton Class="h-24 w-full max-w-md rounded-lg" />
        """;

    public const string Item = """
        <ItemGroup>
            <Item>
                <ItemMedia Variant="ItemMediaVariant.Icon">
                    <LucideIcon Name="mail" Size="18" />
                </ItemMedia>
                <ItemContent>
                    <ItemTitle>新消息</ItemTitle>
                    <ItemDescription>你有一条来自系统的通知</ItemDescription>
                </ItemContent>
                <ItemActions>
                    <Badge Variant="BadgeVariant.Secondary">未读</Badge>
                </ItemActions>
            </Item>
            <ItemSeparator />
            <Item Variant="ItemVariant.Muted">
                <ItemContent>
                    <ItemTitle>已完成任务</ItemTitle>
                    <ItemDescription>昨天 14:30</ItemDescription>
                </ItemContent>
            </Item>
        </ItemGroup>
        """;

    public const string Kbd = """
        <p class="text-sm text-muted-foreground">
            保存 <Kbd>Ctrl</Kbd> + <Kbd>S</Kbd>
        </p>
        <p class="text-sm text-muted-foreground">
            命令面板 <Kbd>⌘</Kbd> <Kbd>K</Kbd>
        </p>
        """;

    public const string ScrollArea = """
        <ScrollArea Class="h-48 w-full max-w-md rounded-md border">
            <div class="space-y-2 p-4">
                @for (int i = 1; i <= 20; i++)
                {
                    <p class="text-sm">滚动内容行 @i</p>
                }
            </div>
        </ScrollArea>
        """;
}
