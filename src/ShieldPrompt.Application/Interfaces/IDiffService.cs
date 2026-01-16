namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for computing file diffs.
/// ISP-compliant: 2 methods for diff operations only.
/// </summary>
public interface IDiffService
{
    /// <summary>
    /// Computes a diff between two strings.
    /// </summary>
    /// <param name="original">The original content.</param>
    /// <param name="modified">The modified content.</param>
    /// <returns>List of diff lines showing changes.</returns>
    IReadOnlyList<DiffLine> ComputeDiff(string original, string modified);

    /// <summary>
    /// Generates a unified diff format string.
    /// </summary>
    /// <param name="original">The original content.</param>
    /// <param name="modified">The modified content.</param>
    /// <param name="filePath">The file path for the diff header.</param>
    /// <returns>Unified diff format string.</returns>
    string GenerateUnifiedDiff(string original, string modified, string filePath);
}

/// <summary>
/// A single line in a diff output.
/// </summary>
/// <param name="Type">The type of diff line (added, removed, etc.).</param>
/// <param name="OldLineNumber">Line number in the original file (null for added lines).</param>
/// <param name="NewLineNumber">Line number in the modified file (null for removed lines).</param>
/// <param name="Content">The content of the line.</param>
public record DiffLine(
    DiffLineType Type,
    int? OldLineNumber,
    int? NewLineNumber,
    string Content);

/// <summary>
/// Type of line in a diff.
/// </summary>
public enum DiffLineType
{
    /// <summary>
    /// Line is unchanged (context).
    /// </summary>
    Context,

    /// <summary>
    /// Line was added.
    /// </summary>
    Added,

    /// <summary>
    /// Line was removed.
    /// </summary>
    Removed,

    /// <summary>
    /// Line was modified (combination of remove + add).
    /// </summary>
    Modified
}
