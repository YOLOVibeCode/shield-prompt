using System.Text.RegularExpressions;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Sanitization.Services;

/// <summary>
/// Engine for restoring original values from sanitized content.
/// </summary>
public class DesanitizationEngine : IDesanitizationEngine
{
    // Matches any alias pattern: CATEGORY_NUMBER
    private static readonly Regex AliasPattern = new(
        @"\b[A-Z_]+_\d+\b",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public string Desanitize(string content, IMappingSession session)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(session);

        if (string.IsNullOrEmpty(content))
            return content;

        var result = content;
        var mappings = session.GetAllMappings();

        if (mappings.Count == 0)
            return content;

        // Replace all aliases with originals
        // Process in reverse order of alias length to handle overlaps correctly
        foreach (var (alias, original) in mappings.OrderByDescending(kvp => kvp.Key.Length))
        {
            result = result.Replace(alias, original);
        }

        return result;
    }
}

