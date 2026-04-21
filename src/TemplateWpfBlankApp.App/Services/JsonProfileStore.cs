using System.IO;
using System.Text.Json;
using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public sealed class JsonProfileStore : IProfileStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    public string StorageFolderPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TemplateWpfBlankApp",
        "profiles");

    public async Task SaveAsync(AppProfile profile)
    {
        Directory.CreateDirectory(StorageFolderPath);
        var path = GetProfilePath(profile.ProfileName);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, profile.Clone(), SerializerOptions);
    }

    public async Task<AppProfile?> LoadAsync(string profileName)
    {
        var path = GetProfilePath(profileName);
        if (!File.Exists(path))
        {
            return null;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<AppProfile>(stream, SerializerOptions);
    }

    public Task<IReadOnlyList<string>> GetProfileNamesAsync()
    {
        Directory.CreateDirectory(StorageFolderPath);
        IReadOnlyList<string> profiles = Directory
            .EnumerateFiles(StorageFolderPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(profiles);
    }

    public Task DeleteAsync(string profileName)
    {
        var path = GetProfilePath(profileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetProfilePath(string profileName)
    {
        var safeName = string.Concat(profileName.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        return Path.Combine(StorageFolderPath, $"{safeName}.json");
    }
}
