using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Context information for generating a prompt.
/// Contains all data needed to construct a formatted prompt.
/// </summary>
public record PromptContext(
    string SessionId,
    Role SelectedRole,
    string CustomInstructions,
    IReadOnlyList<FileNode> SelectedFiles,
    string BaseDirectory,
    string TargetModel,
    bool IsSanitized,
    int TotalTokens);

/// <summary>
/// Strategy interface for formatting LLM prompts with response instructions.
/// ISP-compliant: Each format is a separate implementation.
/// </summary>
public interface IResponseFormatStrategy
{
    /// <summary>
    /// Human-readable name of the format (e.g., "Hybrid XML-in-Markdown").
    /// </summary>
    string FormatName { get; }
    
    /// <summary>
    /// Brief description of when to use this format.
    /// </summary>
    string FormatDescription { get; }
    
    /// <summary>
    /// The format type enum value.
    /// </summary>
    ResponseFormat FormatType { get; }
    
    /// <summary>
    /// Whether this format supports partial file updates (line-range modifications).
    /// </summary>
    bool SupportsPartialUpdates { get; }
    
    /// <summary>
    /// LLM adherence rate (0.0 to 1.0).
    /// Indicates how reliably LLMs follow this format's instructions.
    /// </summary>
    double LlmAdherenceRate { get; }
    
    /// <summary>
    /// Parse reliability score (0.0 to 1.0).
    /// Indicates how reliably this format can be parsed.
    /// </summary>
    double ParseReliability { get; }
    
    /// <summary>
    /// Token efficiency score (0.0 to 1.0).
    /// Higher means less token overhead compared to base content.
    /// </summary>
    double TokenEfficiency { get; }
    
    /// <summary>
    /// Generates the complete prompt with response format instructions.
    /// </summary>
    /// <param name="context">All information needed for the prompt</param>
    /// <returns>Fully formatted prompt ready to send to LLM</returns>
    string GeneratePrompt(PromptContext context);
    
    /// <summary>
    /// Estimates the token overhead this format adds to the base content.
    /// </summary>
    /// <param name="baseTokens">Token count of just the file content</param>
    /// <returns>Estimated total tokens including format overhead</returns>
    int EstimateTokenOverhead(int baseTokens);
    
    /// <summary>
    /// Generates a preview/example of what the LLM response should look like.
    /// Used in settings UI to show users what to expect.
    /// </summary>
    /// <returns>Example response format</returns>
    string GenerateResponseExample();
}

