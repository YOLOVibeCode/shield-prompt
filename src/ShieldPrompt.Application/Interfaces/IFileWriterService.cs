namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for safely writing files with backup support.
/// ISP-compliant: focused on file operations only.
/// </summary>
public interface IFileWriterService
{
    /// <summary>
    /// Applies file updates with safety checks and backups.
    /// </summary>
    Task<FileWriteResult> ApplyUpdatesAsync(
        IEnumerable<FileUpdate> updates,
        string baseDirectory,
        FileWriteOptions options,
        CancellationToken ct = default);
    
    /// <summary>
    /// Creates backup of files before modification.
    /// </summary>
    Task<string> CreateBackupAsync(
        IEnumerable<string> filePaths,
        CancellationToken ct = default);
    
    /// <summary>
    /// Restores files from backup.
    /// </summary>
    Task RestoreBackupAsync(
        string backupId,
        CancellationToken ct = default);
}

/// <summary>
/// Result of file write operations.
/// </summary>
public record FileWriteResult(
    int FilesCreated,
    int FilesUpdated,
    int FilesDeleted,
    string BackupId,
    IReadOnlyList<string> Errors);

/// <summary>
/// Options for file write operations.
/// </summary>
public record FileWriteOptions
{
    public bool CreateBackup { get; init; } = true;
    public bool AllowCreateDirectories { get; init; } = true;
    public bool AllowDelete { get; init; } = false;
    public bool DryRun { get; init; } = false;
}

