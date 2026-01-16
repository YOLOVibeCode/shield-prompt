namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Result of applying file operations.
/// </summary>
/// <param name="SuccessCount">Number of operations that succeeded.</param>
/// <param name="FailureCount">Number of operations that failed.</param>
/// <param name="Operations">Details of each applied operation.</param>
/// <param name="Errors">Error messages from failed operations.</param>
/// <param name="BackupId">ID of the backup created before applying.</param>
public record ApplyResult(
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<AppliedOperation> Operations,
    IReadOnlyList<string> Errors,
    string BackupId)
{
    /// <summary>
    /// Whether all operations succeeded.
    /// </summary>
    public bool AllSucceeded => FailureCount == 0;

    /// <summary>
    /// Whether any operations succeeded.
    /// </summary>
    public bool AnySucceeded => SuccessCount > 0;

    /// <summary>
    /// Total number of operations attempted.
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;
}

/// <summary>
/// Details of a single applied operation.
/// </summary>
/// <param name="Operation">The file operation that was applied.</param>
/// <param name="Success">Whether the operation succeeded.</param>
/// <param name="Error">Error message if the operation failed.</param>
/// <param name="BackupPath">Path to the backup of the original file, if applicable.</param>
public record AppliedOperation(
    FileOperation Operation,
    bool Success,
    string? Error,
    string? BackupPath);
