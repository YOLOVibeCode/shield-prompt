namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Metadata about a prompt format, including pros and cons.
/// Used to help users choose the best format for their LLM.
/// </summary>
public record FormatMetadata(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> Pros,
    IReadOnlyList<string> Cons,
    string RecommendedFor,
    string Icon = "📄");

