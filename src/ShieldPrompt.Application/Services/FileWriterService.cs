using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for safely writing files with backup support.
/// </summary>
public class FileWriterService : IFileWriterService
{
    private static readonly string BackupRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ShieldPrompt", "backups");

    public async Task<FileWriteResult> ApplyUpdatesAsync(
        IEnumerable<FileUpdate> updates,
        string baseDirectory,
        FileWriteOptions options,
        CancellationToken ct = default)
    {
        var updateList = updates.ToList();
        var errors = new List<string>();
        int created = 0, updated = 0, deleted = 0;
        string backupId = string.Empty;

        try
        {
            // Validate all paths first (security)
            foreach (var update in updateList)
            {
                if (!IsValidPath(update.FilePath, baseDirectory))
                {
                    errors.Add($"Invalid file path: {update.FilePath}");
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                return new FileWriteResult(0, 0, 0, string.Empty, errors);
            }

            // Dry run - don't actually write
            if (options.DryRun)
            {
                return new FileWriteResult(0, 0, 0, string.Empty, errors);
            }

            // Create backup of existing files
            if (options.CreateBackup)
            {
                var filesToBackup = updateList
                    .Where(u => u.Type != FileUpdateType.Create)
                    .Select(u => Path.Combine(baseDirectory, u.FilePath))
                    .Where(File.Exists)
                    .ToList();

                if (filesToBackup.Count > 0)
                {
                    backupId = await CreateBackupAsync(filesToBackup, ct);
                }
                else
                {
                    backupId = Guid.NewGuid().ToString();
                }
            }

            // Apply each update
            foreach (var update in updateList)
            {
                var fullPath = Path.Combine(baseDirectory, update.FilePath);

                try
                {
                    switch (update.Type)
                    {
                        case FileUpdateType.Create:
                        case FileUpdateType.Update:
                            await WriteFileAsync(fullPath, update.Content, options, ct);
                            if (update.Type == FileUpdateType.Create)
                                created++;
                            else
                                updated++;
                            break;

                        case FileUpdateType.Delete:
                            if (!options.AllowDelete)
                            {
                                errors.Add($"Delete not allowed for: {update.FilePath}");
                                continue;
                            }
                            DeleteFile(fullPath);
                            deleted++;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing {update.FilePath}: {ex.Message}");
                }
            }

            return new FileWriteResult(created, updated, deleted, backupId, errors);
        }
        catch (Exception ex)
        {
            // Rollback if we created a backup
            if (!string.IsNullOrEmpty(backupId) && options.CreateBackup)
            {
                try
                {
                    await RestoreBackupAsync(backupId, ct);
                }
                catch
                {
                    // Best effort restore
                }
            }

            errors.Add($"Critical error: {ex.Message}");
            return new FileWriteResult(0, 0, 0, backupId, errors);
        }
    }

    public async Task<string> CreateBackupAsync(
        IEnumerable<string> filePaths,
        CancellationToken ct = default)
    {
        var backupId = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}";
        var backupDir = Path.Combine(BackupRoot, backupId);
        
        Directory.CreateDirectory(backupDir);

        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
                continue;

            var fileName = Path.GetFileName(filePath);
            var backupPath = Path.Combine(backupDir, fileName + ".bak");
            
            // Ensure subdirectory exists if needed
            var backupSubDir = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupSubDir))
            {
                Directory.CreateDirectory(backupSubDir);
            }

            await File.ReadAllBytesAsync(filePath, ct)
                .ContinueWith(async data => 
                    await File.WriteAllBytesAsync(backupPath, data.Result, ct), ct);
        }

        // Write manifest
        var manifest = new
        {
            BackupId = backupId,
            CreatedAt = DateTime.UtcNow,
            Files = filePaths.Select(f => new { OriginalPath = f, BackupName = Path.GetFileName(f) + ".bak" }).ToList()
        };

        var manifestPath = Path.Combine(backupDir, "manifest.json");
        var manifestJson = System.Text.Json.JsonSerializer.Serialize(manifest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, manifestJson, ct);

        return backupId;
    }

    public async Task RestoreBackupAsync(string backupId, CancellationToken ct = default)
    {
        var backupDir = Path.Combine(BackupRoot, backupId);
        
        if (!Directory.Exists(backupDir))
        {
            throw new InvalidOperationException($"Backup not found: {backupId}");
        }

        var manifestPath = Path.Combine(backupDir, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException($"Backup manifest not found: {backupId}");
        }

        var manifestJson = await File.ReadAllTextAsync(manifestPath, ct);
        var manifest = System.Text.Json.JsonSerializer.Deserialize<BackupManifest>(manifestJson);

        if (manifest == null || manifest.Files == null)
        {
            throw new InvalidOperationException($"Invalid backup manifest: {backupId}");
        }

        // Restore each file
        foreach (var fileInfo in manifest.Files)
        {
            var backupFilePath = Path.Combine(backupDir, fileInfo.BackupName);
            if (File.Exists(backupFilePath))
            {
                var content = await File.ReadAllBytesAsync(backupFilePath, ct);
                await File.WriteAllBytesAsync(fileInfo.OriginalPath, content, ct);
            }
        }
    }

    private async Task WriteFileAsync(
        string filePath,
        string content,
        FileWriteOptions options,
        CancellationToken ct)
    {
        // Create directory if needed
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            if (!options.AllowCreateDirectories)
            {
                throw new InvalidOperationException($"Directory creation not allowed: {directory}");
            }
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, ct);
    }

    private void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private bool IsValidPath(string filePath, string baseDirectory)
    {
        // Prevent directory traversal attacks
        if (filePath.Contains(".."))
            return false;

        if (Path.IsPathRooted(filePath))
            return false;

        // Check for invalid path characters
        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return false;

        return true;
    }

    private class BackupManifest
    {
        public string? BackupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BackupFileInfo>? Files { get; set; }
    }

    private class BackupFileInfo
    {
        public string? OriginalPath { get; set; }
        public string? BackupName { get; set; }
    }
}

