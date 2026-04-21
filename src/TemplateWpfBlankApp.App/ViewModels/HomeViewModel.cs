using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class HomeViewModel : PageViewModelBase
{
    private readonly ObservableCollection<ActivityLogEntry> _recentActivities = new();
    private readonly IActivityLogService _activityLogService;
    private readonly IConnectionService _connectionService;
    private readonly WorkspaceService _workspaceService;
    private readonly Action<string> _navigateToPage;

    public HomeViewModel(
        AppProfile profile,
        IActivityLogService activityLogService,
        IConnectionService connectionService,
        WorkspaceService workspaceService,
        Action<string> navigateToPage)
        : base("Home", "Overview of recent work, connection health, and quick-start actions.")
    {
        Profile = profile;
        _activityLogService = activityLogService;
        _connectionService = connectionService;
        _workspaceService = workspaceService;
        _navigateToPage = navigateToPage;
        RefreshActivitySummary();
        OpenDataViewCommand = new RelayCommand(() => _navigateToPage("Data View"));
        CreateNewItemCommand = new RelayCommand(() =>
        {
            _workspaceService.BeginCreateNew();
            _navigateToPage("Edit");
        });
        OpenConnectionsCommand = new RelayCommand(() => _navigateToPage("Connection Manager"));
        OpenLogsCommand = new RelayCommand(() => _navigateToPage("Logs / History"));

        _activityLogService.Entries.CollectionChanged += ActivityEntriesOnCollectionChanged;
        _connectionService.Connections.CollectionChanged += ConnectionsOnCollectionChanged;
        foreach (var connection in _connectionService.Connections)
        {
            connection.PropertyChanged += ConnectionOnPropertyChanged;
        }
    }

    public AppProfile Profile { get; }

    public ObservableCollection<ActivityLogEntry> RecentActivities => _recentActivities;

    public ObservableCollection<ConnectionDefinition> Connections => _connectionService.Connections;

    public int ConnectedCount => Connections.Count(connection => connection.Status == ConnectionState.Connected);

    public int AttentionCount => Connections.Count(connection => connection.Status == ConnectionState.Attention);

    public int ErrorCount => Connections.Count(connection => connection.Status == ConnectionState.Error);

    public ICommand OpenDataViewCommand { get; }

    public ICommand CreateNewItemCommand { get; }

    public ICommand OpenConnectionsCommand { get; }

    public ICommand OpenLogsCommand { get; }

    private void ActivityEntriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshActivitySummary();
    }

    private void ConnectionsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (ConnectionDefinition connection in e.NewItems)
            {
                connection.PropertyChanged += ConnectionOnPropertyChanged;
            }
        }

        if (e.OldItems is not null)
        {
            foreach (ConnectionDefinition connection in e.OldItems)
            {
                connection.PropertyChanged -= ConnectionOnPropertyChanged;
            }
        }

        RaiseConnectionSummaryChanged();
    }

    private void ConnectionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ConnectionDefinition.Status))
        {
            RaiseConnectionSummaryChanged();
        }
    }

    private void RaiseConnectionSummaryChanged()
    {
        OnPropertyChanged(nameof(ConnectedCount));
        OnPropertyChanged(nameof(AttentionCount));
        OnPropertyChanged(nameof(ErrorCount));
    }

    private void RefreshActivitySummary()
    {
        _recentActivities.Clear();
        foreach (var entry in _activityLogService.Entries.Take(10))
        {
            _recentActivities.Add(entry);
        }
    }
}
