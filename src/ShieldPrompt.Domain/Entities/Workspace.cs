namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a workspace (folder) that can be opened in ShieldPrompt v2.0.
/// </summary>
public record Workspace
{
    /// <summary>
    /// Unique identifier for the workspace (GUID).
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the workspace (defaults to folder name).
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Absolute path to the workspace root directory.
    /// </summary>
    public string RootPath { get; init; } = string.Empty;

    /// <summary>
    /// When this workspace was first created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this workspace was opened.
    /// </summary>
    public DateTime LastOpened { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// List of file paths that were pinned/favorited in this workspace.
    /// </summary>
    public IReadOnlyList<string> PinnedFiles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Workspace-specific settings.
    /// </summary>
    public WorkspaceSettings Settings { get; init; } = new();

    /// <summary>
    /// Validates that the workspace has required properties set.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(RootPath) &&
               Directory.Exists(RootPath);
    }
}

/// <summary>
/// Workspace-specific settings that persist per workspace.
/// </summary>
public record WorkspaceSettings
{
    /// <summary>
    /// Whether sanitization is enabled for this workspace.
    /// </summary>
    public bool SanitizationEnabled { get; init; } = true;

    /// <summary>
    /// Default role ID to use when opening this workspace.
    /// </summary>
    public string DefaultRoleId { get; init; } = "general_review";

    /// <summary>
    /// File patterns to ignore (e.g., "node_modules", "*.log").
    /// </summary>
    public IReadOnlyList<string> IgnorePatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// File patterns to include (if specified, only these patterns are included).
    /// </summary>
    public IReadOnlyList<string> IncludePatterns { get; init; } = Array.Empty<string>();
}

