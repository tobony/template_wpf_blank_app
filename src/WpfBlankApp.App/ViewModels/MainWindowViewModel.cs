using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.Models;
using WpfBlankApp.App.Services;

namespace WpfBlankApp.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private static readonly IReadOnlyList<(string GroupTitle, string[] PageTitles)> NavigationGroupDefinitions =
    [
        ("Overview", ["Home"]),
        ("Workspace", ["Data View", "Edit", "Bulk Update", "Sync / Refresh"]),
        ("Administration", ["Connection Manager", "Settings", "Saved Profiles", "Logs / History"]),
        ("Help", ["About"]),
    ];

    private readonly IActivityLogService _activityLogService;
    private readonly SavedProfilesPageViewModel _savedProfilesPageViewModel;
    private readonly SettingsPageViewModel _settingsPageViewModel;
    private readonly List<NavigationItem> _allNavigationItems = [];
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

        var homePageViewModel = new HomeViewModel(profile, activityLogService, connectionService, workspaceService, SelectPage);
        var dataViewPageViewModel = new DataViewPageViewModel(profile, workspaceService, SelectPage);
        var editPageViewModel = new EditPageViewModel(workspaceService);
        var bulkUpdatePageViewModel = new BulkUpdatePageViewModel(profile, workspaceService);
        var syncRefreshPageViewModel = new SyncRefreshPageViewModel(workspaceService);
        var connectionsPageViewModel = new ConnectionsPageViewModel(profile, connectionService, activityLogService);
        _settingsPageViewModel = new SettingsPageViewModel(profile, profileStore, connectionService, activityLogService);
        _savedProfilesPageViewModel = new SavedProfilesPageViewModel(profile, profileStore, connectionService, activityLogService);
        var logsPageViewModel = new LogsPageViewModel(profile, activityLogService);
        var aboutPageViewModel = new AboutPageViewModel();

        _allNavigationItems.AddRange(
        [
            new NavigationItem("Home", "⌂", homePageViewModel),
            new NavigationItem("Data View", "☰", dataViewPageViewModel),
            new NavigationItem("Edit", "✎", editPageViewModel),
            new NavigationItem("Bulk Update", "⇪", bulkUpdatePageViewModel, isAdvanced: true),
            new NavigationItem("Sync / Refresh", "↻", syncRefreshPageViewModel, isAdvanced: true),
            new NavigationItem("Connection Manager", "⇄", connectionsPageViewModel),
            new NavigationItem("Settings", "⚙", _settingsPageViewModel),
            new NavigationItem("Saved Profiles", "★", _savedProfilesPageViewModel, isAdvanced: true),
            new NavigationItem("Logs / History", "☷", logsPageViewModel),
            new NavigationItem("About", "ⓘ", aboutPageViewModel),
        ]);

        NavigationItems = new ObservableCollection<NavigationItem>();
        NavigationMenuGroups = new ObservableCollection<NavigationMenuGroup>();
        SelectNavigationItemCommand = new RelayCommand(parameter =>
        {
            if (parameter is NavigationItem item)
            {
                SelectedNavigationItem = item;
            }
        });
        RefreshNavigationItems();
        _currentPage = NavigationItems.First().ViewModel;
        SelectedNavigationItem = NavigationItems.First();
        _activityLogService.Entries.CollectionChanged += ActivityEntriesOnCollectionChanged;
    }

    public AppProfile Profile { get; }

    public string WindowTitle => $"Internal Tool Shell - {Profile.EnvironmentName}";

    public ObservableCollection<NavigationItem> NavigationItems { get; }

    public ObservableCollection<NavigationMenuGroup> NavigationMenuGroups { get; }

    public ICommand SelectNavigationItemCommand { get; }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value) && value is not null)
            {
                CurrentPage = value.ViewModel;
                UpdateNavigationSelection();
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

        if (e.PropertyName is nameof(AppProfile.ShowAdvancedPages) or nameof(AppProfile.IsAdministrator))
        {
            var currentPageTitle = SelectedNavigationItem?.Title ?? "Home";
            RefreshNavigationItems();
            SelectPage(currentPageTitle);
        }
    }

    private void RefreshNavigationItems()
    {
        NavigationItems.Clear();
        foreach (var item in _allNavigationItems.Where(item => (!item.IsAdvanced || Profile.ShowAdvancedPages) && (!item.RequiresAdministrator || Profile.IsAdministrator)))
        {
            NavigationItems.Add(item);
        }

        if (!NavigationItems.Any())
        {
            NavigationItems.Add(_allNavigationItems.First());
        }

        NavigationMenuGroups.Clear();
        foreach (var (groupTitle, pageTitles) in NavigationGroupDefinitions)
        {
            var items = NavigationItems
                .Where(item => pageTitles.Contains(item.Title, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (items.Length > 0)
            {
                NavigationMenuGroups.Add(new NavigationMenuGroup(groupTitle, items));
            }
        }

        UpdateNavigationSelection();
    }

    private void SelectPage(string pageTitle)
    {
        var item = NavigationItems.FirstOrDefault(nav => string.Equals(nav.Title, pageTitle, StringComparison.OrdinalIgnoreCase))
                   ?? NavigationItems.FirstOrDefault();
        if (item is not null)
        {
            SelectedNavigationItem = item;
        }
    }

    private void UpdateNavigationSelection()
    {
        foreach (var item in _allNavigationItems)
        {
            item.IsSelected = ReferenceEquals(item, SelectedNavigationItem);
        }

        foreach (var group in NavigationMenuGroups)
        {
            group.UpdateSelectionState();
        }
    }
}
