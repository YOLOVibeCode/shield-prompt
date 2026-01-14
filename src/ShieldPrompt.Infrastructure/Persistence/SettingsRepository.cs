using System.Text.Json;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Infrastructure.Interfaces;

namespace ShieldPrompt.Infrastructure.Persistence;

/// <summary>
/// Persists application settings to JSON file.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsRepository() : this(GetDefaultSettingsPath())
    {
    }

    public SettingsRepository(string settingsPath)
    {
        _settingsPath = settingsPath;
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<AppSettings> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_settingsPath))
            return new AppSettings(); // Return defaults

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath, ct).ConfigureAwait(false);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            // Corrupt file - return defaults
            return new AppSettings();
        }
        catch (IOException)
        {
            // Can't read file - return defaults
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json, ct).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // Fail silently - don't crash app on save failure
        }
    }

    private static string GetDefaultSettingsPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "ShieldPrompt");
        
        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        return Path.Combine(appFolder, "settings.json");
    }
}

