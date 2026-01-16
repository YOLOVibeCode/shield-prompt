using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for applying file operations from LLM responses.
/// ISP-compliant: 5 methods for apply operations only.
/// </summary>
public interface IFileApplyService
{
    /// <summary>
    /// Previews operations without applying them.
    /// </summary>
    Task<ApplyPreview> PreviewAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default);

    /// <summary>
    /// Applies all operations with automatic backup.
    /// </summary>
    Task<ApplyResult> ApplyAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default);

    /// <summary>
    /// Applies only the selected operations.
    /// </summary>
    Task<ApplyResult> ApplySelectiveAsync(
        IEnumerable<FileOperation> operations,
        IEnumerable<string> selectedPaths,
        string workspaceRoot,
        CancellationToken ct = default);

    /// <summary>
    /// Undoes the last apply operation by restoring from backup.
    /// </summary>
    Task<bool> UndoAsync(string backupId, CancellationToken ct = default);

    /// <summary>
    /// Checks for conflicts before applying operations.
    /// </summary>
    Task<IReadOnlyList<ConflictInfo>> CheckConflictsAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default);
}

/// <summary>
/// Preview of operations before applying.
/// </summary>
/// <param name="Previews">Preview details for each operation.</param>
/// <param name="TotalFiles">Total number of files affected.</param>
/// <param name="CreatedCount">Number of files to be created.</param>
/// <param name="UpdatedCount">Number of files to be updated.</param>
/// <param name="DeletedCount">Number of files to be deleted.</param>
/// <param name="Warnings">Any warnings detected during preview.</param>
public record ApplyPreview(
    IReadOnlyList<FileOperationPreview> Previews,
    int TotalFiles,
    int CreatedCount,
    int UpdatedCount,
    int DeletedCount,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Preview of a single file operation.
/// </summary>
/// <param name="Operation">The file operation to be applied.</param>
/// <param name="CurrentContent">Current content of the file, if it exists.</param>
/// <param name="ProposedContent">New content to be written.</param>
/// <param name="Diff">Computed diff between current and proposed content.</param>
/// <param name="HasConflict">Whether a conflict was detected.</param>
public record FileOperationPreview(
    FileOperation Operation,
    string? CurrentContent,
    string? ProposedContent,
    IReadOnlyList<DiffLine>? Diff,
    bool HasConflict);
