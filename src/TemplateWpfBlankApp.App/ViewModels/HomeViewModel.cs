using System.Collections.ObjectModel;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class HomeViewModel : PageViewModelBase
{
    public HomeViewModel(AppProfile profile, IActivityLogService activityLogService, IConnectionService connectionService)
        : base("Home", "Overview of profiles, connection health, and recent activity.")
    {
        Profile = profile;
        RecentActivities = activityLogService.Entries;
        Connections = connectionService.Connections;
    }

    public AppProfile Profile { get; }

    public ObservableCollection<ActivityLogEntry> RecentActivities { get; }

    public ObservableCollection<ConnectionDefinition> Connections { get; }
}
