using System.ComponentModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class EditPageViewModel : PageViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public EditPageViewModel(WorkspaceService workspaceService)
        : base("Edit", "Reusable form pattern for record editing, saving, and reset.")
    {
        _workspaceService = workspaceService;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        SaveCommand = new AsyncRelayCommand(_workspaceService.SaveAsync, () => CurrentItem is not null);
        ResetCommand = new RelayCommand(_workspaceService.ResetEdit, () => CurrentItem is not null);
    }

    public WorkItem? CurrentItem => _workspaceService.EditableItem;

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.EditableItem))
        {
            OnPropertyChanged(nameof(CurrentItem));
            if (SaveCommand is AsyncRelayCommand asyncRelayCommand)
            {
                asyncRelayCommand.RaiseCanExecuteChanged();
            }

            if (ResetCommand is RelayCommand relayCommand)
            {
                relayCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
