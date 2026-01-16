namespace ShieldPrompt.Domain.Records;

/// <summary>
/// Defines a prompt template for different AI tasks (code review, debug, etc.)
/// </summary>
public record PromptTemplate(
    string Id,
    string Name,
    string Icon,
    string Description,
    string SystemPrompt,
    IReadOnlyList<string> FocusOptions,
    bool RequiresCustomInput = false,
    string? Placeholder = null);

/// <summary>
/// Options for composing a prompt from a template
/// </summary>
public record PromptOptions(
    string? CustomInstructions = null,
    IReadOnlyList<string>? SelectedFocusAreas = null,
    Role? SelectedRole = null,
    bool IncludeFilePaths = true,
    bool IncludeLineNumbers = false);

/// <summary>
/// The fully composed prompt ready to copy to clipboard
/// </summary>
public record ComposedPrompt(
    string SystemPrompt,
    string UserContent,
    string FullPrompt,
    int EstimatedTokens,
    IReadOnlyList<string> Warnings);

