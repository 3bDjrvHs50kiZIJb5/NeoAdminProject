namespace NeoAdmin.Blazor.Pages.DemoComp;

internal static class NeoSelectComponentsDemoSnippets
{
    public const string SelectDict = """
        <NeoSelectDict ParentName="Gender" @bind-Value="genderValue" />

        <p class="text-sm text-muted-foreground">
            当前：@(string.IsNullOrWhiteSpace(genderValue) ? "未选择" : genderValue)
        </p>
        """;

    public const string SelectEnum = """
        <NeoSelectEnum TEnum="SysMenuType" @bind-Value="menuTypeValue" />

        <p class="text-sm text-muted-foreground">当前：@menuTypeValue</p>
        """;

    public const string SelectEntity = """
        <NeoSelectEntity TItem="SysOrg"
                         TKey="long"
                         @bind-Value="orgId"
                         DisplayText="o => o.Label" />

        <p class="text-sm text-muted-foreground">组织 Id：@orgId</p>
        """;

    public const string InputTable = """
        <NeoInputTable TItem="SysOrg"
                       TKey="long"
                       @bind-Value="orgId2"
                       DisplayText="o => o.Label"
                       OnQuery="FilterOrgQuery">
            <TableCell Context="org">@org.Label</TableCell>
        </NeoInputTable>
        """;

    public const string SelectTable = """
        <NeoSelectTable TItem="SysRole"
                        TKey="long"
                        Items="selectedRoles"
                        ItemsChanged="OnRolesChanged"
                        OnQuery="FilterRoleQuery">
            <RowTemplate Context="role">@role.Name</RowTemplate>
        </NeoSelectTable>

        <p class="text-sm text-muted-foreground">
            已选：@string.Join("、", selectedRoles.Select(r => r.Name))
        </p>
        """;

    public const string InputTags = """
        <NeoInputTags TItem="string"
                      Value="tags"
                      ValueChanged="OnTagsChanged"
                      DisplayText="s => s"
                      OnCreate="s => s"
                      OnSearch="SearchTagsAsync" />
        """;

    public const string AllocTable = """
        <Button OnClick="OpenRoleAlloc">打开「分配用户」示例</Button>

        <NeoAllocTable TItem="SysRole"
                       TChild="SysUser"
                       TKey="long"
                       @bind-Item="allocRole"
                       Title="@($"分配用户 — {allocRole?.Name}")"
                       KeySelector="u => u.Id"
                       DisplayText="u => u.Username"
                       LoadSelectedKeysAsync="LoadRoleUserKeysAsync"
                       OnSaveAsync="SaveRoleUsersAsync"
                       OnQuery="FilterUserAllocQuery" />
        """;
}
