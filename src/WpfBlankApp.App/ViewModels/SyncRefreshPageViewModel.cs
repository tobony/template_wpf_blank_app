using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.Models;
using WpfBlankApp.App.Services;

namespace WpfBlankApp.App.ViewModels;

public sealed class SyncRefreshPageViewModel : PageViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public SyncRefreshPageViewModel(WorkspaceService workspaceService)
        : base("Sync / Refresh", "Reload remote state, push local changes, review conflicts, and retry failures.")
    {
        _workspaceService = workspaceService;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        ReloadFromServerCommand = new AsyncRelayCommand(_workspaceService.ReloadFromServerAsync);
        PushLocalChangesCommand = new AsyncRelayCommand(_workspaceService.PushLocalChangesAsync);
        RetryFailedCommand = new AsyncRelayCommand(_workspaceService.RetryFailedSyncAsync, () => FailedItems.Any());
    }

    public ObservableCollection<SyncConflictRecord> Conflicts => _workspaceService.SyncConflicts;

    public ObservableCollection<SyncConflictRecord> FailedItems => _workspaceService.FailedSyncQueue;

    public DateTimeOffset? LastServerRefreshAt => _workspaceService.LastServerRefreshAt;

    public string SyncStatusMessage => _workspaceService.SyncStatusMessage;

    public ICommand ReloadFromServerCommand { get; }

    public ICommand PushLocalChangesCommand { get; }

    public ICommand RetryFailedCommand { get; }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.LastServerRefreshAt) or nameof(WorkspaceService.SyncStatusMessage))
        {
            OnPropertyChanged(nameof(LastServerRefreshAt));
            OnPropertyChanged(nameof(SyncStatusMessage));
        }

        if (RetryFailedCommand is AsyncRelayCommand retryFailedCommand)
        {
            retryFailedCommand.RaiseCanExecuteChanged();
        }
    }
}
