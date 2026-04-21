using System.Collections.ObjectModel;
using System.Text;
using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public sealed class ActivityLogService : IActivityLogService
{
    public ObservableCollection<ActivityLogEntry> Entries { get; } = new();

    public void Add(string category, string message, string detail = "")
    {
        Entries.Insert(0, new ActivityLogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Category = category,
            Message = message,
            Detail = detail,
        });
    }

    public void Clear()
    {
        Entries.Clear();
        Add("System", "Activity log cleared.");
    }

    public async Task<string> ExportAsync()
    {
        var exportFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TemplateWpfBlankApp",
            "exports");
        Directory.CreateDirectory(exportFolder);

        var path = Path.Combine(exportFolder, $"activity-log-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.txt");
        var builder = new StringBuilder();
        foreach (var entry in Entries)
        {
            builder.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Category} - {entry.Message}");
            if (!string.IsNullOrWhiteSpace(entry.Detail))
            {
                builder.AppendLine($"  {entry.Detail}");
            }
        }

        await File.WriteAllTextAsync(path, builder.ToString());
        Add("Logs", "Exported activity log.", path);
        return path;
    }
}
