namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class AdvancedInputsDemoSnippets
{
    public const string Combobox = """
        <Combobox TItem="FrameworkOption"
                  Items="frameworks"
                  @bind-Value="selectedFramework"
                  ValueSelector="@(f => f.Value)"
                  DisplaySelector="@(f => f.Label)"
                  Placeholder="选择框架…"
                  SearchPlaceholder="搜索框架…"
                  Class="w-full max-w-sm" />
        """;

    public const string MultiSelect = """
        <MultiSelect TItem="FrameworkOption"
                     Items="frameworks"
                     Values="@selectedFrameworks"
                     ValuesChanged="@OnFrameworksChanged"
                     ValueSelector="@(f => f.Value)"
                     DisplaySelector="@(f => f.Label)"
                     Placeholder="选择多个框架…"
                     ShowSelectAll="true"
                     Class="w-full max-w-sm" />
        """;

    public const string TagInput = """
        <TagInput Tags="@skillTags"
                  TagsChanged="@OnSkillTagsChanged"
                  Placeholder="输入技能后按 Enter"
                  AddTrigger="TagInputTrigger.Enter | TagInputTrigger.Comma"
                  Suggestions="@skillSuggestions"
                  Clearable="true"
                  Class="max-w-lg" />
        """;

    public const string ColorPicker = """
        <ColorPicker @bind-Color="themeColor"
                     Format="ColorFormat.Hex"
                     ShowPresets="true"
                     ShowInputs="true" />
        """;

    public const string CurrencyInput = """
        <CurrencyInput TValue="decimal"
                       @bind-Value="currencyAmount"
                       Currency="CNY"
                       Class="w-full max-w-xs" />
        """;

    public const string MaskedInput = """
        <MaskedInput @bind-Value="phoneNumber"
                     Mask="(000) 000-0000"
                     Placeholder="(000) 000-0000"
                     Class="max-w-xs" />
        """;

    public const string InputOtp = """
        <InputOtp @bind-Value="otpValue" Length="6">
            <InputOtpGroup>
                @for (var i = 0; i < 6; i++)
                {
                    <InputOtpSlot Index="@i" />
                }
            </InputOtpGroup>
        </InputOtp>
        """;

    public const string InputGroup = """
        <InputGroup>
            <InputGroupAddon Align="InputGroupAlign.InlineStart">
                <LucideIcon Name="search" Size="16" Class="text-muted-foreground" />
            </InputGroupAddon>
            <InputGroupInput Placeholder="搜索…" @bind-Value="searchText" />
        </InputGroup>

        <InputGroup>
            <InputGroupAddon Align="InputGroupAlign.InlineStart">
                <span class="text-sm text-muted-foreground">https://</span>
            </InputGroupAddon>
            <InputGroupInput Placeholder="example.com" @bind-Value="domainText" />
            <InputGroupAddon Align="InputGroupAlign.InlineEnd">
                <InputGroupButton>访问</InputGroupButton>
            </InputGroupAddon>
        </InputGroup>
        """;
}
