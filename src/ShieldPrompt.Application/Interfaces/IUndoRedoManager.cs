namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Manages undo/redo history with intelligent batching.
/// ISP-compliant: focused on history management only.
/// </summary>
public interface IUndoRedoManager
{
    /// <summary>
    /// Executes an action and adds it to undo history.
    /// </summary>
    Task ExecuteAsync(IUndoableAction action);

    /// <summary>
    /// Undoes the last action.
    /// </summary>
    Task<bool> UndoAsync();

    /// <summary>
    /// Redoes the last undone action.
    /// </summary>
    Task<bool> RedoAsync();

    /// <summary>
    /// Whether undo is available.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Whether redo is available.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Description of the next undo action.
    /// </summary>
    string? UndoDescription { get; }

    /// <summary>
    /// Description of the next redo action.
    /// </summary>
    string? RedoDescription { get; }

    /// <summary>
    /// Clears all history.
    /// </summary>
    void Clear();

    /// <summary>
    /// Event raised when undo/redo state changes.
    /// </summary>
    event EventHandler? StateChanged;
}

