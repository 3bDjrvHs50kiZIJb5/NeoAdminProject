namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class DisplayBasicsDemoSnippets
{
    public const string AspectRatio = """
        <AspectRatio Ratio="16/9" Class="max-w-md overflow-hidden rounded-lg bg-muted">
            <img src="https://picsum.photos/seed/neoadmin/640/360"
                 alt="示例图片"
                 class="h-full w-full object-cover" />
        </AspectRatio>

        <AspectRatio Ratio="1" Class="w-32 overflow-hidden rounded-full bg-muted">
            <div class="flex h-full w-full items-center justify-center text-sm text-muted-foreground">1:1</div>
        </AspectRatio>
        """;

    public const string Avatar = """
        <Avatar Size="AvatarSize.Default">
            <AvatarImage Source="https://picsum.photos/seed/user1/80" Alt="用户头像" />
            <AvatarFallback>NA</AvatarFallback>
        </Avatar>

        <Avatar Size="AvatarSize.Large">
            <AvatarFallback>
                <LucideIcon Name="user" Size="20" />
            </AvatarFallback>
        </Avatar>

        <div class="flex -space-x-2">
            <Avatar Class="border-2 border-background"><AvatarFallback>A</AvatarFallback></Avatar>
            <Avatar Class="border-2 border-background"><AvatarFallback>B</AvatarFallback></Avatar>
        </div>
        """;

    public const string Badge = """
        <Badge Variant="BadgeVariant.Default">默认</Badge>
        <Badge Variant="BadgeVariant.Secondary">次要</Badge>
        <Badge Variant="BadgeVariant.Destructive">3</Badge>
        <Badge Variant="BadgeVariant.Outline">轮廓</Badge>

        <Badge>
            <BadgeIcon><LucideIcon Name="check" Size="12" /></BadgeIcon>
            已完成
        </Badge>
        """;

    public const string Card = """
        <Card Class="max-w-sm">
            <CardHeader>
                <CardTitle>卡片标题</CardTitle>
                <CardDescription>简短说明文字</CardDescription>
            </CardHeader>
            <CardContent>
                <p class="text-sm text-muted-foreground">卡片主体内容区域。</p>
            </CardContent>
            <CardFooter class="justify-end gap-2">
                <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Small">取消</Button>
                <Button Size="ButtonSize.Small">确定</Button>
            </CardFooter>
        </Card>
        """;

    public const string Typography = """
        <Typography Variant="TypographyVariant.H1">一级标题</Typography>
        <Typography Variant="TypographyVariant.H2">二级标题</Typography>
        <Typography Variant="TypographyVariant.P">正文段落文本。</Typography>
        <Typography Variant="TypographyVariant.Muted">辅助说明文字</Typography>
        <Typography Variant="TypographyVariant.Blockquote">引用块样式</Typography>
        <Typography Variant="TypographyVariant.InlineCode">inline code</Typography>
        """;
}
