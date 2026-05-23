namespace NeoAdmin.Blazor.Components;

public sealed class CrudFilterOption
{
    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool Selected { get; set; }

    public CrudFilterOption()
    {
    }

    public CrudFilterOption(string label, string value)
    {
        Label = label;
        Value = value;
    }
}
