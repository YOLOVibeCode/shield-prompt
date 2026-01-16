using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing file backups before applying changes.
/// ISP-compliant: 4 methods for backup operations only.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a backup of the specified files.
    /// </summary>
    /// <param name="filePaths">Absolute paths to the files to back up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The backup ID for later restoration.</returns>
    Task<string> CreateBackupAsync(
        IEnumerable<string> filePaths,
        CancellationToken ct = default);

    /// <summary>
    /// Restores files from a backup.
    /// </summary>
    /// <param name="backupId">The backup ID returned from CreateBackupAsync.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if restoration succeeded.</returns>
    Task<bool> RestoreBackupAsync(string backupId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a backup to free up disk space.
    /// </summary>
    /// <param name="backupId">The backup ID to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteBackupAsync(string backupId, CancellationToken ct = default);

    /// <summary>
    /// Lists all available backups.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of available backups ordered by creation time (newest first).</returns>
    Task<IReadOnlyList<BackupInfo>> ListBackupsAsync(CancellationToken ct = default);
}
