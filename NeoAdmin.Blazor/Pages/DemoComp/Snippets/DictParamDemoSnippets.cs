namespace NeoAdmin.Blazor.Pages.DemoComp;

internal static class DictParamDemoSnippets
{
    public const string Dict = """
        <NeoSelectDict ParentName="Gender"
                       @bind-Value="selectedGender"
                       OnValueChanged="OnGenderChanged" />

        <p class="text-sm text-muted-foreground">
            当前：@(string.IsNullOrWhiteSpace(selectedGender) ? "未选择" : selectedGender)
        </p>
        """;

    public const string Param = """
        <NeoParamText ParamKey="Home_ContactCard"
                      Field="@nameof(SysParam.Value)"
                      DefaultText="未配置"
                      TagName="div" />

        <NeoParamText ParamKey="Home_ContactCard"
                      Field="@nameof(SysParam.Value2)"
                      DefaultText="未配置，请到 /admin/param 新增 Home_ContactCard 参数。"
                      TagName="div" />

        <NeoParamText ParamKey="Home_ContactCard"
                      Field="@nameof(SysParam.Value3)"
                      DefaultText="未配置"
                      TagName="div"
                      Class="font-semibold" />
        """;
}
