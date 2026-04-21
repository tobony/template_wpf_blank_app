using System.Collections.ObjectModel;
using System.Windows.Input;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class SettingsPageViewModel : PageViewModelBase
{
    private readonly IActivityLogService _activityLogService;
    private readonly IConnectionService _connectionService;
    private readonly IProfileStore _profileStore;

    public SettingsPageViewModel(AppProfile profile, IProfileStore profileStore, IConnectionService connectionService, IActivityLogService activityLogService)
        : base("Settings", "Manage environment defaults, user preferences, and current profile persistence.")
    {
        Profile = profile;
        _profileStore = profileStore;
        _connectionService = connectionService;
        _activityLogService = activityLogService;
        SaveCurrentCommand = new AsyncRelayCommand(SaveCurrentAsync);
        LoadCurrentCommand = new AsyncRelayCommand(LoadCurrentAsync);
        ResetTemplateCommand = new RelayCommand(ResetTemplate);
    }

    public AppProfile Profile { get; }

    public ObservableCollection<ConnectionDefinition> CurrentConnections => _connectionService.Connections;

    public string StorageFolderPath => _profileStore.StorageFolderPath;

    public ICommand SaveCurrentCommand { get; }

    public ICommand LoadCurrentCommand { get; }

    public ICommand ResetTemplateCommand { get; }

    public async Task EnsureSavedProfileExistsAsync()
    {
        var existing = await _profileStore.LoadAsync(Profile.ProfileName);
        if (existing is null)
        {
            await SaveCurrentAsync();
        }
    }

    private async Task SaveCurrentAsync()
    {
        SyncConnectionsIntoProfile();
        await _profileStore.SaveAsync(Profile);
        _activityLogService.Add("Settings", $"Saved profile '{Profile.ProfileName}'.", _profileStore.StorageFolderPath);
    }

    private async Task LoadCurrentAsync()
    {
        var loaded = await _profileStore.LoadAsync(Profile.ProfileName);
        if (loaded is null)
        {
            _activityLogService.Add("Settings", $"Profile '{Profile.ProfileName}' was not found.", "Using the in-memory template configuration.");
            return;
        }

        Profile.CopyFrom(loaded);
        _connectionService.ApplySnapshot(Profile.Connections);
        _activityLogService.Add("Settings", $"Loaded profile '{Profile.ProfileName}'.");
        OnPropertyChanged(nameof(CurrentConnections));
    }

    private void ResetTemplate()
    {
        var template = AppProfile.CreateDefault();
        Profile.CopyFrom(template);
        _connectionService.ApplySnapshot(Profile.Connections);
        _activityLogService.Add("Settings", "Reset the current profile to the template defaults.");
        OnPropertyChanged(nameof(CurrentConnections));
    }

    private void SyncConnectionsIntoProfile()
    {
        Profile.Connections = new ObservableCollection<ConnectionDefinition>(_connectionService.CreateSnapshot().Select(connection => connection.Clone()));
    }
}
