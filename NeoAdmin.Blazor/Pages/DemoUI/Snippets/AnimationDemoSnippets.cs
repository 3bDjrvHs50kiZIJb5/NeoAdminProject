namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class AnimationDemoSnippets
{
    public const string Carousel = """
        <Carousel Class="max-w-xl"
                  ShowIndicators="true"
                  ShowNavigation="true"
                  EnableDrag="true"
                  Loop="true"
                  OnSlideChange="OnCarouselSlideChanged">
            <CarouselItem>
                <div class="flex h-48 items-center justify-center rounded-lg bg-sky-500 text-2xl font-semibold text-white">
                    Slide 1
                </div>
            </CarouselItem>
            <CarouselItem>
                <div class="flex h-48 items-center justify-center rounded-lg bg-emerald-500 text-2xl font-semibold text-white">
                    Slide 2
                </div>
            </CarouselItem>
            <CarouselItem>
                <div class="flex h-48 items-center justify-center rounded-lg bg-violet-500 text-2xl font-semibold text-white">
                    Slide 3
                </div>
            </CarouselItem>
        </Carousel>
        """;

    public const string Motion = """
        <!-- 出现时淡入 -->
        <Motion Trigger="MotionTrigger.OnAppear" DisableOnPrerender="true">
            <FadeIn Duration="0.5" />
            <Card Class="max-w-sm">
                <CardHeader><CardTitle>淡入卡片</CardTitle></CardHeader>
                <CardContent>页面加载后自动淡入。</CardContent>
            </Card>
        </Motion>

        <!-- 手动触发：淡入 + 自左侧滑入 -->
        <Motion @ref="manualMotion" Trigger="MotionTrigger.Manual">
            <FadeIn Duration="0.35" />
            <SlideInFromLeft Duration="0.4" />
            <Button OnClick="PlayManualMotionAsync">播放动画</Button>
        </Motion>

        @code {
            private Motion? manualMotion;
            private async Task PlayManualMotionAsync() => await manualMotion!.PlayAsync();
        }
        """;

    public const string PageTransition = """
        <!-- App.razor / Routes.razor：在 Router 外包一层 -->
        <RenderStateProvider>
            <Router AppAssembly="..." AdditionalAssemblies="...">
                <Found Context="routeData">
                    <PageTransition Duration="0.25" EnableSlide="false">
                        <RouteView RouteData="@routeData" DefaultLayout="@typeof(LayoutAdmin)" />
                    </PageTransition>
                </Found>
            </Router>
        </RenderStateProvider>

        <!-- PageTransition 会读取 RenderStateProvider 提供的 RenderContext，
             在增强导航（SPA）时自动淡入淡出，SSR 预渲染阶段可安全跳过动画。 -->
        """;

    public const string ScreenTransition = """
        <!-- 切换前先设置 Direction，再改 Key -->
        <ScreenTransition Key="@screenKey" Direction="@screenDirection" Class="h-64 overflow-hidden rounded-lg border">
            @GetScreenContent()
        </ScreenTransition>

        <Button OnClick='() => GoScreen("home", ScreenTransitionDirection.Tab)'>Tab</Button>
        <Button OnClick='() => GoScreen("detail", ScreenTransitionDirection.Push)'>Push</Button>
        <Button OnClick='() => GoScreen("home", ScreenTransitionDirection.Pop)'>Pop</Button>

        @code {
            private string screenKey = "home";
            private ScreenTransitionDirection screenDirection = ScreenTransitionDirection.Tab;

            private void GoScreen(string key, ScreenTransitionDirection direction)
            {
                screenDirection = direction;
                screenKey = key;
            }
        }
        """;

    public const string SelectionIndicator = """
        <Tabs @bind-Value="activeTab" Class="max-w-md">
            <TabsList Class="relative inline-flex h-10 w-full rounded-lg bg-muted p-1">
                <SelectionIndicator Class="rounded-md bg-background shadow-sm" />
                <TabsTrigger Value="overview" Class="flex-1">概览</TabsTrigger>
                <TabsTrigger Value="analytics" Class="flex-1">分析</TabsTrigger>
                <TabsTrigger Value="settings" Class="flex-1">设置</TabsTrigger>
            </TabsList>
            <TabsContent Value="overview">...</TabsContent>
        </Tabs>
        """;
}
