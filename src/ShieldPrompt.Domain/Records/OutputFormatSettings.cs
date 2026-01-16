using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Configuration settings for output format and response handling.
/// Immutable record for thread-safety.
/// </summary>
public record OutputFormatSettings(
    ResponseFormat PreferredFormat = ResponseFormat.HybridXmlMarkdown,
    bool EnablePartialUpdates = true,
    bool EnableAutoApply = false,
    AutoApplyMode AutoApplyBehavior = AutoApplyMode.PreviewThenPrompt,
    ConflictResolutionStrategy ConflictStrategy = ConflictResolutionStrategy.PromptUser,
    bool IncludeSessionMetadata = true,
    bool IncludeTokenCounts = true,
    bool IncludeTimestamps = false,
    int MaxResponseSizeMB = 10,
    int ParserTimeoutSeconds = 30)
{
    /// <summary>
    /// Default settings (safest configuration).
    /// </summary>
    public static OutputFormatSettings Default => new();
}

/// <summary>
/// Auto-apply behavior modes.
/// </summary>
public enum AutoApplyMode
{
    /// <summary>
    /// Show preview dialog, then prompt for confirmation (safest).
    /// </summary>
    PreviewThenPrompt,
    
    /// <summary>
    /// Show 5-second countdown, auto-apply with backup (recommended).
    /// </summary>
    CountdownWithBackup,
    
    /// <summary>
    /// Apply immediately with no confirmation (dangerous!).
    /// </summary>
    ImmediateApply
}

/// <summary>
/// Strategy for handling file conflicts.
/// </summary>
public enum ConflictResolutionStrategy
{
    /// <summary>
    /// Always prompt user for each conflict (safest).
    /// </summary>
    PromptUser,
    
    /// <summary>
    /// Always skip conflicted files.
    /// </summary>
    AlwaysSkip,
    
    /// <summary>
    /// Always overwrite conflicted files (dangerous!).
    /// </summary>
    AlwaysOverwrite,
    
    /// <summary>
    /// Attempt smart merge if possible, otherwise prompt.
    /// </summary>
    AutoMerge
}

