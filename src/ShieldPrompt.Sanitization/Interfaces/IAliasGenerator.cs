using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Sanitization.Interfaces;

/// <summary>
/// Generates unique aliases for sensitive values.
/// ISP-compliant: single responsibility.
/// </summary>
public interface IAliasGenerator
{
    /// <summary>
    /// Generates a unique alias for the given pattern category.
    /// </summary>
    string Generate(PatternCategory category);

    /// <summary>
    /// Resets all counters (for new session).
    /// </summary>
    void Reset();
}

