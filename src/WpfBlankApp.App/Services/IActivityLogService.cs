using System.Collections.ObjectModel;
using WpfBlankApp.App.Models;

namespace WpfBlankApp.App.Services;

public interface IActivityLogService
{
    ObservableCollection<ActivityLogEntry> Entries { get; }

    void Add(string category, string message, string detail = "", string severity = "Information");

    void Clear();

    Task<string> ExportAsync();
}
