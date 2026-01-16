using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Service for managing active sessions in memory.
/// Follows ISP - session lifecycle only.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Gets the currently active session ID.
    /// </summary>
    string? ActiveSessionId { get; }
    
    /// <summary>
    /// Gets all active session IDs.
    /// </summary>
    IReadOnlyList<string> ActiveSessionIds { get; }
    
    /// <summary>
    /// Creates a new session and sets it as active.
    /// </summary>
    PromptSession CreateSession(string workspaceId, string name);
    
    /// <summary>
    /// Sets the active session.
    /// </summary>
    void SetActiveSession(string sessionId);
    
    /// <summary>
    /// Closes a session.
    /// </summary>
    void CloseSession(string sessionId);
    
    /// <summary>
    /// Event fired when active session changes.
    /// </summary>
    event EventHandler<SessionChangedEventArgs>? ActiveSessionChanged;
}

/// <summary>
/// Event arguments for session change events.
/// </summary>
public record SessionChangedEventArgs(string? OldSessionId, string? NewSessionId);

