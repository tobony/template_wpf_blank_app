using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public interface IProfileStore
{
    string StorageFolderPath { get; }

    Task SaveAsync(AppProfile profile);

    Task<AppProfile?> LoadAsync(string profileName);

    Task<IReadOnlyList<string>> GetProfileNamesAsync();

    Task DeleteAsync(string profileName);
}
