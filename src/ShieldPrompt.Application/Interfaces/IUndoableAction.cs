namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Represents an action that can be undone and redone.
/// Command pattern for undo/redo functionality.
/// </summary>
public interface IUndoableAction
{
    /// <summary>
    /// Human-readable description of this action.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Timestamp when this action was executed.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Executes or re-executes this action.
    /// </summary>
    Task ExecuteAsync();

    /// <summary>
    /// Undoes this action, restoring previous state.
    /// </summary>
    Task UndoAsync();

    /// <summary>
    /// Whether this action can be merged with the next action.
    /// Used for intelligent grouping (e.g., multiple file selections → one undo).
    /// </summary>
    bool CanMergeWith(IUndoableAction other);

    /// <summary>
    /// Merges this action with another for batch undo.
    /// </summary>
    IUndoableAction MergeWith(IUndoableAction other);
}

