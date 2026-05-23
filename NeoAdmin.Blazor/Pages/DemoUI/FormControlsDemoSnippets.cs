namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class FormControlsDemoSnippets
{
    public const string Checkbox = """
        <div class="flex items-center gap-2">
            <Checkbox Id="terms" @bind-Checked="acceptTerms" />
            <Label For="terms" Class="cursor-pointer font-normal">接受条款和条件</Label>
        </div>

        <Checkbox Id="select-all"
                  Checked="@selectAllChecked"
                  Indeterminate="@selectAllIndeterminate"
                  CheckedChanged="OnSelectAllChanged" />
        <Label For="select-all">全选（半选状态）</Label>

        <Checkbox Id="notify-email"
                  Checked="@notificationSelections.Contains("邮件通知")"
                  CheckedChanged="@(v => OnNotificationChanged("邮件通知", v))" />
        """;

    public const string NativeSelect = """
        <NativeSelect TValue="string" Class="w-full" @bind-Value="nativeFruit" Placeholder="选择水果">
            <NativeSelectOption Value="apple">Apple</NativeSelectOption>
            <NativeSelectOption Value="banana">Banana</NativeSelectOption>
            <NativeSelectOption Value="orange">Orange</NativeSelectOption>
        </NativeSelect>

        <NativeSelect TValue="string" Size="NativeSelectSize.Small" Placeholder="Small">...</NativeSelect>
        <NativeSelect TValue="string" Size="NativeSelectSize.Default" Placeholder="Default">...</NativeSelect>
        <NativeSelect TValue="string" Size="NativeSelectSize.Large" Placeholder="Large">...</NativeSelect>
        """;

    public const string RadioGroup = """
        <RadioGroup TValue="string" @bind-Value="radioPlan" Class="gap-3">
            <div class="flex items-center gap-2">
                <RadioGroupItem Value="@("free")" Id="plan-free" />
                <Label For="plan-free">免费版</Label>
            </div>
            <div class="flex items-center gap-2">
                <RadioGroupItem Value="@("pro")" Id="plan-pro" />
                <Label For="plan-pro">专业版</Label>
            </div>
            <div class="flex items-center gap-2">
                <RadioGroupItem Value="@("enterprise")" Id="plan-enterprise" />
                <Label For="plan-enterprise">企业版</Label>
            </div>
        </RadioGroup>
        """;

    public const string Select = """
        <Select TValue="string" Class="w-full" @bind-Value="selectFruit">
            <SelectTrigger>
                <SelectValue Placeholder="选择水果" />
            </SelectTrigger>
            <SelectContent>
                <SelectItem Value="@("apple")" TValue="string">Apple</SelectItem>
                <SelectItem Value="@("banana")" TValue="string">Banana</SelectItem>
            </SelectContent>
        </Select>

        <Select TValue="string" @bind-Value="selectAnimal">
            <SelectTrigger><SelectValue Placeholder="选择动物" /></SelectTrigger>
            <SelectContent>
                <SelectGroup>
                    <SelectLabel>哺乳动物</SelectLabel>
                    <SelectItem Value="@("cat")" TValue="string">猫</SelectItem>
                </SelectGroup>
            </SelectContent>
        </Select>
        """;

    public const string Slider = """
        <Label>音量</Label>
        <Slider @bind-Value="sliderVolume" Min="0" Max="100" Step="1" />
        """;

    public const string Switch = """
        <Switch Id="switch-main" @bind-Checked="switchEnabled" />
        <Label For="switch-main">启用通知</Label>

        <Switch Size="SwitchSize.Small" @bind-Checked="switchSmall" />
        <Switch Size="SwitchSize.Medium" @bind-Checked="switchMedium" />
        <Switch Size="SwitchSize.Large" @bind-Checked="switchLarge" />
        """;

    public const string Rating = """
        <Rating @bind-Value="ratingValue" AllowHalf="true" MaxRating="5" />

        <Rating Value="4.5" ReadOnly="true" AllowHalf="true" MaxRating="5" />
        """;

    public const string Toggle = """
        <Toggle @bind-Pressed="toggleBold" Variant="ToggleVariant.Outline" aria-label="粗体">
            <LucideIcon Name="bold" Size="16" />
        </Toggle>
        <Toggle @bind-Pressed="toggleItalic" Variant="ToggleVariant.Outline" aria-label="斜体">
            <LucideIcon Name="italic" Size="16" />
        </Toggle>
        """;

    public const string ToggleGroup = """
        <ToggleGroup Type="ToggleGroupType.Single" @bind-Value="toggleAlign">
            <ToggleGroupItem Value="left">左</ToggleGroupItem>
            <ToggleGroupItem Value="center">中</ToggleGroupItem>
            <ToggleGroupItem Value="right">右</ToggleGroupItem>
        </ToggleGroup>

        <ToggleGroup Type="ToggleGroupType.Multiple" @bind-Values="toggleFilters">
            <ToggleGroupItem Value="design">设计</ToggleGroupItem>
            <ToggleGroupItem Value="frontend">前端</ToggleGroupItem>
            <ToggleGroupItem Value="backend">后端</ToggleGroupItem>
        </ToggleGroup>
        """;
}
