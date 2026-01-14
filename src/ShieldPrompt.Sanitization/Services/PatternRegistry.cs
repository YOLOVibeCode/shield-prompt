using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Sanitization.Services;

/// <summary>
/// Thread-safe registry for managing sanitization patterns.
/// </summary>
public class PatternRegistry : IPatternRegistry
{
    private readonly Dictionary<string, Pattern> _patterns = new();
    private readonly object _lock = new();

    public void AddPattern(Pattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        lock (_lock)
        {
            _patterns[pattern.Name] = pattern;
        }
    }

    public IEnumerable<Pattern> GetPatterns(PatternCategory? category = null)
    {
        lock (_lock)
        {
            var patterns = _patterns.Values.Where(p => p.IsEnabled);

            if (category.HasValue)
            {
                patterns = patterns.Where(p => p.Category == category.Value);
            }

            return patterns.ToList(); // Return copy to avoid external modification
        }
    }

    public Pattern? GetPattern(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        lock (_lock)
        {
            return _patterns.GetValueOrDefault(name);
        }
    }

    public bool RemovePattern(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        lock (_lock)
        {
            return _patterns.Remove(name);
        }
    }
}

