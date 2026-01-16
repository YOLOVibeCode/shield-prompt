using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Action for creating a new file.
/// Undo: Deletes the created file and cleans up empty directories.
/// </summary>
public class FileCreateAction : IFileAction
{
    private readonly string _filePath;
    private readonly string _content;

    public Guid Id { get; } = Guid.NewGuid();
    public string Description => $"Create {Path.GetFileName(_filePath)}";
    public string FilePath => _filePath;
    public FileOperationType OperationType => FileOperationType.Create;

    public FileCreateAction(string filePath, string content)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _content = content ?? string.Empty;
    }

    public async Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        try
        {
            // Create directory if it doesn't exist
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Write file
            await File.WriteAllTextAsync(_filePath, _content, ct);

            return new ActionResult(true);
        }
        catch (Exception ex)
        {
            return new ActionResult(false, $"Failed to create file: {ex.Message}");
        }
    }

    public Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        try
        {
            // Delete the file if it exists
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);

                // Clean up empty directories
                CleanupEmptyDirectories();
            }

            return Task.FromResult(new ActionResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ActionResult(false, $"Failed to undo file creation: {ex.Message}"));
        }
    }

    private void CleanupEmptyDirectories()
    {
        var dir = Path.GetDirectoryName(_filePath);

        while (!string.IsNullOrEmpty(dir) &&
               Directory.Exists(dir) &&
               !Directory.EnumerateFileSystemEntries(dir).Any())
        {
            try
            {
                Directory.Delete(dir);
                dir = Path.GetDirectoryName(dir);
            }
            catch
            {
                // Stop if we can't delete (e.g., permissions, not empty after all)
                break;
            }
        }
    }
}

