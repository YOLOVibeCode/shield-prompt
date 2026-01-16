using System.Text.Json;
using ShieldPrompt.Infrastructure.Interfaces;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// JSON-based implementation of layout state persistence.
/// Stores layout state in a JSON file in the user's app data folder.
/// </summary>
public class JsonLayoutStateRepository : ILayoutStateRepository
{
    private readonly string _layoutFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public JsonLayoutStateRepository(string? appDataPath = null)
    {
        var dataDirectory = appDataPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShieldPrompt");
        
        Directory.CreateDirectory(dataDirectory);
        _layoutFilePath = Path.Combine(dataDirectory, "layout-state.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }
    
    public async Task SaveLayoutStateAsync(LayoutState state, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(state, _jsonOptions);
            await File.WriteAllTextAsync(_layoutFilePath, json, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Fail gracefully - don't crash app if layout save fails
            Console.WriteLine($"Warning: Failed to save layout state: {ex.Message}");
        }
    }
    
    public async Task<LayoutState?> LoadLayoutStateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_layoutFilePath))
                return null;
            
            var json = await File.ReadAllTextAsync(_layoutFilePath, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<LayoutState>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            // Fail gracefully - return null if load fails
            Console.WriteLine($"Warning: Failed to load layout state: {ex.Message}");
            return null;
        }
    }
    
    public async Task ResetToDefaultAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(_layoutFilePath))
            {
                await Task.Run(() => File.Delete(_layoutFilePath), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // Fail gracefully
            Console.WriteLine($"Warning: Failed to reset layout state: {ex.Message}");
        }
    }
}

