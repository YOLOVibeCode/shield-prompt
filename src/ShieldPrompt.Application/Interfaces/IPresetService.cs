using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for applying and managing presets.
/// ISP-compliant: 4 methods for preset operations only.
/// </summary>
public interface IPresetService
{
    /// <summary>
    /// Creates a preset from current session state.
    /// </summary>
    PromptPreset CreateFromSession(PromptSession session, string name);

    /// <summary>
    /// Applies a preset to a session by selecting matching files.
    /// </summary>
    Task<PresetApplication> ApplyToSessionAsync(
        PromptPreset preset,
        PromptSession session,
        FileNode rootNode,
        CancellationToken ct = default);

    /// <summary>
    /// Records that a preset was used (updates LastUsed and UsageCount).
    /// </summary>
    Task RecordUsageAsync(string presetId, CancellationToken ct = default);

    /// <summary>
    /// Gets pinned presets for quick access.
    /// </summary>
    Task<IReadOnlyList<PromptPreset>> GetPinnedPresetsAsync(string? workspaceId, CancellationToken ct = default);
}
