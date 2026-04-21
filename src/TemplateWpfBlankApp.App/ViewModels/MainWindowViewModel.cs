using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly SavedProfilesPageViewModel _savedProfilesPageViewModel;
    private readonly SettingsPageViewModel _settingsPageViewModel;
    private PageViewModelBase _currentPage;
    private NavigationItem? _selectedNavigationItem;
    private string _statusMessage = "Template initialized.";

    public MainWindowViewModel(
        AppProfile profile,
        IActivityLogService activityLogService,
        IProfileStore profileStore,
        IConnectionService connectionService,
        WorkspaceService workspaceService)
    {
        Profile = profile;
        _activityLogService = activityLogService;
        Profile.PropertyChanged += ProfileOnPropertyChanged;

        var homePageViewModel = new HomeViewModel(profile, activityLogService, connectionService);
        var dataViewPageViewModel = new DataViewPageViewModel(workspaceService);
        var editPageViewModel = new EditPageViewModel(workspaceService);
        var bulkUpdatePageViewModel = new BulkUpdatePageViewModel(workspaceService);
        var connectionsPageViewModel = new ConnectionsPageViewModel(connectionService, activityLogService);
        _settingsPageViewModel = new SettingsPageViewModel(profile, profileStore, connectionService, activityLogService);
        _savedProfilesPageViewModel = new SavedProfilesPageViewModel(profile, profileStore, connectionService, activityLogService);
        var logsPageViewModel = new LogsPageViewModel(activityLogService);
        var aboutPageViewModel = new AboutPageViewModel();

        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new("Home", "⌂", homePageViewModel),
            new("Data View", "☰", dataViewPageViewModel),
            new("Edit", "✎", editPageViewModel),
            new("Bulk Update", "⇪", bulkUpdatePageViewModel),
            new("Connections", "⇄", connectionsPageViewModel),
            new("Settings", "⚙", _settingsPageViewModel),
            new("Saved Profiles", "★", _savedProfilesPageViewModel),
            new("Logs", "☷", logsPageViewModel),
            new("About", "ⓘ", aboutPageViewModel),
        };

        _currentPage = homePageViewModel;
        SelectedNavigationItem = NavigationItems.First();
        _activityLogService.Entries.CollectionChanged += ActivityEntriesOnCollectionChanged;
    }

    public AppProfile Profile { get; }

    public string WindowTitle => $"Internal Tool Shell - {Profile.EnvironmentName}";

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value) && value is not null)
            {
                CurrentPage = value.ViewModel;
            }
        }
    }

    public PageViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public async Task InitializeAsync()
    {
        await _settingsPageViewModel.EnsureSavedProfileExistsAsync();
        await _savedProfilesPageViewModel.RefreshProfilesAsync();
        _activityLogService.Add("System", "Initialized the internal tool shell template.");
        SelectPage(Profile.DefaultPage);
        OnPropertyChanged(nameof(WindowTitle));
    }

    private void ActivityEntriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var latest = _activityLogService.Entries.FirstOrDefault();
        if (latest is not null)
        {
            StatusMessage = $"{latest.Timestamp:t} · {latest.Category}: {latest.Message}";
        }
    }

    private void ProfileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppProfile.EnvironmentName))
        {
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    private void SelectPage(string pageTitle)
    {
        var item = NavigationItems.FirstOrDefault(nav => string.Equals(nav.Title, pageTitle, StringComparison.OrdinalIgnoreCase));
        if (item is not null)
        {
            SelectedNavigationItem = item;
        }
    }
}
