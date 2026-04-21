using System.Collections.ObjectModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class ConnectionsPageViewModel : PageViewModelBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly IConnectionService _connectionService;
    private ConnectionDefinition? _selectedConnection;

    public ConnectionsPageViewModel(IConnectionService connectionService, IActivityLogService activityLogService)
        : base("Connection Manager", "Define and test SQL, SharePoint, Graph, and Power BI endpoints.")
    {
        _connectionService = connectionService;
        _activityLogService = activityLogService;
        TestAllCommand = new AsyncRelayCommand(TestAllAsync);
        TestSelectedCommand = new AsyncRelayCommand(TestSelectedAsync, () => SelectedConnection is not null);
        SelectedConnection = _connectionService.Connections.FirstOrDefault();
    }

    public ObservableCollection<ConnectionDefinition> Connections => _connectionService.Connections;

    public ConnectionDefinition? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (SetProperty(ref _selectedConnection, value) && TestSelectedCommand is AsyncRelayCommand command)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TestSelectedCommand { get; }

    public ICommand TestAllCommand { get; }

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
}
