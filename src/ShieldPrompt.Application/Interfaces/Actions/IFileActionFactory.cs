using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces.Actions;

/// <summary>
/// Factory for creating file actions from file operations.
/// ISP-compliant: Single responsibility - create file actions.
/// </summary>
public interface IFileActionFactory
{
    /// <summary>
    /// Creates the appropriate IFileAction for the given file operation.
    /// </summary>
    /// <param name="operation">The file operation to create an action for.</param>
    /// <param name="workspaceRoot">Root directory of the workspace.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A concrete IFileAction implementation.</returns>
    Task<IFileAction> CreateAsync(
        FileOperation operation,
        string workspaceRoot,
        CancellationToken ct = default);
}

