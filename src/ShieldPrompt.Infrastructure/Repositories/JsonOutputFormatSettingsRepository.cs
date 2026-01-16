using System.Text.Json;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Repositories;

/// <summary>
/// JSON file-based implementation of output format settings repository.
/// Stores settings in ~/.shieldprompt/output-format-settings.json
/// </summary>
public class JsonOutputFormatSettingsRepository : IOutputFormatSettingsRepository
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public JsonOutputFormatSettingsRepository()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".shieldprompt");
        
        Directory.CreateDirectory(appDataPath);
        _settingsFilePath = Path.Combine(appDataPath, "output-format-settings.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    public async Task<OutputFormatSettings> LoadAsync(CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return OutputFormatSettings.Default;
            }
            
            var json = await File.ReadAllTextAsync(_settingsFilePath, ct);
            var settings = JsonSerializer.Deserialize<OutputFormatSettings>(json, _jsonOptions);
            
            return settings ?? OutputFormatSettings.Default;
        }
        catch
        {
            // If any error occurs, return default settings
            return OutputFormatSettings.Default;
        }
    }
    
    public async Task SaveAsync(OutputFormatSettings settings, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await File.WriteAllTextAsync(_settingsFilePath, json, ct);
    }
    
    public async Task ResetToDefaultAsync(CancellationToken ct = default)
    {
        await SaveAsync(OutputFormatSettings.Default, ct);
    }
}

