using System.Collections.ObjectModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class BulkUpdatePageViewModel : PageViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public BulkUpdatePageViewModel(WorkspaceService workspaceService)
        : base("Bulk Update", "Preview-first workflow for validating and staging large updates.")
    {
        _workspaceService = workspaceService;
        GeneratePreviewCommand = new AsyncRelayCommand(_workspaceService.GenerateBulkPreviewAsync);
    }

    public ObservableCollection<BulkUpdatePreviewRow> PreviewRows => _workspaceService.BulkPreviewRows;

    public ICommand GeneratePreviewCommand { get; }
}
