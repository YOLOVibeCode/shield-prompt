namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Result of a sanitization operation.
/// Immutable record.
/// </summary>
public record SanitizationResult(
    string SanitizedContent,
    bool WasSanitized,
    IReadOnlyList<SanitizationMatch> Matches)
{
    public int TotalMatches => Matches.Count;
}

