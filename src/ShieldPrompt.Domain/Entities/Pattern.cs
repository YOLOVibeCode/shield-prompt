using System.Text.RegularExpressions;
using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Entities;

/// <summary>
/// Represents a detection pattern for identifying sensitive data.
/// </summary>
public class Pattern
{
    private Regex? _compiledRegex;

    public Pattern(string name, string regexPattern, PatternCategory category)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(regexPattern);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pattern name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(regexPattern))
            throw new ArgumentException("Regex pattern cannot be empty.", nameof(regexPattern));

        Name = name;
        RegexPattern = regexPattern;
        Category = category;
    }

    /// <summary>
    /// Human-readable name of the pattern.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Regular expression pattern for detection.
    /// </summary>
    public string RegexPattern { get; }

    /// <summary>
    /// Category of sensitive data this pattern detects.
    /// </summary>
    public PatternCategory Category { get; }

    /// <summary>
    /// Whether this pattern is currently enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Processing priority (higher = processed first).
    /// Used to handle overlapping patterns correctly.
    /// </summary>
    public int Priority { get; init; } = 100;

    /// <summary>
    /// Creates a compiled regex from the pattern with timeout for safety.
    /// </summary>
    public Regex CreateRegex()
    {
        if (_compiledRegex == null)
        {
            try
            {
                _compiledRegex = new Regex(
                    RegexPattern,
                    RegexOptions.Compiled | RegexOptions.CultureInvariant,
                    TimeSpan.FromMilliseconds(100)); // Timeout to prevent ReDoS
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid regex pattern '{RegexPattern}': {ex.Message}", ex);
            }
        }

        return _compiledRegex;
    }

    /// <summary>
    /// Checks if this pattern matches the given content.
    /// </summary>
    public bool Matches(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        var regex = CreateRegex();
        return regex.IsMatch(content);
    }
}

