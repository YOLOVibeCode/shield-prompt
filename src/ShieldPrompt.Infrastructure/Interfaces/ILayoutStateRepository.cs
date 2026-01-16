namespace ShieldPrompt.Infrastructure.Interfaces;

/// <summary>
/// Repository for persisting UI layout state (panel sizes, positions).
/// ISP-compliant: only layout state concerns.
/// </summary>
public interface ILayoutStateRepository
{
    /// <summary>
    /// Saves the current layout state to persistent storage.
    /// </summary>
    Task SaveLayoutStateAsync(LayoutState state, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Loads the saved layout state from persistent storage.
    /// </summary>
    /// <returns>Saved layout state, or null if none exists.</returns>
    Task<LayoutState?> LoadLayoutStateAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets layout state to default values.
    /// </summary>
    Task ResetToDefaultAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Value object representing the layout state of the UI.
/// </summary>
public record LayoutState(
    double FileTreeWidth,
    double PromptBuilderHeight,
    bool IsFileTreeCollapsed,
    bool IsPromptBuilderCollapsed,
    bool IsPreviewCollapsed);

/// <summary>
/// Default layout dimensions.
/// </summary>
public static class LayoutDefaults
{
    public const double FileTreeWidth = 300;
    public const double PromptBuilderHeight = 0.5; // 50% of right panel
    public const bool IsFileTreeCollapsed = false;
    public const bool IsPromptBuilderCollapsed = false;
    public const bool IsPreviewCollapsed = false;
    
    public static LayoutState Default => new(
        FileTreeWidth,
        PromptBuilderHeight,
        IsFileTreeCollapsed,
        IsPromptBuilderCollapsed,
        IsPreviewCollapsed);
}

