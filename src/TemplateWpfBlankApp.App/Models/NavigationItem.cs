using TemplateWpfBlankApp.App.ViewModels;

namespace TemplateWpfBlankApp.App.Models;

public sealed class NavigationItem
{
    public NavigationItem(string title, string icon, PageViewModelBase viewModel)
    {
        Title = title;
        Icon = icon;
        ViewModel = viewModel;
    }

    public string Title { get; }

    public string Icon { get; }

    public PageViewModelBase ViewModel { get; }
}
