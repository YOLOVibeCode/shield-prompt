namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Information about a file conflict detected before applying changes.
/// </summary>
/// <param name="FilePath">Path to the conflicted file.</param>
/// <param name="Type">Type of conflict detected.</param>
/// <param name="CurrentContent">Current content of the file, if it exists.</param>
/// <param name="ProposedContent">Content proposed by the LLM.</param>
/// <param name="OriginalContent">Content at the time the prompt was generated, if available.</param>
public record ConflictInfo(
    string FilePath,
    ConflictType Type,
    string? CurrentContent,
    string? ProposedContent,
    string? OriginalContent)
{
    /// <summary>
    /// A human-readable description of the conflict.
    /// </summary>
    public string Description => Type switch
    {
        ConflictType.FileModified => $"File '{FilePath}' has been modified since the prompt was generated",
        ConflictType.FileDeleted => $"File '{FilePath}' was deleted and cannot be updated",
        ConflictType.FileCreatedExists => $"Cannot create '{FilePath}' because it already exists",
        ConflictType.MergeConflict => $"Merge conflict detected in '{FilePath}'",
        _ => $"Unknown conflict in '{FilePath}'"
    };
}

/// <summary>
/// Types of file conflicts that can be detected.
/// </summary>
public enum ConflictType
{
    /// <summary>
    /// File was modified since the prompt was generated.
    /// </summary>
    FileModified,

    /// <summary>
    /// File was deleted and cannot be updated.
    /// </summary>
    FileDeleted,

    /// <summary>
    /// Attempting to CREATE a file that already exists.
    /// </summary>
    FileCreatedExists,

    /// <summary>
    /// Partial update results in a merge conflict.
    /// </summary>
    MergeConflict
}
