using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Actions;

/// <summary>
/// Composite action that executes multiple actions as a single atomic unit.
/// Provides automatic rollback on failure.
/// </summary>
public class CompositeAction : ICompositeAction
{
    private readonly List<IAction> _actions;
    private readonly List<IAction> _executedActions = new();

    public Guid Id { get; } = Guid.NewGuid();
    public string Description { get; }
    public IReadOnlyList<IAction> Actions => _actions.AsReadOnly();
    public int Count => _actions.Count;

    /// <summary>
    /// Creates a composite action from a collection of actions.
    /// </summary>
    /// <param name="description">Human-readable description for UI display.</param>
    /// <param name="actions">Child actions to execute as a batch.</param>
    public CompositeAction(string description, IEnumerable<IAction> actions)
    {
        Description = description;
        _actions = actions.ToList();
    }

    /// <summary>
    /// Executes all child actions in order.
    /// If any action fails, rolls back all previously executed actions.
    /// </summary>
    public async Task<ActionResult> ExecuteAsync(CancellationToken ct = default)
    {
        _executedActions.Clear();

        if (_actions.Count == 0)
        {
            return new ActionResult(true);
        }

        try
        {
            for (int i = 0; i < _actions.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var action = _actions[i];
                var result = await action.ExecuteAsync(ct);

                if (!result.Success)
                {
                    // Rollback all previously executed actions
                    await RollbackExecutedActionsAsync(ct);
                    return new ActionResult(
                        false,
                        $"Failed at action {i + 1}/{_actions.Count} ({action.Description}): {result.ErrorMessage}");
                }

                _executedActions.Add(action);
            }

            return new ActionResult(true);
        }
        catch (Exception ex)
        {
            // Rollback on exception
            await RollbackExecutedActionsAsync(ct);
            return new ActionResult(false, $"Exception during batch execution: {ex.Message}");
        }
    }

    /// <summary>
    /// Undoes all child actions in reverse order.
    /// Continues even if individual undo operations fail (best-effort).
    /// </summary>
    public async Task<ActionResult> UndoAsync(CancellationToken ct = default)
    {
        if (_executedActions.Count == 0)
        {
            // If nothing was executed, undo the originally provided actions (assumes they were executed before)
            return await UndoActionsAsync(_actions, ct);
        }

        return await UndoActionsAsync(_executedActions, ct);
    }

    /// <summary>
    /// Rolls back all executed actions during a failed Execute.
    /// </summary>
    private async Task RollbackExecutedActionsAsync(CancellationToken ct)
    {
        await UndoActionsAsync(_executedActions, ct);
        _executedActions.Clear();
    }

    /// <summary>
    /// Undoes a list of actions in reverse order (best-effort).
    /// </summary>
    private static async Task<ActionResult> UndoActionsAsync(IReadOnlyList<IAction> actions, CancellationToken ct)
    {
        var errors = new List<string>();

        // Undo in reverse order
        for (int i = actions.Count - 1; i >= 0; i--)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                var result = await actions[i].UndoAsync(ct);
                if (!result.Success && result.ErrorMessage != null)
                {
                    errors.Add($"Action {i}: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Action {i}: Exception - {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            return new ActionResult(false, $"Undo completed with {errors.Count} errors: {string.Join("; ", errors)}");
        }

        return new ActionResult(true);
    }
}

