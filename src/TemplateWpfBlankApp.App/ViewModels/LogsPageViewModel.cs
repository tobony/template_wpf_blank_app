using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class LogsPageViewModel : PageViewModelBase
{
    private readonly AppProfile _profile;
    private readonly IActivityLogService _activityLogService;
    private string _selectedSeverity = "All";
    private ActivityLogEntry? _selectedEntry;

    public LogsPageViewModel(AppProfile profile, IActivityLogService activityLogService)
        : base("Logs / History", "Review timeline history, filter outcomes, and export support-ready details.")
    {
        _profile = profile;
        _activityLogService = activityLogService;
        _activityLogService.Entries.CollectionChanged += EntriesOnCollectionChanged;
        ClearCommand = new RelayCommand(_activityLogService.Clear, () => _profile.IsAdministrator);
        ExportCommand = new AsyncRelayCommand(_activityLogService.ExportAsync);
        RefreshFilteredEntries();
    }

    public ObservableCollection<ActivityLogEntry> FilteredEntries { get; } = new();

    public IReadOnlyList<string> SeverityOptions => ["All", "Information", "Warning", "Error"];

    public string SelectedSeverity
    {
        get => _selectedSeverity;
        set
        {
            if (SetProperty(ref _selectedSeverity, value))
            {
                RefreshFilteredEntries();
            }
        }
    }

    public ActivityLogEntry? SelectedEntry
    {
        get => _selectedEntry;
        set => SetProperty(ref _selectedEntry, value);
    }

    public bool IsAdministrator => _profile.IsAdministrator;

    public ICommand ClearCommand { get; }

    public ICommand ExportCommand { get; }

    private void EntriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshFilteredEntries();
    }

    private void RefreshFilteredEntries()
    {
        FilteredEntries.Clear();
        var filtered = string.Equals(SelectedSeverity, "All", StringComparison.OrdinalIgnoreCase)
            ? _activityLogService.Entries
            : new ObservableCollection<ActivityLogEntry>(_activityLogService.Entries.Where(entry => string.Equals(entry.Severity, SelectedSeverity, StringComparison.OrdinalIgnoreCase)));

        foreach (var entry in filtered)
        {
            FilteredEntries.Add(entry);
        }

        SelectedEntry ??= FilteredEntries.FirstOrDefault();
    }
}
