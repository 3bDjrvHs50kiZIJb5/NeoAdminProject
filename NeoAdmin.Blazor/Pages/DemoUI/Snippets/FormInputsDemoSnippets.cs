namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class FormInputsDemoSnippets
{
    public const string Field = """
        <Field Orientation="FieldOrientation.Vertical">
            <FieldLabel For="field-email">邮箱</FieldLabel>
            <FieldContent>
                <Input Id="field-email"
                       Type="InputType.Email"
                       Placeholder="name@example.com"
                       @bind-Value="fieldEmail" />
                <FieldDescription>我们不会分享你的邮箱地址。</FieldDescription>
            </FieldContent>
        </Field>

        <Field Orientation="FieldOrientation.Horizontal" Class="items-center">
            <FieldLabel For="field-username" Class="min-w-24">用户名</FieldLabel>
            <FieldContent>
                <Input Id="field-username" Placeholder="请输入用户名" @bind-Value="fieldUsername" />
            </FieldContent>
        </Field>
        """;

    public const string Input = """
        <Label For="input-text">文本</Label>
        <Input Id="input-text" Placeholder="请输入文本" @bind-Value="inputText" />

        <Input Id="input-email" Type="InputType.Email" Placeholder="name@example.com" @bind-Value="inputEmail" />

        <Input Id="input-password" Type="InputType.Password" Placeholder="••••••••" @bind-Value="inputPassword" />

        <Input Id="input-search" Type="InputType.Search" Placeholder="搜索…" @bind-Value="inputSearch" />
        """;

    public const string Label = """
        <Label For="label-name">姓名</Label>
        <Input Id="label-name" @bind-Value="labelName" Placeholder="请输入姓名" />

        <Label For="label-bio" Class="text-muted-foreground">个人简介</Label>
        <Textarea Id="label-bio" Class="min-h-20" @bind-Value="labelBio" Placeholder="简短介绍自己" />
        """;

    public const string Textarea = """
        <Label For="textarea-notes">备注</Label>
        <Textarea Id="textarea-notes"
                  Class="min-h-28 w-full"
                  Placeholder="输入多行文本…"
                  @bind-Value="textareaNotes" />
        """;

    public const string NumericInput = """
        <NumericInput TValue="int"
                      Id="numeric-qty"
                      Class="w-full"
                      Min="1" Max="99" Step="1"
                      @bind-Value="numericQuantity" />

        <NumericInput TValue="decimal"
                      Id="numeric-price"
                      Class="w-full"
                      Min="0" Max="9999" Step="0.01m"
                      @bind-Value="numericPrice" />
        """;
}
