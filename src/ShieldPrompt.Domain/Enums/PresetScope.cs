namespace ShieldPrompt.Domain.Enums;

/// <summary>
/// Defines the visibility scope of a preset.
/// </summary>
public enum PresetScope
{
    /// <summary>
    /// Preset is available across all workspaces.
    /// </summary>
    Global,

    /// <summary>
    /// Preset is specific to one workspace.
    /// </summary>
    Workspace
}
