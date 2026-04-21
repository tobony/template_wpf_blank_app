using WpfBlankApp.App.Infrastructure;

namespace WpfBlankApp.App.Models;

public sealed class NavigationMenuGroup : ObservableObject
{
    private bool _hasSelectedItem;

    public NavigationMenuGroup(string title, IReadOnlyList<NavigationItem> items)
    {
        Title = title;
        Items = items;
        UpdateSelectionState();
    }

    public string Title { get; }

    public IReadOnlyList<NavigationItem> Items { get; }

    public bool HasSelectedItem
    {
        get => _hasSelectedItem;
        private set => SetProperty(ref _hasSelectedItem, value);
    }

    public void UpdateSelectionState()
    {
        HasSelectedItem = Items.Any(item => item.IsSelected);
    }
}
