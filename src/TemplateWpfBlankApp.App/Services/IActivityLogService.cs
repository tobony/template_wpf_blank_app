using System.Collections.ObjectModel;
using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public interface IActivityLogService
{
    ObservableCollection<ActivityLogEntry> Entries { get; }

    void Add(string category, string message, string detail = "");

    void Clear();

    Task<string> ExportAsync();
}
