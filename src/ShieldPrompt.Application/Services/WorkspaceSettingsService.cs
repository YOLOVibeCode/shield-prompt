using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for workspace-specific settings.
/// </summary>
public class WorkspaceSettingsService : IWorkspaceSettingsService
{
    private readonly IWorkspaceRepository _repository;

    public WorkspaceSettingsService(IWorkspaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkspaceSettings> GetSettingsAsync(string workspaceId, CancellationToken ct = default)
    {
        var workspace = await _repository.GetByIdAsync(workspaceId, ct);
        return workspace?.Settings ?? new WorkspaceSettings();
    }

    public async Task SaveSettingsAsync(string workspaceId, WorkspaceSettings settings, CancellationToken ct = default)
    {
        var workspace = await _repository.GetByIdAsync(workspaceId, ct);
        if (workspace == null)
        {
            throw new InvalidOperationException($"Workspace {workspaceId} not found");
        }

        var updated = workspace with { Settings = settings };
        await _repository.SaveAsync(updated, ct);
    }

    public async Task ResetSettingsAsync(string workspaceId, CancellationToken ct = default)
    {
        var defaultSettings = new WorkspaceSettings();
        await SaveSettingsAsync(workspaceId, defaultSettings, ct);
    }
}

