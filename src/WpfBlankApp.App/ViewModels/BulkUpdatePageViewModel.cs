using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.Models;
using WpfBlankApp.App.Services;

namespace WpfBlankApp.App.ViewModels;

public sealed class BulkUpdatePageViewModel : PageViewModelBase
{
    private readonly AppProfile _profile;
    private readonly WorkspaceService _workspaceService;

    public BulkUpdatePageViewModel(AppProfile profile, WorkspaceService workspaceService)
        : base("Bulk Update", "Preview-first workflow for file-driven validation, execution, and retry.")
    {
        _profile = profile;
        _workspaceService = workspaceService;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        _profile.PropertyChanged += ProfileOnPropertyChanged;
        UseSampleFileCommand = new RelayCommand(_workspaceService.UseSampleImportFile);
        GeneratePreviewCommand = new AsyncRelayCommand(async () =>
        {
            await _workspaceService.GenerateBulkPreviewAsync();
            RaiseCommandStateChanged();
        });
        ExecuteCommand = new AsyncRelayCommand(async () =>
        {
            await _workspaceService.ExecuteBulkUpdateAsync();
            RaiseCommandStateChanged();
        }, () => IsAdministrator && PreviewRows.Any());
        RetryFailuresCommand = new AsyncRelayCommand(async () =>
        {
            await _workspaceService.RetryFailedBulkUpdatesAsync();
            RaiseCommandStateChanged();
        }, () => IsAdministrator && PreviewRows.Any(row => row.CanRetry));
    }

    public ObservableCollection<BulkUpdatePreviewRow> PreviewRows => _workspaceService.BulkPreviewRows;

    public string SelectedFilePath
    {
        get => _workspaceService.BulkImportFilePath;
        set
        {
            _workspaceService.BulkImportFilePath = value;
            OnPropertyChanged();
        }
    }

    public string MappingSummary => _workspaceService.BulkMappingSummary;

    public int ValidCount => _workspaceService.BulkValidCount;

    public int WarningCount => _workspaceService.BulkWarningCount;

    public int ErrorCount => _workspaceService.BulkErrorCount;

    public string ExecutionMessage => _workspaceService.BulkExecutionMessage;

    public bool ExecutionConfirmationRequested => _workspaceService.BulkExecutionConfirmationRequested;

    public bool IsAdministrator => _profile.IsAdministrator;

    public ICommand UseSampleFileCommand { get; }

    public ICommand GeneratePreviewCommand { get; }

    public ICommand ExecuteCommand { get; }

    public ICommand RetryFailuresCommand { get; }

    private void ProfileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppProfile.IsAdministrator))
        {
            OnPropertyChanged(nameof(IsAdministrator));
            RaiseCommandStateChanged();
        }
    }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.BulkImportFilePath) or nameof(WorkspaceService.BulkMappingSummary) or nameof(WorkspaceService.BulkExecutionMessage) or nameof(WorkspaceService.BulkExecutionConfirmationRequested))
        {
            OnPropertyChanged(nameof(SelectedFilePath));
            OnPropertyChanged(nameof(MappingSummary));
            OnPropertyChanged(nameof(ExecutionMessage));
            OnPropertyChanged(nameof(ExecutionConfirmationRequested));
        }

        OnPropertyChanged(nameof(ValidCount));
        OnPropertyChanged(nameof(WarningCount));
        OnPropertyChanged(nameof(ErrorCount));
        RaiseCommandStateChanged();
    }

    private void RaiseCommandStateChanged()
    {
        if (ExecuteCommand is AsyncRelayCommand executeCommand)
        {
            executeCommand.RaiseCanExecuteChanged();
        }

        if (RetryFailuresCommand is AsyncRelayCommand retryFailuresCommand)
        {
            retryFailuresCommand.RaiseCanExecuteChanged();
        }
    }
}
