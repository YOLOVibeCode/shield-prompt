namespace ShieldPrompt.Application.Interfaces.Actions;

/// <summary>
/// Base interface for all undoable actions.
/// ISP-compliant: 5 members total.
/// </summary>
public interface IAction
{
    /// <summary>Unique identifier for this action instance.</summary>
    Guid Id { get; }
    
    /// <summary>Human-readable description for UI display.</summary>
    string Description { get; }
    
    /// <summary>Execute the action. Returns result with success/failure.</summary>
    Task<ActionResult> ExecuteAsync(CancellationToken ct = default);
    
    /// <summary>Undo the action. Returns result with success/failure.</summary>
    Task<ActionResult> UndoAsync(CancellationToken ct = default);
}

/// <summary>
/// Result of action execution.
/// </summary>
public record ActionResult(
    bool Success,
    string? ErrorMessage = null,
    IReadOnlyDictionary<string, object>? Metadata = null);

