using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Action for deleting a file.
/// Undo: Restores the file from backup.
/// </summary>
public class FileDeleteAction : IFileAction
{
    private readonly string _filePath;
    private readonly string _backupContent;

    public Guid Id { get; } = Guid.NewGuid();
    public string Description => $"Delete {Path.GetFileName(_filePath)}";
    public string FilePath => _filePath;
    public FileOperationType OperationType => FileOperationType.Delete;

    public FileDeleteAction(string filePath, string backupContent)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _backupContent = backupContent ?? string.Empty;
    }

    public Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            return Task.FromResult(new ActionResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Failed to delete file: {ex.Message}"));
        }
    }

    public async Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        try
        {
            // Recreate directory if needed
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Restore file
            await File.WriteAllTextAsync(_filePath, _backupContent, ct);
            return new ActionResult(true);
        }
        catch (Exception ex)
        {
            return new ActionResult(false, $"Failed to undo file deletion: {ex.Message}");
        }
    }
}

