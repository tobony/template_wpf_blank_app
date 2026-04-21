using System.Collections.ObjectModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class SavedProfilesPageViewModel : PageViewModelBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly IConnectionService _connectionService;
    private readonly AppProfile _currentProfile;
    private readonly IProfileStore _profileStore;
    private string _newProfileName = string.Empty;
    private AppProfile? _selectedProfile;

    public SavedProfilesPageViewModel(AppProfile currentProfile, IProfileStore profileStore, IConnectionService connectionService, IActivityLogService activityLogService)
        : base("Saved Profiles", "Store reusable task-specific profiles, favorites, and recent combinations.")
    {
        _currentProfile = currentProfile;
        _profileStore = profileStore;
        _connectionService = connectionService;
        _activityLogService = activityLogService;
        RefreshCommand = new AsyncRelayCommand(RefreshProfilesAsync);
        SaveAsCommand = new AsyncRelayCommand(SaveAsAsync, () => !string.IsNullOrWhiteSpace(NewProfileName));
        LoadCommand = new AsyncRelayCommand(LoadSelectedAsync, () => SelectedProfile is not null);
        DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => SelectedProfile is not null);
        SetFavoriteCommand = new AsyncRelayCommand(SetFavoriteAsync, () => SelectedProfile is not null);
    }

    public ObservableCollection<AppProfile> Profiles { get; } = new();

    public string NewProfileName
    {
        get => _newProfileName;
        set
        {
            if (SetProperty(ref _newProfileName, value) && SaveAsCommand is AsyncRelayCommand command)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }

    public AppProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value))
            {
                RaiseCommandStateChanged();
                OnPropertyChanged(nameof(RecentProfiles));
            }
        }
    }

    public IEnumerable<AppProfile> RecentProfiles => Profiles
        .Where(profile => profile.LastUsedAt is not null)
        .OrderByDescending(profile => profile.LastUsedAt)
        .Take(3);

    public ICommand RefreshCommand { get; }

    public ICommand SaveAsCommand { get; }

    public ICommand LoadCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand SetFavoriteCommand { get; }

    public async Task RefreshProfilesAsync()
    {
        Profiles.Clear();
        var names = await _profileStore.GetProfileNamesAsync();
        foreach (var name in names)
        {
            var profile = await _profileStore.LoadAsync(name);
            if (profile is not null)
            {
                Profiles.Add(profile);
            }
        }

        SelectedProfile = SelectedProfile is not null
            ? Profiles.FirstOrDefault(profile => string.Equals(profile.ProfileName, SelectedProfile.ProfileName, StringComparison.OrdinalIgnoreCase))
            : Profiles.FirstOrDefault();

        OnPropertyChanged(nameof(RecentProfiles));
        _activityLogService.Add("Profiles", "Refreshed saved profile list.", $"Profiles available: {Profiles.Count}");
    }

    private async Task SaveAsAsync()
    {
        var snapshot = _currentProfile.Clone();
        snapshot.ProfileName = NewProfileName.Trim();
        snapshot.LastUsedAt = DateTimeOffset.Now;
        snapshot.Connections = new ObservableCollection<ConnectionDefinition>(_connectionService.CreateSnapshot().Select(connection => connection.Clone()));
        await _profileStore.SaveAsync(snapshot);
        _activityLogService.Add("Profiles", $"Saved profile '{snapshot.ProfileName}'.");
        NewProfileName = string.Empty;
        await RefreshProfilesAsync();
    }

    private async Task LoadSelectedAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var profile = await _profileStore.LoadAsync(SelectedProfile.ProfileName);
        if (profile is null)
        {
            _activityLogService.Add("Profiles", $"Profile '{SelectedProfile.ProfileName}' was not found.", string.Empty, "Warning");
            await RefreshProfilesAsync();
            return;
        }

        profile.LastUsedAt = DateTimeOffset.Now;
        _currentProfile.CopyFrom(profile);
        _connectionService.ApplySnapshot(profile.Connections);
        await _profileStore.SaveAsync(profile);
        _activityLogService.Add("Profiles", $"Loaded profile '{profile.ProfileName}'.");
        await RefreshProfilesAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        var deletedProfile = SelectedProfile.ProfileName;
        await _profileStore.DeleteAsync(deletedProfile);
        _activityLogService.Add("Profiles", $"Deleted profile '{deletedProfile}'.");
        SelectedProfile = null;
        await RefreshProfilesAsync();
    }

    private async Task SetFavoriteAsync()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        foreach (var profile in Profiles)
        {
            profile.IsFavorite = string.Equals(profile.ProfileName, SelectedProfile.ProfileName, StringComparison.OrdinalIgnoreCase);
            await _profileStore.SaveAsync(profile);
        }

        _activityLogService.Add("Profiles", $"Marked '{SelectedProfile.ProfileName}' as the favorite profile.");
        await RefreshProfilesAsync();
    }

    private void RaiseCommandStateChanged()
    {
        if (LoadCommand is AsyncRelayCommand loadCommand)
        {
            loadCommand.RaiseCanExecuteChanged();
        }

        if (DeleteCommand is AsyncRelayCommand deleteCommand)
        {
            deleteCommand.RaiseCanExecuteChanged();
        }

        if (SetFavoriteCommand is AsyncRelayCommand setFavoriteCommand)
        {
            setFavoriteCommand.RaiseCanExecuteChanged();
        }
    }
}
