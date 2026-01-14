using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services.UndoRedo;

/// <summary>
/// Undoable action for file selection changes.
/// Can be merged with other selections within time window.
/// </summary>
public class FileSelectionAction : IUndoableAction
{
    private readonly List<string> _selectedPaths;
    private readonly List<string> _previousPaths;
    private readonly Action<IEnumerable<string>> _applySelection;

    public FileSelectionAction(
        IEnumerable<string> previousPaths,
        IEnumerable<string> selectedPaths,
        Action<IEnumerable<string>> applySelection)
    {
        _previousPaths = previousPaths.ToList();
        _selectedPaths = selectedPaths.ToList();
        _applySelection = applySelection;
        Timestamp = DateTime.UtcNow;
    }

    public string Description => $"Select {_selectedPaths.Count} files";
    public DateTime Timestamp { get; }

    public Task ExecuteAsync()
    {
        _applySelection(_selectedPaths);
        return Task.CompletedTask;
    }

    public Task UndoAsync()
    {
        _applySelection(_previousPaths);
        return Task.CompletedTask;
    }

    public bool CanMergeWith(IUndoableAction other)
    {
        return other is FileSelectionAction;
    }

    public IUndoableAction MergeWith(IUndoableAction other)
    {
        if (other is FileSelectionAction otherSelection)
        {
            // Keep our previous state, use their new state
            return new FileSelectionAction(
                _previousPaths,
                otherSelection._selectedPaths,
                _applySelection);
        }
        return this;
    }
}

