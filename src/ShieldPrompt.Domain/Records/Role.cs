namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Defines an AI role/persona for prompt generation.
/// Each role has specific expertise, priorities, and style.
/// </summary>
public record Role(
    string Id,
    string Name,
    string Icon,
    string Description,
    string SystemPrompt,
    string Tone,
    string Style,
    IReadOnlyList<string> Priorities,
    IReadOnlyList<string> Expertise)
{
    /// <summary>
    /// Whether this is a built-in role (cannot be edited/deleted).
    /// </summary>
    public bool IsBuiltIn { get; init; } = true;
}

