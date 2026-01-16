using System.Text.Json;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// File-based implementation of IBackupService.
/// Stores backups in a dedicated backup directory.
/// </summary>
public class FileBackupService : IBackupService
{
    private readonly string _backupRoot;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new FileBackupService using the default backup path.
    /// </summary>
    public FileBackupService()
    {
        _backupRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".shieldprompt",
            "backups");
        Directory.CreateDirectory(_backupRoot);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Creates a new FileBackupService using a custom backup path.
    /// Used for testing.
    /// </summary>
    public FileBackupService(string backupRoot)
    {
        _backupRoot = backupRoot;
        Directory.CreateDirectory(_backupRoot);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<string> CreateBackupAsync(
        IEnumerable<string> filePaths,
        CancellationToken ct = default)
    {
        var backupId = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}";
        var backupDir = Path.Combine(_backupRoot, backupId);
        Directory.CreateDirectory(backupDir);

        var backedUpFiles = new List<BackupFileEntry>();

        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath))
                continue;

            try
            {
                var relativeName = Path.GetFileName(filePath);
                var backupPath = Path.Combine(backupDir, $"{backedUpFiles.Count}_{relativeName}");

                await CopyFileAsync(filePath, backupPath, ct);

                backedUpFiles.Add(new BackupFileEntry
                {
                    OriginalPath = filePath,
                    BackupFileName = Path.GetFileName(backupPath)
                });
            }
            catch
            {
                // Skip files that can't be backed up
            }
        }

        // Save metadata
        var metadata = new BackupMetadata
        {
            Id = backupId,
            CreatedAt = DateTime.UtcNow,
            Files = backedUpFiles
        };

        var metadataPath = Path.Combine(backupDir, "metadata.json");
        var json = JsonSerializer.Serialize(metadata, _jsonOptions);
        await File.WriteAllTextAsync(metadataPath, json, ct);

        return backupId;
    }

    /// <inheritdoc />
    public async Task<bool> RestoreBackupAsync(string backupId, CancellationToken ct = default)
    {
        var backupDir = Path.Combine(_backupRoot, backupId);
        var metadataPath = Path.Combine(backupDir, "metadata.json");

        if (!File.Exists(metadataPath))
            return false;

        try
        {
            var json = await File.ReadAllTextAsync(metadataPath, ct);
            var metadata = JsonSerializer.Deserialize<BackupMetadata>(json, _jsonOptions);

            if (metadata?.Files == null)
                return false;

            foreach (var file in metadata.Files)
            {
                var backupPath = Path.Combine(backupDir, file.BackupFileName);
                if (File.Exists(backupPath))
                {
                    // Ensure directory exists
                    var dir = Path.GetDirectoryName(file.OriginalPath);
                    if (dir != null && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    await CopyFileAsync(backupPath, file.OriginalPath, ct);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Task DeleteBackupAsync(string backupId, CancellationToken ct = default)
    {
        var backupDir = Path.Combine(_backupRoot, backupId);

        if (Directory.Exists(backupDir))
        {
            Directory.Delete(backupDir, recursive: true);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BackupInfo>> ListBackupsAsync(CancellationToken ct = default)
    {
        var results = new List<BackupInfo>();

        if (!Directory.Exists(_backupRoot))
            return results;

        foreach (var dir in Directory.GetDirectories(_backupRoot))
        {
            var metadataPath = Path.Combine(dir, "metadata.json");
            if (!File.Exists(metadataPath))
                continue;

            try
            {
                var json = await File.ReadAllTextAsync(metadataPath, ct);
                var metadata = JsonSerializer.Deserialize<BackupMetadata>(json, _jsonOptions);

                if (metadata != null)
                {
                    results.Add(new BackupInfo(
                        metadata.Id,
                        metadata.CreatedAt,
                        metadata.Files.Count,
                        metadata.Files.Select(f => f.OriginalPath).ToList()));
                }
            }
            catch
            {
                // Skip corrupted backups
            }
        }

        return results.OrderByDescending(b => b.CreatedAt).ToList();
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken ct)
    {
        var content = await File.ReadAllBytesAsync(source, ct);
        await File.WriteAllBytesAsync(destination, content, ct);
    }

    private class BackupMetadata
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<BackupFileEntry> Files { get; set; } = new();
    }

    private class BackupFileEntry
    {
        public string OriginalPath { get; set; } = string.Empty;
        public string BackupFileName { get; set; } = string.Empty;
    }
}
