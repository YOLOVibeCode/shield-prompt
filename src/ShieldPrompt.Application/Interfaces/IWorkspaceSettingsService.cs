using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for workspace-specific settings.
/// Follows ISP - settings operations only.
/// </summary>
public interface IWorkspaceSettingsService
{
    /// <summary>
    /// Gets settings for a workspace.
    /// </summary>
    Task<WorkspaceSettings> GetSettingsAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Saves settings for a workspace.
    /// </summary>
    Task SaveSettingsAsync(string workspaceId, WorkspaceSettings settings, CancellationToken ct = default);
    
    /// <summary>
    /// Resets settings for a workspace to defaults.
    /// </summary>
    Task ResetSettingsAsync(string workspaceId, CancellationToken ct = default);
}

