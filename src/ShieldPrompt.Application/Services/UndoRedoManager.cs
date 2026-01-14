using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Manages undo/redo history with intelligent action batching.
/// Automatically merges consecutive mergeable actions.
/// </summary>
public class UndoRedoManager : IUndoRedoManager
{
    private readonly Stack<IUndoableAction> _undoStack = new();
    private readonly Stack<IUndoableAction> _redoStack = new();
    private readonly TimeSpan _mergeWindow = TimeSpan.FromSeconds(2);

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public string? UndoDescription => CanUndo ? _undoStack.Peek().Description : null;
    public string? RedoDescription => CanRedo ? _redoStack.Peek().Description : null;

    public event EventHandler? StateChanged;

    public async Task ExecuteAsync(IUndoableAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Execute the action
        await action.ExecuteAsync().ConfigureAwait(false);

        // Try to merge with last action if within time window
        if (_undoStack.Count > 0)
        {
            var lastAction = _undoStack.Peek();
            var timeSinceLastAction = action.Timestamp - lastAction.Timestamp;

            if (timeSinceLastAction <= _mergeWindow && lastAction.CanMergeWith(action))
            {
                // Merge instead of adding new action
                var merged = lastAction.MergeWith(action);
                _undoStack.Pop();
                _undoStack.Push(merged);
                
                OnStateChanged();
                return;
            }
        }

        // Add to undo stack
        _undoStack.Push(action);

        // Clear redo stack (new action invalidates redo history)
        _redoStack.Clear();

        OnStateChanged();
    }

    public async Task<bool> UndoAsync()
    {
        if (!CanUndo)
            return false;

        var action = _undoStack.Pop();
        await action.UndoAsync().ConfigureAwait(false);

        _redoStack.Push(action);
        
        OnStateChanged();
        return true;
    }

    public async Task<bool> RedoAsync()
    {
        if (!CanRedo)
            return false;

        var action = _redoStack.Pop();
        await action.ExecuteAsync().ConfigureAwait(false);

        _undoStack.Push(action);
        
        OnStateChanged();
        return true;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

