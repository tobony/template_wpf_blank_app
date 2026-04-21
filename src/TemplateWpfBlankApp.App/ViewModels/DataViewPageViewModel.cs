using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class DataViewPageViewModel : PageViewModelBase
{
    private const int PageSize = 3;
    private readonly AppProfile _profile;
    private readonly WorkspaceService _workspaceService;
    private readonly Action<string> _navigateToPage;
    private string _searchText = string.Empty;
    private string _selectedStatus = "All";
    private string _selectedOwner = "All";
    private string _selectedSystem = "All";
    private string _selectedSort = "Updated (newest)";
    private int _currentPageNumber = 1;

    public DataViewPageViewModel(AppProfile profile, WorkspaceService workspaceService, Action<string> navigateToPage)
        : base("Data View", "Search, filter, sort, page, and inspect connected business records.")
    {
        _profile = profile;
        _workspaceService = workspaceService;
        _navigateToPage = navigateToPage;
        _workspaceService.PropertyChanged += WorkspaceServiceOnPropertyChanged;
        _workspaceService.Items.CollectionChanged += ItemsOnCollectionChanged;
        SubscribeToItems(_workspaceService.Items);
        RefreshCommand = new AsyncRelayCommand(async () =>
        {
            await _workspaceService.RefreshAsync();
            RefreshVisibleItems();
        });
        ExportCommand = new AsyncRelayCommand(_workspaceService.ExportDataAsync);
        OpenDetailCommand = new RelayCommand(() => _navigateToPage("Edit"), () => SelectedItem is not null);
        NextPageCommand = new RelayCommand(() =>
        {
            CurrentPageNumber++;
            RefreshVisibleItems();
        }, () => CurrentPageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(() =>
        {
            CurrentPageNumber--;
            RefreshVisibleItems();
        }, () => CurrentPageNumber > 1);

        _selectedStatus = string.IsNullOrWhiteSpace(_profile.DefaultStatusFilter) ? "All" : _profile.DefaultStatusFilter;
        _selectedOwner = string.IsNullOrWhiteSpace(_profile.DefaultOwnerFilter) ? "All" : _profile.DefaultOwnerFilter;
        RefreshVisibleItems();
    }

    public ObservableCollection<WorkItem> VisibleItems { get; } = new();

    public IReadOnlyList<string> StatusOptions => ["All", .. _workspaceService.Items.Select(item => item.Status).Distinct().OrderBy(value => value)];

    public IReadOnlyList<string> OwnerOptions => ["All", .. _workspaceService.Items.Select(item => item.Owner).Distinct().OrderBy(value => value)];

    public IReadOnlyList<string> SystemOptions => ["All", .. _workspaceService.Items.Select(item => item.SourceSystem).Distinct().OrderBy(value => value)];

    public IReadOnlyList<string> SortOptions => ["Updated (newest)", "Title (A-Z)", "Owner (A-Z)", "Status (A-Z)"];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                CurrentPageNumber = 1;
                RefreshVisibleItems();
            }
        }
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                CurrentPageNumber = 1;
                RefreshVisibleItems();
            }
        }
    }

    public string SelectedOwner
    {
        get => _selectedOwner;
        set
        {
            if (SetProperty(ref _selectedOwner, value))
            {
                CurrentPageNumber = 1;
                RefreshVisibleItems();
            }
        }
    }

    public string SelectedSystem
    {
        get => _selectedSystem;
        set
        {
            if (SetProperty(ref _selectedSystem, value))
            {
                CurrentPageNumber = 1;
                RefreshVisibleItems();
            }
        }
    }

    public string SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (SetProperty(ref _selectedSort, value))
            {
                RefreshVisibleItems();
            }
        }
    }

    public int CurrentPageNumber
    {
        get => _currentPageNumber;
        private set => SetProperty(ref _currentPageNumber, Math.Max(1, value));
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(GetFilteredItems().Count() / (double)PageSize));

    public string PageSummary => $"Page {CurrentPageNumber} of {TotalPages}";

    public WorkItem? SelectedItem
    {
        get => _workspaceService.SelectedItem;
        set
        {
            _workspaceService.SelectedItem = value;
            OnPropertyChanged();
            RaiseCommandStateChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    public ICommand ExportCommand { get; }

    public ICommand OpenDetailCommand { get; }

    public ICommand NextPageCommand { get; }

    public ICommand PreviousPageCommand { get; }

    private IEnumerable<WorkItem> GetFilteredItems()
    {
        IEnumerable<WorkItem> query = _workspaceService.Items;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(item =>
                item.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.Owner.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.SourceSystem.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(SelectedStatus, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => string.Equals(item.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(SelectedOwner, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => string.Equals(item.Owner, SelectedOwner, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(SelectedSystem, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(item => string.Equals(item.SourceSystem, SelectedSystem, StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedSort switch
        {
            "Title (A-Z)" => query.OrderBy(item => item.Title),
            "Owner (A-Z)" => query.OrderBy(item => item.Owner),
            "Status (A-Z)" => query.OrderBy(item => item.Status),
            _ => query.OrderByDescending(item => item.LastUpdated),
        };

        return query.ToArray();
    }

    private void RefreshVisibleItems()
    {
        var filteredItems = GetFilteredItems().ToArray();
        if (CurrentPageNumber > Math.Max(1, (int)Math.Ceiling(filteredItems.Length / (double)PageSize)))
        {
            CurrentPageNumber = 1;
        }

        VisibleItems.Clear();
        foreach (var item in filteredItems.Skip((CurrentPageNumber - 1) * PageSize).Take(PageSize))
        {
            VisibleItems.Add(item);
        }

        OnPropertyChanged(nameof(StatusOptions));
        OnPropertyChanged(nameof(OwnerOptions));
        OnPropertyChanged(nameof(SystemOptions));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageSummary));
        RaiseCommandStateChanged();
    }

    private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            SubscribeToItems(e.NewItems.Cast<WorkItem>());
        }

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.Cast<WorkItem>())
            {
                item.PropertyChanged -= ItemOnPropertyChanged;
            }
        }

        RefreshVisibleItems();
    }

    private void SubscribeToItems(IEnumerable<WorkItem> items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged += ItemOnPropertyChanged;
        }
    }

    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshVisibleItems();
    }

    private void WorkspaceServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(WorkspaceService.SelectedItem))
        {
            OnPropertyChanged(nameof(SelectedItem));
            RaiseCommandStateChanged();
        }
    }

    private void RaiseCommandStateChanged()
    {
        if (OpenDetailCommand is RelayCommand openDetailCommand)
        {
            openDetailCommand.RaiseCanExecuteChanged();
        }

        if (NextPageCommand is RelayCommand nextPageCommand)
        {
            nextPageCommand.RaiseCanExecuteChanged();
        }

        if (PreviousPageCommand is RelayCommand previousPageCommand)
        {
            previousPageCommand.RaiseCanExecuteChanged();
        }
    }
}
