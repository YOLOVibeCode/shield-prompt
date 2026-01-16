namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Runtime state of a prompt session (not persisted).
/// </summary>
public record SessionState(
    string SessionId,
    string GeneratedPrompt,
    int TokenCount,
    bool HasUnsavedChanges,
    IReadOnlyList<SanitizationMatch>? SanitizationMatches = null);

