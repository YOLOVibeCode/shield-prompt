using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for applying file operations from LLM responses.
/// </summary>
public class FileApplyService : IFileApplyService
{
    private readonly IBackupService _backupService;
    private readonly IDiffService _diffService;

    public FileApplyService(IBackupService backupService, IDiffService diffService)
    {
        _backupService = backupService;
        _diffService = diffService;
    }

    /// <inheritdoc />
    public async Task<ApplyPreview> PreviewAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var previews = new List<FileOperationPreview>();
        var warnings = new List<string>();
        int created = 0, updated = 0, deleted = 0;

        foreach (var op in operations)
        {
            var fullPath = Path.Combine(workspaceRoot, op.Path);
            var currentContent = File.Exists(fullPath)
                ? await File.ReadAllTextAsync(fullPath, ct)
                : null;

            var diff = op.Type switch
            {
                FileOperationType.Update when currentContent != null =>
                    _diffService.ComputeDiff(currentContent, op.Content ?? ""),
                FileOperationType.PartialUpdate when currentContent != null =>
                    _diffService.ComputeDiff(currentContent, op.Content ?? ""),
                _ => null
            };

            var hasConflict = HasConflict(op, fullPath);
            if (hasConflict)
                warnings.Add($"Conflict detected: {op.Path}");

            previews.Add(new FileOperationPreview(
                op, currentContent, op.Content, diff, hasConflict));

            switch (op.Type)
            {
                case FileOperationType.Create: created++; break;
                case FileOperationType.Update:
                case FileOperationType.PartialUpdate: updated++; break;
                case FileOperationType.Delete: deleted++; break;
            }
        }

        return new ApplyPreview(
            previews, previews.Count, created, updated, deleted, warnings);
    }

    /// <inheritdoc />
    public async Task<ApplyResult> ApplyAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var opList = operations.ToList();

        // Create backup of existing files first
        var filesToBackup = opList
            .Where(op => op.Type != FileOperationType.Create)
            .Select(op => Path.Combine(workspaceRoot, op.Type == FileOperationType.Rename ? op.OriginalPath ?? op.Path : op.Path))
            .Where(File.Exists)
            .ToList();

        var backupId = await _backupService.CreateBackupAsync(filesToBackup, ct);

        var results = new List<AppliedOperation>();
        var errors = new List<string>();

        foreach (var op in opList)
        {
            try
            {
                await ApplyOperationAsync(op, workspaceRoot, ct);
                results.Add(new AppliedOperation(op, true, null, null));
            }
            catch (Exception ex)
            {
                errors.Add($"{op.Path}: {ex.Message}");
                results.Add(new AppliedOperation(op, false, ex.Message, null));
            }
        }

        return new ApplyResult(
            SuccessCount: results.Count(r => r.Success),
            FailureCount: results.Count(r => !r.Success),
            Operations: results,
            Errors: errors,
            BackupId: backupId);
    }

    /// <inheritdoc />
    public async Task<ApplyResult> ApplySelectiveAsync(
        IEnumerable<FileOperation> operations,
        IEnumerable<string> selectedPaths,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var selectedSet = selectedPaths.ToHashSet();
        var filtered = operations.Where(op => selectedSet.Contains(op.Path));
        return await ApplyAsync(filtered, workspaceRoot, ct);
    }

    /// <inheritdoc />
    public async Task<bool> UndoAsync(string backupId, CancellationToken ct = default)
    {
        return await _backupService.RestoreBackupAsync(backupId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ConflictInfo>> CheckConflictsAsync(
        IEnumerable<FileOperation> operations,
        string workspaceRoot,
        CancellationToken ct = default)
    {
        var conflicts = new List<ConflictInfo>();

        foreach (var op in operations)
        {
            var fullPath = Path.Combine(workspaceRoot, op.Path);
            var exists = File.Exists(fullPath);

            if (op.Type == FileOperationType.Create && exists)
            {
                conflicts.Add(new ConflictInfo(
                    op.Path,
                    ConflictType.FileCreatedExists,
                    await File.ReadAllTextAsync(fullPath, ct),
                    op.Content,
                    null));
            }
            else if (op.Type == FileOperationType.Delete && !exists)
            {
                conflicts.Add(new ConflictInfo(
                    op.Path,
                    ConflictType.FileDeleted,
                    null, null, null));
            }
        }

        return conflicts;
    }

    private async Task ApplyOperationAsync(
        FileOperation op,
        string workspaceRoot,
        CancellationToken ct)
    {
        var fullPath = Path.Combine(workspaceRoot, op.Path);
        var directory = Path.GetDirectoryName(fullPath);

        switch (op.Type)
        {
            case FileOperationType.Create:
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                await File.WriteAllTextAsync(fullPath, op.Content ?? "", ct);
                break;

            case FileOperationType.Update:
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                await File.WriteAllTextAsync(fullPath, op.Content ?? "", ct);
                break;

            case FileOperationType.PartialUpdate:
                await ApplyPartialUpdateAsync(fullPath, op, ct);
                break;

            case FileOperationType.Delete:
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                break;

            case FileOperationType.Rename:
                if (op.OriginalPath != null)
                {
                    var sourcePath = Path.Combine(workspaceRoot, op.OriginalPath);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    if (File.Exists(sourcePath))
                        File.Move(sourcePath, fullPath);
                }
                break;
        }
    }

    private static async Task ApplyPartialUpdateAsync(
        string fullPath,
        FileOperation op,
        CancellationToken ct)
    {
        if (op.StartLine == null || op.EndLine == null || op.Content == null)
            throw new InvalidOperationException("Partial update requires StartLine, EndLine, and Content");

        var lines = (await File.ReadAllLinesAsync(fullPath, ct)).ToList();
        var newLines = op.Content.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        // Remove old lines and insert new
        var startIndex = op.StartLine.Value - 1;
        var removeCount = op.EndLine.Value - op.StartLine.Value + 1;

        if (startIndex >= 0 && startIndex < lines.Count)
        {
            removeCount = Math.Min(removeCount, lines.Count - startIndex);
            lines.RemoveRange(startIndex, removeCount);
            lines.InsertRange(startIndex, newLines);
        }

        await File.WriteAllLinesAsync(fullPath, lines, ct);
    }

    private static bool HasConflict(FileOperation op, string fullPath)
    {
        var exists = File.Exists(fullPath);

        return op.Type switch
        {
            FileOperationType.Create => exists,
            FileOperationType.Delete => !exists,
            _ => false
        };
    }
}
