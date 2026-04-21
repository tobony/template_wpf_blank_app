using System.ComponentModel;
using System.Windows.Input;
using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.Models;
using WpfBlankApp.App.Services;

namespace WpfBlankApp.App.ViewModels;

public sealed class EditPageViewModel : PageViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public EditPageViewModel(WorkspaceService workspaceService)
        : base("Edit", "Reusable form pattern for create, edit, validation, and deactivate workflows.")
    {
        _workspaceService = workspaceService;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        SaveCommand = new AsyncRelayCommand(_workspaceService.SaveAsync, () => CurrentItem is not null && (IsCreatingNewItem || !IsReadOnlyItem));
        ResetCommand = new RelayCommand(_workspaceService.ResetEdit, () => CurrentItem is not null);
        NewItemCommand = new RelayCommand(_workspaceService.BeginCreateNew);
        DeleteOrDeactivateCommand = new AsyncRelayCommand(_workspaceService.DeleteOrDeactivateAsync, () => CurrentItem is not null && (IsCreatingNewItem || !IsReadOnlyItem));
    }

    public WorkItem? CurrentItem => _workspaceService.EditableItem;

    public string ValidationMessage => _workspaceService.ValidationMessage;

    public bool IsCreatingNewItem => _workspaceService.IsCreatingNewItem;

    public bool DeleteConfirmationRequested => _workspaceService.DeleteConfirmationRequested;

    public bool IsReadOnlyItem => !IsCreatingNewItem && CurrentItem?.IsReadOnly == true;

    public string ModeTitle => IsCreatingNewItem ? "New item mode" : "Existing item mode";

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    public ICommand NewItemCommand { get; }

    public ICommand DeleteOrDeactivateCommand { get; }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.EditableItem) or nameof(WorkspaceService.ValidationMessage) or nameof(WorkspaceService.IsCreatingNewItem) or nameof(WorkspaceService.DeleteConfirmationRequested))
        {
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(ValidationMessage));
            OnPropertyChanged(nameof(IsCreatingNewItem));
            OnPropertyChanged(nameof(DeleteConfirmationRequested));
            OnPropertyChanged(nameof(IsReadOnlyItem));
            OnPropertyChanged(nameof(ModeTitle));
            RaiseCommandStateChanged();
        }
    }

    private void RaiseCommandStateChanged()
    {
        if (SaveCommand is AsyncRelayCommand saveCommand)
        {
            saveCommand.RaiseCanExecuteChanged();
        }

        if (ResetCommand is RelayCommand resetCommand)
        {
            resetCommand.RaiseCanExecuteChanged();
        }

        if (DeleteOrDeactivateCommand is AsyncRelayCommand deleteCommand)
        {
            deleteCommand.RaiseCanExecuteChanged();
        }
    }
}
