using ShieldPrompt.Domain.Enums;

namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Represents a match found during sanitization.
/// Immutable record.
/// </summary>
public record SanitizationMatch(
    string Original,
    string Alias,
    PatternCategory Category,
    string PatternName,
    int StartIndex,
    int Length);

