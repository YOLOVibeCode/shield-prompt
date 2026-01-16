using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for managing session runtime state.
/// </summary>
public class SessionStateService : ISessionStateService
{
    private readonly Dictionary<string, SessionState> _states = new();

    public SessionState? GetState(string sessionId)
    {
        return _states.TryGetValue(sessionId, out var state) ? state : null;
    }

    public void UpdateState(string sessionId, SessionState state)
    {
        _states[sessionId] = state;
    }

    public void MarkDirty(string sessionId)
    {
        if (_states.TryGetValue(sessionId, out var current))
        {
            _states[sessionId] = current with { HasUnsavedChanges = true };
        }
        else
        {
            // Create a new state if it doesn't exist
            _states[sessionId] = new SessionState(sessionId, string.Empty, 0, true);
        }
    }

    public void MarkClean(string sessionId)
    {
        if (_states.TryGetValue(sessionId, out var current))
        {
            _states[sessionId] = current with { HasUnsavedChanges = false };
        }
    }
}

