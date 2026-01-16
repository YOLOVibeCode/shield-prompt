using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// A saved prompt configuration that can be reused.
/// Captures file selections, role, instructions, and model preferences.
/// </summary>
public record PromptPreset
{
    /// <summary>
    /// Unique identifier for the preset.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name for the preset.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional description of what the preset is for.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Emoji icon for visual identification.
    /// </summary>
    public string Icon { get; init; } = "📋";

    /// <summary>
    /// When this preset was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this preset was used.
    /// </summary>
    public DateTime LastUsed { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times this preset has been used.
    /// </summary>
    public int UsageCount { get; init; }

    /// <summary>
    /// Whether this preset is global or workspace-specific.
    /// </summary>
    public PresetScope Scope { get; init; } = PresetScope.Workspace;

    /// <summary>
    /// The workspace this preset belongs to (null for global presets).
    /// </summary>
    public string? WorkspaceId { get; init; }

    /// <summary>
    /// Glob patterns to match files (e.g., "**/*.cs").
    /// </summary>
    public IReadOnlyList<string> FilePatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Explicit file paths to include.
    /// </summary>
    public IReadOnlyList<string> ExplicitFilePaths { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Custom instructions to include in the prompt.
    /// </summary>
    public string CustomInstructions { get; init; } = string.Empty;

    /// <summary>
    /// ID of the role to use with this preset.
    /// </summary>
    public string? RoleId { get; init; }

    /// <summary>
    /// ID of the model to use with this preset.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Whether to include line numbers in output.
    /// </summary>
    public bool IncludeLineNumbers { get; init; }

    /// <summary>
    /// Whether this preset is pinned for quick access.
    /// </summary>
    public bool IsPinned { get; init; }

    /// <summary>
    /// Whether this is a built-in preset (cannot be deleted).
    /// </summary>
    public bool IsBuiltIn { get; init; }

    /// <summary>
    /// Creates a new preset with generated ID.
    /// </summary>
    public static PromptPreset Create(string name, string? workspaceId = null)
    {
        return new PromptPreset
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            WorkspaceId = workspaceId,
            Scope = workspaceId is null ? PresetScope.Global : PresetScope.Workspace
        };
    }

    /// <summary>
    /// Validates that the preset has required properties set.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name);
    }
}
