namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class DataDisplayDemoSnippets
{
    public const string DataTable = """
        <DataTable TData="DemoProduct" Data="@products" ShowToolbar="true" InitialPageSize="5">
            <Columns>
                <DataTableColumn TData="DemoProduct" TValue="string"
                                 Property="@(p => p.Name)" Header="名称" Sortable="true" />
                <DataTableColumn TData="DemoProduct" TValue="string"
                                 Property="@(p => p.Category)" Header="分类" Sortable="true" />
                <DataTableColumn TData="DemoProduct" TValue="decimal"
                                 Property="@(p => p.Price)" Header="价格" Sortable="true" />
            </Columns>
        </DataTable>
        """;

    public const string DataGrid = """
        <DataGrid TItem="DemoProduct" Items="@products" Height="300px" PageSize="10" FillWidth="true">
            <DataGridColumn TItem="DemoProduct" Field="Name" Header="名称" Sortable="true" Flex="1" />
            <DataGridColumn TItem="DemoProduct" Field="Category" Header="分类" Sortable="true" />
            <DataGridColumn TItem="DemoProduct" Field="Price" Header="价格" Sortable="true" Width="100px" />
        </DataGrid>
        """;

    public const string DataView = """
        <DataView TItem="DemoProduct" Items="@products" PageSize="6" Layout="@dataViewLayout">
            <Fields>
                <DataViewColumn TItem="DemoProduct" Header="名称"
                                Property="@(p => p.Name)" Sortable="true" Filterable="true" />
            </Fields>
            <ListTemplate Context="product">
                <div class="rounded-lg border p-3">@product.Name — @product.Price.ToString("C0")</div>
            </ListTemplate>
            <GridTemplate Context="product">
                <Card><CardContent Class="p-3 text-sm">@product.Name</CardContent></Card>
            </GridTemplate>
        </DataView>
        """;

    public const string TreeView = """
        <TreeView TItem="DemoTreeNode"
                  Items="@treeNodes"
                  ValueField="n => n.Id"
                  TextField="n => n.Name"
                  ChildrenProperty="n => n.Children"
                  SelectionMode="TreeSelectionMode.Single"
                  @bind-SelectedValue="selectedTreeId"
                  DefaultExpandDepth="2"
                  ShowLines="true" />
        """;

    public const string Timeline = """
        <Timeline>
            <TimelineItem Title="项目启动" Time="2025-01-10"
                          Description="完成立项" Status="TimelineStatus.Completed" />
            <TimelineItem Title="开发中" Time="2025-02-15"
                          Description="核心功能开发" Status="TimelineStatus.InProgress" />
            <TimelineItem Title="上线计划" Time="2025-04-01"
                          Description="待发布" Status="TimelineStatus.Pending" />
        </Timeline>
        """;
}
