using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.ViewModels;

namespace WpfBlankApp.App.Models;

public sealed class NavigationItem : ObservableObject
{
    private bool _isSelected;

    public NavigationItem(string title, string icon, PageViewModelBase viewModel, bool isAdvanced = false, bool requiresAdministrator = false)
    {
        Title = title;
        Icon = icon;
        ViewModel = viewModel;
        IsAdvanced = isAdvanced;
        RequiresAdministrator = requiresAdministrator;
    }

    public string Title { get; }

    public string Icon { get; }

    public PageViewModelBase ViewModel { get; }

    public bool IsAdvanced { get; }

    public bool RequiresAdministrator { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
