namespace WpfBlankApp.App.Models;

public sealed class BulkUpdatePreviewRow
{
    public int RowNumber { get; init; }

    public string ItemTitle { get; init; } = string.Empty;

    public string ProposedAction { get; init; } = string.Empty;

    public string OriginalValue { get; init; } = string.Empty;

    public string UpdatedValue { get; init; } = string.Empty;

    public string ValidationStatus { get; init; } = string.Empty;

    public bool CanRetry { get; init; }

    public string Notes { get; init; } = string.Empty;
}
