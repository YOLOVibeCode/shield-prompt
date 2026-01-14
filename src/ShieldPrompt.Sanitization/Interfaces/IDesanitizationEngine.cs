using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Sanitization.Interfaces;

/// <summary>
/// Engine for restoring original values from sanitized content.
/// ISP-compliant: single responsibility - desanitization only.
/// </summary>
public interface IDesanitizationEngine
{
    /// <summary>
    /// Restores original values from aliases in content.
    /// </summary>
    string Desanitize(string content, IMappingSession session);
}

