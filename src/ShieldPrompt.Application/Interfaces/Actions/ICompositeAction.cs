namespace ShieldPrompt.Application.Interfaces.Actions;

/// <summary>
/// Composite action that groups multiple actions into a single undoable unit.
/// Follows ISP: Only 2 additional members beyond IAction.
/// </summary>
public interface ICompositeAction : IAction
{
    /// <summary>
    /// Read-only collection of child actions.
    /// </summary>
    IReadOnlyList<IAction> Actions { get; }

    /// <summary>
    /// Number of actions in the composite.
    /// </summary>
    int Count { get; }
}

