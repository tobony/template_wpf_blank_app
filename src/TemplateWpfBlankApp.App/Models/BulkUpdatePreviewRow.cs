namespace TemplateWpfBlankApp.App.Models;

public sealed class BulkUpdatePreviewRow
{
    public int RowNumber { get; init; }

    public string ItemTitle { get; init; } = string.Empty;

    public string ProposedAction { get; init; } = string.Empty;

    public string ValidationStatus { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;
}
