using WpfBlankApp.App.Models;

namespace WpfBlankApp.App.Services;

public interface IProfileStore
{
    string StorageFolderPath { get; }

    Task SaveAsync(AppProfile profile);

    Task<AppProfile?> LoadAsync(string profileName);

    Task<IReadOnlyList<string>> GetProfileNamesAsync();

    Task DeleteAsync(string profileName);
}
