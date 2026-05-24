namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class MobileDemoSnippets
{
    public const string AppBar = """
        <AppBar Title="详情" OnBack="@GoBack">
            <RightContent>
                <Button Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon" OnClick="@Share">
                    <LucideIcon Name="share-2" Size="20" />
                </Button>
            </RightContent>
        </AppBar>

        @code {
            private void GoBack() { /* 返回上一页 */ }
            private void Share() { /* 分享 */ }
        }
        """;

    public const string BottomNav = """
        <BottomNav @bind-ActiveTab="activeTab" Fixed="false">
            <BottomNavItem Value="home" Icon="house" Label="首页" />
            <BottomNavItem Value="search" Icon="search" Label="搜索" />
            <BottomNavItem Value="orders" Icon="clipboard-list" Label="订单" BadgeCount="3" />
            <BottomNavItem Value="account" Icon="user" Label="我的" />
        </BottomNav>

        @code {
            private string activeTab = "home";
        }
        """;

    public const string NotificationBadge = """
        <NotificationBadge Count="5">
            <Button Variant="ButtonVariant.Ghost" Size="ButtonSize.Icon">
                <LucideIcon Name="shopping-cart" Size="20" />
            </Button>
        </NotificationBadge>

        <NotificationBadge Count="1" Dot="true">
            <LucideIcon Name="bell" Size="24" />
        </NotificationBadge>

        <NotificationBadge Count="3" Variant="NotificationBadgeVariant.Primary">
            <Avatar>
                <AvatarFallback>NA</AvatarFallback>
            </Avatar>
        </NotificationBadge>
        """;

    public const string QuantityStepper = """
        <QuantityStepper @bind-Value="quantity"
                         Min="1"
                         Max="10"
                         DestructiveAtMin="true"
                         OnDestructiveClick="@RemoveFromCart" />

        @code {
            private int quantity = 1;

            private void RemoveFromCart()
            {
                // 从购物车移除
            }
        }
        """;

    public const string SectionHeader = """
        <SectionHeader Title="热门推荐"
                       ViewAllText="查看全部"
                       OnViewAll="@ViewAllPromo" />

        <SectionHeader Title="订单摘要" ShowSeparator="false" />

        @code {
            private void ViewAllPromo() { /* 跳转列表 */ }
        }
        """;
}
