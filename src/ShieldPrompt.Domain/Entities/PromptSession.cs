namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a single prompt session (tab) within a workspace.
/// </summary>
public record PromptSession
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string Name { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastModified { get; init; } = DateTime.UtcNow;
    
    // Per-session state
    public IReadOnlyList<string> SelectedFilePaths { get; init; } = Array.Empty<string>();
    public string CustomInstructions { get; init; } = string.Empty;
    public string? SelectedRoleId { get; init; }
    public string? SelectedModelId { get; init; }
    
    // UI state
    public double PreviewScrollPosition { get; init; }
    public bool IsPinned { get; init; }
    
    /// <summary>
    /// Creates a new session with default values.
    /// </summary>
    public static PromptSession CreateNew(string workspaceId, string name)
    {
        return new PromptSession
        {
            Id = Guid.NewGuid().ToString(),
            WorkspaceId = workspaceId,
            Name = name
        };
    }
}

