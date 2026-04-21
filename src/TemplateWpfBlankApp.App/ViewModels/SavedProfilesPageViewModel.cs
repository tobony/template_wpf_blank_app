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
    private string? _selectedProfileName;

    public SavedProfilesPageViewModel(AppProfile currentProfile, IProfileStore profileStore, IConnectionService connectionService, IActivityLogService activityLogService)
        : base("Saved Profiles", "Store reusable environment and connection combinations for different tasks.")
    {
        _currentProfile = currentProfile;
        _profileStore = profileStore;
        _connectionService = connectionService;
        _activityLogService = activityLogService;
        RefreshCommand = new AsyncRelayCommand(RefreshProfilesAsync);
        SaveAsCommand = new AsyncRelayCommand(SaveAsAsync, () => !string.IsNullOrWhiteSpace(NewProfileName));
        LoadCommand = new AsyncRelayCommand(LoadSelectedAsync, () => !string.IsNullOrWhiteSpace(SelectedProfileName));
        DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync, () => !string.IsNullOrWhiteSpace(SelectedProfileName));
    }

    public ObservableCollection<string> Profiles { get; } = new();

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

    public string? SelectedProfileName
    {
        get => _selectedProfileName;
        set
        {
            if (SetProperty(ref _selectedProfileName, value))
            {
                if (LoadCommand is AsyncRelayCommand loadCommand)
                {
                    loadCommand.RaiseCanExecuteChanged();
                }

                if (DeleteCommand is AsyncRelayCommand deleteCommand)
                {
                    deleteCommand.RaiseCanExecuteChanged();
                }
            }
        }
    }

    public ICommand RefreshCommand { get; }

    public ICommand SaveAsCommand { get; }

    public ICommand LoadCommand { get; }

    public ICommand DeleteCommand { get; }

    public async Task RefreshProfilesAsync()
    {
        Profiles.Clear();
        var names = await _profileStore.GetProfileNamesAsync();
        foreach (var name in names)
        {
            Profiles.Add(name);
        }

        var nextSelection = !string.IsNullOrWhiteSpace(SelectedProfileName) && Profiles.Contains(SelectedProfileName)
            ? SelectedProfileName
            : Profiles.FirstOrDefault();

        SelectedProfileName = nextSelection;
        if (nextSelection is null)
        {
            OnPropertyChanged(nameof(SelectedProfileName));
        }

        _activityLogService.Add("Profiles", "Refreshed saved profile list.", $"Profiles available: {Profiles.Count}");
    }

    private async Task SaveAsAsync()
    {
        var snapshot = _currentProfile.Clone();
        snapshot.ProfileName = NewProfileName.Trim();
        snapshot.Connections = new ObservableCollection<ConnectionDefinition>(_connectionService.CreateSnapshot().Select(connection => connection.Clone()));
        await _profileStore.SaveAsync(snapshot);
        _activityLogService.Add("Profiles", $"Saved profile '{snapshot.ProfileName}'.");
        NewProfileName = string.Empty;
        await RefreshProfilesAsync();
    }

    private async Task LoadSelectedAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedProfileName))
        {
            return;
        }

        var profile = await _profileStore.LoadAsync(SelectedProfileName);
        if (profile is null)
        {
            _activityLogService.Add("Profiles", $"Profile '{SelectedProfileName}' was not found.");
            await RefreshProfilesAsync();
            return;
        }

        _currentProfile.CopyFrom(profile);
        _connectionService.ApplySnapshot(profile.Connections);
        _activityLogService.Add("Profiles", $"Loaded profile '{SelectedProfileName}'.");
    }

    private async Task DeleteSelectedAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedProfileName))
        {
            return;
        }

        var deletedProfile = SelectedProfileName;
        await _profileStore.DeleteAsync(deletedProfile);
        _activityLogService.Add("Profiles", $"Deleted profile '{deletedProfile}'.");
        SelectedProfileName = null;
        await RefreshProfilesAsync();
    }
}
