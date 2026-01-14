using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Sanitization.Interfaces;

/// <summary>
/// Manages the mapping between original sensitive values and their aliases.
/// ISP-compliant: focused on mapping management only.
/// </summary>
public interface IMappingSession : IDisposable
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    string SessionId { get; }

    /// <summary>
    /// When this session was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When this session expires.
    /// </summary>
    DateTime ExpiresAt { get; }

    /// <summary>
    /// Adds a mapping from original value to alias.
    /// </summary>
    void AddMapping(string original, string alias, PatternCategory category);

    /// <summary>
    /// Gets the original value for an alias.
    /// </summary>
    string? GetOriginal(string alias);

    /// <summary>
    /// Gets the alias for an original value.
    /// </summary>
    string? GetAlias(string original);

    /// <summary>
    /// Gets all mappings (alias → original).
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllMappings();

    /// <summary>
    /// Clears all mappings securely.
    /// </summary>
    void Clear();

    /// <summary>
    /// Extends the session expiry time.
    /// </summary>
    void ExtendSession(TimeSpan duration);
}

