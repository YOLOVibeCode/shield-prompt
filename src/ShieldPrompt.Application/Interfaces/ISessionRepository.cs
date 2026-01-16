using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for prompt session persistence.
/// Follows ISP - session CRUD only.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Gets all sessions for a workspace.
    /// </summary>
    Task<IReadOnlyList<PromptSession>> GetByWorkspaceAsync(string workspaceId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    Task<PromptSession?> GetByIdAsync(string sessionId, CancellationToken ct = default);
    
    /// <summary>
    /// Saves or updates a session.
    /// </summary>
    Task SaveAsync(PromptSession session, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a session.
    /// </summary>
    Task DeleteAsync(string sessionId, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes all sessions for a workspace.
    /// </summary>
    Task DeleteByWorkspaceAsync(string workspaceId, CancellationToken ct = default);
}

