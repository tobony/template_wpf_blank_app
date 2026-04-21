using System.Collections.ObjectModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class LogsPageViewModel : PageViewModelBase
{
    private readonly IActivityLogService _activityLogService;

    public LogsPageViewModel(IActivityLogService activityLogService)
        : base("Logs / History", "Review audit-friendly activity history and export it for support.")
    {
        _activityLogService = activityLogService;
        ClearCommand = new RelayCommand(_activityLogService.Clear);
        ExportCommand = new AsyncRelayCommand(_activityLogService.ExportAsync);
    }

    public ObservableCollection<ActivityLogEntry> Entries => _activityLogService.Entries;

    public ICommand ClearCommand { get; }

    public ICommand ExportCommand { get; }
}
