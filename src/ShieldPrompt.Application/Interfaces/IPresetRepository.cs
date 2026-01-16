using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for preset persistence.
/// ISP-compliant: 5 methods for CRUD operations only.
/// </summary>
public interface IPresetRepository
{
    /// <summary>
    /// Gets all global presets (available across all workspaces).
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetGlobalPresetsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets presets for a specific workspace.
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetWorkspacePresetsAsync(string workspaceId, CancellationToken ct = default);

    /// <summary>
    /// Gets a preset by its ID.
    /// </summary>
    Task<PromptPreset?> GetByIdAsync(string presetId, CancellationToken ct = default);

    /// <summary>
    /// Saves or updates a preset.
    /// </summary>
    Task SaveAsync(PromptPreset preset, CancellationToken ct = default);

    /// <summary>
    /// Deletes a preset by ID.
    /// </summary>
    Task DeleteAsync(string presetId, CancellationToken ct = default);
}
