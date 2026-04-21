using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class DataViewPageViewModel : PageViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public DataViewPageViewModel(WorkspaceService workspaceService)
        : base("Data View", "Shared table and detail view for connected business records.")
    {
        _workspaceService = workspaceService;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        RefreshCommand = new AsyncRelayCommand(_workspaceService.RefreshAsync);
    }

    public ObservableCollection<WorkItem> Items => _workspaceService.Items;

    public WorkItem? SelectedItem
    {
        get => _workspaceService.SelectedItem;
        set
        {
            _workspaceService.SelectedItem = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.SelectedItem))
        {
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
