using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing recent workspace history.
/// Follows ISP - history operations only.
/// </summary>
public interface IRecentWorkspaceService
{
    /// <summary>
    /// Gets the N most recently opened workspaces.
    /// </summary>
    Task<IReadOnlyList<Workspace>> GetRecentAsync(int count = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Records a workspace as recently opened (updates timestamp).
    /// </summary>
    Task RecordOpenedAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Clears recent workspace history.
    /// </summary>
    Task ClearHistoryAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Removes a workspace from recent history.
    /// </summary>
    Task RemoveFromHistoryAsync(string workspaceId, CancellationToken ct = default);
}

