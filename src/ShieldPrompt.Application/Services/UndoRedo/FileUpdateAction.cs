using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services.UndoRedo;

/// <summary>
/// Undoable action for file updates from AI response.
/// Allows complete restoration via backup.
/// </summary>
public class FileUpdateAction : IUndoableAction
{
    private readonly string _backupId;
    private readonly IFileWriterService _fileWriter;
    private readonly FileWriteResult _result;
    private readonly string _description;

    public FileUpdateAction(
        string backupId,
        FileWriteResult result,
        IFileWriterService fileWriter)
    {
        _backupId = backupId;
        _result = result;
        _fileWriter = fileWriter;
        
        // Build description for undo UI
        var parts = new List<string>();
        if (_result.FilesUpdated > 0) parts.Add($"{_result.FilesUpdated} updated");
        if (_result.FilesCreated > 0) parts.Add($"{_result.FilesCreated} created");
        if (_result.FilesDeleted > 0) parts.Add($"{_result.FilesDeleted} deleted");
        
        _description = $"AI file changes: {string.Join(", ", parts)}";
    }

    public string Description => _description;

    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public bool RequiresConfirmation => true;  // Important - ask user before undoing file changes!

    public string ConfirmationMessage => 
        $"⚠️ This will undo the AI file changes:\n\n" +
        $"Files modified: {_result.FilesUpdated + _result.FilesCreated + _result.FilesDeleted}\n\n" +
        $"All files will be restored to their state before AI modifications.\n\n" +
        $"Are you sure you want to continue?";

    public async Task ExecuteAsync()
    {
        // This action doesn't have an Execute - it's created after files are already updated
        await Task.CompletedTask;
    }

    public async Task UndoAsync()
    {
        // Restore files from backup
        if (!string.IsNullOrEmpty(_backupId))
        {
            await _fileWriter.RestoreBackupAsync(_backupId);
        }
    }

    public bool CanMergeWith(IUndoableAction other)
    {
        // File updates should never merge - each is a distinct operation
        return false;
    }

    public IUndoableAction MergeWith(IUndoableAction other)
    {
        // Never merges
        return this;
    }
}

