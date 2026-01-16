using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing session runtime state.
/// Follows ISP - state management only.
/// </summary>
public interface ISessionStateService
{
    /// <summary>
    /// Gets the current state for a session.
    /// </summary>
    SessionState? GetState(string sessionId);
    
    /// <summary>
    /// Updates the state for a session.
    /// </summary>
    void UpdateState(string sessionId, SessionState state);
    
    /// <summary>
    /// Marks a session as having unsaved changes.
    /// </summary>
    void MarkDirty(string sessionId);
    
    /// <summary>
    /// Clears the dirty flag for a session.
    /// </summary>
    void MarkClean(string sessionId);
}

