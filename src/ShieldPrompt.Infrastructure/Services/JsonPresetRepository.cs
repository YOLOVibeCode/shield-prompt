using System.Text.Json;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// JSON-based implementation of IPresetRepository.
/// Stores preset data in ~/.shieldprompt/presets.json
/// </summary>
public class JsonPresetRepository : IPresetRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;
    private List<PromptPreset> _presets = new();
    private bool _loaded;

    /// <summary>
    /// Creates a new JsonPresetRepository using the default app data path.
    /// </summary>
    public JsonPresetRepository()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".shieldprompt");
        Directory.CreateDirectory(appDataPath);
        _filePath = Path.Combine(appDataPath, "presets.json");

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Creates a new JsonPresetRepository using a custom directory path.
    /// Used for testing.
    /// </summary>
    public JsonPresetRepository(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "presets.json");

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptPreset>> GetGlobalPresetsAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _presets
            .Where(p => p.Scope == PresetScope.Global)
            .OrderByDescending(p => p.LastUsed)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PromptPreset>> GetWorkspacePresetsAsync(string workspaceId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _presets
            .Where(p => p.Scope == PresetScope.Workspace && p.WorkspaceId == workspaceId)
            .OrderByDescending(p => p.LastUsed)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<PromptPreset?> GetByIdAsync(string presetId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _presets.FirstOrDefault(p => p.Id == presetId);
    }

    /// <inheritdoc />
    public async Task SaveAsync(PromptPreset preset, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);

        var existingIndex = _presets.FindIndex(p => p.Id == preset.Id);
        if (existingIndex >= 0)
        {
            _presets[existingIndex] = preset;
        }
        else
        {
            _presets.Add(preset);
        }

        await PersistAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string presetId, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        _presets.RemoveAll(p => p.Id == presetId);
        await PersistAsync(ct);
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loaded) return;

        if (File.Exists(_filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath, ct);
                _presets = JsonSerializer.Deserialize<List<PromptPreset>>(json, _options)
                    ?? new List<PromptPreset>();
            }
            catch
            {
                // If deserialization fails, start with empty list
                _presets = new List<PromptPreset>();
            }
        }

        _loaded = true;
    }

    private async Task PersistAsync(CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(_presets, _options);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}
