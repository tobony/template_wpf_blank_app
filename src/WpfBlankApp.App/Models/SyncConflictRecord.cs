namespace WpfBlankApp.App.Models;

public sealed class SyncConflictRecord
{
    public int WorkItemId { get; init; }

    public string ItemTitle { get; init; } = string.Empty;

    public string LocalValue { get; init; } = string.Empty;

    public string RemoteValue { get; init; } = string.Empty;

    public string ResolutionHint { get; init; } = string.Empty;

    public bool CanRetry { get; init; }
}
