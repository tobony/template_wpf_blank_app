using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class ConnectionsPageViewModel : PageViewModelBase
{
    private readonly AppProfile _profile;
    private readonly IActivityLogService _activityLogService;
    private readonly IConnectionService _connectionService;
    private ConnectionDefinition? _selectedConnection;

    public ConnectionsPageViewModel(AppProfile profile, IConnectionService connectionService, IActivityLogService activityLogService)
        : base("Connection Manager", "Define endpoints, test connectivity, and review token/authentication state.")
    {
        _profile = profile;
        _connectionService = connectionService;
        _activityLogService = activityLogService;
        _profile.PropertyChanged += ProfileOnPropertyChanged;
        TestAllCommand = new AsyncRelayCommand(TestAllAsync, () => IsAdministrator);
        TestSelectedCommand = new AsyncRelayCommand(TestSelectedAsync, () => SelectedConnection is not null);
        SaveConnectionSettingsCommand = new RelayCommand(SaveConnectionSettings, () => IsAdministrator);
        SelectedConnection = _connectionService.Connections.FirstOrDefault();
    }

    public ObservableCollection<ConnectionDefinition> Connections => _connectionService.Connections;

    public bool IsAdministrator => _profile.IsAdministrator;

    public ConnectionDefinition? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (SetProperty(ref _selectedConnection, value))
            {
                RaiseCommandStateChanged();
            }
        }
    }

    public ICommand TestSelectedCommand { get; }

    public ICommand TestAllCommand { get; }

    public ICommand SaveConnectionSettingsCommand { get; }

    private void ProfileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppProfile.IsAdministrator))
        {
            OnPropertyChanged(nameof(IsAdministrator));
            RaiseCommandStateChanged();
        }
    }

    private async Task TestSelectedAsync()
    {
        await _connectionService.TestConnectionAsync(SelectedConnection);
        if (SelectedConnection is not null)
        {
            _activityLogService.Add("Connections", $"Tested {SelectedConnection.Name}.", SelectedConnection.Notes);
        }
    }

    private async Task TestAllAsync()
    {
        await _connectionService.TestAllAsync();
        _activityLogService.Add("Connections", "Tested all configured connections.");
    }

    private void SaveConnectionSettings()
    {
        _activityLogService.Add("Connections", "Saved connection parameters.", "Connection endpoints and authentication hints were updated.");
    }

    private void RaiseCommandStateChanged()
    {
        if (TestSelectedCommand is AsyncRelayCommand testSelectedCommand)
        {
            testSelectedCommand.RaiseCanExecuteChanged();
        }

        if (TestAllCommand is AsyncRelayCommand testAllCommand)
        {
            testAllCommand.RaiseCanExecuteChanged();
        }

        if (SaveConnectionSettingsCommand is RelayCommand saveConnectionSettingsCommand)
        {
            saveConnectionSettingsCommand.RaiseCanExecuteChanged();
        }
    }
}
