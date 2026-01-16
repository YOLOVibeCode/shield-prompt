using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for managing workspaces.
/// Follows ISP: Only workspace CRUD operations, no file operations.
/// </summary>
public interface IWorkspaceRepository
{
    /// <summary>
    /// Gets all workspaces, ordered by last opened (most recent first).
    /// </summary>
    Task<IReadOnlyList<Workspace>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a workspace by ID.
    /// </summary>
    Task<Workspace?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Gets a workspace by root path.
    /// </summary>
    Task<Workspace?> GetByPathAsync(string rootPath, CancellationToken ct = default);

    /// <summary>
    /// Saves a workspace (creates if new, updates if exists).
    /// </summary>
    Task SaveAsync(Workspace workspace, CancellationToken ct = default);

    /// <summary>
    /// Deletes a workspace.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Updates the LastOpened timestamp for a workspace.
    /// </summary>
    Task UpdateLastOpenedAsync(string id, CancellationToken ct = default);
}
