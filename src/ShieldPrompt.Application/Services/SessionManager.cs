using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for managing active sessions in memory.
/// </summary>
public class SessionManager : ISessionManager
{
    private readonly Dictionary<string, PromptSession> _activeSessions = new();
    private string? _activeSessionId;

    public string? ActiveSessionId => _activeSessionId;
    
    public IReadOnlyList<string> ActiveSessionIds => _activeSessions.Keys.ToList();

    public event EventHandler<SessionChangedEventArgs>? ActiveSessionChanged;

    public PromptSession CreateSession(string workspaceId, string name)
    {
        var session = PromptSession.CreateNew(workspaceId, name);
        _activeSessions[session.Id] = session;
        SetActiveSession(session.Id);
        return session;
    }

    public void SetActiveSession(string sessionId)
    {
        if (!_activeSessions.ContainsKey(sessionId))
            throw new InvalidOperationException($"Session {sessionId} not found");

        var oldId = _activeSessionId;
        _activeSessionId = sessionId;
        ActiveSessionChanged?.Invoke(this, new SessionChangedEventArgs(oldId, sessionId));
    }

    public void CloseSession(string sessionId)
    {
        if (!_activeSessions.Remove(sessionId))
            return;

        if (_activeSessionId == sessionId)
        {
            // Switch to another session or null
            _activeSessionId = _activeSessions.Keys.FirstOrDefault();
            ActiveSessionChanged?.Invoke(this, new SessionChangedEventArgs(sessionId, _activeSessionId));
        }
    }
}

