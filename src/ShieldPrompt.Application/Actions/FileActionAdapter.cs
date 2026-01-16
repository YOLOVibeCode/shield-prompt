using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Interfaces.Actions;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Adapter that wraps IFileAction to implement IUndoableAction.
/// Allows file actions to work with the existing UndoRedoManager.
/// </summary>
public class FileActionAdapter : IUndoableAction
{
    private readonly IFileAction _fileAction;
    private readonly DateTime _timestamp;

    public string Description => _fileAction.Description;
    public DateTime Timestamp => _timestamp;

    public FileActionAdapter(IFileAction fileAction)
    {
        _fileAction = fileAction ?? throw new ArgumentNullException(nameof(fileAction));
        _timestamp = DateTime.UtcNow;
    }

    public async Task ExecuteAsync()
    {
        var result = await _fileAction.ExecuteAsync();
        if (!result.Success)
        {
            throw new InvalidOperationException($"Action failed: {result.ErrorMessage}");
        }
    }

    public async Task UndoAsync()
    {
        var result = await _fileAction.UndoAsync();
        if (!result.Success)
        {
            throw new InvalidOperationException($"Undo failed: {result.ErrorMessage}");
        }
    }

    public bool CanMergeWith(IUndoableAction other) => false;

    public IUndoableAction MergeWith(IUndoableAction other) => throw new NotSupportedException();

    bool IUndoableAction.RequiresConfirmation => false;

    string? IUndoableAction.ConfirmationMessage => null;
}

