using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Sanitization.Interfaces;

/// <summary>
/// Registry for managing sanitization patterns.
/// ISP-compliant: focused on pattern management only.
/// </summary>
public interface IPatternRegistry
{
    /// <summary>
    /// Adds a pattern to the registry.
    /// </summary>
    void AddPattern(Pattern pattern);

    /// <summary>
    /// Gets all patterns, optionally filtered by category.
    /// </summary>
    IEnumerable<Pattern> GetPatterns(PatternCategory? category = null);

    /// <summary>
    /// Gets a pattern by name.
    /// </summary>
    Pattern? GetPattern(string name);

    /// <summary>
    /// Removes a pattern by name.
    /// </summary>
    bool RemovePattern(string name);
}

