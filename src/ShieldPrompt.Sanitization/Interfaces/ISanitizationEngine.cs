using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Sanitization.Interfaces;

/// <summary>
/// Engine for sanitizing sensitive data in content.
/// ISP-compliant: single responsibility - sanitization only.
/// </summary>
public interface ISanitizationEngine
{
    /// <summary>
    /// Sanitizes content by replacing sensitive values with aliases.
    /// </summary>
    SanitizationResult Sanitize(string content, SanitizationOptions options);
}

