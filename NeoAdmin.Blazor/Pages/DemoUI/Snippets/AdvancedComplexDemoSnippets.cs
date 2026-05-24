namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class AdvancedComplexDemoSnippets
{
    public const string DynamicForm = """
        <DynamicForm Schema="@contactSchema"
                     @bind-Values="contactValues"
                     SubmitText="提交"
                     OnValidSubmit="OnContactSubmit"
                     ShowValidationSummary="true"
                     Columns="1" />
        """;

    public const string FileUpload = """
        <FileUpload @bind-Files="uploadedFiles"
                    Multiple="true"
                    Accept="image/*,.pdf"
                    MaxFileSize="5242880"
                    MaxFiles="5"
                    ShowPreview="true" />
        """;

    public const string FilterBuilder = """
        <FilterBuilder TData="DemoProduct"
                       @bind-Filters="productFilters"
                       OnFilterChange="ApplyProductFilter">
            <FilterField Field="@nameof(DemoProduct.Name)" Label="名称" Type="FilterFieldType.Text" />
            <FilterField Field="@nameof(DemoProduct.Category)" Label="分类" Type="FilterFieldType.Select"
                         Options="@categoryFilterOptions" />
            <FilterField Field="@nameof(DemoProduct.Price)" Label="价格" Type="FilterFieldType.Number" />
            <FilterField Field="@nameof(DemoProduct.InStock)" Label="有货" Type="FilterFieldType.Boolean" />
        </FilterBuilder>
        """;

    public const string MarkdownEditor = """
        <MarkdownEditor @bind-Value="markdownNotes"
                        Placeholder="使用 Markdown 编写内容…"
                        Class="max-w-2xl" />
        """;

    public const string RichTextEditor = """
        <RichTextEditor @bind-Value="richTextHtml"
                        Toolbar="ToolbarPreset.Standard"
                        Placeholder="输入富文本…"
                        MinHeight="200px"
                        Class="max-w-2xl" />
        """;

    public const string RangeSlider = """
        <RangeSlider @bind-MinValue="priceMin"
                     @bind-MaxValue="priceMax"
                     Min="0"
                     Max="1000"
                     Step="10"
                     ShowTooltips="true"
                     Class="max-w-md" />
        """;
}
