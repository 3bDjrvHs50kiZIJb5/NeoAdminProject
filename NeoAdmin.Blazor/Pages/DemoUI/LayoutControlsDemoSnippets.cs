namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class LayoutControlsDemoSnippets
{
    public const string Button = """
        <Button>Default</Button>
        <Button Variant="ButtonVariant.Destructive">Destructive</Button>
        <Button Variant="ButtonVariant.Outline">Outline</Button>
        <Button Variant="ButtonVariant.Secondary">Secondary</Button>
        <Button Variant="ButtonVariant.Ghost">Ghost</Button>
        <Button Variant="ButtonVariant.Link">Link</Button>

        <Button Size="ButtonSize.Small">Small</Button>
        <Button Size="ButtonSize.Large">Large</Button>
        """;

    public const string ButtonGroup = """
        <ButtonGroup AriaLabel="文本格式">
            <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Small">加粗</Button>
            <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Small">斜体</Button>
            <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Small">下划线</Button>
        </ButtonGroup>
        """;

    public const string SplitButton = """
        <SplitButton OnClick="OnSplitSave" Variant="ButtonVariant.Default">
            保存
            <DropdownContent>
                <SplitButtonItem OnClick="OnSplitSaveAs">另存为…</SplitButtonItem>
                <SplitButtonSeparator />
                <SplitButtonItem OnClick="OnSplitDiscard">放弃更改</SplitButtonItem>
            </DropdownContent>
        </SplitButton>
        """;

    public const string Tabs = """
        <Tabs DefaultValue="overview" Class="max-w-lg">
            <TabsList>
                <TabsTrigger Value="overview">概览</TabsTrigger>
                <TabsTrigger Value="settings">设置</TabsTrigger>
            </TabsList>
            <TabsContent Value="overview">概览内容</TabsContent>
            <TabsContent Value="settings">设置内容</TabsContent>
        </Tabs>
        """;

    public const string Accordion = """
        <Accordion Type="AccordionType.Single" Collapsible="true" Class="max-w-lg">
            <AccordionItem Value="faq-1">
                <AccordionTrigger>是否支持无障碍？</AccordionTrigger>
                <AccordionContent>是的，组件遵循 WCAG 与 ARIA 规范。</AccordionContent>
            </AccordionItem>
            <AccordionItem Value="faq-2">
                <AccordionTrigger>可以自定义样式吗？</AccordionTrigger>
                <AccordionContent>可通过 Tailwind Class 覆盖默认样式。</AccordionContent>
            </AccordionItem>
        </Accordion>
        """;

    public const string Collapsible = """
        <Collapsible @bind-Open="collapsibleOpen" Class="max-w-lg">
            <CollapsibleTrigger>
                <Button Variant="ButtonVariant.Outline" Size="ButtonSize.Small">
                    @(collapsibleOpen ? "收起详情" : "展开详情")
                </Button>
            </CollapsibleTrigger>
            <CollapsibleContent>
                <p class="pt-2 text-sm text-muted-foreground">可折叠区域的补充说明内容。</p>
            </CollapsibleContent>
        </Collapsible>
        """;
}
