namespace TemplateWpfBlankApp.App.Models;

public sealed class ActivityLogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;

    public string Category { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}
