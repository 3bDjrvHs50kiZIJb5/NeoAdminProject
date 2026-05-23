namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class NavigationDemoSnippets
{
    public const string Breadcrumb = """
        <Breadcrumb>
            <BreadcrumbList>
                <BreadcrumbItem>
                    <BreadcrumbLink Href="/">首页</BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator>
                    <LucideIcon Name="chevron-right" Size="14" />
                </BreadcrumbSeparator>
                <BreadcrumbItem>
                    <BreadcrumbLink Href="/neo-demo">演示</BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator>/</BreadcrumbSeparator>
                <BreadcrumbItem>
                    <BreadcrumbPage>导航组件</BreadcrumbPage>
                </BreadcrumbItem>
            </BreadcrumbList>
        </Breadcrumb>
        """;

    public const string Command = """
        <Dialog @bind-Open="commandDialogOpen">
            <Button Variant="ButtonVariant.Outline" OnClick="@OpenCommandDialog">
                打开命令面板
            </Button>
            <DialogContent Class="overflow-hidden p-0 sm:max-w-lg">
                <CommandContent>
                    <CommandInput Placeholder="搜索命令…" />
                    <CommandList>
                        <CommandEmpty>未找到命令。</CommandEmpty>
                        <CommandGroup Heading="导航">
                            <CommandItem Value="dashboard" OnSelect="@(() => RunCommand("仪表盘"))">
                                仪表盘
                            </CommandItem>
                        </CommandGroup>
                    </CommandList>
                </CommandContent>
            </DialogContent>
        </Dialog>
        """;

    public const string Menubar = """
        <Menubar>
            <MenubarMenu>
                <MenubarTrigger>文件</MenubarTrigger>
                <MenubarContent>
                    <MenubarItem OnClick="@(() => menubarStatus = "新建")">新建</MenubarItem>
                    <MenubarSeparator />
                    <MenubarSub>
                        <MenubarSubTrigger>导出</MenubarSubTrigger>
                        <MenubarSubContent>
                            <MenubarItem>PDF</MenubarItem>
                            <MenubarItem>CSV</MenubarItem>
                        </MenubarSubContent>
                    </MenubarSub>
                </MenubarContent>
            </MenubarMenu>
        </Menubar>
        """;

    public const string NavigationMenu = """
        <NavigationMenu @bind-Value="navMenuValue">
            <NavigationMenuList>
                <NavigationMenuItem Value="products">
                    <NavigationMenuTrigger>产品</NavigationMenuTrigger>
                    <NavigationMenuContent>
                        <ul class="grid gap-2 p-4 md:w-[400px]">
                            <li>
                                <NavigationMenuLink Href="#">分析</NavigationMenuLink>
                            </li>
                        </ul>
                    </NavigationMenuContent>
                </NavigationMenuItem>
            </NavigationMenuList>
            <NavigationMenuViewport />
        </NavigationMenu>
        """;

    public const string Pagination = """
        <Pagination State="@paginationState">
            <PaginationContent>
                <PaginationItem>
                    <PaginationPrevious />
                </PaginationItem>
                <PaginationItem>
                    <PaginationLink IsActive="true">1</PaginationLink>
                </PaginationItem>
                <PaginationItem>
                    <PaginationEllipsis />
                </PaginationItem>
                <PaginationItem>
                    <PaginationNext />
                </PaginationItem>
            </PaginationContent>
        </Pagination>
        """;

    public const string ResponsiveNav = """
        <div class="max-w-md rounded-lg border">
            <ResponsiveNavProvider>
                <div class="flex items-center justify-between px-3 py-2">
                    <span class="font-medium">站点</span>
                    <NavigationMenu Class="hidden sm:flex">
                        <NavigationMenuList>
                            <NavigationMenuItem>
                                <NavigationMenuLink Href="#">首页</NavigationMenuLink>
                            </NavigationMenuItem>
                        </NavigationMenuList>
                    </NavigationMenu>
                    <ResponsiveNavTrigger Class="sm:hidden" />
                </div>
                <ResponsiveNavContent Header="菜单">
                    <NavigationMenuLink Href="#">首页</NavigationMenuLink>
                </ResponsiveNavContent>
            </ResponsiveNavProvider>
        </div>
        """;

    public const string Sidebar = """
        <div class="h-64 overflow-hidden rounded-lg border">
            <SidebarProvider HeightClass="h-full" CookieKey="sidebar:nav-demo" StaticRendering="true">
                <div class="flex h-full">
                    <Sidebar Collapsible="true">
                        <SidebarContent>
                            <SidebarMenu>
                                <SidebarMenuItem>
                                    <SidebarMenuButton>概览</SidebarMenuButton>
                                </SidebarMenuItem>
                            </SidebarMenu>
                        </SidebarContent>
                    </Sidebar>
                    <main class="flex flex-1 items-center justify-center text-sm text-muted-foreground">
                        主内容
                    </main>
                </div>
            </SidebarProvider>
        </div>
        """;
}
