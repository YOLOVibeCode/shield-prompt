using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Sanitization.Services;

/// <summary>
/// In-memory session for storing sanitization mappings.
/// Thread-safe and secure - clears data on disposal.
/// </summary>
public class MappingSession : IMappingSession, IDisposable
{
    private readonly Dictionary<string, string> _aliasToOriginal = new();
    private readonly Dictionary<string, string> _originalToAlias = new();
    private readonly object _lock = new();
    private DateTime _expiresAt;

    public MappingSession() : this(TimeSpan.FromHours(4))
    {
    }

    public MappingSession(TimeSpan sessionDuration)
    {
        SessionId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        _expiresAt = CreatedAt.Add(sessionDuration);
    }

    public string SessionId { get; }
    public DateTime CreatedAt { get; }
    public DateTime ExpiresAt
    {
        get
        {
            lock (_lock)
            {
                return _expiresAt;
            }
        }
    }

    public void AddMapping(string original, string alias, PatternCategory category)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(alias);

        lock (_lock)
        {
            // Don't overwrite existing mappings for the same original
            if (_originalToAlias.ContainsKey(original))
                return;

            _aliasToOriginal[alias] = original;
            _originalToAlias[original] = alias;
        }
    }

    public string? GetOriginal(string alias)
    {
        lock (_lock)
        {
            return _aliasToOriginal.GetValueOrDefault(alias);
        }
    }

    public string? GetAlias(string original)
    {
        lock (_lock)
        {
            return _originalToAlias.GetValueOrDefault(original);
        }
    }

    public IReadOnlyDictionary<string, string> GetAllMappings()
    {
        lock (_lock)
        {
            return new Dictionary<string, string>(_aliasToOriginal);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            // Secure clear - overwrite strings before clearing
            foreach (var key in _aliasToOriginal.Keys.ToList())
            {
                _aliasToOriginal[key] = new string('0', _aliasToOriginal[key].Length);
            }

            foreach (var key in _originalToAlias.Keys.ToList())
            {
                _originalToAlias[key] = new string('0', _originalToAlias[key].Length);
            }

            _aliasToOriginal.Clear();
            _originalToAlias.Clear();
        }
    }

    public void ExtendSession(TimeSpan duration)
    {
        lock (_lock)
        {
            _expiresAt = _expiresAt.Add(duration);
        }
    }

    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }
}

