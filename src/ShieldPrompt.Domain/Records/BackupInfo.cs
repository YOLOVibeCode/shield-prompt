namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Information about a file backup.
/// </summary>
/// <param name="Id">Unique identifier for the backup.</param>
/// <param name="CreatedAt">When the backup was created.</param>
/// <param name="FileCount">Number of files in the backup.</param>
/// <param name="FilePaths">Paths of the files that were backed up.</param>
public record BackupInfo(
    string Id,
    DateTime CreatedAt,
    int FileCount,
    IReadOnlyList<string> FilePaths)
{
    /// <summary>
    /// Whether the backup contains any files.
    /// </summary>
    public bool HasFiles => FileCount > 0;

    /// <summary>
    /// Age of the backup.
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - CreatedAt;
}
