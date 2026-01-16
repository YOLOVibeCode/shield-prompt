using System.Text.Json;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// JSON-based implementation of IWorkspaceRepository.
/// Stores workspace data in ~/.shieldprompt/workspaces.json
/// </summary>
public class JsonWorkspaceRepository : IWorkspaceRepository
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;
    private List<Workspace> _workspaces = new();
    private bool _loaded;

    public JsonWorkspaceRepository()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".shieldprompt");
        Directory.CreateDirectory(appDataPath);
        _filePath = Path.Combine(appDataPath, "workspaces.json");

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IReadOnlyList<Workspace>> GetAllAsync(CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _workspaces
            .OrderByDescending(w => w.LastOpened)
            .ToList();
    }

    public async Task<Workspace?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _workspaces.FirstOrDefault(w => w.Id == id);
    }

    public async Task<Workspace?> GetByPathAsync(string rootPath, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        return _workspaces.FirstOrDefault(w =>
            string.Equals(w.RootPath, rootPath, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SaveAsync(Workspace workspace, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);

        var existingIndex = _workspaces.FindIndex(w => w.Id == workspace.Id);
        if (existingIndex >= 0)
        {
            _workspaces[existingIndex] = workspace;
        }
        else
        {
            _workspaces.Add(workspace);
        }

        await PersistAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        _workspaces.RemoveAll(w => w.Id == id);
        await PersistAsync(ct);
    }

    public async Task UpdateLastOpenedAsync(string id, CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);
        var workspace = _workspaces.FirstOrDefault(w => w.Id == id);
        if (workspace != null)
        {
            var index = _workspaces.IndexOf(workspace);
            _workspaces[index] = workspace with { LastOpened = DateTime.UtcNow };
            await PersistAsync(ct);
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loaded) return;

        if (File.Exists(_filePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_filePath, ct);
                _workspaces = JsonSerializer.Deserialize<List<Workspace>>(json, _options)
                    ?? new List<Workspace>();
            }
            catch
            {
                // If deserialization fails, start with empty list
                _workspaces = new List<Workspace>();
            }
        }

        _loaded = true;
    }

    private async Task PersistAsync(CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(_workspaces, _options);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}

